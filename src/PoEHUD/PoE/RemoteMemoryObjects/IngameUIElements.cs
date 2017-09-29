using System.Collections.Generic;
using PoEHUD.PoE.Elements;

namespace PoEHUD.PoE.RemoteMemoryObjects
{
    public class IngameUIElements : RemoteMemoryObject
    {
        public Element QuestTracker => ReadObjectAt<Element>(0xBF8);
        public Element OpenLeftPanel => ReadObjectAt<Element>(0xC38);
        public Element OpenRightPanel => ReadObjectAt<Element>(0xC40);
        public InventoryElement InventoryPanel => ReadObjectAt<InventoryElement>(0xC70);
        public Element TreePanel => ReadObjectAt<Element>(0xCA0);
        public Element AtlasPanel => ReadObjectAt<Element>(0xCA8);
        public Map Map => ReadObjectAt<Map>(0xCF8);
        public IEnumerable<ItemsOnGroundLabelElement> ItemsOnGroundLabels
        {
            get
            {
                var itemsOnGroundLabelRoot = ReadObjectAt<ItemsOnGroundLabelElement>(0xD00);
                return itemsOnGroundLabelRoot.Children;
            }
        }

        public Element GemLevelUpPanel => ReadObjectAt<Element>(0xF00);
        public ItemOnGroundTooltip ItemOnGroundTooltip => ReadObjectAt<ItemOnGroundTooltip>(0xF68);
    }
}
