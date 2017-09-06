using System.Collections.Generic;
using System.Linq;
using PoEHUD.Models.Enums;
using PoEHUD.Models.Interfaces;
using PoEHUD.PoE.Components;
using SharpDX;

namespace PoEHUD.HUD.Loot
{
    public class ItemUsefulProperties
    {
        private readonly string name;
        private readonly IEntity item;
        private readonly CraftingBase craftingBase;
        private ItemRarity rarity;
        private int quality, borderWidth, alertIcon = -1;
        private string alertText;
        private Color color;

        public ItemUsefulProperties(string name, IEntity item, CraftingBase craftingBase)
        {
            this.name = name;
            this.item = item;
            this.craftingBase = craftingBase;
        }

        public AlertDrawStyle GetDrawStyle()
        {
            return new AlertDrawStyle(new Color().Equals(color) ? (object)rarity : color, borderWidth, alertText, alertIcon);
        }

        public bool ShouldAlert(HashSet<string> currencyNames, ItemAlertSettings settings)
        {
            Mods mods = item.GetComponent<Mods>();
            QualityItemsSettings qualitySettings = settings.QualityItems;

            rarity = mods.ItemRarity;

            if (item.HasComponent<Quality>())
            {
                quality = item.GetComponent<Quality>().ItemQuality;
            }

            alertText = string.Concat(quality > 0 ? "Superior " : string.Empty, name);

            if (settings.Maps && (item.HasComponent<Map>() || item.Path.Contains("VaalFragment")))
            {
                borderWidth = 1;
                return true;
            }

            if (settings.Currency && item.Path.Contains("Currency"))
            {
                color = HUDSkin.CurrencyColor;
                return currencyNames?.Contains(name) ?? !name.Contains("Wisdom") && !name.Contains("Portal");
            }

            if (settings.DivinationCards && item.Path.Contains("DivinationCards"))
            {
                color = HUDSkin.DivinationCardColor;
                return true;
            }

            if (settings.Talisman && item.Path.Contains("Talisman"))
            {
                color = HUDSkin.TalismanColor;
                return true;
            }

            Sockets sockets = item.GetComponent<Sockets>();

            if (sockets.LargestLinkSize >= settings.MinLinks)
            {
                if (sockets.LargestLinkSize == 6)
                {
                    alertIcon = 3;
                }

                return true;
            }

            if (IsCraftingBase(mods.ItemLevel))
            {
                alertIcon = 2;
                return true;
            }

            if (sockets.NumberOfSockets >= settings.MinSockets)
            {
                alertIcon = 0;
                return true;
            }

            if (settings.Rgb && sockets.IsRGB)
            {
                alertIcon = 1;
                return true;
            }

            if (settings.Jewels && item.Path.Contains("Jewels"))
            {
                return true;
            }

            switch (rarity)
            {
                case ItemRarity.Rare:
                    return settings.Rares;
                case ItemRarity.Unique:
                    return settings.Uniques;
            }

            if (!qualitySettings.Enable)
            {
                return false;
            }

            if (qualitySettings.Flask.Enable && item.HasComponent<Flask>())
            {
                return quality >= qualitySettings.Flask.MinQuality;
            }

            if (qualitySettings.SkillGem.Enable && item.HasComponent<SkillGem>())
            {
                color = HUDSkin.SkillGemColor;
                return quality >= qualitySettings.SkillGem.MinQuality;
            }

            if (qualitySettings.Weapon.Enable && item.HasComponent<Weapon>())
            {
                return quality >= qualitySettings.Weapon.MinQuality;
            }

            if (qualitySettings.Armour.Enable && item.HasComponent<Armour>())
            {
                return quality >= qualitySettings.Armour.MinQuality;
            }

            return false;
        }

        private bool IsCraftingBase(int itemLevel)
        {
            return !string.IsNullOrEmpty(craftingBase.Name) && itemLevel >= craftingBase.MinItemLevel && quality >= craftingBase.MinQuality && (craftingBase.Rarities == null || craftingBase.Rarities.Contains(rarity));
        }
    }
}
