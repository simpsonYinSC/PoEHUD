using System;
using System.Collections.Generic;
using PoEHUD.Controllers;
using PoEHUD.PoE;
using PoEHUD.PoE.Elements;

namespace PoEHUD.Models
{
    public sealed class EntityListWrapper
    {
        private readonly GameController gameController;
        private readonly HashSet<string> ignoredEntities;
        private Dictionary<long, EntityWrapper> entityCache;
        private EntityWrapper player;

        public EntityListWrapper(GameController gameController)
        {
            this.gameController = gameController;
            entityCache = new Dictionary<long, EntityWrapper>();
            ignoredEntities = new HashSet<string>();
            gameController.Area.AreaChanged += OnAreaChanged;
        }

        public event Action<EntityWrapper> EntityAdded;
        public event Action<EntityWrapper> EntityRemoved;

        public IEnumerable<EntityWrapper> Entities => entityCache.Values;

        public EntityWrapper Player
        {
            get
            {
                if (player == null)
                {
                    UpdatePlayer();
                }

                return player;
            }
        }

        public void RefreshState()
        {
            UpdatePlayer();
            if (gameController.Area.CurrentArea == null)
            {
                return;
            }

            Dictionary<int, Entity> newEntities = gameController.Game.IngameState.Data.EntityList.EntitiesAsDictionary;
            var newCache = new Dictionary<long, EntityWrapper>();
            foreach (KeyValuePair<int, Entity> keyEntity in newEntities)
            {
                if (!keyEntity.Value.IsValid)
                {
                    continue;
                }

                long entityId = keyEntity.Key;
                string uniqueEntityName = keyEntity.Value.Path + entityId;

                if (ignoredEntities.Contains(uniqueEntityName))
                {
                    continue;
                }

                if (entityCache.ContainsKey(entityId) && entityCache[entityId].IsValid)
                {
                    newCache.Add(entityId, entityCache[entityId]);
                    entityCache[entityId].IsInList = true;
                    entityCache.Remove(entityId);
                    continue;
                }

                var entity = new EntityWrapper(gameController, keyEntity.Value);
                if (entity.Path.StartsWith("Metadata/Effects") || (entityId & 0x80000000L) != 0L ||
                    entity.Path.StartsWith("Metadata/Monsters/Daemon"))
                {
                    ignoredEntities.Add(uniqueEntityName);
                    continue;
                }

                EntityAdded?.Invoke(entity);
                newCache.Add(entityId, entity);
            }

            RemoveOldEntitiesFromCache();
            entityCache = newCache;
        }

        public EntityWrapper GetEntityById(long id)
        {
            EntityWrapper result;
            return entityCache.TryGetValue(id, out result) ? result : null;
        }

        public EntityLabel GetLabelForEntity(Entity entity)
        {
            var hashSet = new HashSet<long>();
            long entityLabelMap = gameController.Game.IngameState.EntityLabelMap;
            long num = entityLabelMap;
            
            while (true)
            {
                hashSet.Add(num);
                if (gameController.Memory.ReadLong(num + 0x10) == entity.Address)
                {
                    break;
                }

                num = gameController.Memory.ReadLong(num);
                if (hashSet.Contains(num) || num == 0 || num == -1)
                {
                    return null;
                }
            }

            return gameController.Game.ReadObject<EntityLabel>(num + 0x18);
        }

        private void OnAreaChanged(AreaController area)
        {
            ignoredEntities.Clear();
            RemoveOldEntitiesFromCache();
        }

        private void RemoveOldEntitiesFromCache()
        {
            foreach (var current in Entities)
            {
                EntityRemoved?.Invoke(current);
                current.IsInList = false;
            }

            entityCache.Clear();
        }

        private void UpdatePlayer()
        {
            long address = gameController.Game.IngameState.Data.LocalPlayer.Address;
            if (player == null || player.Address != address)
            {
                player = new EntityWrapper(gameController, address);
            }
        }
    }
}
