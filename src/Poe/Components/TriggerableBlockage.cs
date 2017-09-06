namespace PoEHUD.PoE.Components
{
    public class TriggerableBlockage : Component
    {
        public bool IsClosed => Address != 0 && Memory.ReadByte(Address + 0x30) == 1;
    }
}
