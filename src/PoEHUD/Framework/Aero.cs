using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace PoEHUD.Framework
{
    public class Aero
    {
        private const uint WMClose = 0x10;
        private readonly string windowsFolder = Environment.GetFolderPath(Environment.SpecialFolder.Windows);

        public static bool SwitchTheme(string themePath)
        {
            try
            {
                //// String themePath = System.Environment.GetFolderPath(Environment.SpecialFolder.Windows) + @"\Resources\Ease of Access Themes\basic.theme";
                // Set the theme
                // essentially runs the command line:  rundll32.exe %SystemRoot%\system32\shell32.dll,Control_RunDLL %SystemRoot%\system32\desk.cpl desk,@Themes /Action:OpenTheme /file:"%WINDIR%\Resources\Ease of Access Themes\classic.theme"
                string themeOutput = StartProcessAndWait("rundll32.exe", Environment.GetFolderPath(Environment.SpecialFolder.System) + @"\shell32.dll,Control_RunDLL " + Environment.GetFolderPath(Environment.SpecialFolder.System) + "\\desk.cpl desk,@Themes /Action:OpenTheme /file:\"" + themePath + "\"", 30);

                Console.WriteLine(themeOutput);

                // Wait for the theme to be set
                System.Threading.Thread.Sleep(1000);

                // Close the Theme UI Window
                IntPtr hWndTheming = FindWindow("CabinetWClass", null);
                SendMessage(hWndTheming, WMClose, IntPtr.Zero, IntPtr.Zero);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An exception occured while setting the theme: " + ex.Message);

                return false;
            }

            return true;
        }

        public bool SwitchToClassicTheme()
        {
            return SwitchTheme(windowsFolder + @"\Resources\Ease of Access Themes\basic.theme");
        }

        public bool SwitchToAeroTheme()
        {
            return SwitchTheme(windowsFolder + @"\Resources\Themes\aero.theme");
        }

        public string GetTheme()
        {
            const string registryKey = @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes";
            var theme = (string)Registry.GetValue(registryKey, "CurrentTheme", string.Empty);
            theme = theme.Split('\\').Last().Split('.').First();
            return theme;
        }

        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        private static extern IntPtr FindWindow(string sClassName, string sAppName);

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        private static string StartProcessAndWait(string filename, string arguments, int seconds)
        {
            string msg = string.Empty;
            Process p = new Process
            {
                StartInfo =
                {
                    WindowStyle = ProcessWindowStyle.Minimized,
                    FileName = filename,
                    Arguments = arguments
                }
            };
            p.Start();

            bool exited = false;
            int counter = 0;
            // give it "seconds" seconds to run
            while (!exited && counter < seconds)
            {
                exited = p.HasExited;
                counter++;
                System.Threading.Thread.Sleep(1000);
            }

            if (counter == seconds)
            {
                msg = "Program did not close in expected time.";
            }

            return msg;
        }
    }
}
