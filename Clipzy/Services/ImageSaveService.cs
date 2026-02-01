using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace Clipzy.Services
{
    public class ImageSaveService
    {
        public string SaveImage(BitmapSource image, string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                throw new DirectoryNotFoundException($"The directory '{folderPath}' does not exist.");
            }

            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string fileName = $"Screenshot_{timestamp}.png";
            string fullPath = Path.Combine(folderPath, fileName);

            // Handle potential collision (unlikely with seconds, but good practice)
            int counter = 1;
            while (File.Exists(fullPath))
            {
                fileName = $"Screenshot_{timestamp}_{counter}.png";
                fullPath = Path.Combine(folderPath, fileName);
                counter++;
            }

            using (var fileStream = new FileStream(fullPath, FileMode.Create))
            {
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(image));
                encoder.Save(fileStream);
            }

            return fullPath;
        }
    }
}
