using System;

namespace PoEHUD.HUD.Interfaces
{
    public interface IPlugin : IDisposable
    {
        void Render();
    }
}
