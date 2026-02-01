using System;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Interop;
using Clipzy.Core;

namespace Clipzy.Services
{
    public class HotkeyService : IDisposable
    {
        public event EventHandler? HotkeyPressed;
        private IntPtr _windowHandle;
        private HwndSource? _source;
        private const int HOTKEY_ID = 9000;

        public void Register(IntPtr windowHandle)
        {
            _windowHandle = windowHandle;
            _source = HwndSource.FromHwnd(_windowHandle);
            _source.AddHook(HwndHook);

            // Register Ctrl + Alt + V
            uint modifiers = NativeMethods.MOD_CONTROL | NativeMethods.MOD_ALT;
            // V key is 0x56
            uint vk = 0x56;

            if (!NativeMethods.RegisterHotKey(_windowHandle, HOTKEY_ID, modifiers, vk))
            {
                // Handle error: likely hotkey already taken
                System.Diagnostics.Debug.WriteLine("Failed to register hotkey.");
            }
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == NativeMethods.WM_HOTKEY)
            {
                if (wParam.ToInt32() == HOTKEY_ID)
                {
                    HotkeyPressed?.Invoke(this, EventArgs.Empty);
                    handled = true;
                }
            }
            return IntPtr.Zero;
        }

        public void Dispose()
        {
            NativeMethods.UnregisterHotKey(_windowHandle, HOTKEY_ID);
            _source?.RemoveHook(HwndHook);
        }
    }
}
