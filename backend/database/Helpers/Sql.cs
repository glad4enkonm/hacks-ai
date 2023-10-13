using MySql.Data.MySqlClient;

namespace database.Helpers;
using System;

public static class Sql
{
    public static T QueryUsingConnection<T>(this string connectionString, Func<MySqlConnection,T> query)
    {
        using var connection = new MySqlConnection(connectionString); 
        return query(connection);
    }
}