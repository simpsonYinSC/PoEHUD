using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using PoEHUD.Framework.Helpers;
using PoEHUD.Framework.InputHooks.Structures;

namespace PoEHUD.Framework.InputHooks
{
    public static class MouseHook
    {
        private const int WH_MOUSE_LL = 14;
        private const int WM_MOUSEMOVE = 0x200;
        private const int WM_LBUTTONDOWN = 0x201;
        private const int WM_RBUTTONDOWN = 0x204;
        private const int WM_LBUTTONUP = 0x202;
        private const int WM_RBUTTONUP = 0x205;
        private const int WM_MOUSEWHEEL = 0x020A;
        private static HookProc hookProc;
        private static int handle;

        #region Mouse Events

        public static event Action<MouseInfo> OnMouseMove
        {
            add
            {
                Subscribe();
                MouseMove += value;
            }
            remove
            {
                MouseMove -= value;
                TryUnsubscribe();
            }
        }

        public static event Action<MouseInfo> OnMouseDown
        {
            add
            {
                Subscribe();
                MouseDown += value;
            }
            remove
            {
                MouseDown -= value;
                TryUnsubscribe();
            }
        }

        public static event Action<MouseInfo> OnMouseUp
        {
            add
            {
                Subscribe();
                MouseUp += value;
            }
            remove
            {
                MouseUp -= value;
                TryUnsubscribe();
            }
        }

        public static event Action<MouseInfo> OnMouseWheel
        {
            add
            {
                Subscribe();
                MouseWheel += value;
            }
            remove
            {
                MouseWheel -= value;
                TryUnsubscribe();
            }
        }

        private static event Action<MouseInfo> MouseMove;
        private static event Action<MouseInfo> MouseDown;
        private static event Action<MouseInfo> MouseUp;
        private static event Action<MouseInfo> MouseWheel;

        #endregion

        private static int MouseHookProc(int nCode, int wParam, IntPtr lParam)
        {
            if (nCode < 0)
            {
                return WindowsAPI.CallNextHookEx(handle, nCode, wParam, lParam);
            }

            var mouseHookStruct = (MouseLowLevelHookStruct)Marshal.PtrToStructure(lParam, typeof(MouseLowLevelHookStruct));
            Point position = mouseHookStruct.Point;

            MouseInfo mouseInfo = null;
            switch (wParam)
            {
                case WM_LBUTTONDOWN:
                    mouseInfo = new MouseInfo(MouseButtons.Left, position, 0);
                    MouseDown.SafeInvoke(mouseInfo);
                    break;

                case WM_LBUTTONUP:
                    mouseInfo = new MouseInfo(MouseButtons.Left, position, 0);
                    MouseUp.SafeInvoke(mouseInfo);
                    break;

                case WM_RBUTTONDOWN:
                    mouseInfo = new MouseInfo(MouseButtons.Right, position, 0);
                    MouseDown.SafeInvoke(mouseInfo);
                    break;

                case WM_RBUTTONUP:
                    mouseInfo = new MouseInfo(MouseButtons.Right, position, 0);
                    MouseUp.SafeInvoke(mouseInfo);
                    break;

                case WM_MOUSEWHEEL:
                    int delta = (mouseHookStruct.MouseData >> 16) & 0xffff;
                    mouseInfo = new MouseInfo(MouseButtons.None, position, delta);
                    MouseWheel.SafeInvoke(mouseInfo);
                    break;

                case WM_MOUSEMOVE:
                    mouseInfo = new MouseInfo(MouseButtons.None, position, 0);
                    MouseMove.SafeInvoke(mouseInfo);
                    break;
            }

            if (mouseInfo != null && mouseInfo.Handled)
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

            hookProc = MouseHookProc;
            handle = WindowsAPI.SetWindowsHookEx(WH_MOUSE_LL, hookProc, IntPtr.Zero, 0);
            if (handle != 0)
            {
                return;
            }

            int errorCode = Marshal.GetLastWin32Error();
            throw new Win32Exception(errorCode);
        }

        private static void TryUnsubscribe()
        {
            if (handle == 0 || MouseDown != null || MouseUp != null || MouseMove != null || MouseWheel != null)
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
