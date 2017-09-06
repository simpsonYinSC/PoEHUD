using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using PoEHUD.Framework.Helpers;
using PoEHUD.Framework.InputHooks.Structures;

namespace PoEHUD.Framework.InputHooks
{
    public static class KeyboardHook
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x100;
        private const int WM_KEYUP = 0x101;
        private const int WM_SYSKEYDOWN = 0x104;
        private const int WM_SYSKEYUP = 0x105;
        private static HookProc hookProc;
        private static int handle;
        private static bool control, alt, shift;

        #region Keyboard Events

        public static event Action<KeyInfo> OnKeyUp
        {
            add
            {
                Subscribe();
                KeyUp += value;
            }
            remove
            {
                KeyUp -= value;
                TryUnsubscribe();
            }
        }

        public static event Action<KeyInfo> OnKeyDown
        {
            add
            {
                Subscribe();
                KeyDown += value;
            }
            remove
            {
                KeyDown -= value;
                TryUnsubscribe();
            }
        }

        private static event Action<KeyInfo> KeyUp;
        private static event Action<KeyInfo> KeyDown;

        #endregion
        
        private static KeyInfo GetKeys(Keys keyData, bool specialValue)
        {
            switch (keyData)
            {
                case Keys.RControlKey:
                case Keys.LControlKey:
                    control = specialValue;
                    break;

                case Keys.RMenu:
                case Keys.LMenu:
                    alt = specialValue;
                    break;

                case Keys.RShiftKey:
                case Keys.LShiftKey:
                    shift = specialValue;
                    break;
            }

            return new KeyInfo(keyData, control, alt, shift);
        }

        private static int KeyboardHookProc(int nCode, Int32 wParam, IntPtr lParam)
        {
            if (nCode < 0)
            {
                return WindowsAPI.CallNextHookEx(handle, nCode, wParam, lParam);
            }

            var keyboardHookStruct = (KeyboardHookStruct)Marshal.PtrToStructure(lParam, typeof(KeyboardHookStruct));

            KeyInfo keyInfo = null;
            if (wParam == WM_KEYDOWN || wParam == WM_SYSKEYDOWN)
            {
                var keyData = (Keys)keyboardHookStruct.VirtualKeyCode;
                keyInfo = GetKeys(keyData, true);
                KeyDown.SafeInvoke(keyInfo);
            }

            if (wParam == WM_KEYUP || wParam == WM_SYSKEYUP)
            {
                var keyData = (Keys)keyboardHookStruct.VirtualKeyCode;
                keyInfo = GetKeys(keyData, false);
                KeyUp.SafeInvoke(keyInfo);
            }

            if (keyInfo != null && keyInfo.Handled)
            {
                return -1;
            }

            return WindowsAPI.CallNextHookEx(handle, nCode, wParam, lParam);
        }

        private static void Subscribe()
        {
            if (handle != 0)
            {
                return;
            }

            hookProc = KeyboardHookProc;
            handle = WindowsAPI.SetWindowsHookEx(WH_KEYBOARD_LL, hookProc, IntPtr.Zero, 0);
            if (handle != 0)
            {
                return;
            }

            int errorCode = Marshal.GetLastWin32Error();
            throw new Win32Exception(errorCode);
        }

        private static void TryUnsubscribe()
        {
            if (handle == 0 || KeyDown != null || KeyUp != null)
            {
                return;
            }

            int result = WindowsAPI.UnhookWindowsHookEx(handle);
            handle = 0;
            hookProc = null;
            if (result != 0)
            {
                return;
            }

            int errorCode = Marshal.GetLastWin32Error();
            throw new Win32Exception(errorCode);
        }
    }
}
