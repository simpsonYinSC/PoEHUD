using System;
using System.Collections.Generic;
using PoEHUD.Framework;

namespace PoEHUD.PoE.FilesInMemory
{
    public class TagsDat : FileInMemory
    {
        public Dictionary<string, TagRecord> Records = new Dictionary<string, TagRecord>(StringComparer.OrdinalIgnoreCase);

        public TagsDat(Memory memory, long address) : base(memory, address)
        {
            LoadItems();
        }

        private void LoadItems()
        {
            foreach (long address in RecordAddresses())
            {
                var r = new TagRecord(Memory, address);
                if (!Records.ContainsKey(r.Key))
                {
                    Records.Add(r.Key, r);
                }
            }
        }

        public class TagRecord
        {
            public readonly string Key;
            public int Hash;
            //// more fields can be added (see in visualGGPK)

            public TagRecord(Memory memory, long address)
            {
                Key = memory.ReadStringU(memory.ReadLong(address + 0), 255);
                Hash = memory.ReadInt(address + 0x8);
            }
        }
    }
}
