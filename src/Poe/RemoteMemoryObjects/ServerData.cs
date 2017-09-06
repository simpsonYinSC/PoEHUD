using PoEHUD.PoE.Elements;

namespace PoEHUD.PoE.RemoteMemoryObjects
{
    public class ServerData : RemoteMemoryObject
    {
        public bool IsInGame => Address != 0 && InGameType == 3;
        public int InGameType => Address != 0 ? Memory.ReadInt(Address + /* Offsets.InGameOffset */ 0x42C8) : 0; // 0 - not in game (login screen), 1 - loading location, 3 - on location
        public StashElement StashPanel => Address != 0 ? GetObject<StashElement>(Memory.ReadLong(Address + 0x3C0, 0xA0, 0x78)) : null;
    }
}
