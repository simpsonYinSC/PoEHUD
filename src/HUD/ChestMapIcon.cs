using System;
using PoEHUD.Models;
using PoEHUD.PoE.Components;

namespace PoEHUD.HUD
{
    public class ChestMapIcon : MapIcon
    {
        public ChestMapIcon(EntityWrapper entityWrapper, HUDTexture hudTexture, Func<bool> show, int iconSize) : base(entityWrapper, hudTexture, show, iconSize)
        {
        }

        public override bool IsEntityStillValid()
        {
            return EntityWrapper.IsValid && !EntityWrapper.GetComponent<Chest>().IsOpened;
        }
    }
}
