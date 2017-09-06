using System.Drawing;
using System.Runtime.InteropServices;

namespace PoEHUD.Framework.InputHooks.Structures
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct MouseLowLevelHookStruct
    {
        public Point Point;
        public int MouseData;
        public int Flags;
        public int Time;
        public int ExtraInfo;
    }
}
