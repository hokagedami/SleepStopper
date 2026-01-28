using System.Diagnostics;

namespace SleepStopper.Services;

public class MacOSSleepPreventer : ISleepPreventer
{
    private Process? _caffeinateProcess;

    public bool IsActive { get; private set; }

    public void Enable()
    {
        if (IsActive) return;

        try
        {
            _caffeinateProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "caffeinate",
                    Arguments = "-d",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };
            _caffeinateProcess.Start();
            IsActive = true;
        }
        catch
        {
            _caffeinateProcess?.Dispose();
            _caffeinateProcess = null;
            IsActive = false;
        }
    }

    public void Disable()
    {
        if (!IsActive || _caffeinateProcess == null) return;

        try
        {
            if (!_caffeinateProcess.HasExited)
            {
                _caffeinateProcess.Kill();
                _caffeinateProcess.WaitForExit(1000);
            }
        }
        catch
        {
            // Process may have already exited
        }
        finally
        {
            _caffeinateProcess?.Dispose();
            _caffeinateProcess = null;
            IsActive = false;
        }
    }

    public void Dispose()
    {
        Disable();
    }
}
