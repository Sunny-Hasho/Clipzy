using System.Windows;
using System.Windows.Input;

namespace Clipzy
{
    public partial class NotificationWindow : Window
    {
        public NotificationWindow(string title, string message, string buttonText)
        {
            InitializeComponent();
            this.MouseLeftButtonDown += (s, e) => this.DragMove();

            TitleText.Text = title;
            MessageText.Text = message;
            ButtonText.Text = buttonText;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
