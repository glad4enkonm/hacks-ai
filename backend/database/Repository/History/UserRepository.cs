using Dapper;
using database.Helpers;
using database.Models.History;
using database.Repository.Base;

namespace database.Repository.History;

public interface IUserRepository : IDataRepositoryWithHistory<User>
{
    /// <summary>
    /// Получаем  пользователя по login
    /// </summary>
    /// <returns>Пользователь с этим login или null</returns>
    User? GetUserByLogin(string login);

    /// <summary>
    /// Получаем пользователя по кулючу
    /// </summary>
    /// <param name="token">Ключ</param>
    /// <returns>Пользователь, если найден</returns>
    User? GetUserByRefreshToken(string token);

    /// <summary>
    /// Получаем пользователя с hash пароля
    /// </summary>
    /// <param name="id">Идентификатор пользователя</param>
    User GetWithPasswordHash(ulong id);
}

public class UserRepository : DataRepositoryWithHistoryBase<User, UserHistory>, IUserRepository
{
    public User GetWithPasswordHash(ulong id)
    {
        return base.Get(id);
    }
    
    public override User Get(ulong id)
    {
        var user = base.Get(id); // при обычном получении данных о пользователе не возвращаем данные пароля и login
        user.PasswordHash = string.Empty;
        user.Login = string.Empty;
        return user;
    }

    public override IEnumerable<User> GetAll() =>
        base.GetAll().Select(user =>
        { // При обычном получении данных о пользователе не возвращаем данные пароля
            user.PasswordHash = string.Empty;
            return user;
        }).ToList();

    public User? GetUserByLogin(string login) =>
        ConnectionString.QueryUsingConnection(
            con => con.Query<User>(
                "SELECT * FROM User WHERE Login = @Login",
                new { Login = login })
        ).FirstOrDefault();
    

    public User? GetUserByRefreshToken(string token) =>
        ConnectionString.QueryUsingConnection(
            con => con.Query<User>(
                @"SELECT User.*
                        FROM UserToRefreshToken
                        INNER JOIN User ON User.UserId = UserToRefreshToken.UserId
                        INNER JOIN RefreshToken ON RefreshToken.RefreshTokenId = UserToRefreshToken.RefreshTokenId
                        WHERE RefreshToken.Token = @Token",
                new { Token = token })
        ).FirstOrDefault();
}
