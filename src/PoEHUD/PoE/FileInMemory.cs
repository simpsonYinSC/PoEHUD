using System.Collections.Generic;
using PoEHUD.Framework;

namespace PoEHUD.PoE
{
    public abstract class FileInMemory
    {
        protected FileInMemory(Memory memory, long address)
        {
            Memory = memory;
            Address = address;
        }

        protected Memory Memory { get; }
        private long Address { get; }
        private int NumberOfRecords => Memory.ReadInt(Address + 0x48, 0x28);

        protected IEnumerable<long> RecordAddresses()
        {
            long firstRecord = Memory.ReadLong(Address + 0x48, 0x8);
            long lastRecord = Memory.ReadLong(Address + 0x48, 0x10);
            int count = NumberOfRecords;
            long recordLength = (lastRecord - firstRecord) / count;

            for (int i = 0; i < count; i++)
            {
                yield return firstRecord + i * recordLength;
            }
        }
    }
}
