using Dapper.Contrib.Extensions;
using database.Helpers;

namespace database.Repository.Base;

public interface IDataRepository<T>: IDataRepositoryReadOnly<T> where T : class
{
    T Insert(T obj);
    ulong Insert(IEnumerable<T> list);
    bool Update(T obj);
    bool Update(IEnumerable<T> list);
    bool Delete(T obj);
    bool Delete(IEnumerable<T> list);
    bool DeleteAll();

}

public interface IDataRepositoryReadOnly<T> where T : class
{
    T Get(ulong id);
    IEnumerable<T> GetAll();
}

public static class DataRepositoryEnvironment
{
    public static string VariablePrefix = "";    
}


public class DataRepositoryBase<T>: IDataRepository<T> where T : class
{
    protected readonly string ConnectionString;

    protected internal DataRepositoryBase()
    {
        var server = Environment.GetEnvironmentVariable(DataRepositoryEnvironment.VariablePrefix + "_DATABASE_SERVER");
        var user = Environment.GetEnvironmentVariable(DataRepositoryEnvironment.VariablePrefix + "_DATABASE_USER");
        var password = Environment.GetEnvironmentVariable(DataRepositoryEnvironment.VariablePrefix + "_DATABASE_PASSWORD");
        var database = Environment.GetEnvironmentVariable("MYSQL_DATABASE");
                
        ConnectionString = $"Server='{server}';Database='{database}';Uid='{user}';Pwd='{password}';";
    }

    public virtual T Get(ulong id) => ConnectionString.QueryUsingConnection(con => con.Get<T>(id));

    public virtual IEnumerable<T> GetAll() =>
        ConnectionString.QueryUsingConnection(con => con.GetAll<T>());

    public virtual T Insert(T obj)
    {
        var insertedId = (ulong)ConnectionString.QueryUsingConnection(con => con.Insert(obj));
        return Get(insertedId);
    }

    public virtual ulong Insert(IEnumerable<T> list) => (ulong)ConnectionString.QueryUsingConnection(con => con.Insert(list));

    public virtual bool Update(T obj) => ConnectionString.QueryUsingConnection(con => con.Update(obj));

    public virtual bool Update(IEnumerable<T> list) => ConnectionString.QueryUsingConnection(con => con.Update(list));

    public virtual bool Delete(T obj) => ConnectionString.QueryUsingConnection(con => con.Delete(obj));

    public virtual bool Delete(IEnumerable<T> list) => ConnectionString.QueryUsingConnection(con => con.Delete(list));

    public virtual bool DeleteAll() => ConnectionString.QueryUsingConnection(con => con.DeleteAll<T>());
}