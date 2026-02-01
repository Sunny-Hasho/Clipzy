using Clipzy.Services;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

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

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

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

            _trayService.Initialize(hotkeyEnabled: _isHotkeyEnabled, contextMenuEnabled: true);
            _trayService.ExitRequested += (s, ev) => Shutdown();
            _trayService.HotkeyToggleRequested += (s, enabled) => 
            {
                _isHotkeyEnabled = enabled;
            };
            _trayService.ContextMenuToggleRequested += (s, enabled) =>
            {
                if (enabled) _contextMenuService.Register();
                else _contextMenuService.Unregister();
            };
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

        protected override void OnExit(ExitEventArgs e)
        {
            _hotkeyService.Dispose();
            _trayService.Dispose();
            _contextMenuService.Unregister();
            base.OnExit(e);
        }
    }
}
