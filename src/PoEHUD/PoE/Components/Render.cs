using SharpDX;

namespace PoEHUD.PoE.Components
{
    public class Render : Component
    {
        public float X => Address != 0 ? Memory.ReadFloat(Address + 0x70) : 0f;
        public float Y => Address != 0 ? Memory.ReadFloat(Address + 0x74) : 0f;
        public float Z => Address != 0 ? Memory.ReadFloat(Address + 0x78) : 0f;
        public Vector3 Position => new Vector3(X, Y, Z);
    }
}
