using PoeHUD.Poe.Elements;
using System.Collections.Generic;

namespace PoeHUD.Poe.RemoteMemoryObjects
{
    public class IngameUIElements : RemoteMemoryObject
    {
        public Element QuestTracker => ReadObjectAt<Element>(0x9C4);
        public Element OpenLeftPanel => ReadObjectAt<Element>(0x9F4);
        public Element OpenRightPanel => ReadObjectAt<Element>(0x9F8);
        public Elements.Inventory InventoryPanel => ReadObjectAt<Elements.Inventory>(0xA08);
        public Element TreePanel => ReadObjectAt<Element>(0xA20);
        public Element AtlasPanel => ReadObjectAt<Element>(0xA24);
        public Map Map => ReadObjectAt<Map>(0xA3C);
        public IEnumerable<ItemsOnGroundLabelElement> ItemsOnGroundLabels
        {
            get
            {
                var itemsOnGroundLabelRoot = ReadObjectAt<ItemsOnGroundLabelElement>(0xA40);
                return itemsOnGroundLabelRoot.Children;
            }
        }
        public Element GemLvlUpPanel => ReadObjectAt<Element>(0xB14);
        public ItemOnGroundTooltip ItemOnGroundTooltip => ReadObjectAt<ItemOnGroundTooltip>(0xB24);
    }
}
