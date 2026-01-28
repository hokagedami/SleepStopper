# SleepStopper

A cross-platform application that prevents your computer from going to sleep.

## Features

- Toggle sleep prevention with a single button click
- System tray integration with context menu
- Activity log display
- Cross-platform support: Windows, Linux, and macOS

## Requirements

- .NET 8.0 Runtime (or use self-contained builds)

## Installation

### Pre-built Binaries

Download the latest release for your platform from the [Releases](../../releases) page.

### Build from Source

```bash
# Clone the repository
git clone https://github.com/yourusername/SleepStopper.git
cd SleepStopper

# Build
dotnet build

# Run
dotnet run --project SleepStopper
```

### Publish Self-Contained

```bash
# Windows
dotnet publish SleepStopper/SleepStopper.csproj -c Release -r win-x64 --self-contained true

# Linux
dotnet publish SleepStopper/SleepStopper.csproj -c Release -r linux-x64 --self-contained true

# macOS
dotnet publish SleepStopper/SleepStopper.csproj -c Release -r osx-x64 --self-contained true
```

## Usage

1. Launch the application
2. Click **ACTIVATE** to prevent sleep
3. Click **DEACTIVATE** to allow sleep again
4. Close the window to minimize to system tray
5. Right-click the tray icon for options:
   - Activate/Deactivate sleep prevention
   - Show the main window
   - Exit the application

## How It Works

### Windows
Uses the Windows API `SetThreadExecutionState` to inform the system that the application is in use and prevent sleep.

### Linux
Spawns a `systemd-inhibit` process that blocks idle and sleep states while the application is active.

### macOS
Spawns a `caffeinate` process that prevents the display from sleeping while the application is active.

## License

This project is open source.
