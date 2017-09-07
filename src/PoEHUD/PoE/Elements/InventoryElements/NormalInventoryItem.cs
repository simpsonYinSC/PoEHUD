namespace PoEHUD.PoE.Elements.InventoryElements
{
    public class NormalInventoryItem : Element
    {
        public virtual int InventoryPositionX => Memory.ReadInt(Address + 0xb60);
        public virtual int InventoryPositionY => Memory.ReadInt(Address + 0xb64);
        public Entity Item => ReadObject<Entity>(Address + 0xB58);
        public ToolTipType ToolTipType => ToolTipType.InventoryItem;
        public Element ToolTip => ReadObject<Element>(Address + 0xB10);
    }
}
