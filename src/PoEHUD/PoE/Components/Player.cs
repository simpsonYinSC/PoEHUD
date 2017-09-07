namespace PoEHUD.PoE.Components
{
    public class Player : Component
    {
        public string PlayerName
        {
            get
            {
                if (Address == 0)
                {
                    return string.Empty;
                }

                int nameLength = Memory.ReadInt(Address + 0x30);
                if (nameLength > 512)
                {
                    return string.Empty;
                }

                return nameLength < 8 ? Memory.ReadStringU(Address + 0x20, nameLength * 2) : Memory.ReadStringU(Memory.ReadLong(Address + 0x20), nameLength * 2);
            }
        }

        public uint XP => Address != 0 ? Memory.ReadUInt(Address + 0x48) : 0;
        public int Strength => Address != 0 ? Memory.ReadInt(Address + 0x4c) : 0;
        public int Dexterity => Address != 0 ? Memory.ReadInt(Address + 0x50) : 0;
        public int Intelligence => Address != 0 ? Memory.ReadInt(Address + 0x54) : 0;
        public int Level => Address != 0 ? Memory.ReadByte(Address + 0x58) : 1;
    }
}
