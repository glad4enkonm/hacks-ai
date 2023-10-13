using Dapper.Contrib.Extensions;

namespace database.Models;

[Table("UserToRefreshToken")]
public class UserToRefreshToken: IEntity
{
    [Key]
    public ulong UserToRefreshTokenId { get; set; }
    public ulong UserId { get; set; }
    public ulong RefreshTokenId { get; set; }
    public ulong GetId ()
    {
        return UserToRefreshTokenId;
    }
}