using System.Collections.Generic;

namespace PoEHUD.HUD.Interfaces
{
    public interface IPluginWithMapIcons
    {
        IEnumerable<MapIcon> GetIcons();
    }
}
