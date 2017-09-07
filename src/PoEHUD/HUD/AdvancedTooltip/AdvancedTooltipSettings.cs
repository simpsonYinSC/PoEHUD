using Newtonsoft.Json;

using PoEHUD.HUD.Settings;

namespace PoEHUD.HUD.AdvancedTooltip
{
    public class AdvancedTooltipSettings : SettingsBase
    {
        public AdvancedTooltipSettings()
        {
            Enable = true;
            ItemLevel = new ItemLevelSettings();
            ItemMods = new ItemModsSettings();
            WeaponDPS = new WeaponDPSSettings();
        }

        [JsonProperty("Item level")]
        public ItemLevelSettings ItemLevel { get; set; }

        [JsonProperty("Item mods")]
        public ItemModsSettings ItemMods { get; set; }

        [JsonProperty("Weapon DPS")]
        public WeaponDPSSettings WeaponDPS { get; set; }
    }
}
