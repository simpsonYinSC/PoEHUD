using System.Collections.Generic;
using PoEHUD.Models.Enums;

namespace PoEHUD.PoE.Components
{
    public class ObjectMagicProperties : Component
    {
        public MonsterRarity Rarity
        {
            get
            {
                if (Address != 0)
                {
                    return (MonsterRarity)Memory.ReadInt(Address + 0x7C);
                }

                return MonsterRarity.White;
            }
        }

        public IEnumerable<string> Mods
        {
            get
            {
                if (Address == 0)
                {
                    return new List<string>();
                }

                long begin = Memory.ReadLong(Address + 0x98);
                long end = Memory.ReadLong(Address + 0xA0);
                var list = new List<string>();
                if (begin == 0 || end == 0)
                {
                    return list;
                }

                for (long i = begin; i < end; i += 0x28)
                {
                    string mod = Memory.ReadStringU(Memory.ReadLong(i + 0x20, 0));
                    list.Add(mod);
                }

                return list;
            }
        }
    }
}
