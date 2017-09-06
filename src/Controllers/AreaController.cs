using System;
using PoEHUD.Models;
using PoEHUD.PoE.RemoteMemoryObjects;

namespace PoEHUD.Controllers
{
    public class AreaController
    {
        private readonly GameController root;

        public AreaController(GameController gameController)
        {
            root = gameController;
        }

        public event Action<AreaController> AreaChanged;

        public AreaInstance CurrentArea { get; private set; }

        public void RefreshState()
        {
            var ingameStateData = root.Game.IngameState.Data;
            AreaTemplate clientsArea = ingameStateData.CurrentArea;
            int currentAreaHash = ingameStateData.CurrentAreaHash;

            if (CurrentArea != null && currentAreaHash == CurrentArea.Hash)
            {
                return;
            }

            CurrentArea = new AreaInstance(clientsArea, currentAreaHash, ingameStateData.CurrentAreaLevel);
            AreaChanged?.Invoke(this);
        }
    }
}
