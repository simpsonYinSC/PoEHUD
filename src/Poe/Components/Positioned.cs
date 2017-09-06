using SharpDX;

namespace PoEHUD.PoE.Components
{
    public class Positioned : Component
    {
        public int GridX => Address != 0 ? Memory.ReadInt(Address + 0x20) : 0;
        public int GridY => Address != 0 ? Memory.ReadInt(Address + 0x24) : 0;
        public float X => Address != 0 ? Memory.ReadFloat(Address + 0x2c) : 0f;
        public float Y => Address != 0 ? Memory.ReadFloat(Address + 0x30) : 0f;
        public Vector2 GridPosition => new Vector2(GridX, GridY);
    }
}
