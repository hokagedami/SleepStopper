# SleepStopper

A cross-platform desktop application that prevents your computer from going to sleep. Keep your system awake during long downloads, presentations, or any task that requires uninterrupted operation.

## Features

- One-click toggle to prevent/allow system sleep
- System tray integration for background operation
- Real-time activity log
- Cross-platform: Windows, Linux, and macOS
- Lightweight and resource-efficient

## Installation

### Windows

1. Download `SleepStopper-windows-x64.zip` from the [Releases](../../releases) page
2. Extract the zip file to a folder of your choice (e.g., `C:\Program Files\SleepStopper`)
3. Run `SleepStopper.exe`

**Optional:** Create a shortcut to `SleepStopper.exe` on your Desktop or Start Menu for easy access.

**Run at Startup (Optional):**
1. Press `Win + R`, type `shell:startup`, and press Enter
2. Create a shortcut to `SleepStopper.exe` in this folder

### Linux

#### Debian/Ubuntu (.deb)
```bash
# Download and install
sudo dpkg -i SleepStopper-linux-x64.deb

# Run
sleepstopper
```

#### Fedora/RHEL/CentOS (.rpm)
```bash
# Download and install
sudo rpm -i SleepStopper-linux-x64.rpm

# Run
sleepstopper
```

#### Portable (tar.gz)
```bash
tar -xzf SleepStopper-linux-x64.tar.gz
cd SleepStopper-linux-x64
chmod +x SleepStopper
./SleepStopper
```

**Optional:** Move portable version to a system location:
```bash
sudo mv SleepStopper-linux-x64 /opt/SleepStopper
sudo ln -s /opt/SleepStopper/SleepStopper /usr/local/bin/sleepstopper
```

### macOS

#### DMG Installer (Recommended)
1. Download `SleepStopper-macos-x64.dmg` (Intel) or `SleepStopper-macos-arm64.dmg` (Apple Silicon)
2. Open the DMG file
3. Drag SleepStopper to the Applications folder
4. Launch from Applications

**Note:** On first run, macOS may block the app. Go to **System Preferences > Security & Privacy > General** and click "Open Anyway".

#### Portable (tar.gz)
```bash
tar -xzf SleepStopper-macos-x64.tar.gz
cd SleepStopper-macos-x64
chmod +x SleepStopper
./SleepStopper
```

### Build from Source

Requires [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).

```bash
# Clone the repository
git clone https://github.com/hokagedami/SleepStopper.git
cd SleepStopper

# Build and run
dotnet build
dotnet run --project SleepStopper

# Or publish a self-contained executable
dotnet publish SleepStopper/SleepStopper.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
dotnet publish SleepStopper/SleepStopper.csproj -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true
dotnet publish SleepStopper/SleepStopper.csproj -c Release -r osx-x64 --self-contained true -p:PublishSingleFile=true
```

## Usage

### Starting the Application

Launch `SleepStopper.exe` (Windows), `SleepStopper` (Linux/macOS), or run from source.

### Main Window

| Element | Description |
|---------|-------------|
| **ACTIVATE** button (green) | Click to prevent system sleep |
| **DEACTIVATE** button (red) | Click to allow system sleep |
| **Log panel** | Shows activity history and status messages |

### Basic Operation

1. **Prevent Sleep:** Click the green **ACTIVATE** button
   - Button turns red and shows **DEACTIVATE**
   - Log displays: "System Auto-Sleep Deactivated"
   - Your computer will now stay awake

2. **Allow Sleep:** Click the red **DEACTIVATE** button
   - Button turns green and shows **ACTIVATE**
   - Log displays: "System Auto-Sleep Activated"
   - Normal sleep behavior is restored

### System Tray

The application minimizes to the system tray when you close the window:

- **Double-click tray icon:** Restore the main window
- **Right-click tray icon:** Opens context menu
  - **ACTIVATE/DEACTIVATE:** Toggle sleep prevention
  - **Show:** Restore the main window
  - **Exit:** Close the application completely

### Closing the Application

- **Clicking X (close button):** Minimizes to system tray (app keeps running)
- **Right-click tray > Exit:** Fully closes the application and restores normal sleep

## How It Works

### Windows
Uses the Windows API `SetThreadExecutionState` with `ES_DISPLAY_REQUIRED` and `ES_CONTINUOUS` flags to inform the system that the display is in use, preventing both display sleep and system sleep.

### Linux
Spawns a `systemd-inhibit` process with `--what=idle:sleep` that creates an inhibitor lock, preventing the system from entering idle or sleep states while active.

### macOS
Spawns a `caffeinate -d` process that asserts a "prevent display sleep" assertion, keeping the display and system awake.

## Troubleshooting

### Windows
- **App won't start:** Ensure you extracted all files from the zip, not just the .exe
- **Sleep still occurs:** Run as Administrator for full compatibility

### Linux
- **Permission denied:** Run `chmod +x SleepStopper` to make it executable
- **systemd-inhibit not found:** Install systemd or use a systemd-based distribution
- **No tray icon:** Ensure your desktop environment supports system tray (may need an extension on GNOME)

### macOS
- **"App is damaged" error:** Run `xattr -cr /path/to/SleepStopper` to remove quarantine
- **Blocked by Gatekeeper:** Go to System Preferences > Security & Privacy and allow the app

## License

This project is open source.

## Contributing

Contributions are welcome! Please feel free to submit issues and pull requests.
