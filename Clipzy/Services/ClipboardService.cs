using System.Windows.Media.Imaging;
using System.Windows;
using System.IO;

namespace Clipzy.Services
{
    public class ClipboardService
    {
        public bool ContainsImage()
        {
            return System.Windows.Clipboard.ContainsImage();
        }

        public BitmapSource? GetClipboardImage()
        {
            if (System.Windows.Clipboard.ContainsImage())
            {
                return System.Windows.Clipboard.GetImage();
            }
            return null;
        }
    }
}
