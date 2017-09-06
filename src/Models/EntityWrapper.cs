using System.Collections.Generic;
using System.Linq;
using PoEHUD.Controllers;
using PoEHUD.Models.Interfaces;
using PoEHUD.PoE;
using PoEHUD.PoE.Components;
using Vector3 = SharpDX.Vector3;

namespace PoEHUD.Models
{
    public class EntityWrapper : IEntity
    {
        public bool IsInList = true;
        private readonly long cachedId;
        private readonly Dictionary<string, long> components;
        private readonly GameController gameController;
        private readonly Entity internalEntity;

        public EntityWrapper(GameController poE, Entity entity)
        {
            gameController = poE;
            internalEntity = entity;
            components = internalEntity.GetComponents();
            Path = internalEntity.Path;
            cachedId = internalEntity.Id;
            LongId = internalEntity.Id;
        }

        public EntityWrapper(GameController poE, long address) : this(poE, poE.Game.GetObject<Entity>(address))
        {
        }

        public Entity InternalEntity => internalEntity.Address == 0 ? null : internalEntity;

        public string Path { get; }
        public bool IsValid => internalEntity.IsValid && IsInList && cachedId == internalEntity.Id;
        public long Address => internalEntity.Address;
        public long Id => cachedId;
        public bool IsHostile => internalEntity.IsHostile;
        public long LongId { get; }
        public bool IsAlive => GetComponent<Life>().CurrentHP > 0;

        public Vector3 Pos
        {
            get
            {
                var p = GetComponent<Positioned>();
                return new Vector3(p.X, p.Y, GetComponent<Render>().Z);
            }
        }

        public IEnumerable<EntityWrapper> Minions
        {
            get
            {
                return GetComponent<Actor>().Minions.Select(current => gameController.EntityListWrapper.GetEntityById(current)).Where(byId => byId != null).ToList();
            }
        }

        public T GetComponent<T>() where T : Component, new()
        {
            string name = typeof(T).Name;
            return gameController.Game.GetObject<T>(components.ContainsKey(name) ? components[name] : 0);
        }

        public bool HasComponent<T>() where T : Component, new()
        {
            return components.ContainsKey(typeof(T).Name);
        }

        public List<string> PrintComponents()
        {
            List<string> result = new List<string> { internalEntity.Path + " " + internalEntity.Address.ToString("X") };
            result.AddRange(components.Select(current => current.Key + " " + current.Value.ToString("X")));

            return result;
        }

        public override bool Equals(object obj)
        {
            var entity = obj as EntityWrapper;
            return entity != null && entity.LongId == LongId;
        }

        public override int GetHashCode()
        {
            return LongId.GetHashCode();
        }

        public override string ToString()
        {
            return "EntityWrapper: " + Path;
        }
    }
}
