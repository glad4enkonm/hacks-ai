
using Dapper;
using database.Helpers;
using database.Models;
using database.Repository.Base;

namespace database.Repository;

public interface IRefreshTokenRepository : IDataRepository<RefreshToken>
{
    /// <summary>
    /// Проверяем есть ли refresh ключ с заданным значением
    /// </summary>
    /// <param name="refreshTokenValueToCheck">Значение ключа для проверки</param>
    /// <returns>Ture если ключ найден</returns>
    bool RefreshTokenExistsByTokenValue(string refreshTokenValueToCheck);

    /// <summary>
    /// Удалчем ключи пользователя отозваные или истёкшие
    /// </summary>
    /// <param name="userId">Пользователь</param>
    void DeleteRefreshTokenIdsExpiredByUser(ulong userId);
    
    /// <summary>
    /// Получаем ключ обновления по пользователю
    /// </summary>
    /// <param name="userId">Пользователь</param>
    /// <returns>Соответсвующий ключ</returns>
    RefreshToken GetRefreshTokenByUser(ulong userId);

    /// <summary>
    /// Получить объект ключа по значению
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    RefreshToken? GetRefreshTokenByToken(string token);
}

public class RefreshTokenRepository : DataRepositoryBase<RefreshToken>, IRefreshTokenRepository
{
    public bool RefreshTokenExistsByTokenValue(string refreshTokenValueToCheck) =>
        ConnectionString.QueryUsingConnection(
            con => con.Query<int>(
                $"SELECT EXISTS(SELECT RefreshTokenId FROM RefreshToken WHERE Token = @RefreshTokenValueToCheck)",
                new { RefreshTokenValueToCheck = refreshTokenValueToCheck })
        ).FirstOrDefault() == 1;
    
    public void DeleteRefreshTokenIdsExpiredByUser(ulong userId) =>
        ConnectionString.QueryUsingConnection(
            con => con.Query(
                @"DELETE
                        FROM RefreshToken USING UserToRefreshToken
                        INNER JOIN User ON User.UserId = UserToRefreshToken.UserId
                        INNER JOIN RefreshToken ON RefreshToken.RefreshTokenId = UserToRefreshToken.RefreshTokenId
                        WHERE User.UserId = @UserId
                            AND RefreshToken.Revoked IS NULL
                            AND CURRENT_TIMESTAMP() >= RefreshToken.Expires",
                new { UserId = userId })
        );

    public RefreshToken GetRefreshTokenByUser(ulong userId) =>
        ConnectionString.QueryUsingConnection(
            con => con.Query<RefreshToken>(
                @"SELECT RefreshToken.*
                        FROM RefreshToken
                        INNER JOIN UserToRefreshToken ON RefreshToken.RefreshTokenId = UserToRefreshToken.RefreshTokenId
                        INNER JOIN User ON User.UserId = UserToRefreshToken.UserId
                        WHERE User.UserId = @UserId",
                new { UserId = userId })
        ).First();
    
    public RefreshToken? GetRefreshTokenByToken(string token) =>
        ConnectionString.QueryUsingConnection(
            con => con.Query<RefreshToken>(
                @"SELECT *
                        FROM RefreshToken
                        WHERE RefreshToken.Token = @Token",
                new { Token = token })
        ).FirstOrDefault();
}