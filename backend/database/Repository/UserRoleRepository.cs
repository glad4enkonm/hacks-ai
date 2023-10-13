using database.Repository.Base;
using database.Models;

namespace database.Repository;

public interface IUserRoleRepository : IDataRepository<UserRole>
{
}

public class UserRoleRepository : DataRepositoryBase<UserRole>, IUserRoleRepository
{
}
