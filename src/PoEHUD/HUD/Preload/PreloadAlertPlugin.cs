using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using PoEHUD.Controllers;
using PoEHUD.Framework;
using PoEHUD.Framework.Helpers;
using PoEHUD.HUD.Settings;
using PoEHUD.HUD.UI;
using PoEHUD.Models;
using SharpDX;
using SharpDX.Direct3D9;

namespace PoEHUD.HUD.Preload
{
    public class PreloadAlertPlugin : SizedPlugin<PreloadAlertSettings>
    {
        public static Color AreaNameColor;
        public static Dictionary<string, PreloadConfigLine> Essences;
        public static Dictionary<string, PreloadConfigLine> PerandusLeague;
        public static Dictionary<string, PreloadConfigLine> Strongboxes;
        public static Dictionary<string, PreloadConfigLine> Preload;

        private readonly SettingsHub settingsHub;
        private readonly HashSet<PreloadConfigLine> alerts;
        private readonly Dictionary<string, PreloadConfigLine> alertStrings;
        private bool foundSpecificPerandusChest;
        private bool essenceFound;
        private bool holdKey;
        private bool isAreaChanged;

        public PreloadAlertPlugin(GameController gameController, Graphics graphics, PreloadAlertSettings settings, SettingsHub settingsHub) : base(gameController, graphics, settings)
        {
            this.settingsHub = settingsHub;
            alerts = new HashSet<PreloadConfigLine>();
            alertStrings = LoadConfig("config/preload_alerts.txt");
            GameController.Area.AreaChanged += OnAreaChange;
            AreaNameColor = Settings.AreaTextColor;
            SetupPredefinedConfigs();
        }

        // Can be used by external plugins:
        public static event Action<HashSet<PreloadConfigLine>> ExternalPreloadResult = delegate { };

        public static Dictionary<string, PreloadConfigLine> LoadConfig(string path)
        {
            return LoadConfigBase(path, 3).ToDictionary(line => line[0], line =>
            {
                var preloadAlerConfigLine = new PreloadConfigLine
                {
                    Text = line[1],
                    Color = line.ConfigColorValueExtractor(2)
                };
                return preloadAlerConfigLine;
            });
        }

        public override void Render()
        {
            // do a full refresh if F5 is hit
            if (WindowsAPI.IsKeyDown(Keys.F5))
            {
                ResetArea();
                Parse();
            }

            if (!holdKey && WindowsAPI.IsKeyDown(Keys.F10))
            {
                holdKey = true;
                Settings.Enable.Value = !Settings.Enable.Value;
                SettingsHub.Save(settingsHub);
            }
            else if (holdKey && !WindowsAPI.IsKeyDown(Keys.F10))
            {
                holdKey = false;
            }

            if (!Settings.Enable || GameController.Area.CurrentArea.IsTown)
            {
                Size = new Size2F();
                return;
            }

            if (alerts.Count <= 0)
            {
                Size = new Size2F();
                return;
            }
            
            if (isAreaChanged)
            {
                ResetArea();
                Parse();
                isAreaChanged = false;
            }

            Vector2 startPosition = StartDrawPointFunc();
            Vector2 position = startPosition;
            int maxWidth = 0;
            foreach (Size2 size in alerts.Select(preloadConfigLine => Graphics.DrawText(preloadConfigLine.Text, Settings.TextSize, position, preloadConfigLine.FastColor?.Invoke() ?? preloadConfigLine.Color ?? Settings.DefaultTextColor, FontDrawFlags.Right)))
            {
                maxWidth = Math.Max(size.Width, maxWidth);
                position.Y += size.Height;
            }

            if (maxWidth <= 0)
            {
                return;
            }

            var bounds = new RectangleF(startPosition.X - maxWidth - 45, startPosition.Y - 5, maxWidth + 50, position.Y - startPosition.Y + 10);
            Graphics.DrawImage("preload-start.png", bounds, Settings.BackgroundColor);
            Graphics.DrawImage("preload-end.png", bounds, Settings.BackgroundColor);
            Size = bounds.Size;
            Margin = new Vector2(0, 5);
        }

        private void SetupPredefinedConfigs()
        {
            Essences = new Dictionary<string, PreloadConfigLine>
            {
                { "Metadata/Monsters/Daemon/EssenceDaemonMeteorFirestorm", new PreloadConfigLine { Text = "Essence of Anguish", FastColor = () => Settings.EssenceOfAnguish } },
                { "Metadata/Monsters/Daemon/EssenceDaemonFirePulse", new PreloadConfigLine { Text = "Essence of Anger", FastColor = () => Settings.EssenceOfAnger } },
                { "Metadata/Monsters/Daemon/EssenceDaemonColdPulse", new PreloadConfigLine { Text = "Essence of Hatred", FastColor = () => Settings.EssenceOfHatred } },
                { "Metadata/Monsters/Daemon/EssenceDaemonLightningPulse", new PreloadConfigLine { Text = "Essence of Wrath", FastColor = () => Settings.EssenceOfWrath } },
                { "Metadata/Monsters/Daemon/EssenceDaemonChaosDegenPulse", new PreloadConfigLine { Text = "Essence of Misery", FastColor = () => Settings.EssenceOfMisery } },
                { "Metadata/Monsters/Daemon/EssenceDaemonSummonOrbOfStormsDaemon", new PreloadConfigLine { Text = "Essence of Torment", FastColor = () => Settings.EssenceOfTorment } },
                { "Metadata/Monsters/Daemon/EssenceDaemonSummonGhost", new PreloadConfigLine { Text = "Essence of Fear", FastColor = () => Settings.EssenceOfFear } },
                { "Metadata/Monsters/Daemon/EssenceDaemonFrostBomb", new PreloadConfigLine { Text = "Essence of Suffering", FastColor = () => Settings.EssenceOfSuffering } },
                { "Metadata/Monsters/Daemon/EssenceDaemonGrab", new PreloadConfigLine { Text = "Essence of Envy", FastColor = () => Settings.EssenceOfEnvy } },
                { "Metadata/Monsters/Daemon/EssenceDaemonBuffToParentCasterDodge", new PreloadConfigLine { Text = "Essence of Zeal", FastColor = () => Settings.EssenceOfZeal } },
                { "Metadata/Monsters/Daemon/EssenceDaemonBuffToParentCasterDamageTaken", new PreloadConfigLine { Text = "Essence of Loathing", FastColor = () => Settings.EssenceOfLoathing } },
                { "Metadata/Monsters/Daemon/EssenceDaemonBuffToParentCasterCrit", new PreloadConfigLine { Text = "Essence of Scorn", FastColor = () => Settings.EssenceOfScorn } },
                { "Metadata/Monsters/Daemon/EssenceDaemonTotemGroundEffectVortex", new PreloadConfigLine { Text = "Essence of Sorrow", FastColor = () => Settings.EssenceOfSorrow } },
                { "Metadata/Monsters/Daemon/EssenceDaemonSummonKaruiSpirit", new PreloadConfigLine { Text = "Essence of Contempt", FastColor = () => Settings.EssenceOfContempt } },
                { "Metadata/Monsters/Daemon/EssenceDaemonFireRuneTrap", new PreloadConfigLine { Text = "Essence of Rage", FastColor = () => Settings.EssenceOfRage } },
                { "Metadata/Monsters/Daemon/EssenceDaemonSummonChaosGolem", new PreloadConfigLine { Text = "Essence of Dread", FastColor = () => Settings.EssenceOfDread } },
                { "Metadata/Monsters/Daemon/EssenceDaemonBloodProjectileDaemon", new PreloadConfigLine { Text = "Essence of Greed", FastColor = () => Settings.EssenceOfGreed } },
                { "Metadata/Monsters/Daemon/EssenceDaemonSummonLivingCrystals", new PreloadConfigLine { Text = "Essence of Woe", FastColor = () => Settings.EssenceOfWoe } },
                { "Metadata/Monsters/Daemon/EssenceDaemonSummonSpiders", new PreloadConfigLine { Text = "Essence of Doubt", FastColor = () => Settings.EssenceOfDoubt } },
                { "Metadata/Monsters/Daemon/EssenceDaemonTotemGroundEffectShocked", new PreloadConfigLine { Text = "Essence of Spite", FastColor = () => Settings.EssenceOfSpite } },
                { "Metadata/Monsters/Daemon/EssenceDaemonMadness1", new PreloadConfigLine { Text = "Essence of Hysteria", FastColor = () => Settings.EssenceOfHysteria } },
                { "Metadata/Monsters/Daemon/EssenceDaemonInsanity1", new PreloadConfigLine { Text = "Essence of Insanity", FastColor = () => Settings.EssenceOfInsanity } },
                { "Metadata/Monsters/Daemon/EssenceDaemonHorror1", new PreloadConfigLine { Text = "Essence of Horror", FastColor = () => Settings.EssenceOfHorror } },
                { "Metadata/Monsters/Daemon/EssenceDaemonTerror1_", new PreloadConfigLine { Text = "Essence of Delirium", FastColor = () => Settings.EssenceOfDelirium } }
            };

            PerandusLeague = new Dictionary<string, PreloadConfigLine>
            {
                { "Metadata/NPC/League/Cadiro", new PreloadConfigLine { Text = "Cadiro Trader", FastColor = () => Settings.CadiroTrader } },
                { "Metadata/Chests/PerandusChests/PerandusChestStandard", new PreloadConfigLine { Text = "Perandus Chest", FastColor = () => Settings.PerandusChestStandard } },
                { "Metadata/Chests/PerandusChests/PerandusChestRarity", new PreloadConfigLine { Text = "Perandus Cache", FastColor = () => Settings.PerandusChestRarity } },
                { "Metadata/Chests/PerandusChests/PerandusChestQuantity", new PreloadConfigLine { Text = "Perandus Hoard", FastColor = () => Settings.PerandusChestQuantity } },
                { "Metadata/Chests/PerandusChests/PerandusChestCoins", new PreloadConfigLine { Text = "Perandus Coffer", FastColor = () => Settings.PerandusChestCoins } },
                { "Metadata/Chests/PerandusChests/PerandusChestJewellery", new PreloadConfigLine { Text = "Perandus Jewellery Box", FastColor = () => Settings.PerandusChestJewellery } },
                { "Metadata/Chests/PerandusChests/PerandusChestGems", new PreloadConfigLine { Text = "Perandus Safe", FastColor = () => Settings.PerandusChestGems } },
                { "Metadata/Chests/PerandusChests/PerandusChestCurrency", new PreloadConfigLine { Text = "Perandus Treasury", FastColor = () => Settings.PerandusChestCurrency } },
                { "Metadata/Chests/PerandusChests/PerandusChestInventory", new PreloadConfigLine { Text = "Perandus Wardrobe", FastColor = () => Settings.PerandusChestInventory } },
                { "Metadata/Chests/PerandusChests/PerandusChestDivinationCards", new PreloadConfigLine { Text = "Perandus Catalogue", FastColor = () => Settings.PerandusChestDivinationCards } },
                { "Metadata/Chests/PerandusChests/PerandusChestKeepersOfTheTrove", new PreloadConfigLine { Text = "Perandus Trove", FastColor = () => Settings.PerandusChestKeepersOfTheTrove } },
                { "Metadata/Chests/PerandusChests/PerandusChestUniqueItem", new PreloadConfigLine { Text = "Perandus Locker", FastColor = () => Settings.PerandusChestUniqueItem } },
                { "Metadata/Chests/PerandusChests/PerandusChestMaps", new PreloadConfigLine { Text = "Perandus Archive", FastColor = () => Settings.PerandusChestMaps } },
                { "Metadata/Chests/PerandusChests/PerandusChestFishing", new PreloadConfigLine { Text = "Perandus Tackle Box", FastColor = () => Settings.PerandusChestFishing } },
                { "Metadata/Chests/PerandusChests/PerandusManorUniqueChest", new PreloadConfigLine { Text = "Cadiro's Locker", FastColor = () => Settings.PerandusManorUniqueChest } },
                { "Metadata/Chests/PerandusChests/PerandusManorCurrencyChest", new PreloadConfigLine { Text = "Cadiro's Treasury", FastColor = () => Settings.PerandusManorCurrencyChest } },
                { "Metadata/Chests/PerandusChests/PerandusManorMapsChest", new PreloadConfigLine { Text = "Cadiro's Archive", FastColor = () => Settings.PerandusManorMapsChest } },
                { "Metadata/Chests/PerandusChests/PerandusManorJewelryChest", new PreloadConfigLine { Text = "Cadiro's Jewellery Box", FastColor = () => Settings.PerandusManorJewelryChest } },
                { "Metadata/Chests/PerandusChests/PerandusManorDivinationCardsChest", new PreloadConfigLine { Text = "Cadiro's Catalogue", FastColor = () => Settings.PerandusManorDivinationCardsChest } },
                { "Metadata/Chests/PerandusChests/PerandusManorLostTreasureChest", new PreloadConfigLine { Text = "Grand Perandus Vault", FastColor = () => Settings.PerandusManorLostTreasureChest } }
            };

            Strongboxes = new Dictionary<string, PreloadConfigLine>
            {
                { "Metadata/Chests/StrongBoxes/Arcanist", new PreloadConfigLine { Text = "Arcanist's Strongbox", FastColor = () => Settings.ArcanistStrongbox } },
                { "Metadata/Chests/StrongBoxes/Artisan", new PreloadConfigLine { Text = "Artisan's Strongbox", FastColor = () => Settings.ArtisanStrongbox } },
                { "Metadata/Chests/StrongBoxes/Cartographer", new PreloadConfigLine { Text = "Cartographer's Strongbox", FastColor = () => Settings.CartographerStrongbox } },
                { "Metadata/Chests/StrongBoxes/Diviner", new PreloadConfigLine { Text = "Diviner's Strongbox", FastColor = () => Settings.DivinerStrongbox } },
                { "Metadata/Chests/StrongBoxes/StrongboxDivination", new PreloadConfigLine { Text = "Diviner's Strongbox", FastColor = () => Settings.DivinerStrongbox } },
                { "Metadata/Chests/StrongBoxes/Gemcutter", new PreloadConfigLine { Text = "Gemcutter's Strongbox", FastColor = () => Settings.GemcutterStrongbox } },
                { "Metadata/Chests/StrongBoxes/Jeweller", new PreloadConfigLine { Text = "Jeweller's Strongbox", FastColor = () => Settings.JewellerStrongbox } },
                { "Metadata/Chests/StrongBoxes/Arsenal", new PreloadConfigLine { Text = "Blacksmith's Strongbox", FastColor = () => Settings.BlacksmithStrongbox } },
                { "Metadata/Chests/StrongBoxes/Armory", new PreloadConfigLine { Text = "Armourer's Strongbox", FastColor = () => Settings.ArmourerStrongbox } },
                { "Metadata/Chests/StrongBoxes/Ornate", new PreloadConfigLine { Text = "Ornate Strongbox", FastColor = () => Settings.OrnateStrongbox } },
                { "Metadata/Chests/StrongBoxes/Large", new PreloadConfigLine { Text = "Large Strongbox", FastColor = () => Settings.LargeStrongbox } },
                { "Metadata/Chests/StrongBoxes/Strongbox", new PreloadConfigLine { Text = "Simple Strongbox", FastColor = () => Settings.SimpleStrongbox } },
                { "Metadata/Chests/CopperChests/CopperChestEpic3", new PreloadConfigLine { Text = "Epic Chest", FastColor = () => Settings.EpicStrongbox } },
                { "Metadata/Chests/StrongBoxes/PerandusBox", new PreloadConfigLine { Text = "Perandus Strongbox", FastColor = () => Settings.PerandusStrongbox } },
                { "Metadata/Chests/StrongBoxes/KaomBox", new PreloadConfigLine { Text = "Kaom Strongbox", FastColor = () => Settings.KaomStrongbox } },
                { "Metadata/Chests/StrongBoxes/MalachaisBox", new PreloadConfigLine { Text = "Malachai Strongbox", FastColor = () => Settings.MalachaiStrongbox } }
            };

            Preload = new Dictionary<string, PreloadConfigLine>
            {
                { "Wild/StrDexInt", new PreloadConfigLine { Text = "Zana, Master Cartographer", FastColor = () => Settings.MasterZana } },
                { "Wild/Int", new PreloadConfigLine { Text = "Catarina, Master of the Dead", FastColor = () => Settings.MasterCatarina } },
                { "Wild/Dex", new PreloadConfigLine { Text = "Tora, Master of the Hunt", FastColor = () => Settings.MasterTora } },
                { "Wild/DexInt", new PreloadConfigLine { Text = "Vorici, Master Assassin", FastColor = () => Settings.MasterVorici } },
                { "Wild/Str", new PreloadConfigLine { Text = "Haku, Armourmaster", FastColor = () => Settings.MasterHaku } },
                { "Wild/StrInt", new PreloadConfigLine { Text = "Elreon, Loremaster", FastColor = () => Settings.MasterElreon } },
                { "Wild/Fish", new PreloadConfigLine { Text = "Krillson, Master Fisherman", FastColor = () => Settings.MasterKrillson } },
                { "MasterStrDex1", new PreloadConfigLine { Text = "Vagan, Weaponmaster (2HSword)", FastColor = () => Settings.MasterVagan } },
                { "MasterStrDex2", new PreloadConfigLine { Text = "Vagan, Weaponmaster (Staff)", FastColor = () => Settings.MasterVagan } },
                { "MasterStrDex3", new PreloadConfigLine { Text = "Vagan, Weaponmaster (Bow)", FastColor = () => Settings.MasterVagan } },
                { "MasterStrDex4", new PreloadConfigLine { Text = "Vagan, Weaponmaster (DaggerRapier)", FastColor = () => Settings.MasterVagan } },
                { "MasterStrDex5", new PreloadConfigLine { Text = "Vagan, Weaponmaster (Blunt)", FastColor = () => Settings.MasterVagan } },
                { "MasterStrDex6", new PreloadConfigLine { Text = "Vagan, Weaponmaster (Blades)", FastColor = () => Settings.MasterVagan } },
                { "MasterStrDex7", new PreloadConfigLine { Text = "Vagan, Weaponmaster (SwordAxe)", FastColor = () => Settings.MasterVagan } },
                { "MasterStrDex8", new PreloadConfigLine { Text = "Vagan, Weaponmaster (Punching)", FastColor = () => Settings.MasterVagan } },
                { "MasterStrDex9", new PreloadConfigLine { Text = "Vagan, Weaponmaster (Flickerstrike)", FastColor = () => Settings.MasterVagan } },
                { "MasterStrDex10", new PreloadConfigLine { Text = "Vagan, Weaponmaster (Elementalist)", FastColor = () => Settings.MasterVagan } },
                { "MasterStrDex11", new PreloadConfigLine { Text = "Vagan, Weaponmaster (Cyclone)", FastColor = () => Settings.MasterVagan } },
                { "MasterStrDex12", new PreloadConfigLine { Text = "Vagan, Weaponmaster (PhysSpells)", FastColor = () => Settings.MasterVagan } },
                { "MasterStrDex13", new PreloadConfigLine { Text = "Vagan, Weaponmaster (Traps)", FastColor = () => Settings.MasterVagan } },
                { "MasterStrDex14", new PreloadConfigLine { Text = "Vagan, Weaponmaster (RighteousFire)", FastColor = () => Settings.MasterVagan } },
                { "MasterStrDex15", new PreloadConfigLine { Text = "Vagan, Weaponmaster (CastOnHit)", FastColor = () => Settings.MasterVagan } },
                { "ExileDuelist1", new PreloadConfigLine { Text = "Exile Torr Olgosso", FastColor = () => Settings.TorrOlgosso } },
                { "ExileDuelist2", new PreloadConfigLine { Text = "Exile Armios Bell", FastColor = () => Settings.ArmiosBell } },
                { "ExileDuelist4", new PreloadConfigLine { Text = "Exile Zacharie Desmarais", FastColor = () => Settings.ZacharieDesmarais } },
                { "ExileDuelist5", new PreloadConfigLine { Text = "Exile Oyra Ona", FastColor = () => Settings.OyraOna } },
                { "ExileMarauder1", new PreloadConfigLine { Text = "Exile Jonah Unchained", FastColor = () => Settings.JonahUnchained } },
                { "ExileMarauder2", new PreloadConfigLine { Text = "Exile Damoi Tui", FastColor = () => Settings.DamoiTui } },
                { "ExileMarauder3", new PreloadConfigLine { Text = "Exile Xandro Blooddrinker", FastColor = () => Settings.XandroBlooddrinker } },
                { "ExileMarauder5", new PreloadConfigLine { Text = "Exile Vickas Giantbone", FastColor = () => Settings.VickasGiantbone } },
                { "ExileMarauder6__", new PreloadConfigLine { Text = "Exile Bolt Brownfur", FastColor = () => Settings.BoltBrownfur } },
                { "ExileRanger1", new PreloadConfigLine { Text = "Exile Orra Greengate", FastColor = () => Settings.OrraGreengate } },
                { "ExileRanger2", new PreloadConfigLine { Text = "Exile Thena Moga", FastColor = () => Settings.ThenaMoga } },
                { "ExileRanger3", new PreloadConfigLine { Text = "Exile Antalie Napora", FastColor = () => Settings.AntalieNapora } },
                { "ExileRanger5", new PreloadConfigLine { Text = "Exile Ailentia Rac", FastColor = () => Settings.AilentiaRac } },
                { "ExileScion2", new PreloadConfigLine { Text = "Exile Augustina Solaria", FastColor = () => Settings.AugustinaSolaria } },
                { "ExileScion3", new PreloadConfigLine { Text = "Exile Lael Furia", FastColor = () => Settings.LaelFuria } },
                { "ExileScion4", new PreloadConfigLine { Text = "Exile Vanth Agiel", FastColor = () => Settings.VanthAgiel } },
                { "ExileShadow1_", new PreloadConfigLine { Text = "Exile Ion Darkshroud", FastColor = () => Settings.IonDarkshroud } },
                { "ExileShadow2", new PreloadConfigLine { Text = "Exile Ash Lessard", FastColor = () => Settings.AshLessard } },
                { "ExileShadow4", new PreloadConfigLine { Text = "Exile Wilorin Demontamer", FastColor = () => Settings.WilorinDemontamer } },
                { "ExileShadow5", new PreloadConfigLine { Text = "Exile Ulysses Morvant", FastColor = () => Settings.UlyssesMorvant } },
                { "ExileTemplar1", new PreloadConfigLine { Text = "Exile Eoin Greyfur", FastColor = () => Settings.EoinGreyfur } },
                { "ExileTemplar2", new PreloadConfigLine { Text = "Exile Tinevin Highdove", FastColor = () => Settings.TinevinHighdove } },
                { "ExileTemplar4", new PreloadConfigLine { Text = "Exile Magnus Stonethorn", FastColor = () => Settings.MagnusStonethorn } },
                { "ExileTemplar5", new PreloadConfigLine { Text = "Exile Aurelio Voidsinger", FastColor = () => Settings.AurelioVoidsinger } },
                { "ExileWitch1", new PreloadConfigLine { Text = "Exile Minara Anenima", FastColor = () => Settings.MinaraAnenima } },
                { "ExileWitch2", new PreloadConfigLine { Text = "Exile Igna Phoenix", FastColor = () => Settings.IgnaPhoenix } },
                { "ExileWitch4", new PreloadConfigLine { Text = "Exile Dena Lorenni", FastColor = () => Settings.DenaLorenni } }
            };
        }

        private void ResetArea()
        {
            alerts.Clear();
            AreaNameColor = Settings.AreaTextColor;
            foundSpecificPerandusChest = false;
        }

        private void OnAreaChange(AreaController area)
        {
            ResetArea();
            essenceFound = false;
            Parse();
            isAreaChanged = true;
        }

        private void Parse()
        {
            Memory memory = GameController.Memory;
            long ptrFileRoot = memory.AddressOfProcess + memory.Offset.FileRoot;
            int count = memory.ReadInt(ptrFileRoot + 0x10); // check how many files are loaded

            int areaChangeCount = GameController.Game.AreaChangeCount;
            long listIterator = memory.ReadLong(ptrFileRoot + 0x8, 0x0);
            
            List<string> preloadStrings = new List<string>();

            for (int i = 0; i < count; i++)
            {
                listIterator = memory.ReadLong(listIterator);
                if (listIterator == 0)
                {
                    //// MessageBox.Show("address is null, something has gone wrong, start over");
                    // address is null, something has gone wrong, start over
                    return;
                }

                if (memory.ReadLong(listIterator + 0x10) == 0 || memory.ReadInt(listIterator + 0x18, 0x50) != areaChangeCount)
                {
                    continue;
                }
                
                string text = memory.ReadStringU(memory.ReadLong(listIterator + 0x10), 512);

                if (text.Contains('@'))
                {
                    text = text.Split('@')[0];
                }

                preloadStrings.Add(text);
            }

            preloadStrings.Sort();

            foreach (string strings in preloadStrings)
            {
                CheckForPreload(strings);
            }

            ExternalPreloadResult(alerts);
        }

        private void CheckForPreload(string text)
        {
            if (alertStrings.ContainsKey(text))
            {
                alerts.Add(alertStrings[text]);
                return;
            }

            if (text.Contains("Metadata/Terrain/Doodads/vaal_sidearea_effects/soulcoaster.ao"))
            {
                if (Settings.CorruptedTitle)
                {
                    // using corrupted titles so set the color here, XPRatePlugin will grab the color to use when drawing the title.
                    AreaNameColor = Settings.CorruptedAreaColor;
                }
                else
                {
                    // not using corrupted titles, so throw it in a preload alert
                    alerts.Add(new PreloadConfigLine { Text = "Corrupted Area", FastColor = () => Settings.CorruptedAreaColor });
                }

                return;
            }

            if (Settings.Essence)
            {
                PreloadConfigLine essenceAlert = Essences.Where(kv => text.StartsWith(kv.Key, StringComparison.OrdinalIgnoreCase)).Select(kv => kv.Value).FirstOrDefault();
                if (essenceAlert != null)
                {
                    essenceFound = true;
                    if (alerts.Contains(new PreloadConfigLine { Text = "Remnant of Corruption", FastColor = () => Settings.RemnantOfCorruption }))
                    {
                        alerts.Remove(new PreloadConfigLine { Text = "Remnant of Corruption", FastColor = () => Settings.RemnantOfCorruption });
                    }

                    alerts.Add(essenceAlert);
                    return;
                }

                if (!essenceFound && text.Contains("MiniMonolith"))
                {
                    alerts.Add(new PreloadConfigLine { Text = "Remnant of Corruption", FastColor = () => Settings.RemnantOfCorruption });
                }
            }

            PreloadConfigLine perandusAlert = PerandusLeague.Where(kv => text.StartsWith(kv.Key, StringComparison.OrdinalIgnoreCase)).Select(kv => kv.Value).FirstOrDefault();
            if (perandusAlert != null && Settings.PerandusBoxes)
            {
                foundSpecificPerandusChest = true;
                if (alerts.Contains(new PreloadConfigLine { Text = "Unknown Perandus Chest", FastColor = () => Settings.PerandusChestStandard }))
                {
                    alerts.Remove(new PreloadConfigLine { Text = "Unknown Perandus Chest", FastColor = () => Settings.PerandusChestStandard });
                }

                alerts.Add(perandusAlert);
                return;
            }

            if (Settings.PerandusBoxes && !foundSpecificPerandusChest && text.StartsWith("Metadata/Chests/PerandusChests"))
            {
                alerts.Add(new PreloadConfigLine { Text = "Unknown Perandus Chest", FastColor = () => Settings.PerandusChestStandard });
            }

            PreloadConfigLine alert = Strongboxes.Where(kv => text.StartsWith(kv.Key, StringComparison.OrdinalIgnoreCase)).Select(kv => kv.Value).FirstOrDefault();
            if (alert != null && Settings.Strongboxes)
            {
                alerts.Add(alert);
                return;
            }

            alert = Preload.Where(kv => text.EndsWith(kv.Key, StringComparison.OrdinalIgnoreCase)).Select(kv => kv.Value).FirstOrDefault();
            if (alert != null && Settings.Exiles)
            {
                alerts.Add(alert);
                return;
            }
        }
    }
}
