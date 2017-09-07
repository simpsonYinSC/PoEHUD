using PoEHUD.HUD.UI;
using PoEHUD.Models.Enums;
using SharpDX;

namespace PoEHUD.HUD
{
    public class HUDTexture
    {
        private readonly Color color;
        private string fileName;

        public HUDTexture(string fileName) : this(fileName, Color.White)
        {
        }

        public HUDTexture(string fileName, MonsterRarity rarity) : this(fileName, Color.White)
        {
            switch (rarity)
            {
                case MonsterRarity.Magic:
                    color = HUDSkin.MagicColor;
                    break;
                case MonsterRarity.Rare:
                    color = HUDSkin.RareColor;
                    break;
                case MonsterRarity.Unique:
                    color = HUDSkin.UniqueColor;
                    break;
            }
        }

        public HUDTexture(string fileName, Color color)
        {
            this.fileName = fileName;
            this.color = color;
        }

        public string FileName
        {
            get => fileName;
            set
            {
                if (fileName != null && fileName != value)
                {
                    fileName = value;
                }
            }
        }

        public void Draw(Graphics graphics, RectangleF rectangle)
        {
            graphics.DrawImage(fileName, rectangle, color);
        }

        public void DrawPluginImage(Graphics graphics, RectangleF rectangle)
        {
            graphics.DrawPluginImage(fileName, rectangle, color);
        }
    }
}
