using System.Diagnostics;

namespace SleepStopper.Services;

public class LinuxSleepPreventer : ISleepPreventer
{
    private Process? _inhibitProcess;

    public bool IsActive { get; private set; }

    public void Enable()
    {
        if (IsActive) return;

        try
        {
            _inhibitProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "systemd-inhibit",
                    Arguments = "--what=idle:sleep --who=SleepStopper --why=\"Preventing system sleep\" --mode=block sleep infinity",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };
            _inhibitProcess.Start();
            IsActive = true;
        }
        catch
        {
            _inhibitProcess?.Dispose();
            _inhibitProcess = null;
            IsActive = false;
        }
    }

    public void Disable()
    {
        if (!IsActive || _inhibitProcess == null) return;

        try
        {
            if (!_inhibitProcess.HasExited)
            {
                _inhibitProcess.Kill();
                _inhibitProcess.WaitForExit(1000);
            }
        }
        catch
        {
            // Process may have already exited
        }
        finally
        {
            _inhibitProcess?.Dispose();
            _inhibitProcess = null;
            IsActive = false;
        }
    }

    public void Dispose()
    {
        Disable();
    }
}
