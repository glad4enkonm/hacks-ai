using Dapper.Contrib.Extensions;
using database.Models.History.Base;

namespace database.Models.History;

[Table("`User`")]
public class User: EntityWithHistoryBase
{
    [Key]
    public ulong UserId { get; set; }
    public string Name { get; set; }
    public string Login { get; set; }
    public string PasswordHash { get; set; }
    public string Description { get; set; }
    public override bool IsDeleted { get; set; }
    public override ulong GetId ()
    {
        return UserId;
    }
}
