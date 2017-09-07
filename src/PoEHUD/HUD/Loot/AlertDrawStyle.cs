using System.Collections.Generic;
using PoEHUD.Models.Enums;
using SharpDX;

namespace PoEHUD.HUD.Loot
{
    public sealed class AlertDrawStyle
    {
        public static readonly Color DefaultBackgroundColor = new ColorBGRA(0, 0, 0, 180);

        private static readonly Dictionary<ItemRarity, Color> Colors = new Dictionary<ItemRarity, Color>
        {
            { ItemRarity.Normal, Color.White },
            { ItemRarity.Magic, HUDSkin.MagicColor },
            { ItemRarity.Rare, HUDSkin.RareColor },
            { ItemRarity.Unique, HUDSkin.UniqueColor }
        };

        public AlertDrawStyle(object colorRef, int borderWidth, string text, int iconIndex)
        {
            BorderWidth = borderWidth;
            Text = text;
            IconIndex = iconIndex;

            if (colorRef is Color)
            {
                TextColor = (Color)colorRef;
            }
            else
            {
                TextColor = GetTextColorByRarity((ItemRarity)colorRef);
            }

            BorderColor = TextColor;
            BackgroundColor = DefaultBackgroundColor;
        }

        public AlertDrawStyle(string text, Color textColor, int borderWidth, Color borderColor, Color backgroundColor, int iconIndex)
        {
            TextColor = textColor;
            BorderWidth = borderWidth;
            BorderColor = borderColor;
            Text = text;
            IconIndex = iconIndex;
            BackgroundColor = backgroundColor;
        }

        public Color TextColor { get; }
        public int BorderWidth { get; private set; }
        public Color BorderColor { get; private set; }
        public Color BackgroundColor { get; private set; }
        public string Text { get; private set; }
        public int IconIndex { get; private set; }

        public static Color GetTextColorByRarity(ItemRarity itemRarity)
        {
            return Colors.TryGetValue(itemRarity, out Color tempColor) ? tempColor : Color.White;
        }
    }
}
