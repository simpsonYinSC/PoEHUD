namespace PoEHUD.PoE.RemoteMemoryObjects
{
    public class Buff : RemoteMemoryObject
    {
        public string Name => Memory.ReadStringU(Memory.ReadLong(Address + 8, 0));
        public byte Charges => Memory.ReadByte(Address + 0x2C);
        public int SkillId => Memory.ReadInt(Address + 0x50);
        public float Timer => Memory.ReadFloat(Address + 0x14);
    }
}
