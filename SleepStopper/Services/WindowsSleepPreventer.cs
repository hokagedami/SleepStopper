using System.Runtime.InteropServices;

namespace SleepStopper.Services;

public partial class WindowsSleepPreventer : ISleepPreventer
{
    [Flags]
    private enum EXECUTION_STATE : uint
    {
        ES_AWAYMODE_REQUIRED = 0x00000040,
        ES_CONTINUOUS = 0x80000000,
        ES_DISPLAY_REQUIRED = 0x00000002,
        ES_SYSTEM_REQUIRED = 0x00000001
    }

    [LibraryImport("kernel32.dll", SetLastError = true)]
    private static partial uint SetThreadExecutionState(uint esFlags);

    public bool IsActive { get; private set; }

    public void Enable()
    {
        SetThreadExecutionState((uint)(EXECUTION_STATE.ES_CONTINUOUS | EXECUTION_STATE.ES_DISPLAY_REQUIRED));
        IsActive = true;
    }

    public void Disable()
    {
        SetThreadExecutionState((uint)EXECUTION_STATE.ES_CONTINUOUS);
        IsActive = false;
    }

    public void Dispose()
    {
        if (IsActive)
        {
            Disable();
        }
    }
}
