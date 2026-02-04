using System.Windows;

namespace Clipzy
{
    public partial class NotificationWindow : Window
    {
        public NotificationWindow(string message)
        {
            InitializeComponent();
            TxtMessage.Text = message;
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
            int exStyle = Clipzy.Core.NativeMethods.GetWindowLong(hwnd, Clipzy.Core.NativeMethods.GWL_EXSTYLE);
            Clipzy.Core.NativeMethods.SetWindowLong(hwnd, Clipzy.Core.NativeMethods.GWL_EXSTYLE,
                exStyle | Clipzy.Core.NativeMethods.WS_EX_TOOLWINDOW);
        }
    }
}
