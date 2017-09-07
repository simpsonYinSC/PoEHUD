using PoEHUD.HUD.Settings;
using SharpDX;

namespace PoEHUD.HUD.XPRate
{
    public sealed class XPRateSettings : SettingsBase
    {
        public XPRateSettings()
        {
            Enable = true;
            OnlyAreaName = false;
            ShowLatency = true;
            ShowInTown = true;
            TextSize = new RangeNode<int>(16, 10, 20);
            BackgroundColor = new ColorBGRA(0, 0, 0, 255);
            AreaTextColor = new ColorBGRA(140, 200, 255, 255);
            XPHTextColor = new ColorBGRA(220, 190, 130, 255);
            TimeLeftColor = new ColorBGRA(220, 190, 130, 255);
            FPSTextColor = new ColorBGRA(220, 190, 130, 255);
            TimerTextColor = new ColorBGRA(220, 190, 130, 255);
            LatencyTextColor = new ColorBGRA(220, 190, 130, 255);
        }

        public ToggleNode ShowInTown { get; set; }
        public ToggleNode ShowLatency { get; set; }
        public ToggleNode OnlyAreaName { get; set; }
        public RangeNode<int> TextSize { get; set; }
        public ColorNode BackgroundColor { get; set; }
        public ColorNode AreaTextColor { get; set; }
        public ColorNode XPHTextColor { get; set; }
        public ColorNode TimeLeftColor { get; set; }
        public ColorNode FPSTextColor { get; set; }
        public ColorNode TimerTextColor { get; set; }
        public ColorNode LatencyTextColor { get; set; }
    }
}
