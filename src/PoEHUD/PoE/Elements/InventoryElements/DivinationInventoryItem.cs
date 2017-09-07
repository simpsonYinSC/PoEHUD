using SharpDX;

namespace PoEHUD.PoE.Elements.InventoryElements
{
    public class DivinationInventoryItem : NormalInventoryItem
    {
        // Inventory Position in Essence Stash is always invalid.
        // Also, as items are fixed, so Inventory Position doesn't matter.
        public override int InventoryPositionX => 0;
        public override int InventoryPositionY => 0;

        public override RectangleF GetClientRect()
        {
            var tmp = Parent.GetClientRect();

            // div stash tab scrollbar element scroll value calculator
            var address = Parent.Parent.Parent.Parent.Children[2].Address + 0xA64;
            float sub = Memory.ReadInt(address) * (float)107.5;
            tmp.Y -= sub;

            return tmp;
        }
    }
}
