namespace PoEHUD.PoE.RemoteMemoryObjects
{
    public class IngameData : RemoteMemoryObject
    {
        public AreaTemplate CurrentArea => ReadObject<AreaTemplate>(Address + 0x28);
        public int CurrentAreaLevel => Memory.ReadByte(Address + 0x40);
        public int CurrentAreaHash => Memory.ReadInt(Address + 0x60);
        public Entity LocalPlayer => ReadObject<Entity>(Address + 0x1A8);
        public EntityList EntityList => GetObject<EntityList>(Address + 0x270);
    }
}
