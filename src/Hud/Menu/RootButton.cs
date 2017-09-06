using System;
using System.Linq;
using PoEHUD.Framework.Helpers;
using PoEHUD.HUD.UI;
using SharpDX;
using SharpDX.Direct3D9;

namespace PoEHUD.HUD.Menu
{
    public sealed class RootButton : MenuItem
    {
        public static RootButton Instance;
        private MenuItem currentHover;
        private bool visible;

        public RootButton(Vector2 position)
        {
            Bounds = new RectangleF(position.X - 5, position.Y - 3, DesiredWidth, DesiredHeight);
            Instance = this;
        }

        public event Action ExternalOnClose = delegate { };

        public override int DesiredWidth => 80;
        public override int DesiredHeight => 25;

        public override void AddChild(MenuItem item)
        {
            base.AddChild(item);
            float x = item.Bounds.X - DesiredWidth;
            float y = item.Bounds.Y + DesiredHeight;
            item.Bounds = new RectangleF(x, y, item.Bounds.Width, item.Bounds.Height);
        }

        public bool OnMouseEvent(MouseEventId id, Vector2 pos)
        {
            if (currentHover != null && currentHover.TestHit(pos))
            {
                currentHover.OnEvent(id, pos);
                return id != MouseEventId.MouseMove;
            }

            if (id == MouseEventId.MouseMove)
            {
                MenuItem button = Children.FirstOrDefault(b => b.TestHit(pos));
                if (button == null)
                {
                    return false;
                }

                currentHover?.SetHovered(false);
                currentHover = button;
                button.SetHovered(true);

                return false;
            }

            if (!Bounds.Contains(pos) || id != MouseEventId.LeftButtonDown)
            {
                return false;
            }

            CloseRootMenu();
            return true;
        }

        public void CloseRootMenu()
        {
            visible = !visible;
            if (!visible)
            {
                ExternalOnClose();
            }

            currentHover = null;
            Children.ForEach(button => button.SetVisible(visible));
        }

        public override void Render(Graphics graphics, MenuSettings settings)
        {
            graphics.DrawText(settings.TitleName, settings.TitleFontSize, Bounds.TopLeft.Translate(25, 12), settings.TitleFontColor, FontDrawFlags.VerticalCenter | FontDrawFlags.Center);
            graphics.DrawImage("menu-background.png", Bounds, settings.BackgroundColor);
            Children.ForEach(x => x.Render(graphics, settings));
        }

        protected override void HandleEvent(MouseEventId id, Vector2 pos)
        {
        }
    }
}
