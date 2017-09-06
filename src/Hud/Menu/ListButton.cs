using System.Collections.Generic;
using System.Linq;
using PoEHUD.Controllers;
using PoEHUD.HUD.Settings;
using PoEHUD.HUD.UI;
using SharpDX;
using SharpDX.Direct3D9;

namespace PoEHUD.HUD.Menu
{
    public class ListButton : MenuItem
    {
        public readonly string Name;
        private readonly ListNode node;
        private readonly List<MenuItem> subMenuValues;
        private List<string> listValues;
        private ToggleNode highlightedNode;

        public ListButton(string name, ListNode node)
        {
            Name = name;
            this.node = node;
            subMenuValues = new List<MenuItem>();
        }

        public override int DesiredWidth => 180;
        public override int DesiredHeight => 25;

        public void SetValues(List<string> values)
        {
            listValues = values;
            if (listValues == null)
            {
                return;
            }

            if (subMenuValues.Count > 0)
            {
                foreach (var child in subMenuValues)
                {
                    Children.Remove(child);
                }

                subMenuValues.Clear();
            }
      
            foreach (var listValue in listValues)
            {
                var buttonNode = new ToggleNode
                {
                    Value = node.Value == listValue
                };
                if (buttonNode.Value)
                {
                    highlightedNode = buttonNode;
                }

                buttonNode.OnValueChanged += delegate { ChangedValue(listValue, buttonNode); };
                var valueToggleButton = new ToggleButton(this, listValue, buttonNode, null, null);

                AddChild(valueToggleButton);
                subMenuValues.Add(valueToggleButton);
            }
    
            WrapChilds();
        }

        public override void Render(Graphics graphics, MenuSettings settings)
        {
            if (!IsVisible)
            {
                return;
            }

            base.Render(graphics, settings);

            var textPosition = new Vector2(Bounds.X - 50 + Bounds.Width / 3, Bounds.Y + Bounds.Height / 2);

            string buttonDisplayName = Name + ": [" + node.Value + "]";
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
        }

        public override void SetHovered(bool hover)
        {
            if (hover)
            {
                return;
            }

            HideChildren();
        }

        protected override void HandleEvent(MouseEventId id, Vector2 pos)
        {
            if (id != MouseEventId.LeftButtonDown)
            {
                return;
            }

            bool visible = Children.Any(x => x.IsVisible);
            Children.ForEach(x =>
            {
                x.SetVisible(!visible);
            });
        }

        private void WrapChilds()
        {
            var windowRect = GameController.Instance.Window.GetWindowRectangle();

            int wrapChildRowCount = (int)((windowRect.Height - Bounds.BottomLeft.Y) / DesiredHeight);
            wrapChildRowCount--;

            int childCount = Children.Count;
            int column = 1;
            int row = 0;
            var listBounds = Bounds;

            for (int i = 0; i < childCount; i++)
            {
                var child = Children[i];
                var posX = listBounds.X + DesiredWidth * column;
                var posY = listBounds.Y + DesiredHeight * row;

                child.Bounds = new RectangleF(posX, posY, DesiredWidth, DesiredHeight);
                row++;
                if (row <= wrapChildRowCount)
                {
                    continue;
                }

                row = 0;
                column++;
            }
        }

        private void ChangedValue(string value, ToggleNode pressedNode)
        {
            highlightedNode?.SetValueNoEvent(false);
            pressedNode.SetValueNoEvent(true);
            highlightedNode = pressedNode;

            node.Value = value;
            HideChildren();
        }

        private void HideChildren()
        {
            Children.ForEach(x =>
            {
                x.SetVisible(false);
            });
        }
    }
}
