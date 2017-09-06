using System.Collections.Generic;

namespace PoEHUD.PoE.RemoteMemoryObjects
{
    public class EntityList : RemoteMemoryObject
    {
        public IEnumerable<Entity> Entities => EntitiesAsDictionary.Values;

        public Dictionary<int, Entity> EntitiesAsDictionary
        {
            get
            {
                var dictionary = new Dictionary<int, Entity>();
                CollectEntities(Memory.ReadLong(Address), dictionary);
                return dictionary;
            }
        }

        private void CollectEntities(long address, IDictionary<int, Entity> list)
        {
            long num = address;
            address = Memory.ReadLong(address + 0x8);
            var hashSet = new HashSet<long>();
            var queue = new Queue<long>();
            queue.Enqueue(address);
            int loopcount = 0;
            while (queue.Count > 0 && loopcount < 10000)
            {
                loopcount++;
                long nextAddress = queue.Dequeue();
                if (hashSet.Contains(nextAddress))
                {
                    continue;
                }

                hashSet.Add(nextAddress);
                if (nextAddress == num || nextAddress == 0)
                {
                    continue;
                }

                int entityId = Memory.ReadInt(nextAddress + 0x28, 0x40);
                if (!list.ContainsKey(entityId))
                {
                    long address2 = Memory.ReadLong(nextAddress + 0x28);
                    var entity = GetObject<Entity>(address2);
                    list.Add(entityId, entity);
                }

                queue.Enqueue(Memory.ReadLong(nextAddress));
                queue.Enqueue(Memory.ReadLong(nextAddress + 0x10));
            }
        }
    }
}
