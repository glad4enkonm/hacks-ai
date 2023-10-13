using System.Text.Json;
using System.Transactions;
using database.Helpers;
using database.Models.History.Base;

namespace database.Repository.Base;

public class DataRepositoryWithHistoryAndBusinessLogicBase<T, THistoryModel>:DataRepositoryWithBusinessLogicBase<T>, IDataRepositoryWithHistory<T> 
    where T : EntityWithHistoryBase
    where THistoryModel : HistoryBase, new()
{
    private const string CreatedText = "Created";
    private const string DeletedText = "Deleted";

    private readonly IDataRepository<THistoryModel> _historyDataRepository;
    
    public DataRepositoryWithHistoryAndBusinessLogicBase()
    {
        _historyDataRepository = new DataRepositoryBase<THistoryModel>();
    }

    #region Get

    public override T Get(ulong id)
    {
        var possiblyDeletedEntity = base.Get(id);
        if (possiblyDeletedEntity.IsDeleted == false)
            return possiblyDeletedEntity;
        throw new InvalidOperationException("No element retrieved during Get from DB");
    }
    
    public T? GetOrDefault(ulong id)
    {
        var possiblyDeletedEntity = base.Get(id);
        return possiblyDeletedEntity.IsDeleted == false ? possiblyDeletedEntity : null;
    }

    public override IEnumerable<T> GetAll()
    {
        IEnumerable<T> possiblyDeletedEntities =  base.GetAll();
        return possiblyDeletedEntities.Where(e => e.IsDeleted == false);
    }

    #endregion
    
    #region Insert
    public virtual T Insert(T entity, ulong changedBy)
    {
        return Insert(base.Insert, entity, changedBy);
    }
    
    protected T InsertWithoutBusinessLogic(T entity, ulong changedBy)
    {
        return Insert(base.InsertWithoutBusinessLogic, entity, changedBy);
    }
    
    private T Insert(Func<T, T> insertFunc, T entity, ulong changedBy)
    {
        using var transactionScope = new TransactionScope();
        
        var insertedEntity = insertFunc(entity);
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
    #endregion
    
    #region Update
    public virtual bool Update(T entity, ulong changedBy, IEnumerable<KeyValuePair<string, string>> patch)
    {
        return Update(base.Update, entity, changedBy, patch);
    }
    
    protected bool UpdateWithoutBusinessLogic(T entity, ulong changedBy, 
        IEnumerable<KeyValuePair<string, string>> patch)
    {
        return Update(base.UpdateWithoutBusinessLogic, entity, changedBy, patch);
    }
    
    private bool Update(Func<T, bool> updateFunc, T entity, 
        ulong changedBy, IEnumerable<KeyValuePair<string, string>> patch)
    {
        using var transactionScope = new TransactionScope();

        bool isUpdated = updateFunc(entity);
        if (!isUpdated)
            return isUpdated;

        var patchString = Diff.PatchString(patch);
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
    #endregion

    #region Delete

    public override bool Delete(T entity)
    {
        using var transactionScope = new TransactionScope();
        
        bool markedAsDeleted = MarkAsDeleted(entity);
        if (!markedAsDeleted) 
            return false;
        
        base.DeleteBusinessLogic(entity);
        transactionScope.Complete();
        return true;
    }

    private new bool DeleteWithoutBusinessLogic(T entity)
    {
        return MarkAsDeleted(entity);
    }

    private bool MarkAsDeleted(T entity)
    {
        entity.IsDeleted = true;
        return base.Update(entity);
    }


    public virtual bool Delete(T entity, ulong changedBy)
    {
        return Delete(Delete, entity, changedBy);
    }
    
    protected bool DeleteWithoutBusinessLogic(T entity, ulong changedBy)
    {
        return Delete(DeleteWithoutBusinessLogic, entity, changedBy);
    }
    
    private bool Delete(Func<T, bool> deleteFunc,T entity, ulong changedBy)
    {
        using var transactionScope = new TransactionScope();
        bool isDeleted = deleteFunc(entity);
        if (!isDeleted)
            return isDeleted;
        var historyModel = new THistoryModel()
        {
            Changed = DateTime.Now, ChangedBy = changedBy, Difference = DeletedText, EntityId = entity.GetId()
        };
        _historyDataRepository.Insert(historyModel);
        
        transactionScope.Complete();
        return isDeleted;
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
    #endregion
}