using MySql.Data.MySqlClient;
using System.Data;
using UNUM.Infrastructure;

namespace UNUM.Services;

public class ObjectiveService
{
    public void AddObjective(int userId, string nombre, decimal costeTotal, int prioridad, decimal ahorroInicial = 0m)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ArgumentException("El nombre del objetivo no puede estar vacío.", nameof(nombre));
        }

        if (costeTotal <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(costeTotal), "El coste total debe ser mayor que cero.");
        }

        if (prioridad is < 1 or > 3)
        {
            throw new ArgumentOutOfRangeException(nameof(prioridad), "La prioridad debe estar entre 1 y 3.");
        }

        if (ahorroInicial < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(ahorroInicial), "El ahorro inicial no puede ser negativo.");
        }

        using var connection = DbConnectionFactory.CreateOpenConnection();

        const string query = "INSERT INTO Objetivos (UsuarioId, Nombre, CosteTotal, AhorroActual, Prioridad) " +
                             "VALUES (@usuarioId, @nombre, @costeTotal, @ahorroActual, @prioridad)";

        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@usuarioId", userId);
        command.Parameters.AddWithValue("@nombre", nombre);
        command.Parameters.AddWithValue("@costeTotal", costeTotal);
        command.Parameters.AddWithValue("@ahorroActual", ahorroInicial);
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
        if (ahorroActual < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(ahorroActual), "El ahorro actual no puede ser negativo.");
        }

        using var connection = DbConnectionFactory.CreateOpenConnection();

        const string query = "UPDATE Objetivos SET AhorroActual = @ahorroActual " +
                             "WHERE Id = @idObjetivo AND UsuarioId = @usuarioId";

        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@ahorroActual", ahorroActual);
        command.Parameters.AddWithValue("@idObjetivo", objectiveId);
        command.Parameters.AddWithValue("@usuarioId", userId);

        return command.ExecuteNonQuery() > 0;
    }

    public bool UpdateObjective(int objectiveId, int userId, string nombre, decimal costeTotal, decimal ahorroActual, int prioridad)
    {
        if (objectiveId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(objectiveId), "El identificador del objetivo no es válido.");
        }

        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ArgumentException("El nombre del objetivo no puede estar vacío.", nameof(nombre));
        }

        if (costeTotal <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(costeTotal), "El coste total debe ser mayor que cero.");
        }

        if (ahorroActual < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(ahorroActual), "El ahorro actual no puede ser negativo.");
        }

        if (prioridad is < 1 or > 3)
        {
            throw new ArgumentOutOfRangeException(nameof(prioridad), "La prioridad debe estar entre 1 y 3.");
        }

        using var connection = DbConnectionFactory.CreateOpenConnection();

        const string query = "UPDATE Objetivos " +
                             "SET Nombre = @nombre, CosteTotal = @costeTotal, AhorroActual = @ahorroActual, Prioridad = @prioridad " +
                             "WHERE Id = @idObjetivo AND UsuarioId = @usuarioId";

        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@nombre", nombre);
        command.Parameters.AddWithValue("@costeTotal", costeTotal);
        command.Parameters.AddWithValue("@ahorroActual", ahorroActual);
        command.Parameters.AddWithValue("@prioridad", prioridad);
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
