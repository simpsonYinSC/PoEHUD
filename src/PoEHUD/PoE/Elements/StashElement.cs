using System.Collections.Generic;
using PoEHUD.PoE.RemoteMemoryObjects;

namespace PoEHUD.PoE.Elements
{
    public class StashElement : Element
    {
        public long TotalStashes => StashInventoryPanel.ChildCount;

        // Children[0]
        public Element ExitButton => Address != 0 ? GetObject<Element>(Memory.ReadLong(Address + 0xA88)) : null;

        public Element ViewAllStashPanel => Address != 0 ? GetObject<Element>(Memory.ReadLong(StashListPanel.Address + 0x10, 0xB58)) : null;

        // Children[2].Children[0].Children[1].Children[2]
        public Element ViewAllStashButton => Address != 0 ? GetObject<Element>(Memory.ReadLong(Address + 0xAA8, 0xb90)) : null;

        // TODO: Gonna convert this to Address + offset mode later on.
        /*
        public Element SearchBar => Address != 0 ? this.Children[3].Children[3].Children[0] : null;
        public Element NextStashButton => Address != 0 ? this.Children[2].Children[0].Children[1].Children[5] : null;
        public Element PreviousStashButton => Address != 0 ? this.Children[2].Children[0].Children[1].Children[4] : null;
        public Element ClearSearchButton => Address != 0 ? this.Children[3].Children[3].Children[1] : null;
        */

        public Inventory VisibleStash => GetVisibleStash();
        public List<string> AllStashNames => GetAllStashNames();

        // Children[2].Children[0].Children[1].Children[0]
        private Element StashTitlePanel => Address != 0 ? GetObject<Element>(Memory.ReadLong(Address + 0xAA8, 0x10, 0xb60)) : null;

        // Children[2].Children[0].Children[1].Children[1]
        private Element StashInventoryPanel => Address != 0 ? GetObject<Element>(Memory.ReadLong(Address + 0xAA8, 0x10, 0xb70)) : null;

        // Children[2].Children[0].Children[1].Children[3]
        private Element StashListPanel => Address != 0 ? GetObject<Element>(Memory.ReadLong(Address + 0xAA8, 0x10, 0xb80)) : null;

        public Inventory GetStashInventoryByIndex(int index)
        {
            if (index >= TotalStashes)
            {
                return null;
            }

            return StashInventoryPanel.Children[index].ChildCount != 0 
                ? StashInventoryPanel.Children[index].Children[0].Children[0].AsObject<Inventory>()
                : null;
        }

        public string GetStashName(int index)
        {
            if (index >= TotalStashes || index < 0)
            {
                return string.Empty;
            }

            long add = ViewAllStashPanel.Children[index].Address;
            int nameLength = Memory.ReadInt(add + 0x690, 0x658);
            return nameLength < 8
                ? Memory.ReadStringU(Memory.ReadLong(add + 0x690) + 0x648, nameLength * 2)
                : Memory.ReadStringU(Memory.ReadLong(add + 0x690, 0x648), nameLength * 2);
        }

        private Inventory GetVisibleStash()
        {
            for (int i = 0; i < TotalStashes; i++)
            {
                if (StashInventoryPanel.Children[i].ChildCount == 0)
                {
                    continue;
                }

                Inventory ret = StashInventoryPanel.Children[i].Children[0].Children[0].AsObject<Inventory>();
                if (ret.InventoryUIElement.IsVisible)
                {
                    return ret;
                }
            }

            return null;
        }

        private List<string> GetAllStashNames()
        {
            List<string> ret = new List<string>();
            for (int i = 0; i < TotalStashes; i++)
            {
                ret.Add(GetStashName(i));
            }

            return ret;
        }

        // Making it private because currently no plugin use it.
        private Element GetStashTitleElement(string stashName)
        {
            if (stashName == string.Empty)
            {
                return null;
            }

            for (int i = 0; i < StashTitlePanel.ChildCount; i++)
            {
                if (StashTitlePanel.Children[i].ChildCount == 0)
                {
                    continue;
                }

                long address = StashTitlePanel.Children[i].Children[0].Address;
                if (address == 0)
                {
                    continue;
                }

                int nameLength = Memory.ReadInt(address + 0x390, 0x830);
                string text = nameLength < 8
                    ? Memory.ReadStringU(Memory.ReadLong(address + 0x390) + 0x820, nameLength * 2)
                    : Memory.ReadStringU(Memory.ReadLong(address + 0x390, 0x820), nameLength * 2);
                if (text == stashName)
                {
                    return StashTitlePanel.Children[i];
                }
            }

            return null;
        }
    }
}
