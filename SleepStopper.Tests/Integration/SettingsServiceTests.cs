using SleepStopper.Services;
using SleepStopper.Tests.Fakes;
using Xunit;

namespace SleepStopper.Tests.Integration;

public class SettingsServiceTests
{
    [Fact]
    public void Load_ReturnsDefaultsWhenFileMissing()
    {
        using var temp = new TempSettingsService();
        var settings = temp.Service.Load();

        Assert.True(settings.Presence.WeekdaysOnly);
        Assert.Equal(55, settings.Presence.IntervalSeconds);
        Assert.True(settings.Presence.Window1.Enabled);
        Assert.Equal("09:30", settings.Presence.Window1.Start);
        Assert.Equal("18:00", settings.Presence.Window2.End);
        Assert.False(settings.Startup.AutoStartPresence);
    }

    [Fact]
    public void Load_ReturnsDefaultsWhenFileCorrupt()
    {
        using var temp = new TempSettingsService();
        File.WriteAllText(temp.Path, "{ this is not valid json");

        var settings = temp.Service.Load();

        Assert.NotNull(settings);
        Assert.True(settings.Presence.WeekdaysOnly);
    }

    [Fact]
    public void SaveAndLoad_RoundTripPreservesValues()
    {
        using var temp = new TempSettingsService();
        var original = new AppSettings
        {
            Presence = new PresenceSettings
            {
                WeekdaysOnly = false,
                IntervalSeconds = 120,
                PauseWhenLocked = false,
                Window1 = new PresenceWindowSettings { Enabled = false, Start = "07:00", End = "09:30" },
                Window2 = new PresenceWindowSettings { Enabled = true, Start = "13:30", End = "17:30" }
            },
            Startup = new StartupSettings
            {
                AutoStartSleepPrevention = true,
                AutoStartPresence = true
            }
        };

        temp.Service.Save(original);
        var loaded = temp.Service.Load();

        Assert.False(loaded.Presence.WeekdaysOnly);
        Assert.Equal(120, loaded.Presence.IntervalSeconds);
        Assert.False(loaded.Presence.PauseWhenLocked);
        Assert.False(loaded.Presence.Window1.Enabled);
        Assert.Equal("07:00", loaded.Presence.Window1.Start);
        Assert.Equal("17:30", loaded.Presence.Window2.End);
        Assert.True(loaded.Startup.AutoStartSleepPrevention);
        Assert.True(loaded.Startup.AutoStartPresence);
    }

    [Fact]
    public void Save_CreatesParentDirectoryIfMissing()
    {
        var dir = Path.Combine(Path.GetTempPath(), "SleepStopperTests", Guid.NewGuid().ToString("N"), "nested");
        var path = Path.Combine(dir, "settings.json");
        try
        {
            var service = new SettingsService(path);
            service.Save(new AppSettings());
            Assert.True(File.Exists(path));
        }
        finally
        {
            if (Directory.Exists(dir)) Directory.Delete(dir, true);
        }
    }
}
