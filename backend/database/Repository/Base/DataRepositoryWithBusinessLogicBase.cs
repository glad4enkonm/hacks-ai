using System.Transactions;

namespace database.Repository.Base;

public class DataRepositoryWithBusinessLogicBase<T>: DataRepositoryBase<T> where T : class
{

    #region Insert
    protected virtual void InsertBusinessLogic(T insertedEntity)
    {
        
    }

    public override T Insert(T entity)
    {
        using var transactionScope = new TransactionScope();
        var insertedEntity = base.Insert(entity);
        InsertBusinessLogic(entity);
        
        transactionScope.Complete();
        return insertedEntity;
    }
    
    protected T InsertWithoutBusinessLogic(T entity)
    {
        var insertedEntity = base.Insert(entity);
        return insertedEntity;
    }

    public override ulong Insert(IEnumerable<T> list)
    {
        var enumerable = list as T[] ?? list.ToArray();
        foreach (var entity in enumerable) 
            Insert(entity);
        return (ulong)enumerable.Length;
    }
    # endregion
    
    #region Update
    protected virtual void UpdateBusinessLogic(T updatedEntity)
    {
        
    }

    public override bool Update(T entity)
    {
        using var transactionScope = new TransactionScope();
        var isUpdated = base.Update(entity);
        UpdateBusinessLogic(entity);
        
        transactionScope.Complete();
        return isUpdated;
    }
    
    protected bool UpdateWithoutBusinessLogic(T entity)
    {
        var isUpdated = base.Update(entity);
        return isUpdated;
    }
    
    public override bool Update(IEnumerable<T> list)
    {
        var result = list.Select(Update).All(el => el);
        return result;
    }
    #endregion
    
    #region Delete
    protected virtual void DeleteBusinessLogic(T deletedEntity)
    {
        
    }

    public override bool Delete(T entity)
    {
        using var transactionScope = new TransactionScope();
        var isDeleted = base.Delete(entity);
        DeleteBusinessLogic(entity);
        
        transactionScope.Complete();
        return isDeleted;
    }
    
    protected bool DeleteWithoutBusinessLogic(T entity)
    {
        var isDeleted = base.Delete(entity);
        return isDeleted;
    }

    public override bool Delete(IEnumerable<T> list)
    {
        var result = list.Select(Delete).All(el => el);
        return result;
    }
    
    public override bool DeleteAll()
    {
        var all = GetAll();
        var result = all.Select(Delete).All(el => el);
        return result;
    }
    #endregion
}