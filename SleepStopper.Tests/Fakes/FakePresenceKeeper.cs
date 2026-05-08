using SleepStopper.Services;

namespace SleepStopper.Tests.Fakes;

public class FakePresenceKeeper : IPresenceKeeper
{
    public bool IsSupported { get; set; } = true;
    public bool ScreenLocked { get; set; }
    public int NudgeCount { get; private set; }
    public int DisposeCount { get; private set; }

    public bool IsScreenLocked() => ScreenLocked;

    public void Nudge()
    {
        NudgeCount++;
    }

    public void Dispose()
    {
        DisposeCount++;
    }
}
