namespace PoEHUD.PoE.RemoteMemoryObjects
{
    public class ItemMod : RemoteMemoryObject
    {
        private int level;
        private string name;
        private string rawName;

        public int Value1 => Memory.ReadInt(Address, 0);
        public int Value2 => Memory.ReadInt(Address, 4);
        public int Value3 => Memory.ReadInt(Address, 8);
        public int Value4 => Memory.ReadInt(Address, 0xC);

        public string RawName
        {
            get
            {
                if (rawName == null)
                {
                    ParseName();
                }

                return rawName;
            }
        }

        public string Name
        {
            get
            {
                if (rawName == null)
                {
                    ParseName();
                }

                return name;
            }
        }

        public int Level
        {
            get
            {
                if (rawName == null)
                {
                    ParseName();
                }

                return level;
            }
        }

        private void ParseName()
        {
            rawName = Memory.ReadStringU(Memory.ReadLong(Address + 0x20, 0));
            name = rawName.Replace("_", string.Empty); // Master Crafted mod can have underscore on the end, need to ignore
            int index = name.IndexOfAny("0123456789".ToCharArray());
            if (index < 0 || !int.TryParse(name.Substring(index), out level))
            {
                level = 1;
            }
            else
            {
                name = name.Substring(0, index);
            }
        }
    }
}
