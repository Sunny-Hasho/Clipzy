using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Clipzy.Core;

namespace Clipzy.Services
{
    public class ExplorerService
    {
        public string? GetActiveExplorerPath()
        {
            IntPtr handle = NativeMethods.GetForegroundWindow();
            if (handle == IntPtr.Zero) return null;

            // Verify if the foreground window is "CabinetWClass" (File Explorer) or "Progman" (Desktop)
            // But usually we just iterate windows to find the match.
            // Actually, querying the ShellWindows is better.

            return GetExplorerPathFromHandle(handle);
        }

        private string? GetExplorerPathFromHandle(IntPtr handle)
        {
            try
            {
                Type? shellAppType = Type.GetTypeFromProgID("Shell.Application");
                if (shellAppType == null) return null;

                object? shellInstance = Activator.CreateInstance(shellAppType);
                if (shellInstance == null) return null;

                dynamic shell = shellInstance;

                foreach (dynamic window in shell.Windows())
                {
                    if (window.HWND == (long)handle)
                    {
                        // Check if it's a file system folder
                        // Note: LocationURL comes as file:///C:/...
                        string url = window.LocationURL;
                        if (!string.IsNullOrEmpty(url))
                        {
                            return new Uri(url).LocalPath;
                        }
                    }
                }
            }
            catch (Exception) 
            {
                // COM errors or non-file folders
            }

            return null;
        }
    }
}
