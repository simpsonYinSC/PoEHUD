namespace PoEHUD.PoE.Components
{
    public class Quality : Component
    {
        public int ItemQuality => Address != 0 ? Memory.ReadInt(Address + 0x18) : 0;
    }
}
