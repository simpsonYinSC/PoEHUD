using System;
using System.Linq;
using System.Windows.Forms;
using PoEHUD.Controllers;
using PoEHUD.Framework;
using PoEHUD.Framework.InputHooks;
using PoEHUD.HUD.Health;
using PoEHUD.HUD.Loot;
using PoEHUD.HUD.Settings;
using PoEHUD.HUD.UI;
using SharpDX;

namespace PoEHUD.HUD.Menu
{
    public class MenuPlugin : Plugin<MenuSettings>
    {
        public static RootButton MenuRootButton;

        /// <summary>
        /// Use this event if you want your mouse clicks to be handled by poehud and does not passed to the game
        /// </summary>
        public static Func<MouseEventId, Vector2, bool> ExternalMouseClick = delegate { return false; };

        /// <summary>
        /// Use this event if you want your mouse clicks to be handled by poehud and passed to the game
        /// </summary>
        public static Action<MouseEventId, Vector2> ExternalMouseEvent = delegate { };

        private readonly SettingsHub settingsHub;
        private readonly Action<MouseInfo> onMouseDown, onMouseUp, onMouseMove;
        private bool holdKey;

        public MenuPlugin(GameController gameController, Graphics graphics, SettingsHub settingsHub) : base(gameController, graphics, settingsHub.MenuSettings)
        {
            this.settingsHub = settingsHub;
            CreateMenu();
            MouseHook.OnMouseDown += onMouseDown = info => info.Handled = OnMouseEvent(MouseEventId.LeftButtonDown, info.Position);
            MouseHook.OnMouseUp += onMouseUp = info => info.Handled = OnMouseEvent(MouseEventId.LeftButtonUp, info.Position);
            MouseHook.OnMouseMove += onMouseMove = info => info.Handled = OnMouseEvent(MouseEventId.MouseMove, info.Position);
        }

        public static event Action<RootButton> ExternalInitMenu = delegate { }; // For spawning the menu in external plugins

        public static MenuItem AddChild(MenuItem parent, string text, ToggleNode node, string key = null, Func<MenuItem, bool> hide = null)
        {
            var item = new ToggleButton(parent, text, node, key, hide);
            parent.AddChild(item);
            return item;
        }

        public static MenuItem AddChild(MenuItem parent, string text)
        {
            var item = new SimpleMenu(parent, text);
            parent.AddChild(item);
            return item;
        }

        public static void AddChild(MenuItem parent, FileNode path)
        {
            var item = new FileButton(path);
            parent.AddChild(item);
        }

        public static MenuItem AddChild(MenuItem parent, string text, ColorNode node)
        {
            var item = new ColorButton(text, node);
            parent.AddChild(item);
            return item;
        }

        public static MenuItem AddChild<T>(MenuItem parent, string text, RangeNode<T> node) where T : struct
        {
            var item = new Picker<T>(text, node);
            parent.AddChild(item);
            return item;
        }

        public static MenuItem AddChild(MenuItem parent, string text, HotkeyNode node)
        {
            var item = new HotkeyButton(text, node);
            parent.AddChild(item);
            return item;
        }

        public static MenuItem AddChild(MenuItem parent, string text, ButtonNode node)
        {
            var item = new ButtonButton(text, node);
            parent.AddChild(item);
            return item;
        }

        public static ListButton AddChild(MenuItem parent, string text, ListNode node)
        {
            var item = new ListButton(text, node);
            parent.AddChild(item);
            //// item.StartInitItems(); // Can't be in ListButton.ctor coz first of all ListButton menu should be added to childs to initialise it's own bounds.
            node.SettingsListButton = item;
            return item;
        }

        public override void Dispose()
        {
            MouseHook.OnMouseDown -= onMouseDown;
            MouseHook.OnMouseUp -= onMouseUp;
            MouseHook.OnMouseMove -= onMouseMove;
        }

        public override void Render()
        {
            try
            {
                if (!holdKey && WindowsAPI.IsKeyDown(Keys.F12))
                {
                    holdKey = true;
                    Settings.Enable.Value = !Settings.Enable.Value;
                    SettingsHub.Save(settingsHub);
                }
                else if (holdKey && !WindowsAPI.IsKeyDown(Keys.F12))
                {
                    holdKey = false;
                }

                if (Settings.Enable)
                {
                    MenuRootButton.Render(Graphics, Settings);
                }
            }
            catch
            {
                // ignore
            }
        }

        private void CreateMenu()
        {
            MenuRootButton = new RootButton(new Vector2(Settings.X, Settings.Y));
            MenuRootButton.ExternalOnClose += delegate { SettingsHub.Save(settingsHub); };

            // Health bars
            HealthBarSettings healthBarPlugin = settingsHub.HealthBarSettings;
            MenuItem healthMenu = AddChild(MenuRootButton, "Health bars", healthBarPlugin.Enable);
            MenuItem playersMenu = AddChild(healthMenu, "Players", healthBarPlugin.Players.Enable);
            MenuItem enemiesMenu = AddChild(healthMenu, "Enemies", healthBarPlugin.ShowEnemies);
            MenuItem minionsMenu = AddChild(healthMenu, "Minions", healthBarPlugin.Minions.Enable);
            AddChild(healthMenu, "Show nrg shield", healthBarPlugin.ShowES);
            AddChild(healthMenu, "Show in town", healthBarPlugin.ShowInTown);
            MenuItem debuffPanelMenu = AddChild(healthMenu, "Show debuffs", healthBarPlugin.ShowDebuffPanel);
            AddChild(debuffPanelMenu, "Icon size", healthBarPlugin.DebuffPanelIconSize);
            AddChild(playersMenu, "Print percents", healthBarPlugin.Players.ShowPercents);
            AddChild(playersMenu, "Print health text", healthBarPlugin.Players.ShowHealthText);
            AddChild(playersMenu, "Width", healthBarPlugin.Players.Width);
            AddChild(playersMenu, "Height", healthBarPlugin.Players.Height);
            MenuItem playerDPSMenu = AddChild(playersMenu, "Floating combat text", healthBarPlugin.Players.ShowFloatingCombatDamage);
            AddChild(playerDPSMenu, "Text size", healthBarPlugin.Players.FloatingCombatTextSize);
            AddChild(playerDPSMenu, "Damage Color", healthBarPlugin.Players.FloatingCombatDamageColor);
            AddChild(playerDPSMenu, "Heal Color", healthBarPlugin.Players.FloatingCombatHealColor);
            AddChild(playerDPSMenu, "Number of lines", healthBarPlugin.Players.FloatingCombatStackSize);
            AddChild(minionsMenu, "Print percents", healthBarPlugin.Minions.ShowPercents);
            AddChild(minionsMenu, "Print health text", healthBarPlugin.Minions.ShowHealthText);
            AddChild(minionsMenu, "Width", healthBarPlugin.Minions.Width);
            AddChild(minionsMenu, "Height", healthBarPlugin.Minions.Height);
            MenuItem minionsDPSMenu = AddChild(minionsMenu, "Floating combat text", healthBarPlugin.Minions.ShowFloatingCombatDamage);
            AddChild(minionsDPSMenu, "Text size", healthBarPlugin.Minions.FloatingCombatTextSize);
            AddChild(minionsDPSMenu, "Damage color", healthBarPlugin.Minions.FloatingCombatDamageColor);
            AddChild(minionsDPSMenu, "Heal color", healthBarPlugin.Minions.FloatingCombatHealColor);
            AddChild(minionsDPSMenu, "Number of lines", healthBarPlugin.Minions.FloatingCombatStackSize);
            MenuItem whiteEnemyMenu = AddChild(enemiesMenu, "White", healthBarPlugin.NormalEnemy.Enable);
            AddChild(whiteEnemyMenu, "Print percents", healthBarPlugin.NormalEnemy.ShowPercents);
            AddChild(whiteEnemyMenu, "Print health text", healthBarPlugin.NormalEnemy.ShowHealthText);
            AddChild(whiteEnemyMenu, "Width", healthBarPlugin.NormalEnemy.Width);
            AddChild(whiteEnemyMenu, "Height", healthBarPlugin.NormalEnemy.Height);
            MenuItem whiteEnemyDPSMenu = AddChild(whiteEnemyMenu, "Floating combat text", healthBarPlugin.NormalEnemy.ShowFloatingCombatDamage);
            AddChild(whiteEnemyDPSMenu, "Text size", healthBarPlugin.NormalEnemy.FloatingCombatTextSize);
            AddChild(whiteEnemyDPSMenu, "Damage color", healthBarPlugin.NormalEnemy.FloatingCombatDamageColor);
            AddChild(whiteEnemyDPSMenu, "Heal color", healthBarPlugin.NormalEnemy.FloatingCombatHealColor);
            AddChild(whiteEnemyDPSMenu, "Number of lines", healthBarPlugin.NormalEnemy.FloatingCombatStackSize);
            MenuItem magicEnemyMenu = AddChild(enemiesMenu, "Magic", healthBarPlugin.MagicEnemy.Enable);
            AddChild(magicEnemyMenu, "Print percents", healthBarPlugin.MagicEnemy.ShowPercents);
            AddChild(magicEnemyMenu, "Print health text", healthBarPlugin.MagicEnemy.ShowHealthText);
            AddChild(magicEnemyMenu, "Width", healthBarPlugin.MagicEnemy.Width);
            AddChild(magicEnemyMenu, "Height", healthBarPlugin.MagicEnemy.Height);
            MenuItem magicEnemyDPSMenu = AddChild(magicEnemyMenu, "Floating combat text", healthBarPlugin.MagicEnemy.ShowFloatingCombatDamage);
            AddChild(magicEnemyDPSMenu, "Text size", healthBarPlugin.MagicEnemy.FloatingCombatTextSize);
            AddChild(magicEnemyDPSMenu, "Damage color", healthBarPlugin.MagicEnemy.FloatingCombatDamageColor);
            AddChild(magicEnemyDPSMenu, "Heal color", healthBarPlugin.MagicEnemy.FloatingCombatHealColor);
            AddChild(magicEnemyDPSMenu, "Number of lines", healthBarPlugin.MagicEnemy.FloatingCombatStackSize);
            MenuItem rareEnemyMenu = AddChild(enemiesMenu, "Rare", healthBarPlugin.RareEnemy.Enable);
            AddChild(rareEnemyMenu, "Print percents", healthBarPlugin.RareEnemy.ShowPercents);
            AddChild(rareEnemyMenu, "Print health text", healthBarPlugin.RareEnemy.ShowHealthText);
            AddChild(rareEnemyMenu, "Width", healthBarPlugin.RareEnemy.Width);
            AddChild(rareEnemyMenu, "Height", healthBarPlugin.RareEnemy.Height);
            MenuItem rareEnemyDPSMenu = AddChild(rareEnemyMenu, "Floating combat text", healthBarPlugin.RareEnemy.ShowFloatingCombatDamage);
            AddChild(rareEnemyDPSMenu, "Text size", healthBarPlugin.RareEnemy.FloatingCombatTextSize);
            AddChild(rareEnemyDPSMenu, "Damage color", healthBarPlugin.RareEnemy.FloatingCombatDamageColor);
            AddChild(rareEnemyDPSMenu, "Heal color", healthBarPlugin.RareEnemy.FloatingCombatHealColor);
            AddChild(rareEnemyDPSMenu, "Number of lines", healthBarPlugin.RareEnemy.FloatingCombatStackSize);
            MenuItem uniquesEnemyMenu = AddChild(enemiesMenu, "Uniques", healthBarPlugin.UniqueEnemy.Enable);
            AddChild(uniquesEnemyMenu, "Print percents", healthBarPlugin.UniqueEnemy.ShowPercents);
            AddChild(uniquesEnemyMenu, "Print health text", healthBarPlugin.UniqueEnemy.ShowHealthText);
            AddChild(uniquesEnemyMenu, "Width", healthBarPlugin.UniqueEnemy.Width);
            AddChild(uniquesEnemyMenu, "Height", healthBarPlugin.UniqueEnemy.Height);
            MenuItem uniqueEnemyDPSMenu = AddChild(uniquesEnemyMenu, "Floating combat text", healthBarPlugin.UniqueEnemy.ShowFloatingCombatDamage);
            AddChild(uniqueEnemyDPSMenu, "Text size", healthBarPlugin.UniqueEnemy.FloatingCombatTextSize);
            AddChild(uniqueEnemyDPSMenu, "Damage color", healthBarPlugin.UniqueEnemy.FloatingCombatDamageColor);
            AddChild(uniqueEnemyDPSMenu, "Heal color", healthBarPlugin.UniqueEnemy.FloatingCombatHealColor);
            AddChild(uniqueEnemyDPSMenu, "Number of lines", healthBarPlugin.UniqueEnemy.FloatingCombatStackSize);

            // XPH Display
            MenuItem expRateMenu = AddChild(MenuRootButton, "XPH & area", settingsHub.XPRateSettings.Enable, "F10");
            MenuItem areaName = AddChild(expRateMenu, "Only area name", settingsHub.XPRateSettings.OnlyAreaName);
            AddChild(areaName, "Show latency", settingsHub.XPRateSettings.ShowLatency);
            AddChild(areaName, "Latency color", settingsHub.XPRateSettings.LatencyTextColor);
            AddChild(expRateMenu, "Show in town", settingsHub.XPRateSettings.ShowInTown);
            AddChild(expRateMenu, "Font size", settingsHub.XPRateSettings.TextSize);
            AddChild(expRateMenu, "Fps font color", settingsHub.XPRateSettings.FPSTextColor);
            AddChild(expRateMenu, "XPH font color", settingsHub.XPRateSettings.XPHTextColor);
            AddChild(expRateMenu, "Area font color", settingsHub.PreloadAlertSettings.AreaTextColor);
            AddChild(expRateMenu, "Time left color", settingsHub.XPRateSettings.TimeLeftColor);
            AddChild(expRateMenu, "Timer font color", settingsHub.XPRateSettings.TimerTextColor);
            AddChild(expRateMenu, "Latency font color", settingsHub.XPRateSettings.LatencyTextColor);
            AddChild(expRateMenu, "Background color", settingsHub.XPRateSettings.BackgroundColor);

            // Item Alert
            MenuItem itemAlertMenu = AddChild(MenuRootButton, "Item alert", settingsHub.ItemAlertSettings.Enable);
            var itemAlertStaticMenuList = new[] { "Alternative", "Item tooltips", "Play sound", "Show text", "Hide others", "Show border", "Dim Others" };
            MenuItem alternative = AddChild(itemAlertMenu, itemAlertStaticMenuList[0], settingsHub.ItemAlertSettings.Alternative, null, y => itemAlertStaticMenuList.All(x => x != (y as ToggleButton)?.Name));
            AddChild(alternative, settingsHub.ItemAlertSettings.FilePath);
            AddChild(alternative, "With border", settingsHub.ItemAlertSettings.WithBorder);
            AddChild(alternative, "With sound", settingsHub.ItemAlertSettings.WithSound);
            MenuItem tooltipMenu = AddChild(itemAlertMenu, itemAlertStaticMenuList[1], settingsHub.AdvancedTooltipSettings.Enable);
            MenuItem itemLevelMenu = AddChild(tooltipMenu, "Item level", settingsHub.AdvancedTooltipSettings.ItemLevel.Enable);
            AddChild(itemLevelMenu, "Text size", settingsHub.AdvancedTooltipSettings.ItemLevel.TextSize);
            AddChild(itemLevelMenu, "Text color", settingsHub.AdvancedTooltipSettings.ItemLevel.TextColor);
            AddChild(itemLevelMenu, "Background color", settingsHub.AdvancedTooltipSettings.ItemLevel.BackgroundColor);
            MenuItem itemModsMenu = AddChild(tooltipMenu, "Item mods", settingsHub.AdvancedTooltipSettings.ItemMods.Enable, "F9");
            AddChild(itemModsMenu, "Mods size", settingsHub.AdvancedTooltipSettings.ItemMods.ModTextSize);
            AddChild(itemModsMenu, "Tier 1 color", settingsHub.AdvancedTooltipSettings.ItemMods.T1Color);
            AddChild(itemModsMenu, "Tier 2 color", settingsHub.AdvancedTooltipSettings.ItemMods.T2Color);
            AddChild(itemModsMenu, "Tier 3 color", settingsHub.AdvancedTooltipSettings.ItemMods.T3Color);
            AddChild(itemModsMenu, "Suffix color", settingsHub.AdvancedTooltipSettings.ItemMods.SuffixColor);
            AddChild(itemModsMenu, "Prefix color", settingsHub.AdvancedTooltipSettings.ItemMods.PrefixColor);
            MenuItem weaponDPSMenu = AddChild(tooltipMenu, "Weapon DPS", settingsHub.AdvancedTooltipSettings.WeaponDPS.Enable);
            var damageColors = AddChild(weaponDPSMenu, "Damage colors", settingsHub.AdvancedTooltipSettings.WeaponDPS.Enable);
            AddChild(damageColors, "Cold damage", settingsHub.AdvancedTooltipSettings.WeaponDPS.ColdDamageColor);
            AddChild(damageColors, "Fire damage", settingsHub.AdvancedTooltipSettings.WeaponDPS.FireDamageColor);
            AddChild(damageColors, "Lightning damage", settingsHub.AdvancedTooltipSettings.WeaponDPS.LightningDamageColor);
            AddChild(damageColors, "Chaos damage", settingsHub.AdvancedTooltipSettings.WeaponDPS.ChaosDamageColor);
            AddChild(damageColors, "Physical damage", settingsHub.AdvancedTooltipSettings.WeaponDPS.PhysicalDamageColor);
            AddChild(damageColors, "Elemental damage", settingsHub.AdvancedTooltipSettings.WeaponDPS.ElementalDamageColor);
            AddChild(weaponDPSMenu, "Text color", settingsHub.AdvancedTooltipSettings.WeaponDPS.TextColor);
            AddChild(weaponDPSMenu, "DPS size", settingsHub.AdvancedTooltipSettings.WeaponDPS.DPSTextSize);
            AddChild(weaponDPSMenu, "DPS text size", settingsHub.AdvancedTooltipSettings.WeaponDPS.DPSNameTextSize);
            AddChild(weaponDPSMenu, "Background color", settingsHub.AdvancedTooltipSettings.WeaponDPS.BackgroundColor);
            MenuItem itemSound = AddChild(itemAlertMenu, itemAlertStaticMenuList[2], settingsHub.ItemAlertSettings.PlaySound);
            AddChild(itemSound, "Sound volume", settingsHub.ItemAlertSettings.SoundVolume);
            MenuItem alertTextMenu = AddChild(itemAlertMenu, itemAlertStaticMenuList[3], settingsHub.ItemAlertSettings.ShowText);
            AddChild(alertTextMenu, "Text size", settingsHub.ItemAlertSettings.TextSize);
            AddChild(itemAlertMenu, itemAlertStaticMenuList[4], settingsHub.ItemAlertSettings.HideOthers);
            ItemAlertSettings dimOtherSettings = settingsHub.ItemAlertSettings;
            MenuItem dimOtherMenu = AddChild(itemAlertMenu, itemAlertStaticMenuList[6], dimOtherSettings.DimOtherByPercentToggle);
            AddChild(dimOtherMenu, "Dim Others By %", dimOtherSettings.DimOtherByPercent);
            BorderSettings borderSettings = settingsHub.ItemAlertSettings.BorderSettings;
            MenuItem showBorderMenu = AddChild(itemAlertMenu, itemAlertStaticMenuList[5], borderSettings.Enable);
            AddChild(showBorderMenu, "Border width", borderSettings.BorderWidth);
            AddChild(showBorderMenu, "Border color:", borderSettings.BorderColor);
            AddChild(showBorderMenu, "Can't pick up :", borderSettings.CantPickUpBorderColor);
            AddChild(showBorderMenu, "Not my item :", borderSettings.NotMyItemBorderColor);
            AddChild(showBorderMenu, "Show timer", borderSettings.ShowTimer);
            AddChild(showBorderMenu, "Timer text size", borderSettings.TimerTextSize);
            AddChild(itemAlertMenu, "Uniques", settingsHub.ItemAlertSettings.Uniques);
            AddChild(itemAlertMenu, "Rares", settingsHub.ItemAlertSettings.Rares);
            AddChild(itemAlertMenu, "Currency", settingsHub.ItemAlertSettings.Currency);
            AddChild(itemAlertMenu, "Maps", settingsHub.ItemAlertSettings.Maps);
            AddChild(itemAlertMenu, "RGB", settingsHub.ItemAlertSettings.Rgb);
            AddChild(itemAlertMenu, "Crafting bases", settingsHub.ItemAlertSettings.Crafting);
            AddChild(itemAlertMenu, "Divination cards", settingsHub.ItemAlertSettings.DivinationCards);
            AddChild(itemAlertMenu, "Jewels", settingsHub.ItemAlertSettings.Jewels);
            AddChild(itemAlertMenu, "Talisman", settingsHub.ItemAlertSettings.Talisman);
            QualityItemsSettings qualityItemsSettings = settingsHub.ItemAlertSettings.QualityItems;
            MenuItem qualityMenu = AddChild(itemAlertMenu, "Show quality items", qualityItemsSettings.Enable);
            MenuItem qualityWeaponMenu = AddChild(qualityMenu, "Weapons", qualityItemsSettings.Weapon.Enable);
            AddChild(qualityWeaponMenu, "Min. quality", qualityItemsSettings.Weapon.MinQuality);
            MenuItem qualityArmourMenu = AddChild(qualityMenu, "Armours", qualityItemsSettings.Armour.Enable);
            AddChild(qualityArmourMenu, "Min. quality", qualityItemsSettings.Armour.MinQuality);
            MenuItem qualityFlaskMenu = AddChild(qualityMenu, "Flasks", qualityItemsSettings.Flask.Enable);
            AddChild(qualityFlaskMenu, "Min. quality", qualityItemsSettings.Flask.MinQuality);
            MenuItem qualitySkillGemMenu = AddChild(qualityMenu, "Skill gems", qualityItemsSettings.SkillGem.Enable);
            AddChild(qualitySkillGemMenu, "Min. quality", qualityItemsSettings.SkillGem.MinQuality);

            // Preload Alert
            var preloadMenu = AddChild(MenuRootButton, "Preload alert", settingsHub.PreloadAlertSettings.Enable, "F5");
            var masters = AddChild(preloadMenu, "Masters", settingsHub.PreloadAlertSettings.Masters);
            AddChild(masters, "Zana", settingsHub.PreloadAlertSettings.MasterZana);
            AddChild(masters, "Tora", settingsHub.PreloadAlertSettings.MasterTora);
            AddChild(masters, "Haku", settingsHub.PreloadAlertSettings.MasterHaku);
            AddChild(masters, "Vorici", settingsHub.PreloadAlertSettings.MasterVorici);
            AddChild(masters, "Elreon", settingsHub.PreloadAlertSettings.MasterElreon);
            AddChild(masters, "Vagan", settingsHub.PreloadAlertSettings.MasterVagan);
            AddChild(masters, "Catarina", settingsHub.PreloadAlertSettings.MasterCatarina);
            AddChild(masters, "Krillson", settingsHub.PreloadAlertSettings.MasterKrillson);

            var exiles = AddChild(preloadMenu, "Exiles", settingsHub.PreloadAlertSettings.Exiles);
            AddChild(exiles, "Orra Greengate", settingsHub.PreloadAlertSettings.OrraGreengate);
            AddChild(exiles, "Thena Moga", settingsHub.PreloadAlertSettings.ThenaMoga);
            AddChild(exiles, "Antalie Napora", settingsHub.PreloadAlertSettings.AntalieNapora);
            AddChild(exiles, "Torr Olgosso", settingsHub.PreloadAlertSettings.TorrOlgosso);
            AddChild(exiles, "Armios Bell", settingsHub.PreloadAlertSettings.ArmiosBell);
            AddChild(exiles, "Zacharie Desmarais", settingsHub.PreloadAlertSettings.ZacharieDesmarais);
            AddChild(exiles, "Minara Anenima", settingsHub.PreloadAlertSettings.MinaraAnenima);
            AddChild(exiles, "Igna Phoenix", settingsHub.PreloadAlertSettings.IgnaPhoenix);
            AddChild(exiles, "Jonah Unchained", settingsHub.PreloadAlertSettings.JonahUnchained);
            AddChild(exiles, "Damoi Tui", settingsHub.PreloadAlertSettings.DamoiTui);
            AddChild(exiles, "Xandro Blooddrinker", settingsHub.PreloadAlertSettings.XandroBlooddrinker);
            AddChild(exiles, "Vickas Giantbone", settingsHub.PreloadAlertSettings.VickasGiantbone);
            AddChild(exiles, "Eoin Greyfur", settingsHub.PreloadAlertSettings.EoinGreyfur);
            AddChild(exiles, "Tinevin Highdove", settingsHub.PreloadAlertSettings.TinevinHighdove);
            AddChild(exiles, "Magnus Stonethorn", settingsHub.PreloadAlertSettings.MagnusStonethorn);
            AddChild(exiles, "Ion Darkshroud", settingsHub.PreloadAlertSettings.IonDarkshroud);
            AddChild(exiles, "Ash Lessard", settingsHub.PreloadAlertSettings.AshLessard);
            AddChild(exiles, "Wilorin Demontamer", settingsHub.PreloadAlertSettings.WilorinDemontamer);
            AddChild(exiles, "Augustina Solaria", settingsHub.PreloadAlertSettings.AugustinaSolaria);
            AddChild(exiles, "Dena Lorenni", settingsHub.PreloadAlertSettings.DenaLorenni);
            AddChild(exiles, "Vanth Agiel", settingsHub.PreloadAlertSettings.VanthAgiel);
            AddChild(exiles, "Lael Furia", settingsHub.PreloadAlertSettings.LaelFuria);
            AddChild(exiles, "OyraOna", settingsHub.PreloadAlertSettings.OyraOna);
            AddChild(exiles, "BoltBrownfur", settingsHub.PreloadAlertSettings.BoltBrownfur);
            AddChild(exiles, "AilentiaRac", settingsHub.PreloadAlertSettings.AilentiaRac);
            AddChild(exiles, "UlyssesMorvant", settingsHub.PreloadAlertSettings.UlyssesMorvant);
            AddChild(exiles, "AurelioVoidsinger", settingsHub.PreloadAlertSettings.AurelioVoidsinger);

            var strongboxes = AddChild(preloadMenu, "Strongboxes", settingsHub.PreloadAlertSettings.Strongboxes);
            AddChild(strongboxes, "Arcanist", settingsHub.PreloadAlertSettings.ArcanistStrongbox);
            AddChild(strongboxes, "Artisan", settingsHub.PreloadAlertSettings.ArtisanStrongbox);
            AddChild(strongboxes, "Cartographer", settingsHub.PreloadAlertSettings.CartographerStrongbox);
            AddChild(strongboxes, "Diviner", settingsHub.PreloadAlertSettings.DivinerStrongbox);
            AddChild(strongboxes, "Gemcutter", settingsHub.PreloadAlertSettings.GemcutterStrongbox);
            AddChild(strongboxes, "Jeweller", settingsHub.PreloadAlertSettings.JewellerStrongbox);
            AddChild(strongboxes, "Blacksmith", settingsHub.PreloadAlertSettings.BlacksmithStrongbox);
            AddChild(strongboxes, "Armourer", settingsHub.PreloadAlertSettings.ArmourerStrongbox);
            AddChild(strongboxes, "Ornate", settingsHub.PreloadAlertSettings.OrnateStrongbox);
            AddChild(strongboxes, "Large", settingsHub.PreloadAlertSettings.LargeStrongbox);
            AddChild(strongboxes, "Perandus", settingsHub.PreloadAlertSettings.PerandusStrongbox);
            AddChild(strongboxes, "Kaom", settingsHub.PreloadAlertSettings.KaomStrongbox);
            AddChild(strongboxes, "Malachai", settingsHub.PreloadAlertSettings.MalachaiStrongbox);
            AddChild(strongboxes, "Epic", settingsHub.PreloadAlertSettings.EpicStrongbox);
            AddChild(strongboxes, "Simple", settingsHub.PreloadAlertSettings.SimpleStrongbox);

            var essences = AddChild(preloadMenu, "Essences", settingsHub.PreloadAlertSettings.Essence);
            AddChild(essences, "Remnant of Corruption", settingsHub.PreloadAlertSettings.RemnantOfCorruption);
            AddChild(essences, "Essence of Anger", settingsHub.PreloadAlertSettings.EssenceOfAnger);
            AddChild(essences, "Essence of Anguish", settingsHub.PreloadAlertSettings.EssenceOfAnguish);
            AddChild(essences, "Essence of Contempt", settingsHub.PreloadAlertSettings.EssenceOfContempt);
            AddChild(essences, "Essence of Delirium", settingsHub.PreloadAlertSettings.EssenceOfDelirium);
            AddChild(essences, "Essence of Doubt", settingsHub.PreloadAlertSettings.EssenceOfDoubt);
            AddChild(essences, "Essence of Dread", settingsHub.PreloadAlertSettings.EssenceOfDread);
            AddChild(essences, "Essence of Envy", settingsHub.PreloadAlertSettings.EssenceOfEnvy);
            AddChild(essences, "Essence of Fear", settingsHub.PreloadAlertSettings.EssenceOfFear);
            AddChild(essences, "Essence of Greed", settingsHub.PreloadAlertSettings.EssenceOfGreed);
            AddChild(essences, "Essence of Hatred", settingsHub.PreloadAlertSettings.EssenceOfHatred);
            AddChild(essences, "Essence of Horror", settingsHub.PreloadAlertSettings.EssenceOfHorror);
            AddChild(essences, "Essence of Hysteria", settingsHub.PreloadAlertSettings.EssenceOfHysteria);
            AddChild(essences, "Essence of Insanity", settingsHub.PreloadAlertSettings.EssenceOfInsanity);
            AddChild(essences, "Essence of Loathing", settingsHub.PreloadAlertSettings.EssenceOfLoathing);
            AddChild(essences, "Essence of Misery", settingsHub.PreloadAlertSettings.EssenceOfMisery);
            AddChild(essences, "Essence of Rage", settingsHub.PreloadAlertSettings.EssenceOfRage);
            AddChild(essences, "Essence of Scorn", settingsHub.PreloadAlertSettings.EssenceOfScorn);
            AddChild(essences, "Essence of Sorrow", settingsHub.PreloadAlertSettings.EssenceOfSorrow);
            AddChild(essences, "Essence of Spite", settingsHub.PreloadAlertSettings.EssenceOfSpite);
            AddChild(essences, "Essence of Suffering", settingsHub.PreloadAlertSettings.EssenceOfSuffering);
            AddChild(essences, "Essence of Torment", settingsHub.PreloadAlertSettings.EssenceOfTorment);
            AddChild(essences, "Essence of Woe", settingsHub.PreloadAlertSettings.EssenceOfWoe);
            AddChild(essences, "Essence of Wrath", settingsHub.PreloadAlertSettings.EssenceOfWrath);
            AddChild(essences, "Essence of Zeal", settingsHub.PreloadAlertSettings.EssenceOfZeal);

            var perandus = AddChild(preloadMenu, "Perandus Chests", settingsHub.PreloadAlertSettings.PerandusBoxes);
            AddChild(perandus, "Cadiro Trader", settingsHub.PreloadAlertSettings.CadiroTrader);
            AddChild(perandus, "Perandus Chest", settingsHub.PreloadAlertSettings.PerandusChestStandard);
            AddChild(perandus, "Perandus Cache", settingsHub.PreloadAlertSettings.PerandusChestRarity);
            AddChild(perandus, "Perandus Hoard", settingsHub.PreloadAlertSettings.PerandusChestQuantity);
            AddChild(perandus, "Perandus Coffer", settingsHub.PreloadAlertSettings.PerandusChestCoins);
            AddChild(perandus, "Perandus Jewellery", settingsHub.PreloadAlertSettings.PerandusChestJewellery);
            AddChild(perandus, "Perandus Safe", settingsHub.PreloadAlertSettings.PerandusChestGems);
            AddChild(perandus, "Perandus Treasury", settingsHub.PreloadAlertSettings.PerandusChestCurrency);
            AddChild(perandus, "Perandus Wardrobe", settingsHub.PreloadAlertSettings.PerandusChestInventory);
            AddChild(perandus, "Perandus Catalogue", settingsHub.PreloadAlertSettings.PerandusChestDivinationCards);
            AddChild(perandus, "Perandus Trove", settingsHub.PreloadAlertSettings.PerandusChestKeepersOfTheTrove);
            AddChild(perandus, "Perandus Locker", settingsHub.PreloadAlertSettings.PerandusChestUniqueItem);
            AddChild(perandus, "Perandus Archive", settingsHub.PreloadAlertSettings.PerandusChestMaps);
            AddChild(perandus, "Perandus Tackle Box", settingsHub.PreloadAlertSettings.PerandusChestFishing);
            AddChild(perandus, "Cadiro's Locker", settingsHub.PreloadAlertSettings.PerandusManorUniqueChest);
            AddChild(perandus, "Cadiro's Treasury", settingsHub.PreloadAlertSettings.PerandusManorCurrencyChest);
            AddChild(perandus, "Cadiro's Archive", settingsHub.PreloadAlertSettings.PerandusManorMapsChest);
            AddChild(perandus, "Cadiro's Jewellery", settingsHub.PreloadAlertSettings.PerandusManorJewelryChest);
            AddChild(perandus, "Cadiro's Catalogue", settingsHub.PreloadAlertSettings.PerandusManorDivinationCardsChest);
            AddChild(perandus, "Grand Perandus Vault", settingsHub.PreloadAlertSettings.PerandusManorLostTreasureChest);

            var corruptedMenu = AddChild(preloadMenu, "Corrupted Area", settingsHub.PreloadAlertSettings.CorruptedArea);
            AddChild(corruptedMenu, "Corrupted title", settingsHub.PreloadAlertSettings.CorruptedTitle, "F5");
            AddChild(corruptedMenu, "Corrupted color", settingsHub.PreloadAlertSettings.CorruptedAreaColor);
            AddChild(preloadMenu, "Background color", settingsHub.PreloadAlertSettings.BackgroundColor);
            AddChild(preloadMenu, "Font color", settingsHub.PreloadAlertSettings.DefaultTextColor);
            AddChild(preloadMenu, "Font size", settingsHub.PreloadAlertSettings.TextSize);

            // Monster alert
            MenuItem monsterTrackerMenu = AddChild(MenuRootButton, "Monster alert", settingsHub.MonsterTrackerSettings.Enable);
            MenuItem alertSound = AddChild(monsterTrackerMenu, "Sound warning", settingsHub.MonsterTrackerSettings.PlaySound);
            AddChild(alertSound, "Sound volume", settingsHub.MonsterTrackerSettings.SoundVolume);
            MenuItem warningTextMenu = AddChild(monsterTrackerMenu, "Text warning", settingsHub.MonsterTrackerSettings.ShowText);
            AddChild(warningTextMenu, "Text size", settingsHub.MonsterTrackerSettings.TextSize);
            AddChild(warningTextMenu, "Default text color:", settingsHub.MonsterTrackerSettings.DefaultTextColor);
            AddChild(warningTextMenu, "Background color:", settingsHub.MonsterTrackerSettings.BackgroundColor);
            AddChild(warningTextMenu, "Position X", settingsHub.MonsterTrackerSettings.TextPositionX);
            AddChild(warningTextMenu, "Position Y", settingsHub.MonsterTrackerSettings.TextPositionY);

            // Monster Kills
            MenuItem showMonsterKillsMenu = AddChild(MenuRootButton, "Monster kills", settingsHub.KillCounterSettings.Enable);
            AddChild(showMonsterKillsMenu, "Show details", settingsHub.KillCounterSettings.ShowDetail);
            AddChild(showMonsterKillsMenu, "Show in town", settingsHub.KillCounterSettings.ShowInTown);
            AddChild(showMonsterKillsMenu, "Font color", settingsHub.KillCounterSettings.TextColor);
            AddChild(showMonsterKillsMenu, "Background color", settingsHub.KillCounterSettings.BackgroundColor);
            AddChild(showMonsterKillsMenu, "Label font size", settingsHub.KillCounterSettings.LabelTextSize);
            AddChild(showMonsterKillsMenu, "Kills font size", settingsHub.KillCounterSettings.KillsTextSize);

            // DPS options
            MenuItem showDPSMenu = AddChild(MenuRootButton, "Show dps", settingsHub.DPSMeterSettings.Enable);
            AddChild(showDPSMenu, "Show in town", settingsHub.DPSMeterSettings.ShowInTown);
            AddChild(showDPSMenu, "DPS font size", settingsHub.DPSMeterSettings.DPSTextSize);
            AddChild(showDPSMenu, "Top dps font size", settingsHub.DPSMeterSettings.PeakDPSTextSize);
            AddChild(showDPSMenu, "Background color", settingsHub.DPSMeterSettings.BackgroundColor);
            AddChild(showDPSMenu, "DPS font color", settingsHub.DPSMeterSettings.DPSFontColor);
            AddChild(showDPSMenu, "Top dps font color", settingsHub.DPSMeterSettings.PeakFontColor);

            // Map icons
            MenuItem mapIconsMenu = AddChild(MenuRootButton, "Map icons", settingsHub.MapIconsSettings.Enable);
            MenuItem iconSizeMenu = AddChild(mapIconsMenu, "Icon sizes", settingsHub.MonsterTrackerSettings.ShowText);
            AddChild(iconSizeMenu, "White Mob Icon", settingsHub.MonsterTrackerSettings.WhiteMobIcon);
            AddChild(iconSizeMenu, "Magic Mob Icon", settingsHub.MonsterTrackerSettings.MagicMobIcon);
            AddChild(iconSizeMenu, "Rare Mob Icon", settingsHub.MonsterTrackerSettings.RareMobIcon);
            AddChild(iconSizeMenu, "Unique Mob Icon", settingsHub.MonsterTrackerSettings.UniqueMobIcon);
            AddChild(iconSizeMenu, "Minions Icon", settingsHub.MonsterTrackerSettings.MinionsIcon);
            AddChild(iconSizeMenu, "Masters Icon", settingsHub.PoITrackerSettings.MastersIcon);
            AddChild(iconSizeMenu, "Chests Icon", settingsHub.PoITrackerSettings.ChestsIcon);
            AddChild(iconSizeMenu, "Strongboxes Icon", settingsHub.PoITrackerSettings.StrongboxesIcon);
            AddChild(iconSizeMenu, "PerandusChest Icon", settingsHub.PoITrackerSettings.PerandusChestIcon);
            AddChild(iconSizeMenu, "BreachChest Icon", settingsHub.PoITrackerSettings.BreachChestIcon);
            MenuItem itemLootIcon = AddChild(iconSizeMenu, "Item loot Icon", settingsHub.ItemAlertSettings.LootIcon);
            AddChild(iconSizeMenu, "Use border color for loot icon", settingsHub.ItemAlertSettings.LootIconBorderColor); // Adding a ToggleNode as a RangeNode child doesn't display it
            AddChild(mapIconsMenu, "Minimap icons", settingsHub.MapIconsSettings.IconsOnMinimap);
            AddChild(mapIconsMenu, "Large map icons", settingsHub.MapIconsSettings.IconsOnLargeMap);
            AddChild(mapIconsMenu, "Drop items", settingsHub.ItemAlertSettings.ShowItemOnMap);
            AddChild(mapIconsMenu, "Monsters", settingsHub.MonsterTrackerSettings.Monsters);
            AddChild(mapIconsMenu, "Minions", settingsHub.MonsterTrackerSettings.Minions);
            AddChild(mapIconsMenu, "Strongboxes", settingsHub.PoITrackerSettings.Strongboxes);
            AddChild(mapIconsMenu, "Chests", settingsHub.PoITrackerSettings.Chests);
            AddChild(mapIconsMenu, "Masters", settingsHub.PoITrackerSettings.Masters);

            // Menu Settings
            var menuSettings = AddChild(MenuRootButton, "Menu settings", settingsHub.MenuSettings.ShowMenu, "F12");
            AddChild(menuSettings, "Menu font color", settingsHub.MenuSettings.MenuFontColor);
            AddChild(menuSettings, "Title font color", settingsHub.MenuSettings.TitleFontColor);
            AddChild(menuSettings, "Enabled color", settingsHub.MenuSettings.EnabledBoxColor);
            AddChild(menuSettings, "Disabled color", settingsHub.MenuSettings.DisabledBoxColor);
            AddChild(menuSettings, "Slider color", settingsHub.MenuSettings.SliderColor);
            AddChild(menuSettings, "Background color", settingsHub.MenuSettings.BackgroundColor);
            AddChild(menuSettings, "Menu font size", settingsHub.MenuSettings.MenuFontSize);
            AddChild(menuSettings, "Title font size", settingsHub.MenuSettings.TitleFontSize);
            AddChild(menuSettings, "Picker font size", settingsHub.MenuSettings.PickerFontSize);

            ExternalInitMenu(MenuRootButton); // Spawning the menu in external plugins
        }

        private bool OnMouseEvent(MouseEventId id, Point position)
        {
            if (!GameController.Window.IsForeground())
            {
                return false;
            }

            Vector2 mousePosition = GameController.Window.ScreenToClient(position.X, position.Y);
            ExternalMouseEvent(id, mousePosition);
            if (Settings.Enable)
            {
                // I dunno how to handle this in plugins. Seems there is only this way {Stridemann}
                return MenuRootButton.OnMouseEvent(id, mousePosition) || ExternalMouseClick(id, mousePosition);
            }

            return false;
        }
    }
}
