using PoEHUD.Framework;

namespace PoEHUD.PoE.RemoteMemoryObjects
{
    public class TheGame : RemoteMemoryObject
    {
        public TheGame(Memory memory)
        {
            Memory = memory;
            Address = memory.ReadLong(Offset.Base + memory.AddressOfProcess, 0x8, 0xf8); // 0xC40
            Game = this;
        }

        public IngameState IngameState => ReadObject<IngameState>(Address + 0x38);
        public int AreaChangeCount => Memory.ReadInt(Memory.AddressOfProcess + Offset.AreaChangeCount);
    }
}
