using Dapper.Contrib.Extensions;

namespace database.Models;

[Table("`UserRole`")]
public class UserRole: IEntity
{
    [Key]
    public ulong UserRoleId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public ulong GetId ()
    {
        return UserRoleId;
    }
}
