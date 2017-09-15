using System;
using PoEHUD.Models;
using PoEHUD.PoE.Components;
using SharpDX;

namespace PoEHUD.HUD
{
    public class MapIcon
    {
        private readonly Func<bool> show;

        public MapIcon(EntityWrapper entityWrapper, HUDTexture hudTexture, Func<bool> show, float iconSize = 10)
        {
            EntityWrapper = entityWrapper;
            TextureIcon = hudTexture;
            this.show = show;
            Size = iconSize;
        }

        public float? SizeOfLargeIcon { get; set; }
        public EntityWrapper EntityWrapper { get; }
        public HUDTexture TextureIcon { get; private set; }
        public float Size { get; private set; }
        public Vector2 WorldPosition => EntityWrapper.GetComponent<Positioned>().GridPosition;

        public static Vector2 DeltaInWorldToMinimapDelta(Vector2 delta, double diag, float scale, float deltaZ = 0)
        {
            const float cameraAngle = 38 * MathUtil.Pi / 180;

            // Values according to 40 degree rotation of cartesian coordiantes, still doesn't seem right but closer
            var cos = (float)(diag * Math.Cos(cameraAngle) / scale);
            var sin = (float)(diag * Math.Sin(cameraAngle) / scale); // possible to use cos so angle = nearly 45 degrees
            // 2D rotation formulas not correct, but it's what appears to work?
            return new Vector2((delta.X - delta.Y) * cos, deltaZ - (delta.X + delta.Y) * sin);
        }

        public virtual bool IsEntityStillValid()
        {
            return EntityWrapper.IsValid;
        }

        public virtual bool IsVisible()
        {
            return show();
        }
    }
}
