using System;
using System.Collections.Generic;
using System.Linq;
using PoEHUD.Controllers;
using PoEHUD.Framework.Helpers;
using PoEHUD.HUD.UI;
using PoEHUD.PoE;
using PoEHUD.PoE.Components;
using PoEHUD.PoE.RemoteMemoryObjects;
using SharpDX;

namespace PoEHUD.HUD.Icons
{
    public class MinimapPlugin : Plugin<MapIconsSettings>
    {
        private readonly Func<IEnumerable<MapIcon>> getIcons;

        public MinimapPlugin(GameController gameController, Graphics graphics, Func<IEnumerable<MapIcon>> gatherMapIcons, MapIconsSettings settings) : base(gameController, graphics, settings)
        {
            getIcons = gatherMapIcons;
        }

        public override void Render()
        {
            try
            {
                if (!Settings.Enable || !GameController.InGame || !Settings.IconsOnMinimap)
                {
                    return;
                }

                Element smallMinimap = GameController.Game.IngameState.IngameUI.Map.SmallMinimap;
                if (!smallMinimap.IsVisible)
                {
                    return;
                }

                Vector2 playerPos = GameController.Player.GetComponent<Positioned>().GridPosition;
                float posZ = GameController.Player.GetComponent<Render>().Z;

                const float scale = 240f;
                RectangleF mapRect = smallMinimap.GetClientRect();
                var mapCenter = new Vector2(mapRect.X + mapRect.Width / 2, mapRect.Y + mapRect.Height / 2).Translate(0, 0);
                double diag = Math.Sqrt(mapRect.Width * mapRect.Width + mapRect.Height * mapRect.Height) / 2.0;
                foreach (MapIcon icon in getIcons().Where(x => x.IsVisible()))
                {
                    float iconZ = icon.EntityWrapper.GetComponent<Render>().Z;
                    Vector2 point = mapCenter
                        + MapIcon.DeltaInWorldToMinimapDelta(icon.WorldPosition - playerPos, diag, scale, (iconZ - posZ) / 20);

                    HUDTexture texture = icon.TextureIcon;
                    float size = icon.Size;
                    var rect = new RectangleF(point.X - size / 2f, point.Y - size / 2f, size, size);
                    mapRect.Contains(ref rect, out bool isContain);
                    if (isContain)
                    {
                        texture.Draw(Graphics, rect);
                    }
                }
            }
            catch
            {
                // ignore
            }
        }
    }
}
