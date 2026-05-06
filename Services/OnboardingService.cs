using System.IO;
using System.Text.Json;

namespace UNUM.Services;

public class OnboardingService
{
    public bool ShouldShowOnboarding(int userId)
    {
        var state = ReadState();
        return !state.TryGetValue(userId, out var dismissed) || !dismissed;
    }

    public void MarkDismissed(int userId, bool doNotShowAgain)
    {
        var state = ReadState();
        state[userId] = doNotShowAgain;
        WriteState(state);
    }

    private static Dictionary<int, bool> ReadState()
    {
        var path = GetStatePath();
        if (!File.Exists(path))
        {
            return new Dictionary<int, bool>();
        }

        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<Dictionary<int, bool>>(json) ?? new Dictionary<int, bool>();
    }

    private static void WriteState(Dictionary<int, bool> state)
    {
        var path = GetStatePath();
        var directory = Path.GetDirectoryName(path);

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(state, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(path, json);
    }

    private static string GetStatePath()
    {
        var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "UNUM");
        return Path.Combine(folder, "onboarding-mainwindow.json");
    }
}