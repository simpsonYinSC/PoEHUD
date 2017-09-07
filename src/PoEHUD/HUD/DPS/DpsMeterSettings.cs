using PoEHUD.HUD.Settings;
using SharpDX;

namespace PoEHUD.HUD.DPS
{
    public sealed class DPSMeterSettings : SettingsBase
    {
        public DPSMeterSettings()
        {
            Enable = false;
            ShowInTown = false;
            DPSTextSize = new RangeNode<int>(16, 10, 20);
            PeakDPSTextSize = new RangeNode<int>(16, 10, 20);
            DPSFontColor = new ColorBGRA(220, 190, 130, 255);
            PeakFontColor = new ColorBGRA(220, 190, 130, 255);
            BackgroundColor = new ColorBGRA(0, 0, 0, 255);
        }

        public ToggleNode ShowInTown { get; set; }
        public RangeNode<int> DPSTextSize { get; set; }
        public RangeNode<int> PeakDPSTextSize { get; set; }
        public ColorNode DPSFontColor { get; set; }
        public ColorNode PeakFontColor { get; set; }
        public ColorNode BackgroundColor { get; set; }
    }
}
