using PoEHUD.HUD.Settings;

namespace PoEHUD.HUD.Icons
{
    public sealed class MapIconsSettings : SettingsBase
    {
        public MapIconsSettings()
        {
            Enable = true;
            IconsOnMinimap = true;
            IconsOnLargeMap = true;
        }

        public ToggleNode IconsOnMinimap { get; set; }
        public ToggleNode IconsOnLargeMap { get; set; }
    }
}
