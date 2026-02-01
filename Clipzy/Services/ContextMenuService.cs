using System;
using Microsoft.Win32;
using System.Diagnostics;
using System.Reflection;

namespace Clipzy.Services
{
    public class ContextMenuService
    {
        private const string MENU_NAME = "Clipzy_ContextMenu";
        private const string MENU_TEXT = "Paste Screenshot Here";

        public void Register()
        {
            try
            {
                string? exePath = Process.GetCurrentProcess().MainModule?.FileName;
                if (string.IsNullOrEmpty(exePath)) return;

                // For Background of directory (when right clicking on empty space in folder)
                RegisterForKey(@"Software\Classes\Directory\Background\shell", exePath);

                // For Directory itself (when right clicking on a folder) - Optional, maybe user wants to paste INTO that folder
                RegisterForKey(@"Software\Classes\Directory\shell", exePath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to register context menu: {ex.Message}");
            }
        }

        private void RegisterForKey(string keyPath, string exePath)
        {
            using RegistryKey key = Registry.CurrentUser.CreateSubKey(keyPath);
            using RegistryKey menuKey = key.CreateSubKey(MENU_NAME);
            
            menuKey.SetValue("", MENU_TEXT);
            menuKey.SetValue("Icon", exePath);

            using RegistryKey commandKey = menuKey.CreateSubKey("command");
            // Pass the target directory as argument
            // %V is the directory path
            commandKey.SetValue("", $"\"{exePath}\" /paste \"%V\"");
        }

        public void Unregister()
        {
            try
            {
                using RegistryKey? bgKey = Registry.CurrentUser.OpenSubKey(@"Software\Classes\Directory\Background\shell", true);
                if (bgKey != null)
                {
                    bgKey.DeleteSubKeyTree(MENU_NAME, false);
                }

                using RegistryKey? dirKey = Registry.CurrentUser.OpenSubKey(@"Software\Classes\Directory\shell", true);
                if (dirKey != null)
                {
                    dirKey.DeleteSubKeyTree(MENU_NAME, false);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to unregister context menu: {ex.Message}");
            }
        }
    }
}
