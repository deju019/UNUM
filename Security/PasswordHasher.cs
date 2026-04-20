namespace UNUM.Security;

public static class PasswordHasher
{
    public static string Hash(string plainPassword)
    {
        return BCrypt.Net.BCrypt.HashPassword(plainPassword);
    }

    public static bool Verify(string plainPassword, string storedValue)
    {
        if (string.IsNullOrWhiteSpace(storedValue))
        {
            return false;
        }

        // Compatibilidad temporal: permite login de usuarios legacy con password en texto plano.
        if (!IsBcryptHash(storedValue))
        {
            return string.Equals(plainPassword, storedValue, StringComparison.Ordinal);
        }

        return BCrypt.Net.BCrypt.Verify(plainPassword, storedValue);
    }

    public static bool IsBcryptHash(string value)
    {
        return value.StartsWith("$2a$", StringComparison.Ordinal)
            || value.StartsWith("$2b$", StringComparison.Ordinal)
            || value.StartsWith("$2y$", StringComparison.Ordinal);
    }
}
