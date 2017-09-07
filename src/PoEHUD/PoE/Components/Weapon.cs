namespace PoEHUD.PoE.Components
{
    public class Weapon : Component
    {
        public int DamageMinimum => Address != 0 ? Memory.ReadInt(Address + 0x28, 0x14) : 0;
        public int DamageMaximum => Address != 0 ? Memory.ReadInt(Address + 0x28, 0x18) : 0;
        public int AttackTime => Address != 0 ? Memory.ReadInt(Address + 0x28, 0x1C) : 1;
        public int CriticalChance => Address != 0 ? Memory.ReadInt(Address + 0x28, 0x20) : 0;
    }
}
