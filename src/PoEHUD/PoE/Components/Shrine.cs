namespace PoEHUD.PoE.Components
{
    public class Shrine : Component
    {
        public bool IsAvailable => Address != 0 && Memory.ReadByte(Address + 0x1c) == 0;
    }
}
