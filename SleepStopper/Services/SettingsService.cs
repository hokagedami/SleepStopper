using System.Text.Json;

namespace SleepStopper.Services;

public class SettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public string SettingsPath { get; }

    public SettingsService() : this(GetDefaultPath())
    {
    }

    public SettingsService(string settingsPath)
    {
        SettingsPath = settingsPath;
        var dir = Path.GetDirectoryName(settingsPath);
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }
    }

    private static string GetDefaultPath()
    {
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SleepStopper");
        return Path.Combine(dir, "settings.json");
    }

    public AppSettings Load()
    {
        if (!File.Exists(SettingsPath))
        {
            return new AppSettings();
        }

        try
        {
            var json = File.ReadAllText(SettingsPath);
            return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }

    public void Save(AppSettings settings)
    {
        var json = JsonSerializer.Serialize(settings, JsonOptions);
        File.WriteAllText(SettingsPath, json);
    }

    public static PresenceSchedule BuildSchedule(PresenceSettings settings)
    {
        var windows = new List<PresenceWindow>();
        AddWindow(windows, settings.Window1);
        AddWindow(windows, settings.Window2);
        return new PresenceSchedule(windows, settings.WeekdaysOnly);
    }

    private static void AddWindow(List<PresenceWindow> windows, PresenceWindowSettings w)
    {
        if (!w.Enabled) return;
        if (!TimeOnly.TryParse(w.Start, out var start)) return;
        if (!TimeOnly.TryParse(w.End, out var end)) return;
        if (end <= start) return;
        windows.Add(new PresenceWindow(start, end));
    }
}
