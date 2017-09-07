namespace PoEHUD.PoE.Components
{
    public class CurrencyInfo : Component
    {
        public int MaximumStackSize => Address != 0 ? Memory.ReadInt(Address + 0x24) : 0;
    }
}
