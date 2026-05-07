using MySql.Data.MySqlClient;
using UNUM.Infrastructure;
using UNUM.Models;

namespace UNUM.Services;

public class BudgetService
{
    private const int MysqlDuplicateKey = 1062;

    public IReadOnlyList<BudgetRecord> GetBudgetsByUser(int userId)
    {
        using var connection = DbConnectionFactory.CreateOpenConnection();

        const string query = """
            SELECT Id, Categoria, LimiteMensual
            FROM Presupuestos
            WHERE UsuarioId = @usuarioId
            ORDER BY Categoria ASC
            """;

        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@usuarioId", userId);

        var list = new List<BudgetRecord>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new BudgetRecord
            {
                Id = reader.GetInt32("Id"),
                Categoria = reader.GetString("Categoria"),
                LimiteMensual = reader.GetDecimal("LimiteMensual")
            });
        }

        return list;
    }

    public int Insert(int userId, string categoria, decimal limiteMensual)
    {
        if (string.IsNullOrWhiteSpace(categoria))
        {
            throw new ArgumentException("La categoría no puede estar vacía.", nameof(categoria));
        }

        if (limiteMensual <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(limiteMensual), "El límite debe ser mayor que cero.");
        }

        using var connection = DbConnectionFactory.CreateOpenConnection();

        const string query = """
            INSERT INTO Presupuestos (UsuarioId, Categoria, LimiteMensual)
            VALUES (@usuarioId, @categoria, @limiteMensual)
            """;

        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@usuarioId", userId);
        command.Parameters.AddWithValue("@categoria", categoria.Trim());
        command.Parameters.AddWithValue("@limiteMensual", limiteMensual);

        try
        {
            command.ExecuteNonQuery();
        }
        catch (MySqlException ex) when (ex.Number == MysqlDuplicateKey)
        {
            throw new InvalidOperationException("Ya existe un presupuesto para esa categoría.", ex);
        }

        return Convert.ToInt32(command.LastInsertedId);
    }

    public bool Update(int presupuestoId, int userId, string categoria, decimal limiteMensual)
    {
        if (presupuestoId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(presupuestoId), "El identificador del presupuesto no es válido.");
        }

        if (string.IsNullOrWhiteSpace(categoria))
        {
            throw new ArgumentException("La categoría no puede estar vacía.", nameof(categoria));
        }

        if (limiteMensual <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(limiteMensual), "El límite debe ser mayor que cero.");
        }

        using var connection = DbConnectionFactory.CreateOpenConnection();

        const string query = """
            UPDATE Presupuestos
            SET Categoria = @categoria, LimiteMensual = @limiteMensual
            WHERE Id = @id AND UsuarioId = @usuarioId
            """;

        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@categoria", categoria.Trim());
        command.Parameters.AddWithValue("@limiteMensual", limiteMensual);
        command.Parameters.AddWithValue("@id", presupuestoId);
        command.Parameters.AddWithValue("@usuarioId", userId);

        try
        {
            return command.ExecuteNonQuery() > 0;
        }
        catch (MySqlException ex) when (ex.Number == MysqlDuplicateKey)
        {
            throw new InvalidOperationException("Ya existe otro presupuesto con esa categoría.", ex);
        }
    }

    public bool Delete(int presupuestoId, int userId)
    {
        using var connection = DbConnectionFactory.CreateOpenConnection();

        const string query = "DELETE FROM Presupuestos WHERE Id = @id AND UsuarioId = @usuarioId";

        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@id", presupuestoId);
        command.Parameters.AddWithValue("@usuarioId", userId);

        return command.ExecuteNonQuery() > 0;
    }

    public int DeleteAllForUser(int userId)
    {
        using var connection = DbConnectionFactory.CreateOpenConnection();

        const string query = "DELETE FROM Presupuestos WHERE UsuarioId = @usuarioId";

        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@usuarioId", userId);

        return command.ExecuteNonQuery();
    }
}
