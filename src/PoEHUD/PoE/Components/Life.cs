using System;
using System.Collections.Generic;
using PoEHUD.PoE.RemoteMemoryObjects;

namespace PoEHUD.PoE.Components
{
    public class Life : Component
    {
        public int MaximumHP => Address != 0 ? Memory.ReadInt(Address + 0x50) : 1;
        public int CurrentHP => Address != 0 ? Memory.ReadInt(Address + 0x54) : 1;
        public int ReservedFlatHP => Address != 0 ? Memory.ReadInt(Address + 0x5C) : 0;
        public int ReservedPercentHP => Address != 0 ? Memory.ReadInt(Address + 0x60) : 0;
        public int MaximumMana => Address != 0 ? Memory.ReadInt(Address + 0x88) : 1;
        public int CurrentMana => Address != 0 ? Memory.ReadInt(Address + 0x8C) : 1;
        public int ReservedFlatMana => Address != 0 ? Memory.ReadInt(Address + 0x94) : 0;
        public int ReservedPercentMana => Address != 0 ? Memory.ReadInt(Address + 0x98) : 0;
        public int MaximumES => Address != 0 ? Memory.ReadInt(Address + 0xB8) : 0;
        public int CurrentES => Address != 0 ? Memory.ReadInt(Address + 0xBC) : 0;
        public float HPPercentage => CurrentHP / (float)(MaximumHP - ReservedFlatHP - Math.Round(ReservedPercentHP * 0.01 * MaximumHP));
        public float MPPercentage => CurrentMana / (float)(MaximumMana - ReservedFlatMana - Math.Round(ReservedPercentMana * 0.01 * MaximumMana));

        public float ESPercentage
        {
            get
            {
                if (MaximumES != 0)
                {
                    return CurrentES / (float)MaximumES;
                }

                return 0f;
            }
        }

        public bool CorpseUsable => Memory.ReadBytes(Address + 0x238, 1)[0] == 1; // Total guess, didn't verify

        public List<Buff> Buffs
        {
            get
            {
                var list = new List<Buff>();
                long start = Memory.ReadLong(Address + 0xE8);
                long end = Memory.ReadLong(Address + 0xF0);
                int count = (int)(end - start) / 8;

                // Randomly bumping to 256 from 32... no idea what real value is.
                if (count <= 0 || count > 256)
                {
                    return list;
                }

                for (int i = 0; i < count; i++)
                {
                    list.Add(ReadObject<Buff>(Memory.ReadLong(start + i * 8) + 8));
                }

                return list;
            }
        }

        public bool HasBuff(string buff)
        {
            return Buffs.Exists(x => x.Name == buff);
        }
    }
}
