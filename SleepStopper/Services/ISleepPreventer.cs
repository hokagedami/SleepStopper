namespace SleepStopper.Services;

public interface ISleepPreventer : IDisposable
{
    bool IsActive { get; }
    void Enable();
    void Disable();
}
