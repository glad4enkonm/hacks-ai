namespace database.Models.History.Base;

public class HistoryBase
{
    public DateTime Changed { get; set; }
    public string Difference { get; set; }
    public ulong EntityId { get; set; }
    public ulong ChangedBy { get; set; }
}