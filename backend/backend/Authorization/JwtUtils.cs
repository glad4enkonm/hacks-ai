using backend.Helpers;
using database.Models;
using database.Repository;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using database.Models.History;
using Microsoft.IdentityModel.Tokens;
using static System.DateTime;

namespace backend.Authorization;

public interface IJwtUtils
{
    public string GenerateJwtToken(User user);
    public ulong? ValidateJwtToken(string token);
    public RefreshToken GenerateRefreshToken(string ipAddress);
}

public class JwtUtils : IJwtUtils
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly AppSettings _appSettings;
    private readonly string _secret;

    public JwtUtils(
        IRefreshTokenRepository refreshTokenRepository,
        IOptions<AppSettings> appSettings)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _appSettings = appSettings.Value;
        _secret = Environment.GetEnvironmentVariable("BACKEND_SECRET") 
                  ?? throw new InvalidOperationException("BACKEND_SECRET");
    }
    
    public string GenerateJwtToken(User user)
    {
        // Создаём токен на _appSettings.AccessTokenTTL минут для доступа
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_secret);
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] { new Claim("id", user.UserId.ToString()) }),
            Expires = UtcNow.AddMinutes(_appSettings.AccessTokenDurationInMin),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public ulong? ValidateJwtToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_secret);
        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero // ключ истекае ровно в обозначеное время (не на 5 минут раньше)
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            var userId = ulong.Parse(jwtToken.Claims.First(x => x.Type == "id").Value);
            
            return userId; // возвращаем id пользователя если проверка пройдена
        }
        catch
        {
            return null; // если не удалось проверить ключ
        }
    }

    public RefreshToken GenerateRefreshToken(string ipAddress)
    {
        var refreshToken = new RefreshToken
        {
            Token = GetUniqueToken(),
            Expires = UtcNow.AddDays(_appSettings.RefreshTokenDurationInDays),
            Created = UtcNow,
            CreatedByIp = ipAddress
        };

        return refreshToken;

        string GetUniqueToken()
        {
            // token is a cryptographically strong random sequence of values
            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            // ensure token is unique by checking against db
            var tokenIsUnique = !_refreshTokenRepository.RefreshTokenExistsByTokenValue(token);

            return !tokenIsUnique ? GetUniqueToken() : token;
        }
    }
}