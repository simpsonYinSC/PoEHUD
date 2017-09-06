using System;
using System.IO;
using Newtonsoft.Json;
using PoEHUD.HUD.AdvancedTooltip;
using PoEHUD.HUD.DPS;
using PoEHUD.HUD.Health;
using PoEHUD.HUD.Icons;
using PoEHUD.HUD.KillCounter;
using PoEHUD.HUD.Loot;
using PoEHUD.HUD.Menu;
using PoEHUD.HUD.Preload;
using PoEHUD.HUD.Settings.Converters;
using PoEHUD.HUD.Trackers;
using PoEHUD.HUD.XPRate;

namespace PoEHUD.HUD.Settings
{
    public sealed class SettingsHub
    {
        public static readonly JsonSerializerSettings JsonSettings;
        private const string SettingsFileName = "config/settings.json";

        static SettingsHub()
        {
            JsonSettings = new JsonSerializerSettings
            {
                ContractResolver = new SortContractResolver(),
                Converters = new JsonConverter[]
                {
                    new ColorNodeConverter(),
                    new ToggleNodeConverter(),
                    new FileNodeConverter()
                }
            };
        }

        public SettingsHub()
        {
            MenuSettings = new MenuSettings();
            DPSMeterSettings = new DPSMeterSettings();
            MapIconsSettings = new MapIconsSettings();
            ItemAlertSettings = new ItemAlertSettings();
            AdvancedTooltipSettings = new AdvancedTooltipSettings();
            MonsterTrackerSettings = new MonsterTrackerSettings();
            PoITrackerSettings = new PoITrackerSettings();
            PreloadAlertSettings = new PreloadAlertSettings();
            XPRateSettings = new XPRateSettings();
            HealthBarSettings = new HealthBarSettings();
            KillCounterSettings = new KillCounterSettings();
        }

        [JsonProperty("Menu")]
        public MenuSettings MenuSettings { get; private set; }

        [JsonProperty("DPS meter")]
        public DPSMeterSettings DPSMeterSettings { get; private set; }

        [JsonProperty("Map icons")]
        public MapIconsSettings MapIconsSettings { get; private set; }

        [JsonProperty("Item alert")]
        public ItemAlertSettings ItemAlertSettings { get; private set; }

        [JsonProperty("Advanced tooltip")]
        public AdvancedTooltipSettings AdvancedTooltipSettings { get; private set; }

        [JsonProperty("Monster tracker")]
        public MonsterTrackerSettings MonsterTrackerSettings { get; private set; }

        [JsonProperty("PoI tracker")]
        public PoITrackerSettings PoITrackerSettings { get; private set; }

        [JsonProperty("Preload alert")]
        public PreloadAlertSettings PreloadAlertSettings { get; private set; }

        [JsonProperty("XP per hour")]
        public XPRateSettings XPRateSettings { get; private set; }

        [JsonProperty("Health bar")]
        public HealthBarSettings HealthBarSettings { get; private set; }

        [JsonProperty("Kills Counter")]
        public KillCounterSettings KillCounterSettings { get; private set; }

        public static SettingsHub Load()
        {
            try
            {
                string json = File.ReadAllText(SettingsFileName);
                return JsonConvert.DeserializeObject<SettingsHub>(json, JsonSettings);
            }
            catch
            {
                if (File.Exists(SettingsFileName))
                {
                    string backupFileName = SettingsFileName + DateTime.Now.Ticks;
                    File.Move(SettingsFileName, backupFileName);
                }

                var settings = new SettingsHub();
                Save(settings);
                return settings;
            }
        }

        public static void Save(SettingsHub settings)
        {
            using (var stream = new StreamWriter(File.Create(SettingsFileName)))
            {
                string json = JsonConvert.SerializeObject(settings, Formatting.Indented, JsonSettings);
                stream.Write(json);
            }
        }
    }
}
