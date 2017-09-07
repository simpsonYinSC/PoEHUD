using PoEHUD.Models.Enums;
using PoEHUD.PoE.RemoteMemoryObjects;

namespace PoEHUD.PoE.Elements
{
    public class InventoryElement : Element
    {
        private InventoryList AllInventories => GetObjectAt<InventoryList>(OffsetBuffers + 0x424);
        public Inventory this[InventoryIndex k] => AllInventories[k];
    }
}
