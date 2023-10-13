using System.Security.Cryptography;
using System.Text;
using backend.Authorization;
using backend.Helpers;
using database.Models;
using database.Models.History;
using database.Repository;
using database.Repository.History;
using Microsoft.Extensions.Options;

namespace backend.Services;

public interface IUserService
{
    (User, string, string) Authenticate(string login, string passwordHash, string ipAddress);
    (User, string, string) RefreshToken(string token, string ipAddress);
    void RevokeToken(string token, string ipAddress);
    string GetHashSalted(string passwordHash);
}

public class UserService: IUserService
{
    private readonly IJwtUtils _jwtUtils;
    private readonly string _hashSalt;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserToRefreshTokenRepository _userToRefreshTokenRepository;
    
    public UserService(
        IJwtUtils jwtUtils,
        IOptions<AppSettings> appSettings,
        IRefreshTokenRepository refreshTokenRepository,
        IUserRepository userRepository, 
        IUserToRefreshTokenRepository userToRefreshTokenRepository)
    {
        _jwtUtils = jwtUtils;
        _hashSalt = Environment.GetEnvironmentVariable("BACKEND_HASH_SALT") 
                    ?? throw new InvalidOperationException("Hash salt not found");
        _refreshTokenRepository = refreshTokenRepository;
        _userRepository = userRepository;
        _userToRefreshTokenRepository = userToRefreshTokenRepository;
    }
    
    public (User, string, string) Authenticate(string login, string passwordHash, string ipAddress)
    {
        if (!IsBase64String(passwordHash))
            throw new AppException("Password is incorrect");
        var user = _userRepository.GetUserByLogin(login);
        var hashSalted = GetHashSalted(passwordHash);
        // Проверяем hash
        if (user == null || user.PasswordHash != hashSalted)
            throw new AppException("Username or password is incorrect");

        // Пароль проверен, продолжаем
        var jwtToken = _jwtUtils.GenerateJwtToken(user);
        RefreshToken refreshToken = _jwtUtils.GenerateRefreshToken(ipAddress);
        
        var refreshTokenInserted = _refreshTokenRepository.Insert(refreshToken);
        _userToRefreshTokenRepository.Insert(new UserToRefreshToken() {
            UserId = user.UserId,
            RefreshTokenId = refreshTokenInserted.RefreshTokenId
        });

        _refreshTokenRepository.DeleteRefreshTokenIdsExpiredByUser(user.UserId);

        return (user, refreshToken.Token, jwtToken);
    }

    public string GetHashSalted(string passwordHash)
    {
        var hashSaltedBytes = SHA512.HashData(Encoding.ASCII.GetBytes(_hashSalt)
            .Concat(Convert.FromBase64String(passwordHash)).ToArray());
        var hashSalted = Convert.ToBase64String(hashSaltedBytes);
        return hashSalted;
    }

    public (User, string, string) RefreshToken(string token, string ipAddress)
    {
        var user = _userRepository.GetUserByRefreshToken(token);
        if (user == null)
            throw new AppException("Invalid token");
        var refreshToken = _refreshTokenRepository.GetRefreshTokenByToken(token);
        if (refreshToken == null)
            throw new AppException("Invalid token");
        
        if (refreshToken.IsRevoked())
        {
            // Удаляем все последующие за текущим ключи, ключ перхвачен
            RevokeDescendantRefreshTokens(refreshToken, ipAddress, 
                $"Attempted reuse of revoked ancestor token: {token}");
        }

        if (!refreshToken.IsActive())
            throw new AppException("Invalid token");
        
        // Заменяем старый ключ на новый
        var newRefreshToken = RotateRefreshToken(refreshToken, ipAddress);
        // Сохраняем в базу данных
        var newRefreshTokenInserted = _refreshTokenRepository.Insert(newRefreshToken);
        _userToRefreshTokenRepository.Insert(new UserToRefreshToken()
        {
            UserId = user.UserId,
            RefreshTokenId = newRefreshTokenInserted.RefreshTokenId
        });

        // Удаляем старые ключи у пользователя
        _refreshTokenRepository.DeleteRefreshTokenIdsExpiredByUser(user.UserId);
        
        // Создаём новый jwt ключ
        var jwtToken = _jwtUtils.GenerateJwtToken(user);
        return (user, jwtToken, newRefreshToken.Token);
    }

    public void RevokeToken(string token, string ipAddress)
    {
        var user = _userRepository.GetUserByRefreshToken(token);
        if (user == null)
            throw new AppException("Invalid token");
        var refreshToken = _refreshTokenRepository.GetRefreshTokenByToken(token);
        if (refreshToken == null || !refreshToken.IsActive())
            throw new AppException("Invalid token");
        
        RevokeRefreshToken(refreshToken, ipAddress, "Revoked without replacement");
    }
    
    private RefreshToken RotateRefreshToken(RefreshToken refreshToken, string ipAddress)
    {
        var newRefreshToken = _jwtUtils.GenerateRefreshToken(ipAddress);
        RevokeRefreshToken(refreshToken, ipAddress, "Replaced by new token", newRefreshToken.Token);
        return newRefreshToken;
    }

    private void RevokeDescendantRefreshTokens(RefreshToken refreshToken, string ipAddress, string reason)
    {
        // Рекурсивно проходим по всй цепочки ключей и проверяем что все отозваны
        if (string.IsNullOrEmpty(refreshToken.ReplacedByToken)) return;
        
        var childToken = _refreshTokenRepository.GetRefreshTokenByToken(refreshToken.ReplacedByToken);
        if (childToken == null) return;
        if (childToken.IsActive())
            RevokeRefreshToken(childToken, ipAddress, reason);
        else
            RevokeDescendantRefreshTokens(childToken, ipAddress, reason);
    }

    private void RevokeRefreshToken(RefreshToken token, string ipAddress, string reason, string? replacedByToken = null)
    {
        token.Revoked = DateTime.UtcNow;
        token.RevokedByIp = ipAddress;
        token.ReasonRevoked = reason;
        token.ReplacedByToken = replacedByToken;
        _refreshTokenRepository.Update(token); // сохраняем запись в базе данных TODO: проверить работает ли сохранение - возможно нужно получать через Get 
    }
    
    private static bool IsBase64String(string base64)
    {
        var buffer = new Span<byte>(new byte[base64.Length]);
        return Convert.TryFromBase64String(base64, buffer , out var bytesParsed);
    }
}