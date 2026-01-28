using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SleepStopper.Services;

namespace SleepStopper.ViewModels;

public partial class MainViewModel : ObservableObject, IDisposable
{
    private readonly ISleepPreventer _sleepPreventer;
    private readonly StringBuilder _logBuilder;

    [ObservableProperty]
    private bool _isActive;

    [ObservableProperty]
    private string _buttonText = "ACTIVATE";

    [ObservableProperty]
    private string _logText = string.Empty;

    public MainViewModel()
    {
        _sleepPreventer = SleepPreventerFactory.Create();
        _logBuilder = new StringBuilder();

        AppendLog("Application started successfully....");
        AppendLog("System Auto-Sleep Active.");
    }

    [RelayCommand]
    private void Toggle()
    {
        if (IsActive)
        {
            _sleepPreventer.Disable();
            IsActive = false;
            ButtonText = "ACTIVATE";
            AppendLog("System Auto-Sleep Activated.");
        }
        else
        {
            _sleepPreventer.Enable();
            IsActive = true;
            ButtonText = "DEACTIVATE";
            AppendLog("System Auto-Sleep Deactivated.");
        }
    }

    public void Shutdown()
    {
        if (IsActive)
        {
            _sleepPreventer.Disable();
            AppendLog("Closing application....");
        }
    }

    private void AppendLog(string message)
    {
        if (_logBuilder.Length > 0)
        {
            _logBuilder.AppendLine();
        }
        _logBuilder.Append(message);
        LogText = _logBuilder.ToString();
    }

    public void Dispose()
    {
        _sleepPreventer.Dispose();
    }
}
