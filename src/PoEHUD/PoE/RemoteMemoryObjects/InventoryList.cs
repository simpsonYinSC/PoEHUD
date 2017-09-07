using PoEHUD.Models.Enums;

namespace PoEHUD.PoE.RemoteMemoryObjects
{
    public class InventoryList : RemoteMemoryObject
    {
        public static int InventoryCount => 15;

        public Inventory this[InventoryIndex inv]
        {
            get
            {
                var num = (int)inv;
                if (num < 0 || num >= InventoryCount)
                {
                    return null;
                }

                return ReadObjectAt<Inventory>(num * 8);
            }
        }
    }
}
