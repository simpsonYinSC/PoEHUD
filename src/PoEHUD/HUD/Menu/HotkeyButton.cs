using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using PoEHUD.Framework;
using PoEHUD.HUD.Settings;
using PoEHUD.HUD.UI;
using SharpDX;
using SharpDX.Direct3D9;

namespace PoEHUD.HUD.Menu
{
    public class HotkeyButton : MenuItem
    {
        public readonly string Name;
        private readonly HotkeyNode node;
        private readonly IEnumerable<Keys> keyCodes;
        private bool keysScan;

        public HotkeyButton(string name, HotkeyNode node)
        {
            Name = name;
            this.node = node;

            keyCodes = Enum.GetValues(typeof(Keys)).Cast<Keys>();
        }

        public override int DesiredWidth => 180;
        public override int DesiredHeight => 25;

        public override void Render(Graphics graphics, MenuSettings settings)
        {
            if (!IsVisible)
            {
                return;
            }
    
            base.Render(graphics, settings);

            var textPosition = new Vector2(Bounds.X - 50 + Bounds.Width / 3, Bounds.Y + Bounds.Height / 2);

            string buttonDisplayName = Name + ": " + (keysScan ? "Press any key..." : "[" + node.Value + "]");
            graphics.DrawText(buttonDisplayName, settings.MenuFontSize, textPosition, settings.MenuFontColor, FontDrawFlags.VerticalCenter | FontDrawFlags.Left);
            graphics.DrawImage("menu-background.png", new RectangleF(Bounds.X, Bounds.Y, Bounds.Width, Bounds.Height), settings.BackgroundColor);

            if (Children.Count > 0)
            {
                float width = (Bounds.Width - 2) * 0.08f;
                float height = (Bounds.Height - 2) / 2;
                var imgRect = new RectangleF(Bounds.X + Bounds.Width - 1 - width, Bounds.Y + 1 + height - height / 2, width, height);
                graphics.DrawImage("menu-arrow.png", imgRect);
            }

            Children.ForEach(x => x.Render(graphics, settings));

            if (!keysScan)
            {
                return;
            }

            foreach (var key in keyCodes)
            {
                if (!WindowsAPI.IsKeyDown(key))
                {
                    continue;
                }

                if (key != Keys.Escape)
                {
                    node.Value = key;
                }

                keysScan = false;
                break;
            }
        }

        public override void SetHovered(bool hover)
        {
            if (!hover)
            {
                keysScan = false;
            }

            Children.ForEach(x =>
            {
                x.SetVisible(hover);
            });
        }

        protected override void HandleEvent(MouseEventId id, Vector2 pos)
        {
            if (id == MouseEventId.LeftButtonDown)
            {
                keysScan = true;
            }
        }
    }
}
