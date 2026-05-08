using SleepStopper.Services;

namespace SleepStopper.Tests.Fakes;

public sealed class TempSettingsService : IDisposable
{
    public string Path { get; }
    public SettingsService Service { get; }

    public TempSettingsService()
    {
        var dir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "SleepStopperTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        Path = System.IO.Path.Combine(dir, "settings.json");
        Service = new SettingsService(Path);
    }

    public void Dispose()
    {
        var dir = System.IO.Path.GetDirectoryName(Path);
        if (dir != null && Directory.Exists(dir))
        {
            try { Directory.Delete(dir, true); } catch { }
        }
    }
}
