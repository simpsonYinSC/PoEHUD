using PoEHUD.Framework;
using PoEHUD.PoE.RemoteMemoryObjects;

namespace PoEHUD.PoE
{
    public abstract class RemoteMemoryObject
    {
        public long Address { get; protected set; }
        protected TheGame Game { get; set; }
        protected Memory Memory { get; set; }

        protected Offset Offset => Memory.Offset;

        public T ReadObjectAt<T>(int offset) where T : RemoteMemoryObject, new()
        {
            return ReadObject<T>(Address + offset);
        }

        public T ReadObject<T>(long addressPointer) where T : RemoteMemoryObject, new()
        {
            return new T
            {
                Memory = Memory,
                Address = Memory.ReadLong(addressPointer),
                Game = Game
            };
        }

        public T GetObjectAt<T>(int offset) where T : RemoteMemoryObject, new()
        {
            return GetObject<T>(Address + offset);
        }

        public T GetObjectAt<T>(long offset) where T : RemoteMemoryObject, new()
        {
            return GetObject<T>(Address + offset);
        }

        public T GetObject<T>(long address) where T : RemoteMemoryObject, new()
        {
            return new T
            {
                Memory = Memory,
                Address = address,
                Game = Game
            };
        }

        public T AsObject<T>() where T : RemoteMemoryObject, new()
        {
            return new T
            {
                Memory = Memory,
                Address = Address,
                Game = Game
            };
        }

        public override bool Equals(object obj)
        {
            var remoteMemoryObject = obj as RemoteMemoryObject;
            return remoteMemoryObject != null && remoteMemoryObject.Address == Address;
        }

        public override int GetHashCode()
        {
            return (int)Address + GetType().Name.GetHashCode();
        }
    }
}
