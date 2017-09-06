using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using PoEHUD.Controllers;
using PoEHUD.Framework;
using PoEHUD.Framework.Helpers;
using PoEHUD.HUD.Settings;
using PoEHUD.Models.Enums;
using PoEHUD.PoE;
using PoEHUD.PoE.Components;
using PoEHUD.PoE.Elements;
using PoEHUD.PoE.FilesInMemory;
using PoEHUD.PoE.RemoteMemoryObjects;
using SharpDX;
using SharpDX.Direct3D9;
using Color = SharpDX.Color;
using Graphics = PoEHUD.HUD.UI.Graphics;
using RectangleF = SharpDX.RectangleF;

namespace PoEHUD.HUD.AdvancedTooltip
{
    public class AdvancedTooltipPlugin : Plugin<AdvancedTooltipSettings>
    {
        private readonly SettingsHub settingsHub;
        private Color color;
        private bool holdKey;
        private Entity itemEntity;
        private List<ModValue> mods = new List<ModValue>();

        public AdvancedTooltipPlugin(GameController gameController, Graphics graphics, AdvancedTooltipSettings settings, SettingsHub settingsHub) : base(gameController, graphics, settings)
        {
            this.settingsHub = settingsHub;
        }

        public override void Render()
        {
            try
            {
                if (!holdKey && WindowsAPI.IsKeyDown(Keys.F9))
                {
                    holdKey = true;
                    Settings.ItemMods.Enable.Value = !Settings.ItemMods.Enable.Value;
                    if (!Settings.ItemMods.Enable.Value)
                    {
                        SettingsHub.Save(settingsHub);
                    }
                }
                else if (holdKey && !WindowsAPI.IsKeyDown(Keys.F9))
                {
                    holdKey = false;
                }

                Element uiHover = GameController.Game.IngameState.UIHover;
                var inventoryItemIcon = uiHover.AsObject<HoverItemIcon>();
                if (inventoryItemIcon == null)
                {
                    return;
                }

                Element tooltip = inventoryItemIcon.Tooltip;
                Entity poeEntity = inventoryItemIcon.Item;

                if (tooltip == null || poeEntity.Address == 0 || !poeEntity.IsValid)
                {
                    return;
                }

                RectangleF tooltipRect = tooltip.GetClientRect();

                var modsComponent = poeEntity.GetComponent<Mods>();
                long id = inventoryItemIcon.ToolTipType == ToolTipType.InventoryItem ? poeEntity.InventoryId : poeEntity.Id;

                if (itemEntity == null || itemEntity.Id != id)
                {
                    IEnumerable<ItemMod> itemMods = modsComponent.ItemMods;
                    mods = itemMods.Select(item => new ModValue(item, GameController.Files, modsComponent.ItemLevel, GameController.Files.BaseItemTypes.Translate(poeEntity.Path))).ToList();
                    itemEntity = poeEntity;
                }
              
                foreach (string tier in from item in mods where item.CouldHaveTiers() && item.Tier == 1 select " \u2605 ")
                {
                    Graphics.DrawText(tier, 18, tooltipRect.TopLeft.Translate(0, 56), Settings.ItemMods.T1Color);
                }

                if (Settings.ItemLevel.Enable)
                {
                    string itemLevel = Convert.ToString(modsComponent.ItemLevel);
                    int imageSize = Settings.ItemLevel.TextSize + 10;
                    Graphics.DrawText(itemLevel, Settings.ItemLevel.TextSize, tooltipRect.TopLeft.Translate(2, 2), Settings.ItemLevel.TextColor);
                    Graphics.DrawImage("menu-colors.png", new RectangleF(tooltipRect.TopLeft.X - 2, tooltipRect.TopLeft.Y - 2, imageSize, imageSize), Settings.ItemLevel.BackgroundColor);
                }

                if (Settings.ItemMods.Enable)
                {
                    float bottomTooltip = tooltipRect.Bottom + 5;
                    var modPosition = new Vector2(tooltipRect.X + 50, bottomTooltip + 4);
                    float height = mods.Aggregate(modPosition, (position, item) => DrawMod(item, position)).Y - bottomTooltip;
                    if (height > 4)
                    {
                        var modsRect = new RectangleF(tooltipRect.X + 1, bottomTooltip, tooltipRect.Width, height);
                        Graphics.DrawBox(modsRect, Settings.ItemMods.BackgroundColor);
                    }
                }

                if (Settings.WeaponDPS.Enable && poeEntity.HasComponent<Weapon>())
                {
                    DrawWeaponDPS(tooltipRect);
                }
            }
            catch
            {
                // ignored
            }
        }

        private Vector2 DrawMod(ModValue item, Vector2 position)
        {
            const float epsilon = 0.001f;
            const int marginBottom = 4, marginLeft = 50;

            Vector2 oldPosition = position;
            ItemModsSettings settings = Settings.ItemMods;

            string affix = item.AffixType == ModsDat.ModType.Prefix ? "[P]" : item.AffixType == ModsDat.ModType.Suffix ? "[S]" : "[?]";

            Dictionary<int, Color> colors = new Dictionary<int, Color>
            {
                { 1, settings.T1Color },
                { 2, settings.T2Color },
                { 3, settings.T3Color }
            };

            if (item.AffixType != ModsDat.ModType.Unique)
            {
                if (item.CouldHaveTiers())
                {
                    affix += $" T{item.Tier} ";
                }

                if (item.AffixType == ModsDat.ModType.Prefix)
                {
                    Graphics.DrawText(affix, settings.ModTextSize, position.Translate(5 - marginLeft, 0), settings.PrefixColor);
                    if (!colors.TryGetValue(item.Tier, out color))
                    {
                        color = settings.PrefixColor;
                    }
                }

                if (item.AffixType == ModsDat.ModType.Suffix)
                {
                    Graphics.DrawText(affix, settings.ModTextSize, position.Translate(5 - marginLeft, 0), settings.SuffixColor);
                    if (!colors.TryGetValue(item.Tier, out color))
                    {
                        color = settings.SuffixColor;
                    }
                }
                
                Size2 textSize = Graphics.DrawText(item.AffixText, settings.ModTextSize, position, color);
                if (textSize != new Size2())
                {
                    position.Y += textSize.Height;
                }
            }

            for (int i = 0; i < 4; i++)
            {
                IntRange range = item.Record.StatRange[i];
                if (range.Min == 0 && range.Max == 0)
                {
                    continue;
                }

                StatsDat.StatRecord stat = item.Record.StatNames[i];
                int value = item.StatValue[i];
                if (value <= -1000 || stat == null)
                {
                    continue;
                }

                bool noSpread = !range.HasSpread();
                string line2 = string.Format(noSpread ? "{0}" : "{0} [{1}]", stat, range);
                Graphics.DrawText(line2, settings.ModTextSize, position, Color.Gainsboro);
                string statText = stat.ValueToString(value);
                Vector2 statPosition = position.Translate(-5, 0);
                Size2 textSize = Graphics.DrawText(statText, settings.ModTextSize, statPosition, Color.Gainsboro, FontDrawFlags.Right);
                position.Y += textSize.Height;
            }

            return Math.Abs(position.Y - oldPosition.Y) > epsilon ? position.Translate(0, marginBottom) : oldPosition;
        }

        private void DrawWeaponDPS(RectangleF clientRect)
        {
            var weapon = itemEntity.GetComponent<Weapon>();
            float attackSpeed = (float)Math.Round(1000f / weapon.AttackTime, 2);
            int cntDamages = Enum.GetValues(typeof(DamageType)).Length;
            float[] doubleDPSPerStat = new float[cntDamages];
            float physDamageMultiplier = 1;
            int physHi = weapon.DamageMaximum;
            int physLo = weapon.DamageMinimum;
            foreach (ModValue mod in mods)
            {
                for (int indexStat = 0; indexStat < 4; indexStat++)
                {
                    IntRange range = mod.Record.StatRange[indexStat];
                    if (range.Min == 0 && range.Max == 0)
                    {
                        continue;
                    }

                    StatsDat.StatRecord theStat = mod.Record.StatNames[indexStat];
                    int value = mod.StatValue[indexStat];
                    switch (theStat.Key)
                    {
                        case "physical_damage_+%":
                        case "local_physical_damage_+%":
                            physDamageMultiplier += value / 100f;
                            break;
                        case "local_attack_speed_+%":
                            attackSpeed *= (100f + value) / 100;
                            break;
                        case "local_minimum_added_physical_damage":
                            physLo += value;
                            break;
                        case "local_maximum_added_physical_damage":
                            physHi += value;
                            break;
                        case "local_minimum_added_fire_damage":
                        case "local_maximum_added_fire_damage":
                        case "unique_local_minimum_added_fire_damage_when_in_main_hand":
                        case "unique_local_maximum_added_fire_damage_when_in_main_hand":
                            doubleDPSPerStat[(int)DamageType.Fire] += value;
                            break;
                        case "local_minimum_added_cold_damage":
                        case "local_maximum_added_cold_damage":
                        case "unique_local_minimum_added_cold_damage_when_in_off_hand":
                        case "unique_local_maximum_added_cold_damage_when_in_off_hand":
                            doubleDPSPerStat[(int)DamageType.Cold] += value;
                            break;
                        case "local_minimum_added_lightning_damage":
                        case "local_maximum_added_lightning_damage":
                            doubleDPSPerStat[(int)DamageType.Lightning] += value;
                            break;
                        case "unique_local_minimum_added_chaos_damage_when_in_off_hand":
                        case "unique_local_maximum_added_chaos_damage_when_in_off_hand":
                        case "local_minimum_added_chaos_damage":
                        case "local_maximum_added_chaos_damage":
                            doubleDPSPerStat[(int)DamageType.Chaos] += value;
                            break;
                    }
                }
            }

            WeaponDPSSettings settings = Settings.WeaponDPS;
            Color[] elementalDamageColors =
            {
                Color.White,
                settings.FireDamageColor,
                settings.ColdDamageColor,
                settings.LightningDamageColor,
                settings.ChaosDamageColor
            };
            physDamageMultiplier += itemEntity.GetComponent<Quality>().ItemQuality / 100f;
            physLo = (int)Math.Round(physLo * physDamageMultiplier);
            physHi = (int)Math.Round(physHi * physDamageMultiplier);
            doubleDPSPerStat[(int)DamageType.Physical] = physLo + physHi;

            attackSpeed = (float)Math.Round(attackSpeed, 2);
            float physicalDPS = doubleDPSPerStat[(int)DamageType.Physical] / 2 * attackSpeed;
            float elementalDPS = 0;
            int firstEmg = 0;
            Color dpsColor = settings.PhysicalDamageColor;

            for (int i = 1; i < cntDamages; i++)
            {
                elementalDPS += doubleDPSPerStat[i] / 2 * attackSpeed;
                if (!(doubleDPSPerStat[i] > 0))
                {
                    continue;
                }

                if (firstEmg == 0)
                {
                    firstEmg = i;
                    dpsColor = elementalDamageColors[i];
                }
                else
                {
                    dpsColor = settings.ElementalDamageColor;
                }
            }

            var textPosition = new Vector2(clientRect.Right - 2, clientRect.Y + 1);
            Size2 physicalDPSSize = physicalDPS > 0
                ? Graphics.DrawText(physicalDPS.ToString("#.#"), settings.DPSTextSize, textPosition, FontDrawFlags.Right)
                : new Size2();
            Size2 elementalDPSSize = elementalDPS > 0
                ? Graphics.DrawText(elementalDPS.ToString("#.#"), settings.DPSTextSize, textPosition.Translate(0, physicalDPSSize.Height), dpsColor, FontDrawFlags.Right)
                : new Size2();
            Vector2 dpsTextPosition = textPosition.Translate(0, physicalDPSSize.Height + elementalDPSSize.Height);
            Graphics.DrawText("dps", settings.DPSNameTextSize, dpsTextPosition, settings.TextColor, FontDrawFlags.Right);
            Graphics.DrawImage("preload-end.png", new RectangleF(textPosition.X - 86, textPosition.Y - 6, 90, 65), settings.BackgroundColor);
        }
    }
}
