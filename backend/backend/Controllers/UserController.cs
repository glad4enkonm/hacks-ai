using backend.Authorization;
using backend.Helpers;
using backend.Models;
using database.Repository;
using backend.Services;
using database.Helpers;
using database.Models.History;
using database.Repository.History;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using static System.String;
using AuthenticateRequest = backend.Models.AuthenticateRequest;

namespace backend.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class UserController: ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly IUserService _userService;
    private readonly AppSettings _appSettings;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUserRepository _repositoryUser;
    private readonly IUserRepository _userRepository;

    public UserController(ILogger<UserController> logger, IUserService userService, IOptions<AppSettings> appSettings, 
        IRefreshTokenRepository refreshTokenRepository, IUserRepository userRepository, IUserRepository repositoryUser)
    {
        _logger = logger;
        _userService = userService;
        _appSettings = appSettings.Value;
        _refreshTokenRepository = refreshTokenRepository;
        _userRepository = userRepository;
        _repositoryUser = repositoryUser;
    }

    [AllowAnonymous]
    [HttpPost("authenticate")]
    public AuthenticateResponse Authenticate(AuthenticateRequest requestTransportModel)
    {

        (database.Models.History.User User, string RefreshToken, string JwtToken) response = 
            _userService.Authenticate(requestTransportModel.Login, requestTransportModel.PasswordHash,
                Http.IpAddress(Request, HttpContext));
        
        Http.SetTokenCookie(response.RefreshToken, Response, _appSettings.RefreshTokenDurationInDays);
        
        return new AuthenticateResponse
        {
            Token = response.JwtToken,
            Login = response.User.Login,
            UserId = response.User.UserId
        };
    }
    
    [AllowAnonymous]
    [HttpPost("refresh-token")]
    public AuthenticateResponse RefreshToken(TokenRequest? tokenRequest)
    {
        // Принимаем ключ для отзыва из тела запроса или из печенья
        var refreshToken = tokenRequest is { Token.Length: > 0 } 
            ? tokenRequest.Token : Request.Cookies["refreshToken"];
        if (refreshToken == null)
            throw new AppException("Invalid token");
        (database.Models.History.User User, string JwtToken, string RefreshToken) response = 
            _userService.RefreshToken(refreshToken, Http.IpAddress(Request, HttpContext));
        
        Http.SetTokenCookie(response.RefreshToken, Response, _appSettings.RefreshTokenDurationInDays);
        return new AuthenticateResponse
        {
            Token = response.JwtToken,
            Login = response.User.Name,
            UserId = response.User.UserId
        };
    }
    
    [HttpPost("revoke-token")]
    public IActionResult RevokeToken(TokenRequest? tokenRequest)
    {
        // Принимаем ключ для отзыва из тела запроса или из печенья
        var token = tokenRequest is { Token.Length: > 0 } 
            ? tokenRequest.Token : Request.Cookies["refreshToken"];

        if (IsNullOrEmpty(token))
            return BadRequest(new { message = "Token is required" });

        _userService.RevokeToken(token, Http.IpAddress(Request, HttpContext));
        return Ok(new { message = "Token revoked" });
    }
    
    [HttpGet]
    public User GetCurrentUser()
    {
        var userId = (ulong)(HttpContext.Items["UserId"] ?? throw new InvalidOperationException());
        return _userRepository.Get(userId);
    }

    [HttpGet("{id}")]
    public User GetById(ulong id)
    {
        return _userRepository.Get(id);
    }
    
    
#region User CRUD
    [HttpPost("User")]
    public User CreateUser(User entity)
    {
        var userId = (ulong)(HttpContext.Items["UserId"] ?? throw new InvalidOperationException());
        
        // Разрешаем редактирование только для администратора
        if (userId != (ulong)UserEnum.SystemOrAdmin) throw new InvalidOperationException();
        // TODO: здесь ещё можно добавить проверку роли пользователя, лучше через HttpContext.Items["UserRoleId"]
        
        entity.PasswordHash = _userService.GetHashSalted(entity.PasswordHash); // солим пароль
        
        return _repositoryUser.Insert(entity, userId);
    }

    [HttpGet("User/{id}")]
    public User? GetUser(ulong id)
    {
        var userId = (ulong)(HttpContext.Items["UserId"] ?? throw new InvalidOperationException());
        
        // Разрешаем редактирование только для администратора
        if (userId != (ulong)UserEnum.SystemOrAdmin) throw new InvalidOperationException();
        // TODO: здесь ещё можно добавить проверку роли пользователя, лучше через HttpContext.Items["UserRoleId"]
        
        return _repositoryUser.Get(id);
    }

    [HttpGet("User/list")]
    public IList<User> GetUserList()
    {
        var userId = (ulong)(HttpContext.Items["UserId"] ?? throw new InvalidOperationException());
        
        // Разрешаем редактирование только для администратора
        if (userId != (ulong)UserEnum.SystemOrAdmin) throw new InvalidOperationException();
        // TODO: здесь ещё можно добавить проверку роли пользователя, лучше через HttpContext.Items["UserRoleId"]
        
        return _repositoryUser.GetAll().ToList();
    }

    [HttpPatch("User")]
    public User UpdateUser(KeyValuePair<string, string>[] patch)
    {
        var userId = (ulong)(HttpContext.Items["UserId"] ?? throw new InvalidOperationException());
        
        // Разрешаем редактирование только для администратора
        if (userId != (ulong)UserEnum.SystemOrAdmin) throw new InvalidOperationException();
        // TODO: здесь ещё можно добавить проверку роли пользователя, лучше через HttpContext.Items["UserRoleId"]
        
        ulong id = Convert.ToUInt64(patch.First(pair => pair.Key == "UserId").Value);
        var previousUserState = _userRepository.GetWithPasswordHash(id);
        
        var previousPwdHash = previousUserState.PasswordHash;
        var allowedPropsToUpdate = new[] {"Name","Login","PasswordHash","Description"};
        var updatedUserState = Diff.ApplyAllowedDiff(previousUserState, patch, allowedPropsToUpdate) as User;
        if (updatedUserState != null && previousPwdHash != updatedUserState.PasswordHash) // солим пароль
            updatedUserState.PasswordHash = _userService.GetHashSalted(updatedUserState.PasswordHash);
        patch = patch.Select(p => // убираем пароль из истории
            p.Key == "PasswordHash" ? new KeyValuePair<string, string>("PasswordHash", "Updated") : p).ToArray();
        
        _userRepository.Update(updatedUserState ?? throw new InvalidOperationException(), userId, patch);
        updatedUserState.PasswordHash = ""; // скрываем пароль при отправке пользователю
        
        return updatedUserState;
    }

    [HttpDelete("User/{id}")]
    public bool DeleteUser(ulong id)
    {
        var userId = (ulong)(HttpContext.Items["UserId"] ?? throw new InvalidOperationException());
        
        // Разрешаем редактирование только для администратора
        if (userId != (ulong)UserEnum.SystemOrAdmin) throw new InvalidOperationException();
        // TODO: здесь ещё можно добавить проверку роли пользователя, лучше через HttpContext.Items["UserRoleId"]
        
        var entity = _repositoryUser.Get(id) ?? throw new InvalidOperationException();
        return _repositoryUser.Delete(entity, userId);
    }

#endregion

    /*
    [HttpGet]
    public IEnumerable<User> GetAll()
    {
        return _userRepository.GetAll();
    }

    [HttpGet("{id}")]
    public User GetById(ulong id)
    {
        return _userRepository.Get(id);
    }

    [HttpGet("{id}/refresh-tokens")]
    public RefreshToken GetRefreshTokens(ulong id)
    {
        return _refreshTokenRepository.GetRefreshTokenByUser(id);
    }
    */
    
}