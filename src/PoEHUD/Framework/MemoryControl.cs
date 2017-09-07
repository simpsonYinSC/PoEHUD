using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace PoEHUD.Framework
{
    public class MemoryControl
    {
        private static MemoryControl memoryControl;

        private MemoryControl()
        {
            long lastTime = DateTime.Now.Ticks;
            Application.Idle += delegate
            {
                try
                {
                    long ticks = DateTime.Now.Ticks;
                    if (ticks - lastTime <= 10000000L)
                    {
                        return;
                    }

                    lastTime = ticks;
                    MemoryFree();
                }
                catch
                {
                    // ignored
                }
            };
        }

        public static void Start()
        {
            try
            {
                if (memoryControl == null && Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    memoryControl = new MemoryControl();
                }
            }
            catch
            {
                // ignored
            }
        }

        private static void MemoryFree()
        {
            try
            {
                using (Process currentProcess = Process.GetCurrentProcess())
                {
                    WindowsAPI.SetProcessWorkingSetSize(currentProcess.Handle, -1, -1);
                }
            }
            catch
            {
                // ignored
            }
        }
    }
}
