using Clipzy.Services;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using Microsoft.Win32;
using System.Threading;

namespace Clipzy
{
    public partial class App : System.Windows.Application
    {
        private ClipboardService _clipboardService = new();
        private ImageSaveService _imageSaveService = new();
        private ExplorerService _explorerService = new();
        private HotkeyService _hotkeyService = new();
        private ContextMenuService _contextMenuService = new();
        private TrayService _trayService = new();
        private bool _isHotkeyEnabled = true;
        private string _lastUsedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

        private static Mutex? _mutex;

        protected override void OnStartup(StartupEventArgs e)
        {
            const string mutexName = "Global\\Clipzy_SingleInstance_Mutex";
            _mutex = new Mutex(true, mutexName, out bool createdNew);

            if (!createdNew)
            {
                var notification = new NotificationWindow("Clipzy is running", "Check the system tray icon.", "Okay");
                notification.ShowDialog();
                Shutdown();
                return;
            }

            base.OnStartup(e);

            CheckFirstRun();

            // Argument Parsing
            string[] args = e.Args;
            if (args.Length >= 2 && args[0].Equals("/paste", StringComparison.OrdinalIgnoreCase))
            {
                string targetPath = args[1];
                PasteImage(targetPath);
                Shutdown();
                return;
            }

            // Normal Startup
            // Ensure Context Menu is registered on first run
            _contextMenuService.Register(); 

            bool isStartup = CheckStartup();
            _trayService.Initialize(hotkeyEnabled: _isHotkeyEnabled, contextMenuEnabled: true, startupEnabled: isStartup);
            _trayService.ExitRequested += (s, ev) => 
            {
                SetStartup(false);
                
                // Reset FirstRun so it shows the "Installed" message next time we run manually
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Sunny-Hasho\Clipzy"))
                {
                    key.SetValue("FirstRun", 1);
                }

                Shutdown();
            };
            _trayService.HotkeyToggleRequested += (s, enabled) => 
            {
                _isHotkeyEnabled = enabled;
            };
            _trayService.ContextMenuToggleRequested += (s, enabled) =>
            {
                if (enabled) _contextMenuService.Register();
                else _contextMenuService.Unregister();
            };
            _trayService.StartupToggleRequested += (s, enabled) => SetStartup(enabled);
            _trayService.OpenFolderRequested += (s, ev) => 
            {
                if (System.IO.Directory.Exists(_lastUsedPath))
                {
                    Process.Start("explorer.exe", _lastUsedPath);
                }
            };

            // Use a dummy hidden window to host the Hotkey Hwnd
            Window dummy = new Window { Visibility = Visibility.Hidden, WindowStyle = WindowStyle.None, ShowInTaskbar = false };
            var helper = new WindowInteropHelper(dummy);
            helper.EnsureHandle();

            _hotkeyService.Register(helper.Handle);
            _hotkeyService.HotkeyPressed += OnHotkeyPressed;
        }

        private void OnHotkeyPressed(object? sender, EventArgs e)
        {
            if (!_isHotkeyEnabled) return;

            string? path = _explorerService.GetActiveExplorerPath();
            if (!string.IsNullOrEmpty(path))
            {
                PasteImage(path);
            }
            else
            {
                Debug.WriteLine("No active Explorer window found.");
            }
        }

        private void PasteImage(string folderPath)
        {
            if (_clipboardService.ContainsImage())
            {
                var image = _clipboardService.GetClipboardImage();
                if (image != null)
                {
                    try
                    {
                        string savedPath = _imageSaveService.SaveImage(image, folderPath);
                        _lastUsedPath = folderPath; // Update last used path
                        Debug.WriteLine($"Image saved to: {savedPath}");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error saving image: {ex.Message}");
                    }
                }
            }
        }

        private const string AppName = "Clipzy";

        private bool CheckStartup()
        {
            try
            {
                using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", false))
                {
                    return key != null && key.GetValue(AppName) != null;
                }
            }
            catch { return false; }
        }

        private void SetStartup(bool enable)
        {
            try
            {
                using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true))
                {
                    if (key == null) return;

                    if (enable)
                    {
                        string? path = Environment.ProcessPath;
                        if (!string.IsNullOrEmpty(path))
                        {
                            // Ensure path is quoted if it contains spaces
                            if (!path.StartsWith("\"") && path.Contains(" "))
                            {
                                path = $"\"{path}\"";
                            }
                            key.SetValue(AppName, path);
                        }
                    }
                    else
                    {
                        key.DeleteValue(AppName, false);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error settings startup: {ex.Message}");
            }
        }


        protected override void OnExit(ExitEventArgs e)
        {
            _hotkeyService.Dispose();
            _trayService.Dispose();
            _contextMenuService.Unregister();
            base.OnExit(e);
        }
        private void CheckFirstRun()
        {
            const string registryKeyPath = @"Software\Sunny-Hasho\Clipzy";
            const string valueName = "FirstRun";

            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(registryKeyPath))
            {
                object? value = key.GetValue(valueName);
                if (value == null || (value is string s && s == "True") || (value is int i && i == 1))
                {
                    // It is the first run (or key missing)
                    var notification = new NotificationWindow("Clipzy Installed", "Clipzy is now running in your system tray.", "Got it");
                    notification.ShowDialog();
                    
                    // Set flag to false so it doesn't show again
                    key.SetValue(valueName, 0);
                }
            }
        }
    }
}
