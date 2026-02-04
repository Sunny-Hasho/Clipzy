using System;
using System.Drawing;
using System.Windows.Forms;
using Application = System.Windows.Application;

namespace Clipzy.Services
{
    public class TrayService : IDisposable
    {
        private NotifyIcon? _notifyIcon;
        private TrayMenu? _trayMenu;

        public event EventHandler<bool>? HotkeyToggleRequested;
        public event EventHandler<bool>? ContextMenuToggleRequested;
        public event EventHandler<bool>? StartupToggleRequested;
        public event EventHandler? OpenFolderRequested;
        public event EventHandler? ExitRequested;

        public void Initialize(bool hotkeyEnabled, bool contextMenuEnabled, bool startupEnabled)
        {
            _trayMenu = new TrayMenu();
            _trayMenu.SetStates(hotkeyEnabled, contextMenuEnabled, startupEnabled);
            
            _trayMenu.HotkeyToggleClicked += (s, enabled) => HotkeyToggleRequested?.Invoke(this, enabled);
            _trayMenu.ContextMenuToggleClicked += (s, enabled) => ContextMenuToggleRequested?.Invoke(this, enabled);
            _trayMenu.StartupToggleClicked += (s, enabled) => StartupToggleRequested?.Invoke(this, enabled);
            _trayMenu.OpenFolderClicked += (s, e) => OpenFolderRequested?.Invoke(this, EventArgs.Empty);
            _trayMenu.ExitClicked += (s, e) => ExitRequested?.Invoke(this, EventArgs.Empty);

            Icon trayIcon = SystemIcons.Application;
            try
            {
                string icoPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app.ico");
                string pngPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icon.png");
                
                if (System.IO.File.Exists(icoPath))
                {
                    // Use the native icon constructor which picks the best size from the multi-size ICO.
                    // This is much sharper than manual scaling.
                    trayIcon = new Icon(icoPath, SystemInformation.SmallIconSize);
                }
                else if (System.IO.File.Exists(pngPath))
                {
                    trayIcon = GetHighQualityIcon(pngPath, SystemInformation.SmallIconSize.Width);
                }
                else
                {
                    string? processPath = Environment.ProcessPath;
                    if (!string.IsNullOrEmpty(processPath))
                    {
                        trayIcon = Icon.ExtractAssociatedIcon(processPath) ?? SystemIcons.Application;
                    }
                }
            }
            catch { /* Fallback */ }

            _notifyIcon = new NotifyIcon
            {
                Icon = trayIcon,
                Visible = true,
                Text = "Clipzy"
            };

            _notifyIcon.MouseClick += (s, e) =>
            {
                if (e.Button == MouseButtons.Right || e.Button == MouseButtons.Left)
                {
                    Application.Current.Dispatcher.BeginInvoke(new Action(() => 
                    {
                        if (_trayMenu == null) return;

                        bool isRecentlyHidden = (DateTime.Now - _trayMenu.LastHideTime).TotalMilliseconds < 250;
                        
                        if (_trayMenu.IsVisible)
                        {
                            _trayMenu.Hide();
                        }
                        else if (!isRecentlyHidden)
                        {
                            _trayMenu.ShowAtMouse();
                        }
                    }));
                }
            };
        }

        private Icon GetHighQualityIcon(string path, int size)
        {
            using (var bitmap = new Bitmap(path))
            {
                var destImage = new Bitmap(size, size);
                using (var graphics = Graphics.FromImage(destImage))
                {
                    graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                    graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                    
                    graphics.DrawImage(bitmap, 0, 0, size, size);
                }
                return Icon.FromHandle(destImage.GetHicon());
            }
        }

        public void Dispose()
        {
            _notifyIcon?.Dispose();
            _trayMenu?.Close();
        }
    }
}
