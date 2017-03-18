using PoeHUD.Poe.Elements;
using System.Collections.Generic;

namespace PoeHUD.Poe.RemoteMemoryObjects
{
    public class IngameUIElements : RemoteMemoryObject
    {
        public Element QuestTracker => ReadObjectAt<Element>(0xB68);
        public Element OpenLeftPanel => ReadObjectAt<Element>(0xBA8);
        public Element OpenRightPanel => ReadObjectAt<Element>(0xBB0);
        public Elements.Inventory InventoryPanel => ReadObjectAt<Elements.Inventory>(0xBE0);
        public Element TreePanel => ReadObjectAt<Element>(0xC10);
        public Element AtlasPanel => ReadObjectAt<Element>(0xC18);
        public Map Map => ReadObjectAt<Map>(0xC48);
        public IEnumerable<ItemsOnGroundLabelElement> ItemsOnGroundLabels
        {
            get
            {
                var itemsOnGroundLabelRoot = ReadObjectAt<ItemsOnGroundLabelElement>(0xC50);
                return itemsOnGroundLabelRoot.Children;
            }
        }
        public Element GemLvlUpPanel => ReadObjectAt<Element>(0xDF8);
        public ItemOnGroundTooltip ItemOnGroundTooltip => ReadObjectAt<ItemOnGroundTooltip>(0xE18);
    }
}

