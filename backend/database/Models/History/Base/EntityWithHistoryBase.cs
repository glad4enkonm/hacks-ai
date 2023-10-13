namespace database.Models.History.Base;

public abstract class EntityWithHistoryBase: IEntity
{
    public abstract bool  IsDeleted { get; set; }
    public abstract ulong GetId();
}