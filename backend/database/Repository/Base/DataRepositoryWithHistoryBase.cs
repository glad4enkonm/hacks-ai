using System.Text.Json;
using System.Transactions;
using database.Models.History.Base;

namespace database.Repository.Base;

public interface IDataRepositoryWithHistory<T>: IDataRepositoryReadOnly<T> where T : class
{
    T Insert(T obj, ulong changedBy);
    ulong Insert(IEnumerable<T> list, ulong changedBy);
    bool Update(T obj, ulong changedBy, IEnumerable<KeyValuePair<string, string>> patch);
    bool Update(IEnumerable<T> list, ulong changedBy, IEnumerable<KeyValuePair<string, string>[]> patchList);
    bool Delete(T obj, ulong changedBy);
    bool Delete(IEnumerable<T> list, ulong changedBy);
    bool DeleteAll(ulong changedBy);

}

public class DataRepositoryWithHistoryBase<T, THistoryModel>:DataRepositoryBase<T>, IDataRepositoryWithHistory<T> 
    where T : EntityWithHistoryBase
    where THistoryModel : HistoryBase, new()
{
    private const string CreatedText = "Created";
    private const string DeletedText = "Deleted";

    private readonly DataRepositoryBase<THistoryModel> _historyDataRepository;

    protected DataRepositoryWithHistoryBase()
    {
        _historyDataRepository = new DataRepositoryBase<THistoryModel>();
    }

    public override T Get(ulong id)
    {
        var possiblyDeletedEntity = base.Get(id);
        if (possiblyDeletedEntity.IsDeleted == false)
            return possiblyDeletedEntity;
        throw new InvalidOperationException("No element retrieved during Get from DB");
    }

    public override IEnumerable<T> GetAll()
    {
        IEnumerable<T> possiblyDeletedEntities =  base.GetAll();
        return possiblyDeletedEntities.Where(e => e.IsDeleted == false);
    }

    public virtual T Insert(T entity, ulong changedBy)
    {
        using var transactionScope = new TransactionScope();
        
        var insertedEntity = base.Insert(entity);
        var differenceString = CreatedText + " " + JsonSerializer.Serialize(entity);
        var historyModel = new THistoryModel()
        {
            Changed = DateTime.Now, ChangedBy = changedBy, Difference = differenceString, EntityId = insertedEntity.GetId()
        };
        _historyDataRepository.Insert(historyModel);
        
        transactionScope.Complete();
        
        return insertedEntity;
    }

    public virtual ulong Insert(IEnumerable<T> list, ulong changedBy)
    {
        var enumerable = list as T[] ?? list.ToArray();
        foreach (var entity in enumerable) 
            Insert(entity, changedBy);
        return (ulong)enumerable.Length;
    }

    public virtual bool Update(T entity, ulong changedBy, IEnumerable<KeyValuePair<string, string>> patch)
    {
        using var transactionScope = new TransactionScope();
        
        bool isUpdated = base.Update(entity);
        if (!isUpdated)
            return isUpdated;
        var patchString = string.Join(", ", patch.Select(pair => $"{pair.Key}={pair.Value}"));
        var historyModel = new THistoryModel()
        {
            Changed = DateTime.Now, ChangedBy = changedBy, Difference = patchString, EntityId = entity.GetId()
        };
        _historyDataRepository.Insert(historyModel);
        transactionScope.Complete();
        
        return isUpdated;
    }

    public virtual bool Update(IEnumerable<T> list, ulong changedBy, IEnumerable<KeyValuePair<string, string>[]> patchList)
    {
        var listAsArray = list as T[] ?? list.ToArray();
        var patchListAsArray = patchList as KeyValuePair<string, string>[][] ?? patchList.ToArray();
        if (listAsArray.Length != patchListAsArray.Length) throw new InvalidOperationException(); // должны быть одинаковы
        var result = listAsArray.Zip(patchListAsArray, (entity, patch) => 
            Update(entity, changedBy, patch)).All(el => el);
        return result;
    }

    public virtual bool Delete(T entity, ulong changedBy)
    {
        using var transactionScope = new TransactionScope();
        entity.IsDeleted = true;
        bool markedAsDeleted = base.Update(entity);
        if (!markedAsDeleted)
            return markedAsDeleted;
        var historyModel = new THistoryModel()
        {
            Changed = DateTime.Now, ChangedBy = changedBy, Difference = DeletedText, EntityId = entity.GetId()
        };
        _historyDataRepository.Insert(historyModel);
        
        transactionScope.Complete();
        return markedAsDeleted;
    }

    public virtual bool Delete(IEnumerable<T> list, ulong changedBy)
    {
        var result = list.Select(entity => Delete(entity, changedBy)).All(el => el);
        return result;
    }

    public virtual bool DeleteAll(ulong changedBy)
    {
        var all = GetAll();
        var result = all.Select(entity => Delete(entity, changedBy)).All(el => el);
        return result;
    }
}