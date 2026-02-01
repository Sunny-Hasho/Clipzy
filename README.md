<img src="icon.png" width="128" height="128" alt="Clipzy Logo" /> <h1> Clipzy</h1>

  <p><b>Paste Images Anywhere in Explorer</b></p>
  <p>
    <img src="https://img.shields.io/badge/.NET-8.0-blue.svg" alt=".NET 8.0" />
    <img src="https://img.shields.io/badge/Platform-Windows-lightgrey.svg" alt="Platform Windows" />
    <img src="https://img.shields.io/badge/License-MIT-green.svg" alt="License MIT" />
  </p>
</align>

---

**Clipzy** is a lightweight, high-performance Windows background utility that lets you paste images directly from your clipboard as `.png` files into your active Windows Explorer window using a global hotkey or a minimalist context menu.

## ✨ Features

- **🎯 Global Hotkey**: Press `Ctrl + Alt + V` to instantly save your clipboard image to the current folder.
- **🖱️ Explorer Integration**: Right-click anywhere in Windows Explorer and select "Paste Screenshot Here".
- **⚡ Zero Interruption**: Runs silently in the background with a DPI-aware, non-intrusive interface.
- **📂 Smart Detection**: Automatically identifies the active folder path in Windows Explorer.

## 🚀 Getting Started

### Prerequisites

- **Windows 10/11**
- **User Runtime**: [.NET 8.0 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Developer SDK**: [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

## 📦 Distribution

Clipzy is built for portability and speed:

1.  **Self-Contained**: Everything included in one `.exe`. Runs anywhere without .NET installed.
2.  **Framework-Dependent**: Minimal file size (~1MB). Requires .NET 8.0 Desktop Runtime.

### Installation

1.  Download the latest [Release](https://github.com/yourusername/Clipzy/releases).
2.  Launch `Clipzy.exe`.
3.  Find the hamster in your System Tray!

## 🛠️ Configuration

Right-click the tray icon to:

- Toggle **Global Hotkey** (`Ctrl + Alt + V`).
- Toggle **Context Menu** integration.
- Instantly **Open Last Save Folder**.

## 🏗️ Building from Source

```bash
# Clone the repo
git clone https://github.com/yourusername/Clipzy.git

# Build the project
dotnet build -c Release
```

## ❓ Troubleshooting

### "dotnet build" command not found

If you see an error saying `build does not exist` or `No .NET SDKs were found`, check these:

1. **Install the SDK**: Ensure you have the [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) installed, not just the Runtime.
2. **x86 vs x64 Path Conflict**:
   - Windows often points to the `Program Files (x86)\dotnet` folder by default, which usually lacks the SDK.
   - **Fix**: Move `C:\Program Files\dotnet\` above the `(x86)` version in your System **Environment Variables** (Path).
   - **Check**: Run `dotnet --list-sdks` in a new terminal. You should see `8.0.xxx`.

## 📜 License

Distributed under the MIT License. See `LICENSE` for more information.
