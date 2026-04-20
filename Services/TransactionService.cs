using MySql.Data.MySqlClient;
using System.Data;
using UNUM.Infrastructure;

namespace UNUM.Services;

public class TransactionService
{
    public void AddTransaction(int userId, string tipo, string categoria, decimal importe, DateTime fecha, string descripcion)
    {
        using var connection = DbConnectionFactory.CreateOpenConnection();

        const string query = "INSERT INTO Transacciones (UsuarioId, Tipo, Categoria, Importe, FechaTransaccion, Descripcion) " +
                             "VALUES (@usuarioId, @tipo, @categoria, @importe, @fecha, @descripcion)";

        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@usuarioId", userId);
        command.Parameters.AddWithValue("@tipo", tipo);
        command.Parameters.AddWithValue("@categoria", categoria);
        command.Parameters.AddWithValue("@importe", importe);
        command.Parameters.AddWithValue("@fecha", fecha.Date);
        command.Parameters.AddWithValue("@descripcion", descripcion);
        command.ExecuteNonQuery();
    }

    public DataTable GetUserTransactions(int userId)
    {
        using var connection = DbConnectionFactory.CreateOpenConnection();

        const string query = "SELECT Id, Tipo, Categoria, Importe, FechaTransaccion AS Fecha, Descripcion " +
                             "FROM Transacciones WHERE UsuarioId = @usuarioId ORDER BY FechaTransaccion DESC, Id DESC";

        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@usuarioId", userId);

        using var adapter = new MySqlDataAdapter(command);
        var table = new DataTable();
        adapter.Fill(table);
        return table;
    }

    public bool DeleteTransaction(int transactionId, int userId)
    {
        using var connection = DbConnectionFactory.CreateOpenConnection();

        const string query = "DELETE FROM Transacciones WHERE Id = @idTransaccion AND UsuarioId = @usuarioId";
        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@idTransaccion", transactionId);
        command.Parameters.AddWithValue("@usuarioId", userId);

        var affectedRows = command.ExecuteNonQuery();
        return affectedRows > 0;
    }
}
