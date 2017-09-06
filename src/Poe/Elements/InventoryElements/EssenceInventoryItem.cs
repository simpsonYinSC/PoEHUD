using SharpDX;

namespace PoEHUD.PoE.Elements.InventoryElements
{
    public class EssenceInventoryItem : NormalInventoryItem
    {
        // Inventory Position in Essence Stash is always invalid.
        // Also, as items are fixed, so Inventory Position doesn't matter.
        public override int InventoryPositionX => 0;
        public override int InventoryPositionY => 0;

        public override RectangleF GetClientRect()
        {
            return Parent.GetClientRect();
        }
    }
}
