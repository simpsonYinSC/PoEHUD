namespace PoEHUD.PoE.Components
{
    public class Stack : Component
    {
        public int Size => Address == 0 ? 0 : Memory.ReadInt(Address + 0x18); // 0xC ?
        public CurrencyInfo Info => Address != 0 ? ReadObject<CurrencyInfo>(Address + 0x10) : null;
    }
}
