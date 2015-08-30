using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace BrightnessControl
{
    class HotkeyManager : IDisposable
    {
        private const int WM_HOTKEY = 0x0312;

        private const int ERROR_HOTKEY_ALREADY_REGISTERED = 0x581;

        [DllImport("user32.dll", SetLastError = true)]
        private extern static bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll", SetLastError = true)]
        private extern static bool UnregisterHotKey(IntPtr hWnd, int id);

        private HwndSource source;
        private HwndSourceHook hook;
        private List<int> ids;

        public event EventHandler<PressedEventArgs> Pressed;

        public HotkeyManager(Window window)
        {
            source = (HwndSource)PresentationSource.FromVisual(window);
            hook = new HwndSourceHook(WndProc);
            source.AddHook(hook);
            ids = new List<int>();
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY)
            {
                EventHandler<PressedEventArgs> handler = Pressed;
                if (handler != null)
                {
                    handler(this, new PressedEventArgs() {
                        Id = (int)wParam
                    });
                }

                handled = true;
            }
            return IntPtr.Zero;
        }

        public void Register(int id, ModifierKeys modifiers, Key key)
        {
            if (!RegisterHotKey(source.Handle, id, (uint)modifiers, (uint)KeyInterop.VirtualKeyFromKey(key)))
            {
                int error = Marshal.GetLastWin32Error();
                if (error == ERROR_HOTKEY_ALREADY_REGISTERED)
                {
                    throw new HotkeyAlreadyRegisteredException();
                }
                else
                {
                    throw new Win32Exception(error);
                }
            }

            ids.Add(id);
        }

        public void Unregister(int id)
        {
            if (!UnregisterHotKey(source.Handle, id))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }

        public void Dispose()
        {
            foreach (int id in ids)
            {
                Unregister(id);
            }
            source.RemoveHook(hook);
        }
    }

    class PressedEventArgs : EventArgs
    {
        public int Id;
    }

    class HotkeyAlreadyRegisteredException : Exception
    {
        public HotkeyAlreadyRegisteredException()
        {

        }
    }
}
