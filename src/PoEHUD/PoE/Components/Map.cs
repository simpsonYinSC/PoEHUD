namespace PoEHUD.PoE.Components
{
    public class Map : Component
    {
        public int Tier => Address != 0 ? Memory.ReadInt(Address + 0x10, 0x90) : 0;
    }
}
