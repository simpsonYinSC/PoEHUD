using System.Collections.Generic;
using System.Linq;
using PoEHUD.Models.Enums;
using PoEHUD.PoE.Elements.InventoryElements;

namespace PoEHUD.PoE.RemoteMemoryObjects
{
    public class Inventory : RemoteMemoryObject
    {
        public long ItemCount => Memory.ReadLong(Address + 0x410, 0x630, 0x50);
        public InventoryType InventoryType => GetInventoryType();
        public Element InventoryUIElement => GetInventoryElement();

        // Shows Item details of visible inventory/stashes
        public List<NormalInventoryItem> VisibleInventoryItems
        {
            get
            {
                var inventoryRoot = InventoryUIElement;
                if (inventoryRoot == null)
                {
                    return null;
                }

                if (!inventoryRoot.IsVisible)
                {
                    return null;
                }

                var list = new List<NormalInventoryItem>();
                switch (InventoryType)
                {
                    case InventoryType.PlayerInventory:
                    case InventoryType.NormalStash:
                    case InventoryType.QuadStash:
                        list.AddRange(inventoryRoot.Children.Select(item => item.AsObject<NormalInventoryItem>()));
                        break;
                    case InventoryType.CurrencyStash:
                        foreach (var item in inventoryRoot.Children)
                        {
                            if (item.ChildCount > 0)
                            {
                                list.Add(item.Children[0].AsObject<CurrencyInventoryItem>());
                            }
                        }

                        break;
                    case InventoryType.EssenceStash:
                        foreach (var item in inventoryRoot.Children)
                        {
                            if (item.ChildCount > 0)
                            {
                                list.Add(item.Children[0].AsObject<EssenceInventoryItem>());
                            }
                        }

                        break;
                    case InventoryType.DivinationStash:
                        foreach (var item in inventoryRoot.Children)
                        {
                            if (item.Children[1].ChildCount > 0)
                            {
                                list.Add(item.Children[1].Children[0].AsObject<DivinationInventoryItem>());
                            }
                        }

                        break;
                }

                return list;
            }
        }

        // Works even if inventory is currently not in view.
        // As long as game have fetched inventory data from Server.
        // Will return the item based on x,y format.
        // Give more controll to user what to do with
        // dublicate items (items taking more than 1 slot)
        // or slots where items doesn't exists (return null).
        public Entity this[int x, int y, int xLength]
        {
            get
            {
                long invAddr = Memory.ReadLong(Address + 0x410, 0x630, 0x30);
                y = y * xLength;
                long itmAddr = Memory.ReadLong(invAddr + (x + y) * 8);
                return itmAddr <= 0 ? null : ReadObject<Entity>(itmAddr);
            }
        }

        private Element GetInventoryElement()
        {
            switch (InventoryType)
            {
                case InventoryType.PlayerInventory:
                case InventoryType.NormalStash:
                case InventoryType.QuadStash:
                    return AsObject<Element>();
                case InventoryType.CurrencyStash:
                case InventoryType.EssenceStash:
                    return AsObject<Element>().Parent;
                case InventoryType.DivinationStash:
                    //// return this.AsObject<Element>().Children[1]; // - throws an errors (out of range exception)
                    var elmnt = AsObject<Element>();
                    return elmnt.ChildCount > 0 ? elmnt.Children[1] : elmnt;
                default:
                    return null;
            }
        }

        private InventoryType GetInventoryType()
        {
            // For PoE MemoryLeak bug where ChildCount of PlayerInventory keep
            // Increasing on Area/Map Change. Ref:
            // http://www.ownedcore.com/forums/mmo/path-of-exile/poe-bots-programs/511580-poehud-overlay-updated-362.html#post3718876
            // Orriginal Value of ChildCount should be 0x18
            for (int j = 1; j < InventoryList.InventoryCount; j++)
            {
                if (Game.IngameState.IngameUI.InventoryPanel[(InventoryIndex)j].Address == Address)
                {
                    return InventoryType.PlayerInventory;
                }
            }

            switch (AsObject<Element>().Parent.ChildCount)
            {
                case 0x6f:
                    return InventoryType.EssenceStash;
                case 0x36:
                    return InventoryType.CurrencyStash;
                case 0x05:
                    return InventoryType.DivinationStash;
                case 0x01:
                    // Normal Stash and Quad Stash is same.
                    return InventoryType.NormalStash;
                default:
                    return InventoryType.InvalidInventory;
            }
        }
    }
}
