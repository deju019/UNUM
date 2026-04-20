using MySql.Data.MySqlClient;
using System.Data;
using UNUM.Infrastructure;

namespace UNUM.Services;

public class ObjectiveService
{
    public void AddObjective(int userId, string nombre, decimal costeTotal, int prioridad)
    {
        using var connection = DbConnectionFactory.CreateOpenConnection();

        const string query = "INSERT INTO Objetivos (UsuarioId, Nombre, CosteTotal, AhorroActual, Prioridad) " +
                             "VALUES (@usuarioId, @nombre, @costeTotal, @ahorroActual, @prioridad)";

        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@usuarioId", userId);
        command.Parameters.AddWithValue("@nombre", nombre);
        command.Parameters.AddWithValue("@costeTotal", costeTotal);
        command.Parameters.AddWithValue("@ahorroActual", 0m);
        command.Parameters.AddWithValue("@prioridad", prioridad);
        command.ExecuteNonQuery();
    }

    public DataTable GetObjectives(int userId)
    {
        using var connection = DbConnectionFactory.CreateOpenConnection();

        const string query = "SELECT Id, Nombre, CosteTotal, AhorroActual, Prioridad, " +
                             "ROUND((AhorroActual / NULLIF(CosteTotal, 0)) * 100, 2) AS ProgresoPorcentaje " +
                             "FROM Objetivos WHERE UsuarioId = @usuarioId ORDER BY Prioridad ASC, Id DESC";

        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@usuarioId", userId);

        using var adapter = new MySqlDataAdapter(command);
        var table = new DataTable();
        adapter.Fill(table);
        return table;
    }

    public bool UpdateCurrentSavings(int objectiveId, int userId, decimal ahorroActual)
    {
        using var connection = DbConnectionFactory.CreateOpenConnection();

        const string query = "UPDATE Objetivos SET AhorroActual = @ahorroActual " +
                             "WHERE Id = @idObjetivo AND UsuarioId = @usuarioId";

        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@ahorroActual", ahorroActual);
        command.Parameters.AddWithValue("@idObjetivo", objectiveId);
        command.Parameters.AddWithValue("@usuarioId", userId);

        return command.ExecuteNonQuery() > 0;
    }

    public bool DeleteObjective(int objectiveId, int userId)
    {
        using var connection = DbConnectionFactory.CreateOpenConnection();

        const string query = "DELETE FROM Objetivos WHERE Id = @idObjetivo AND UsuarioId = @usuarioId";

        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@idObjetivo", objectiveId);
        command.Parameters.AddWithValue("@usuarioId", userId);

        return command.ExecuteNonQuery() > 0;
    }
}
