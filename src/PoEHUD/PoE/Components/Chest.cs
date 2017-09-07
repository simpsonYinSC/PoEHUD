namespace PoEHUD.PoE.Components
{
    public class Chest : Component
    {
        public bool IsOpened => Address != 0 && Memory.ReadByte(Address + 0x40) == 1;
        public bool IsStrongbox => Address != 0 && Memory.ReadInt(Address + 0x60) != 0;
    }
}
