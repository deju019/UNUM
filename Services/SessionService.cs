using System;
using System.IO;
using System.Text.Json;

namespace UNUM.Services;

public static class SessionService
{
    private static readonly string AppFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "UNUM");
    private static readonly string SessionFile = Path.Combine(AppFolder, "session.json");

    public static void SaveSession(int userId)
    {
        try
        {
            Directory.CreateDirectory(AppFolder);
            var doc = new { UserId = userId, SavedAt = DateTime.UtcNow };
            File.WriteAllText(SessionFile, JsonSerializer.Serialize(doc));
        }
        catch
        {
            // No bloquear la app si el disco no permite guardar sesión
        }
    }

    public static int? LoadSession()
    {
        try
        {
            if (!File.Exists(SessionFile)) return null;
            var json = File.ReadAllText(SessionFile);
            using var d = JsonDocument.Parse(json);
            if (d.RootElement.TryGetProperty("UserId", out var u))
            {
                return u.GetInt32();
            }
        }
        catch
        {
            // Ignorar errores de lectura
        }

        return null;
    }

    public static void ClearSession()
    {
        try
        {
            if (File.Exists(SessionFile)) File.Delete(SessionFile);
        }
        catch { }
    }
}
