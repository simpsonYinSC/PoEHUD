using PoEHUD.HUD.Settings;
using SharpDX;

namespace PoEHUD.HUD.AdvancedTooltip
{
    public sealed class WeaponDPSSettings : SettingsBase
    {
        public WeaponDPSSettings()
        {
            Enable = true;
            TextColor = new ColorBGRA(254, 192, 118, 255);
            DPSTextSize = new RangeNode<int>(16, 10, 50);
            DPSNameTextSize = new RangeNode<int>(13, 10, 50);
            BackgroundColor = new ColorBGRA(255, 255, 0, 255);
            ColdDamageColor = new ColorBGRA(54, 100, 146, 255);
            FireDamageColor = new ColorBGRA(150, 0, 0, 255);
            LightningDamageColor = new ColorBGRA(255, 215, 0, 255);
            ChaosDamageColor = new ColorBGRA(208, 31, 144, 255);
            PhysicalDamageColor = new ColorBGRA(255, 255, 255, 255);
            ElementalDamageColor = new ColorBGRA(0, 255, 255, 255);
        }

        public ColorNode TextColor { get; set; }
        public RangeNode<int> DPSTextSize { get; set; }
        public RangeNode<int> DPSNameTextSize { get; set; }
        public ColorNode BackgroundColor { get; set; }
        public ColorNode FireDamageColor { get; set; }
        public ColorNode ColdDamageColor { get; set; }
        public ColorNode LightningDamageColor { get; set; }
        public ColorNode ChaosDamageColor { get; set; }
        public ColorNode PhysicalDamageColor { get; set; }
        public ColorNode ElementalDamageColor { get; set; }
    }
}
