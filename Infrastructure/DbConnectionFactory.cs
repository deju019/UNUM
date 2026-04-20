using MySql.Data.MySqlClient;
using System.Configuration;

namespace UNUM.Infrastructure;

public static class DbConnectionFactory
{
    public static MySqlConnection CreateOpenConnection()
    {
        var connectionString = ConfigurationManager.ConnectionStrings["UNUM_DB"]?.ConnectionString;

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("No se encontro la cadena de conexion UNUM_DB en App.config.");
        }

        var connection = new MySqlConnection(connectionString);
        connection.Open();
        return connection;
    }
}
