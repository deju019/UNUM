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

        if (!IsBcryptHash(storedValue))
        {
            return false;
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
