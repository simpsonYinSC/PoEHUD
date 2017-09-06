namespace PoEHUD.PoE.Components
{
    public class RenderItem : Component
    {
        public string ResourcePath => Memory.ReadStringU(Memory.ReadLong(Address + 0x20));
    }
}
