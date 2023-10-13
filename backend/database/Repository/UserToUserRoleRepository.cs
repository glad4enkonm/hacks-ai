using database.Repository.Base;
using database.Models;

namespace database.Repository;

public interface IUserToUserRoleRepository : IDataRepository<UserToUserRole>
{
}

public class UserToUserRoleRepository : DataRepositoryBase<UserToUserRole>, IUserToUserRoleRepository
{
}
