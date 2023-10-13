
using database.Models;
using database.Repository.Base;


namespace database.Repository;

public interface IUserToRefreshTokenRepository : IDataRepository<UserToRefreshToken>
{
}

public class UserToRefreshTokenRepository : DataRepositoryBase<UserToRefreshToken>, IUserToRefreshTokenRepository
{
}