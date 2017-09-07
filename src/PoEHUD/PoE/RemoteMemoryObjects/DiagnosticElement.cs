namespace PoEHUD.PoE.RemoteMemoryObjects
{
    public class DiagnosticElement : RemoteMemoryObject
    {
        public long DiagnosticArray => Memory.ReadLong(Address + 0x0);
        public float CurrentValue => Memory.ReadFloat(DiagnosticArray + 0x13C);
        public int X => Memory.ReadInt(Address + 0x8);
        public int Y => Memory.ReadInt(Address + 0xC);
        public int Width => Memory.ReadInt(Address + 0x10);
        public int Height => Memory.ReadInt(Address + 0x14);
    }
}
