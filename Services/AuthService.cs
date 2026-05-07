using MySql.Data.MySqlClient;
using UNUM.Infrastructure;
using UNUM.Security;

namespace UNUM.Services;

public class AuthService
{
    public int? Authenticate(string username, string plainPassword)
    {
        using var connection = DbConnectionFactory.CreateOpenConnection();
        var passwordColumn = ResolvePasswordColumnName(connection);

        var query = $"SELECT Id, `{passwordColumn}` AS PasswordValue FROM Usuarios WHERE NombreUsuario = @user LIMIT 1";
        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@user", username);

        int userId;
        string storedPassword;

        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            return null;
        }

        userId = reader.GetInt32("Id");
        storedPassword = reader.GetString("PasswordValue");

        if (PasswordHasher.IsBcryptHash(storedPassword))
        {
            return PasswordHasher.Verify(plainPassword, storedPassword) ? userId : null;
        }

        // Compatibilidad controlada: valida una sola vez credenciales legacy y migra a hash.
        if (!string.Equals(plainPassword, storedPassword, StringComparison.Ordinal))
        {
            return null;
        }

        reader.Close();

        var upgradedHash = PasswordHasher.Hash(plainPassword);
        var updateQuery = $"UPDATE Usuarios SET `{passwordColumn}` = @pass WHERE Id = @id";
        using var updateCommand = new MySqlCommand(updateQuery, connection);
        updateCommand.Parameters.AddWithValue("@pass", upgradedHash);
        updateCommand.Parameters.AddWithValue("@id", userId);
        updateCommand.ExecuteNonQuery();

        return userId;
    }

    public int RegisterUser(string username, string plainPassword)
    {
        var hashedPassword = PasswordHasher.Hash(plainPassword);

        using var connection = DbConnectionFactory.CreateOpenConnection();
        var passwordColumn = ResolvePasswordColumnName(connection);
        var query = $"INSERT INTO Usuarios (NombreUsuario, `{passwordColumn}`) VALUES (@user, @pass)";

        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@user", username);
        command.Parameters.AddWithValue("@pass", hashedPassword);
        command.ExecuteNonQuery();

        return Convert.ToInt32(command.LastInsertedId);
    }

    private static string ResolvePasswordColumnName(MySqlConnection connection)
    {
        const string query = @"
            SELECT COLUMN_NAME
            FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_SCHEMA = DATABASE()
              AND TABLE_NAME = 'Usuarios'
              AND COLUMN_NAME IN ('Contrasena', 'Contraseña')
            ORDER BY FIELD(COLUMN_NAME, 'Contrasena', 'Contraseña')
            LIMIT 1;";

        using var command = new MySqlCommand(query, connection);
        var result = command.ExecuteScalar() as string;

        if (string.IsNullOrWhiteSpace(result))
        {
            throw new InvalidOperationException("No se encontro columna de contrasena en Usuarios (Contrasena/Contraseña).");
        }

        return result;
    }
}
