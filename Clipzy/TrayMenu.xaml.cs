using System;
using System.Windows;
using System.Windows.Media;

namespace Clipzy
{
    public partial class TrayMenu : Window
    {
        public event EventHandler<bool>? HotkeyToggleClicked;
        public event EventHandler<bool>? ContextMenuToggleClicked;
        public event EventHandler<bool>? StartupToggleClicked;
        public event EventHandler? OpenFolderClicked;
        public event EventHandler? ExitClicked;

        private bool _hotkeyEnabled = true;
        private bool _contextMenuEnabled = true;
        private bool _startupEnabled = false;
        private System.Windows.Threading.DispatcherTimer? _clickDetectionTimer;
        public DateTime LastHideTime { get; private set; } = DateTime.Now.AddYears(-1);

        public TrayMenu()
        {
            InitializeComponent();
            _clickDetectionTimer = new System.Windows.Threading.DispatcherTimer();
            _clickDetectionTimer.Interval = TimeSpan.FromMilliseconds(50);
            _clickDetectionTimer.Tick += ClickDetectionTimer_Tick;
        }

        public void SetStates(bool hotkeyEnabled, bool contextMenuEnabled, bool startupEnabled)
        {
            _hotkeyEnabled = hotkeyEnabled;
            _contextMenuEnabled = contextMenuEnabled;
            _startupEnabled = startupEnabled;
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            ChkHotkey.IsChecked = _hotkeyEnabled;
            ChkContextMenu.IsChecked = _contextMenuEnabled;
            ChkStartup.IsChecked = _startupEnabled;
        }

        private void ClickDetectionTimer_Tick(object? sender, EventArgs e)
        {
            if (!this.IsVisible) return;

            // Check if any mouse button is pressed
            bool mouseButtonDown = (Clipzy.Core.NativeMethods.GetAsyncKeyState(Clipzy.Core.NativeMethods.VK_LBUTTON) & 0x8000) != 0 ||
                                 (Clipzy.Core.NativeMethods.GetAsyncKeyState(Clipzy.Core.NativeMethods.VK_RBUTTON) & 0x8000) != 0 ||
                                 (Clipzy.Core.NativeMethods.GetAsyncKeyState(Clipzy.Core.NativeMethods.VK_MBUTTON) & 0x8000) != 0;

            if (mouseButtonDown && !this.IsMouseOver)
            {
                this.Hide();
            }
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
            int exStyle = Clipzy.Core.NativeMethods.GetWindowLong(hwnd, Clipzy.Core.NativeMethods.GWL_EXSTYLE);
            Clipzy.Core.NativeMethods.SetWindowLong(hwnd, Clipzy.Core.NativeMethods.GWL_EXSTYLE, 
                exStyle | Clipzy.Core.NativeMethods.WS_EX_NOACTIVATE | Clipzy.Core.NativeMethods.WS_EX_TOOLWINDOW);
        }

        public void ShowAtMouse()
        {
            var mousePos = System.Windows.Forms.Cursor.Position;
            
            DpiScale dpi = VisualTreeHelper.GetDpi(this);
            double scaleX = dpi.DpiScaleX;
            double scaleY = dpi.DpiScaleY;

            double wpfX = mousePos.X / scaleX;
            double wpfY = mousePos.Y / scaleY;

            double height = this.ActualHeight > 0 ? this.ActualHeight : 160;

            this.Left = wpfX - this.Width + 20;
            this.Top = wpfY - height - 10;

            if (this.Top < 0) this.Top = 10;
            if (this.Left < 0) this.Left = 10;

            this.Show();
            _clickDetectionTimer?.Start();
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            this.Hide();
        }

        public new void Hide()
        {
            if (!this.IsVisible) return;
            LastHideTime = DateTime.Now;
            _clickDetectionTimer?.Stop();
            base.Hide();
        }

        private void BtnHotkey_Click(object sender, RoutedEventArgs e)
        {
            _hotkeyEnabled = !_hotkeyEnabled;
            UpdateVisuals();
            HotkeyToggleClicked?.Invoke(this, _hotkeyEnabled);
        }

        private void BtnContextMenu_Click(object sender, RoutedEventArgs e)
        {
            _contextMenuEnabled = !_contextMenuEnabled;
            UpdateVisuals();
            ContextMenuToggleClicked?.Invoke(this, _contextMenuEnabled);
        }

        private void BtnStartup_Click(object sender, RoutedEventArgs e)
        {
            _startupEnabled = !_startupEnabled;
            UpdateVisuals();
            StartupToggleClicked?.Invoke(this, _startupEnabled);
        }

        private void BtnOpenFolder_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            OpenFolderClicked?.Invoke(this, EventArgs.Empty);
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            ExitClicked?.Invoke(this, EventArgs.Empty);
        }
    }
}
