using MySql.Data.MySqlClient;
using System.Configuration;

namespace UNUM.Infrastructure;

public static class DbConnectionFactory
{
    public static MySqlConnection CreateOpenConnection()
    {
        var envConnectionString = Environment.GetEnvironmentVariable("UNUM_DB_CONNECTION");
        var configConnectionString = ConfigurationManager.ConnectionStrings["UNUM_DB"]?.ConnectionString;
        var baseConnectionString = !string.IsNullOrWhiteSpace(envConnectionString)
            ? envConnectionString
            : configConnectionString;

        if (string.IsNullOrWhiteSpace(baseConnectionString))
        {
            throw new InvalidOperationException(
                "No se encontro configuracion de conexion. Define UNUM_DB_CONNECTION o UNUM_DB en App.config.");
        }

        var builder = new MySqlConnectionStringBuilder(baseConnectionString);
        OverrideFromEnvironment(builder);

        var connectionString = builder.ConnectionString;

        var connection = new MySqlConnection(connectionString);
        connection.Open();
        return connection;
    }

    private static void OverrideFromEnvironment(MySqlConnectionStringBuilder builder)
    {
        var host = Environment.GetEnvironmentVariable("UNUM_DB_HOST");
        var port = Environment.GetEnvironmentVariable("UNUM_DB_PORT");
        var database = Environment.GetEnvironmentVariable("UNUM_DB_NAME");
        var user = Environment.GetEnvironmentVariable("UNUM_DB_USER");
        var password = Environment.GetEnvironmentVariable("UNUM_DB_PASSWORD");

        if (!string.IsNullOrWhiteSpace(host))
        {
            builder.Server = host;
        }

        if (uint.TryParse(port, out var parsedPort))
        {
            builder.Port = parsedPort;
        }

        if (!string.IsNullOrWhiteSpace(database))
        {
            builder.Database = database;
        }

        if (!string.IsNullOrWhiteSpace(user))
        {
            builder.UserID = user;
        }

        if (!string.IsNullOrWhiteSpace(password))
        {
            builder.Password = password;
        }
    }
}
