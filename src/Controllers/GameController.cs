using System.Collections.Generic;
using System.Linq;
using PoEHUD.Framework;
using PoEHUD.Models;
using PoEHUD.PoE.Components;
using PoEHUD.PoE.RemoteMemoryObjects;

namespace PoEHUD.Controllers
{
    public class GameController
    {
        public static GameController Instance;

        public GameController(Memory memory)
        {
            Instance = this;
            Memory = memory;
            Area = new AreaController(this);
            EntityListWrapper = new EntityListWrapper(this);
            Window = new GameWindow(memory.Process);
            Game = new TheGame(memory);
            Files = new FileSystemController(memory);
        }

        public EntityListWrapper EntityListWrapper { get; }
        public GameWindow Window { get; private set; }
        public TheGame Game { get; }
        public AreaController Area { get; }

        public Memory Memory { get; private set; }

        public IEnumerable<EntityWrapper> Entities => EntityListWrapper.Entities;

        public EntityWrapper Player => EntityListWrapper.Player;

        public bool InGame => Game.IngameState.InGame;

        public FileSystemController Files { get; private set; }

        public void RefreshState()
        {
            if (!InGame)
            {
                return;
            }

            EntityListWrapper.RefreshState();
            Area.RefreshState();
        }

        public List<EntityWrapper> GetAllPlayerMinions()
        {
            return Entities.Where(x => x.HasComponent<Player>()).SelectMany(c => c.Minions).ToList();
        }
    }
}
