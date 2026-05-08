namespace SleepStopper.Services;

public interface IPresenceKeeper : IDisposable
{
    bool IsSupported { get; }
    bool IsScreenLocked();
    void Nudge();
}
