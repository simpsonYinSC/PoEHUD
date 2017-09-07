using System.Collections.Generic;
using System.Linq;
using PoEHUD.Models;
using PoEHUD.Models.Enums;
using PoEHUD.PoE.RemoteMemoryObjects;

namespace PoEHUD.PoE.Components
{
    public class Mods : Component
    {
        public string UniqueName => Address != 0 ? Memory.ReadStringU(Memory.ReadLong(Address + 0x30, 0x8, 0x4)) + Memory.ReadStringU(Memory.ReadLong(Address + 0x30, 0x18, 4)) : string.Empty;
        public bool Identified => Address != 0 && Memory.ReadByte(Address + 0x88) == 1;
        public ItemRarity ItemRarity => Address != 0 ? (ItemRarity)Memory.ReadInt(Address + 0x8C) : ItemRarity.Normal;
        public IEnumerable<ItemMod> ItemMods
        {
            get
            {
                IEnumerable<ItemMod> implicitMods = GetMods(0x90, 0x98);
                IEnumerable<ItemMod> explicitMods = GetMods(0xA8, 0xB0);
                return implicitMods.Concat(explicitMods).ToList();
            }
        }

        public int ItemLevel => Address != 0 ? Memory.ReadInt(Address + 0x204) : 1;
        public int RequiredLevel => Address != 0 ? Memory.ReadInt(Address + 0x208) : 1;
        public ItemStats ItemStats => new ItemStats(Owner);

        private IEnumerable<ItemMod> GetMods(int startOffset, int endOffset)
        {
            var list = new List<ItemMod>();
            if (Address == 0)
            {
                return list;
            }

            long begin = Memory.ReadLong(Address + startOffset);
            long end = Memory.ReadLong(Address + endOffset);
            long count = (end - begin) / 0x28;
            
            if (count > 12)
            {
                return list;
            }

            for (long i = begin; i < end; i += 0x28)
            {
                list.Add(GetObject<ItemMod>(i));
            }

            return list;
        }
    }
}
