using System;
using System.Collections.Generic;
using System.Linq;
using PoEHUD.Controllers;
using PoEHUD.Framework.Helpers;
using PoEHUD.HUD.UI;
using PoEHUD.PoE.Components;
using PoEHUD.PoE.RemoteMemoryObjects;
using SharpDX;
using Map = PoEHUD.PoE.Elements.Map;

namespace PoEHUD.HUD.Icons
{
    public class LargeMapPlugin : Plugin<MapIconsSettings>
    {
        private readonly Func<IEnumerable<MapIcon>> getIcons;

        public LargeMapPlugin(GameController gameController, Graphics graphics, Func<IEnumerable<MapIcon>> gatherMapIcons, MapIconsSettings settings) : base(gameController, graphics, settings)
        {
            getIcons = gatherMapIcons;
        }

        public override void Render()
        {
            try
            {
                if (!Settings.Enable || !GameController.InGame || !Settings.IconsOnLargeMap || !GameController.Game.IngameState.IngameUI.Map.LargeMap.IsVisible)
                {
                    return;
                }

                Camera camera = GameController.Game.IngameState.Camera;
                Map mapWindow = GameController.Game.IngameState.IngameUI.Map;
                RectangleF mapRect = mapWindow.GetClientRect();

                Vector2 playerPos = GameController.Player.GetComponent<Positioned>().GridPosition;
                float posZ = GameController.Player.GetComponent<Render>().Z;
                Vector2 screenCenter = new Vector2(mapRect.Width / 2, mapRect.Height / 2).Translate(0, -20)
                                       + new Vector2(mapRect.X, mapRect.Y)
                                       + new Vector2(mapWindow.LargeMapShiftX, mapWindow.LargeMapShiftY);
                var diag = (float)Math.Sqrt(camera.Width * camera.Width + camera.Height * camera.Height);
                float k = camera.Width < 1024f ? 1120f : 1024f;
                float scale = k / camera.Height * camera.Width * 3f / 4f / mapWindow.LargeMapZoom;

                foreach (MapIcon icon in getIcons().Where(x => x.IsVisible()))
                {
                    float iconZ = icon.EntityWrapper.GetComponent<Render>().Z;
                    Vector2 point = screenCenter + MapIcon.DeltaInWorldToMinimapDelta(icon.WorldPosition - playerPos, diag, scale, (iconZ - posZ) / 20);
                    HUDTexture texture = icon.TextureIcon;
                    float size = icon.Size * 2;
                    texture.Draw(Graphics, new RectangleF(point.X - size / 2f, point.Y - size / 2f, size, size));
                }
            }
            catch
            {
                // ignore
            }
        }
    }
}
