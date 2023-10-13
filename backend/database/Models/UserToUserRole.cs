using Dapper.Contrib.Extensions;

namespace database.Models;

[Table("`UserToUserRole`")]
public class UserToUserRole: IEntity
{
    [Key]
    public ulong UserToUserRoleId { get; set; }
    public ulong UserId { get; set; }
    public ulong UserRoleId { get; set; }
    public ulong GetId ()
    {
        return UserToUserRoleId;
    }
}
