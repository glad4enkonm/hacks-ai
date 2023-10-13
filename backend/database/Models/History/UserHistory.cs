using Dapper.Contrib.Extensions;
using database.Models.History.Base;

namespace database.Models.History;

[Table("`UserHistory`")]
public class UserHistory: HistoryBase
{
    [Key]
    public ulong UserHistoryId { get; set; }
}
