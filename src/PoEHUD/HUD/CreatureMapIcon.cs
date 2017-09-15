using System;
using PoEHUD.Models;

namespace PoEHUD.HUD
{
    public class CreatureMapIcon : MapIcon
    {
        public CreatureMapIcon(EntityWrapper entityWrapper, string hudTexture, Func<bool> show, float iconSize) : base(entityWrapper, new HUDTexture(hudTexture), show, iconSize)
        {
        }

        public override bool IsVisible()
        {
            return base.IsVisible() && EntityWrapper.IsAlive;
        }
    }
}
