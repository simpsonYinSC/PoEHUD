using System.Collections.Generic;
using System.Linq;
using PoEHUD.HUD.UI;
using SharpDX;

namespace PoEHUD.HUD.Menu
{
    public abstract class MenuItem
    {
        public readonly List<MenuItem> Children;
        public bool IsVisible;
        public string TooltipText;
        private static Vector2 mousePos;
        private MenuItem currentHover;

        protected MenuItem()
        {
            Children = new List<MenuItem>();
        }

        public RectangleF Bounds { get; set; }
        public abstract int DesiredWidth { get; }
        public abstract int DesiredHeight { get; }

        public virtual void AddChild(MenuItem item)
        {
            float x = Bounds.X + Bounds.Width;
            float y = Bounds.Y + Children.Sum(current => current.Bounds.Height);
            item.Bounds = new RectangleF(x, y, item.DesiredWidth, item.DesiredHeight);
            Children.Add(item);
        }

        public void OnEvent(MouseEventId id, Vector2 pos)
        {
            mousePos = pos;
            if (id == MouseEventId.MouseMove)
            {
                if (TestBounds(pos))
                {
                    HandleEvent(id, pos);
                    if (currentHover == null)
                    {
                        return;
                    }

                    currentHover.SetHovered(false);
                    currentHover = null;

                    return;
                }

                if (currentHover != null)
                {
                    if (currentHover.TestHit(pos))
                    {
                        currentHover.OnEvent(id, pos);
                        return;
                    }

                    currentHover.SetHovered(false);
                }

                MenuItem childAt = Children.FirstOrDefault(current => current.TestHit(pos));
                if (childAt != null)
                {
                    childAt.SetHovered(true);
                    currentHover = childAt;
                    return;
                }

                currentHover = null;
            }
            else
            {
                if (TestBounds(pos))
                {
                    HandleEvent(id, pos);
                }
                else
                {
                    currentHover?.OnEvent(id, pos);
                }
            }
        }

        public virtual void Render(Graphics graphics, MenuSettings settings)
        {
            if (!Bounds.Contains(mousePos) || string.IsNullOrEmpty(TooltipText))
            {
                return;
            }

            var tooltipRect = Bounds;
            tooltipRect.Y -= tooltipRect.Height + 10;
            tooltipRect.X += tooltipRect.Width;

            tooltipRect.Width = TooltipText.Length * 9 + 10;

            graphics.DrawBox(tooltipRect, new Color(0, 0, 0, 230));

            var buubleRect = new RectangleF(tooltipRect.X - 23, tooltipRect.Y, 23, 45);

            graphics.DrawImage("tooltip.png", buubleRect);

            graphics.DrawText(TooltipText, 20, tooltipRect.TopLeft + new Vector2(5, 0));
        }

        public virtual void SetHovered(bool hover)
        {
            Children.ForEach(x => x.SetVisible(hover));
        }

        public void SetVisible(bool visible)
        {
            IsVisible = visible;
            if (!visible)
            {
                Children.ForEach(x => x.SetVisible(false));
            }
        }

        public bool TestHit(Vector2 pos)
        {
            return IsVisible && (TestBounds(pos) || Children.Any(current => current.TestHit(pos)));
        }

        protected abstract void HandleEvent(MouseEventId id, Vector2 pos);

        protected virtual bool TestBounds(Vector2 pos)
        {
            return Bounds.Contains(pos);
        }
    }
}
