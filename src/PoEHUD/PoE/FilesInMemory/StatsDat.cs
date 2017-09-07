using System;
using System.Collections.Generic;
using PoEHUD.Framework;

namespace PoEHUD.PoE.FilesInMemory
{
    public class StatsDat : FileInMemory
    {
        public Dictionary<string, StatRecord> Records = new Dictionary<string, StatRecord>(StringComparer.OrdinalIgnoreCase);

        public StatsDat(Memory memory, long address) : base(memory, address)
        {
            LoadItems();
        }

        public enum StatType
        {
            Percents = 1,
            Value2 = 2,
            IntValue = 3,
            Boolean = 4,
            Precents5 = 5
        }

        private void LoadItems()
        {
            foreach (long address in RecordAddresses())
            {
                var r = new StatRecord(Memory, address);
                if (!Records.ContainsKey(r.Key))
                {
                    Records.Add(r.Key, r);
                }
            }
        }

        public class StatRecord
        {
            public readonly string Key;
            public StatType Type;
            public string UserFriendlyName;
            public bool Unknown4;
            public bool Unknown5;
            public bool Unknown6;
            public bool UnknownB;
            //// more fields can be added (see in visualGGPK)

            public StatRecord(Memory memory, long address)
            {
                Key = memory.ReadStringU(memory.ReadLong(address + 0), 255);
                Unknown4 = memory.ReadByte(address + 0x8) != 0;
                Unknown5 = memory.ReadByte(address + 0x9) != 0;
                Unknown6 = memory.ReadByte(address + 0xA) != 0;
                Type = Key.Contains("%") ? StatType.Percents : (StatType)memory.ReadInt(address + 0xB);
                UnknownB = memory.ReadByte(address + 0xF) != 0;
                UserFriendlyName = memory.ReadStringU(memory.ReadLong(address + 0x10), 255);
            }

            public override string ToString()
            {
                return string.IsNullOrWhiteSpace(UserFriendlyName) ? Key : UserFriendlyName;
            }

            internal string ValueToString(int value)
            {
                switch (Type)
                {
                    case StatType.Boolean:
                        return value != 0 ? "True" : "False";
                    case StatType.IntValue:
                    case StatType.Value2:
                        return value.ToString("+#;-#");
                    case StatType.Percents:
                    case StatType.Precents5:
                        return value.ToString("+#;-#") + "%";
                }

                return string.Empty;
            }
        }
    }
}
