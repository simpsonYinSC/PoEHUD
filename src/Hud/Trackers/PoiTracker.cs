using System.Collections.Generic;
using PoEHUD.Controllers;
using PoEHUD.HUD.UI;
using PoEHUD.Models;
using PoEHUD.PoE.Components;

namespace PoEHUD.HUD.Trackers
{
    public class PoITracker : PluginWithMapIcons<PoITrackerSettings>
    {
        private static readonly List<string> Masters = new List<string>
        {
            "Metadata/NPC/Missions/Wild/Dex",
            "Metadata/NPC/Missions/Wild/DexInt",
            "Metadata/NPC/Missions/Wild/Int",
            "Metadata/NPC/Missions/Wild/Str",
            "Metadata/NPC/Missions/Wild/StrDex",
            "Metadata/NPC/Missions/Wild/StrDexInt",
            "Metadata/NPC/Missions/Wild/StrInt"
        };

        private static readonly List<string> Cadiro = new List<string>
        {
            "Metadata/NPC/League/Cadiro"
        };

        private static readonly List<string> Perandus = new List<string>
        {
            "Metadata/Chests/PerandusChests/PerandusChestStandard",
            "Metadata/Chests/PerandusChests/PerandusChestRarity",
            "Metadata/Chests/PerandusChests/PerandusChestQuantity",
            "Metadata/Chests/PerandusChests/PerandusChestCoins",
            "Metadata/Chests/PerandusChests/PerandusChestJewellery",
            "Metadata/Chests/PerandusChests/PerandusChestGems",
            "Metadata/Chests/PerandusChests/PerandusChestCurrency",
            "Metadata/Chests/PerandusChests/PerandusChestInventory",
            "Metadata/Chests/PerandusChests/PerandusChestDivinationCards",
            "Metadata/Chests/PerandusChests/PerandusChestKeepersOfTheTrove",
            "Metadata/Chests/PerandusChests/PerandusChestUniqueItem",
            "Metadata/Chests/PerandusChests/PerandusChestMaps",
            "Metadata/Chests/PerandusChests/PerandusChestFishing",
            "Metadata/Chests/PerandusChests/PerandusManorUniqueChest",
            "Metadata/Chests/PerandusChests/PerandusManorCurrencyChest",
            "Metadata/Chests/PerandusChests/PerandusManorMapsChest",
            "Metadata/Chests/PerandusChests/PerandusManorJewelryChest",
            "Metadata/Chests/PerandusChests/PerandusManorDivinationCardsChest",
            "Metadata/Chests/PerandusChests/PerandusManorLostTreasureChest"
        };

        public PoITracker(GameController gameController, Graphics graphics, PoITrackerSettings settings) : base(gameController, graphics, settings)
        {
        }

        public override void Render()
        {
            if (!Settings.Enable)
            {
            }
        }

        protected override void OnEntityAdded(EntityWrapper entity)
        {
            if (!Settings.Enable)
            {
                return;
            }

            MapIcon icon = GetMapIcon(entity);
            if (icon != null)
            {
                CurrentIcons[entity] = icon;
            }
        }

        private MapIcon GetMapIcon(EntityWrapper e)
        {
            if (e.HasComponent<NPC>() && Masters.Contains(e.Path))
            {
                return new CreatureMapIcon(e, "ms-cyan.png", () => Settings.Masters, Settings.MastersIcon);
            }

            if (e.HasComponent<NPC>() && Cadiro.Contains(e.Path))
            {
                return new CreatureMapIcon(e, "ms-green.png", () => Settings.Cadiro, Settings.CadiroIcon);
            }

            if (e.HasComponent<Chest>() && Perandus.Contains(e.Path))
            {
                return new ChestMapIcon(e, new HUDTexture("strongbox.png", Settings.PerandusChestColor), () => Settings.PerandusChest, Settings.PerandusChestIcon);
            }

            if (!e.HasComponent<Chest>() || e.GetComponent<Chest>().IsOpened)
            {
                return null;
            }

            if (e.Path.Contains("BreachChest"))
            {
                return new ChestMapIcon(e, new HUDTexture("strongbox.png", Settings.BreachChestColor), () => Settings.BreachChest, Settings.BreachChestIcon);
            }

            return e.GetComponent<Chest>().IsStrongbox
                ? new ChestMapIcon(e, new HUDTexture("strongbox.png", e.GetComponent<ObjectMagicProperties>().Rarity), () => Settings.Strongboxes, Settings.StrongboxesIcon)
                : new ChestMapIcon(e, new HUDTexture("chest.png"), () => Settings.Chests, Settings.ChestsIcon);
        }
    }
}
