using System;
using PoEHUD.PoE.Components;

namespace PoEHUD.PoE.Elements
{
    public enum ToolTipType
    {
        None,
        InventoryItem,
        ItemOnGround,
        ItemInChat
    }

    // Don't confuse this class name with it's purpose.
    // Purpose of this class is to handle/deal with Hover Items, rather than
    // Inventory Item. Hovered items can be on Chat, inventory or on ground.
    // However, if item isn't being hover on then this class isn't reponsible
    // for getting it's info and might give incorrect result.
    public class HoverItemIcon : Element
    {
        private readonly Func<Element> inventoryItemTooltip;
        private readonly Func<Element> itemInChatTooltip;
        private readonly Func<ItemOnGroundTooltip> toolTipOnground;
        private ToolTipType? toolTip;

        public HoverItemIcon()
        {
            toolTipOnground = () => Game.IngameState.IngameUI.ItemOnGroundTooltip;
            inventoryItemTooltip = () => ReadObject<Element>(Address + 0xB10);
            itemInChatTooltip = () => ReadObject<Element>(Address + 0x7B8);
        }

        public int InventoryPositionX => Memory.ReadInt(Address + 0xb60);
        public int InventoryPositionY => Memory.ReadInt(Address + 0xb64);

        public ToolTipType ToolTipType
        {
            get
            {
                try
                {
                    return (ToolTipType)(toolTip ?? (toolTip = GetToolTipType()));
                }
                catch (Exception)
                {
                    return ToolTipType.None;
                }
            }
        }

        public Element Tooltip
        {
            get
            {
                switch (ToolTipType)
                {
                    case ToolTipType.ItemOnGround:
                        return toolTipOnground().Tooltip;
                    case ToolTipType.InventoryItem:
                        return inventoryItemTooltip();
                    case ToolTipType.ItemInChat:
                        return itemInChatTooltip();
                }

                return null;
            }
        }

        public Element ItemFrame
        {
            get
            {
                switch (ToolTipType)
                {
                    case ToolTipType.ItemOnGround:
                        return toolTipOnground().ItemFrame;
                    default:
                        return null;
                }
            }
        }

        public Entity Item
        {
            get
            {
                switch (ToolTipType)
                {
                    case ToolTipType.ItemOnGround:
                        ItemsOnGroundLabelElement le = Game.IngameState.IngameUI.ReadObjectAt<ItemsOnGroundLabelElement>(0xD00);
                        Entity e = le?.ItemOnHover;
                        return e?.GetComponent<WorldItem>().ItemEntity;
                    case ToolTipType.InventoryItem:
                        return ReadObject<Entity>(Address + 0xB58);
                }

                return null;
            }
        }

        private ToolTipType GetToolTipType()
        {
            if (inventoryItemTooltip() != null && inventoryItemTooltip().IsVisible)
            {
                return ToolTipType.InventoryItem;
            }

            if (toolTipOnground?.Invoke().Tooltip != null && toolTipOnground().TooltipUI != null && toolTipOnground().TooltipUI.IsVisible)
            {
                return ToolTipType.ItemOnGround;
            }
            
            return ToolTipType.None;
        }
    }
}
