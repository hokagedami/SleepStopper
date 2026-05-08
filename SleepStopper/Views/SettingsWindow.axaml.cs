using Avalonia.Controls;
using SleepStopper.Services;
using SleepStopper.ViewModels;

namespace SleepStopper.Views;

public partial class SettingsWindow : Window
{
    public AppSettings? Result { get; private set; }

    public SettingsWindow()
    {
        InitializeComponent();
    }

    public SettingsWindow(SettingsViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.Saved += (_, settings) =>
        {
            Result = settings;
            Close();
        };
        viewModel.Cancelled += (_, _) =>
        {
            Result = null;
            Close();
        };
    }
}
