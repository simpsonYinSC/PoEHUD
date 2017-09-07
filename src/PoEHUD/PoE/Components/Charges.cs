namespace PoEHUD.PoE.Components
{
    public class Charges : Component
    {
        public int NumberOfCharges => Address != 0 ? Memory.ReadInt(Address + 0x18) : 0;
        public int ChargesPerUse => Address != 0 ? Memory.ReadInt(Address + 0x10, 0x14) : 0;
        public int ChargesMaximum => Address != 0 ? Memory.ReadInt(Address + 0x10, 0x10) : 0;
    }
}
