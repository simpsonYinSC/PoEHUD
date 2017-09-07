using System;
using System.Collections.Generic;
using PoEHUD.Framework;

namespace PoEHUD.PoE.FilesInMemory
{
    public class ModsDat : FileInMemory
    {
        public Dictionary<string, ModRecord> Records = new Dictionary<string, ModRecord>(StringComparer.OrdinalIgnoreCase);

        public Dictionary<Tuple<string, ModType>, List<ModRecord>> RecordsByTier = new Dictionary<Tuple<string, ModType>, List<ModRecord>>();

        public ModsDat(Memory memory, long address, StatsDat statsDat, TagsDat tagsDat) : base(memory, address)
        {
            LoadItems(statsDat, tagsDat);
        }

        public enum ModType
        {
            // Details: http://pathofexile.gamepedia.com/Modifiers#Mod_Generation_Type
            Prefix = 1,
            Suffix = 2,
            Unique = 3,
            Nemesis = 4,
            Corrupted = 5,
            BloodLines = 6,
            Torment = 7,
            Tempest = 8,
            Talisman = 9,
            Enchantment = 10,
            EssenceMonster = 11
        }

        public enum ModDomain
        {
            // Details: http://pathofexile.gamepedia.com/Modifiers#Mod_Domain
            Item = 1,
            Flask = 2,
            Monster = 3,
            Chest = 4,
            Area = 5,
            Unknown1 = 6,
            Unknown2 = 7,
            Unknown3 = 8,
            Stance = 9,
            Master = 10,
            Jewel = 11,
            Atlas = 12,
            LeagueStone = 13
        }

        private void LoadItems(StatsDat statsDat, TagsDat tagsDat)
        {
            foreach (long address in RecordAddresses())
            {
                var r = new ModRecord(Memory, statsDat, tagsDat, address);
                if (Records.ContainsKey(r.Key))
                {
                    continue;
                }

                Records.Add(r.Key, r);
                bool addToItemIiers = r.Domain != ModDomain.Monster;
                if (!addToItemIiers)
                {
                    continue;
                }

                Tuple<string, ModType> tierKey = Tuple.Create(r.Group, r.AffixType);
                if (!RecordsByTier.TryGetValue(tierKey, out List<ModRecord> groupMembers))
                {
                    groupMembers = new List<ModRecord>();
                    RecordsByTier[tierKey] = groupMembers;
                }

                groupMembers.Add(r);
            }

            foreach (List<ModRecord> list in RecordsByTier.Values)
            {
                list.Sort(ModRecord.ByLevelComparer);
            }
        }

        public class ModRecord
        {
            public const int NumberOfStats = 4;
            public static IComparer<ModRecord> ByLevelComparer = new LevelComparer();
            public readonly string Key;
            public long Address;
            public ModType AffixType;
            public ModDomain Domain;
            public string Group;
            public int MinimumLevel;
            public StatsDat.StatRecord[] StatNames; // Game refers to Stats.dat line
            public IntRange[] StatRange;
            public Dictionary<string, int> TagChances;
            public TagsDat.TagRecord[] Tags; // Game refers to Tags.dat line
            public long Unknown8; // Unknown pointer
            public string UserFriendlyName;
            //// more fields can be added (see in visualGGPK)

            public ModRecord(Memory memory, StatsDat statsDat, TagsDat tagsDat, long address)
            {
                Address = address;
                Key = memory.ReadStringU(memory.ReadLong(address + 0));
                Unknown8 = memory.ReadLong(address + 0x8);
                MinimumLevel = memory.ReadInt(address + 0x1C);

                StatNames = new[]
                {
                    memory.ReadLong(address + 0x28) == 0
                        ? null
                        : statsDat.Records[memory.ReadStringU(memory.ReadLong(memory.ReadLong(address + 0x28)))],
                    memory.ReadLong(address + 0x38) == 0
                        ? null
                        : statsDat.Records[memory.ReadStringU(memory.ReadLong(memory.ReadLong(address + 0x38)))],
                    memory.ReadLong(address + 0x48) == 0
                        ? null
                        : statsDat.Records[memory.ReadStringU(memory.ReadLong(memory.ReadLong(address + 0x48)))],
                    memory.ReadLong(address + 0x58) == 0
                        ? null
                        : statsDat.Records[memory.ReadStringU(memory.ReadLong(memory.ReadLong(address + 0x58)))]
                };

                Domain = (ModDomain)memory.ReadInt(address + 0x60);

                UserFriendlyName = memory.ReadStringU(memory.ReadLong(address + 0x64));

                AffixType = (ModType)memory.ReadInt(address + 0x6C);
                Group = memory.ReadStringU(memory.ReadLong(address + 0x70));

                StatRange = new[]
                {
                    new IntRange(memory.ReadInt(address + 0x78), memory.ReadInt(address + 0x7C)),
                    new IntRange(memory.ReadInt(address + 0x80), memory.ReadInt(address + 0x84)),
                    new IntRange(memory.ReadInt(address + 0x88), memory.ReadInt(address + 0x8C)),
                    new IntRange(memory.ReadInt(address + 0x90), memory.ReadInt(address + 0x94))
                };

                Tags = new TagsDat.TagRecord[memory.ReadLong(address + 0x98)];
                long ta = memory.ReadLong(address + 0xA0);
                for (int i = 0; i < Tags.Length; i++)
                {
                    long ii = ta + 0x8 + 0x10 * i;
                    Tags[i] = tagsDat.Records[memory.ReadStringU(memory.ReadLong(ii, 0), 255)];
                }

                TagChances = new Dictionary<string, int>(memory.ReadInt(address + 0xA8));
                long tc = memory.ReadLong(address + 0xB0);
                for (int i = 0; i < Tags.Length; i++)
                {
                    TagChances[Tags[i].Key] = memory.ReadInt(tc + 4 * i);
                }
            }

            private class LevelComparer : IComparer<ModRecord>
            {
                public int Compare(ModRecord x, ModRecord y)
                {
                    if (x != null && y != null)
                    {
                        return -x.MinimumLevel + y.MinimumLevel;
                    }

                    return 0;
                }
            }
        }
    }
}
