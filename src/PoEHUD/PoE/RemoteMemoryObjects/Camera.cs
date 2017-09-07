using System;
using System.Numerics;
using PoEHUD.Models;
using PoEHUD.PoE.Components;
using Vector2 = SharpDX.Vector2;
using Vector3 = SharpDX.Vector3;
using Vector4 = System.Numerics.Vector4;

namespace PoEHUD.PoE.RemoteMemoryObjects
{
    public class Camera : RemoteMemoryObject
    {
        // cameraarray 0x17c
        private static Vector2 oldplayerCord;

        public int Width => Memory.ReadInt(Address + 0x4);
        public int Height => Memory.ReadInt(Address + 0x8);
        public float ZFar => Memory.ReadFloat(Address + 0x204);
        public Vector3 Position => new Vector3(Memory.ReadFloat(Address + 0x15C), Memory.ReadFloat(Address + 0x160), Memory.ReadFloat(Address + 0x164));

        public unsafe Vector2 WorldToScreen(Vector3 vec3, EntityWrapper entityWrapper)
        {
            Entity localPlayer = Game.IngameState.Data.LocalPlayer;
            bool isPlayer = localPlayer.Address == entityWrapper.Address && localPlayer.IsValid;
            bool playerMoving = isPlayer && localPlayer.GetComponent<Actor>().IsMoving;
            float x, y;
            long address = Address + 0xE4;
            fixed (byte* numRef = Memory.ReadBytes(address, 0x40))
            {
                Matrix4x4 matrix = *(Matrix4x4*)numRef;
                Vector4 cord = *(Vector4*)&vec3;
                cord.W = 1;
                cord = Vector4.Transform(cord, matrix);
                cord = Vector4.Divide(cord, cord.W);
                x = (cord.X + 1.0f) * 0.5f * Width;
                y = (1.0f - cord.Y) * 0.5f * Height;
            }

            var resultCord = new Vector2(x, y);
            if (playerMoving)
            {
                if (Math.Abs(oldplayerCord.X - resultCord.X) < 40 || Math.Abs(oldplayerCord.X - resultCord.Y) < 40)
                {
                    resultCord = oldplayerCord;
                }
                else
                {
                    oldplayerCord = resultCord;
                }
            }
            else if (isPlayer)
            {
                oldplayerCord = resultCord;
            }

            return resultCord;
        }
    }
}
