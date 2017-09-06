using System.Collections.Generic;
using PoEHUD.Controllers;
using PoEHUD.HUD.Interfaces;
using PoEHUD.HUD.Settings;
using PoEHUD.HUD.UI;
using PoEHUD.Models;

namespace PoEHUD.HUD
{
    public abstract class PluginWithMapIcons<TSettings> : Plugin<TSettings>, IPluginWithMapIcons where TSettings : SettingsBase
    {
        protected readonly Dictionary<EntityWrapper, MapIcon> CurrentIcons;

        protected PluginWithMapIcons(GameController gameController, Graphics graphics, TSettings settings) : base(gameController, graphics, settings)
        {
            CurrentIcons = new Dictionary<EntityWrapper, MapIcon>();
            GameController.Area.AreaChanged += delegate
             {
                 CurrentIcons.Clear();
             };
        }

        public IEnumerable<MapIcon> GetIcons()
        {
            var toRemove = new List<EntityWrapper>();
            foreach (KeyValuePair<EntityWrapper, MapIcon> kv in CurrentIcons)
            {
                if (kv.Value.IsEntityStillValid())
                {
                    yield return kv.Value;
                }
                else
                {
                    toRemove.Add(kv.Key);
                }
            }

            foreach (EntityWrapper wrapper in toRemove)
            {
                CurrentIcons.Remove(wrapper);
            }
        }

        protected override void OnEntityRemoved(EntityWrapper entityWrapper)
        {
            base.OnEntityRemoved(entityWrapper);
            CurrentIcons.Remove(entityWrapper);
        }
    }
}
