namespace SleepStopper.Services;

public class NoOpPresenceKeeper : IPresenceKeeper
{
    public bool IsSupported => false;
    public bool IsScreenLocked() => false;
    public void Nudge() { }
    public void Dispose() { }
}
