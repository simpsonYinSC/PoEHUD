using System;
using System.Collections.Generic;
using System.Linq;
using PoEHUD.Controllers;
using PoEHUD.Framework;
using PoEHUD.PoE.FilesInMemory;
using PoEHUD.PoE.RemoteMemoryObjects;
using SharpDX;

namespace PoEHUD.HUD.AdvancedTooltip
{
    public class ModValue
    {
        private readonly int totalTiers = 1;

        public ModValue(ItemMod itemMod, FileSystemController fileSystemController, int itemLevel, Models.BaseItemType baseItem)
        {
            string baseClassName = baseItem.ClassName.ToLower().Replace(' ', '_');
            Record = fileSystemController.Mods.Records[itemMod.RawName];
            AffixType = Record.AffixType;
            AffixText = string.IsNullOrEmpty(Record.UserFriendlyName) ? Record.Key : Record.UserFriendlyName;
            IsCrafted = Record.Domain == ModsDat.ModDomain.Master;
            StatValue = new[] { itemMod.Value1, itemMod.Value2, itemMod.Value3, itemMod.Value4 };
            Tier = -1;

            int subOptimalTierDistance = 0;

            if (fileSystemController.Mods.RecordsByTier.TryGetValue(Tuple.Create(Record.Group, Record.AffixType), out List<ModsDat.ModRecord> allTiers))
            {
                bool tierFound = false;
                totalTiers = 0;
                char[] keyRcd = Record.Key.Where(char.IsLetter).ToArray();
                foreach (var tmp in allTiers)
                {
                    char[] keyrcd = tmp.Key.Where(char.IsLetter).ToArray();
                    if (!keyrcd.SequenceEqual(keyRcd))
                    {
                        continue;
                    }

                    int baseChance;
                    if (!tmp.TagChances.TryGetValue(baseClassName, out baseChance))
                    {
                        baseChance = -1;
                    }

                    int defaultChance;
                    if (!tmp.TagChances.TryGetValue("default", out defaultChance))
                    {
                        defaultChance = 0;
                    }

                    int tagChance = -1;
                    foreach (string tg in baseItem.Tags)
                    {
                        if (tmp.TagChances.ContainsKey(tg))
                        {
                            tagChance = tmp.TagChances[tg];
                        }
                    }

                    int moreTagChance = -1;
                    foreach (string tg in baseItem.MoreTagsFromPath)
                    {
                        if (tmp.TagChances.ContainsKey(tg))
                        {
                            moreTagChance = tmp.TagChances[tg];
                        }
                    }

                    // GetOnlyValidMods
                    switch (baseChance)
                    {
                        case 0:
                            break;
                        case -1: // baseClass name not found in mod tags.
                            switch (tagChance)
                            {
                                case 0:
                                    break;
                                case -1: // item tags not found in mod tags.
                                    switch (moreTagChance)
                                    {
                                        case 0:
                                            break;
                                        case -1: // more item tags not found in mod tags.
                                            if (defaultChance > 0)
                                            {
                                                totalTiers++;
                                                if (tmp.Equals(Record))
                                                {
                                                    Tier = totalTiers;
                                                    tierFound = true;
                                                }

                                                if (!tierFound && tmp.MinimumLevel <= itemLevel)
                                                {
                                                    subOptimalTierDistance++;
                                                }
                                            }

                                            break;
                                        default:
                                            totalTiers++;
                                            if (tmp.Equals(Record))
                                            {
                                                Tier = totalTiers;
                                                tierFound = true;
                                            }

                                            if (!tierFound && tmp.MinimumLevel <= itemLevel)
                                            {
                                                subOptimalTierDistance++;
                                            }

                                            break;
                                    }

                                    break;
                                default:
                                    totalTiers++;
                                    if (tmp.Equals(Record))
                                    {
                                        Tier = totalTiers;
                                        tierFound = true;
                                    }

                                    if (!tierFound && tmp.MinimumLevel <= itemLevel)
                                    {
                                        subOptimalTierDistance++;
                                    }

                                    break;
                            }

                            break;
                        default:
                            totalTiers++;
                            if (tmp.Equals(Record))
                            {
                                Tier = totalTiers;
                                tierFound = true;
                            }

                            if (!tierFound && tmp.MinimumLevel <= itemLevel)
                            {
                                subOptimalTierDistance++;
                            }

                            break;
                    }
                }
            }

            double hue = totalTiers == 1 ? 180 : 120 - Math.Min(subOptimalTierDistance, 3) * 40;
            Color = ColorUtils.ColorFromHsv(hue, totalTiers == 1 ? 0 : 1, 1);
        }

        public ModsDat.ModType AffixType { get; private set; }
        public bool IsCrafted { get; private set; }
        public string AffixText { get; private set; }
        public Color Color { get; private set; }
        public ModsDat.ModRecord Record { get; }
        public int[] StatValue { get; private set; }
        public int Tier { get; private set; }

        public bool CouldHaveTiers()
        {
            return totalTiers > 1;
        }
    }
}
