namespace PoEHUD.PoE.Components
{
    public class AttributeRequirements : Component
    {
        public int Strength => Address != 0 ? Memory.ReadInt(Address + 0x10, 0x10) : 0;
        public int Dexterity => Address != 0 ? Memory.ReadInt(Address + 0x10, 0x14) : 0;
        public int Intelligence => Address != 0 ? Memory.ReadInt(Address + 0x10, 0x18) : 0;
    }
}
