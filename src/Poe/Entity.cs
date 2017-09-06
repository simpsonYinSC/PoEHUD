using System;
using System.Collections.Generic;
using PoEHUD.Models.Interfaces;

namespace PoEHUD.PoE
{
    public sealed class Entity : RemoteMemoryObject, IEntity
    {
        public long Id => (long)Memory.ReadInt(Address + 0x40) << 32 ^ Path.GetHashCode();
        public int InventoryId => Memory.ReadInt(Address + 0x58);
        public string Path => Memory.ReadStringU(Memory.ReadLong(Address, 0x20));
        public bool IsValid => Memory.ReadInt(Address, 0x20, 0) == 0x65004D;

        /// <summary>
        /// 0x65004D = "Me"(4 bytes) from word Metadata
        /// </summary>
        public bool IsHostile => (Memory.ReadByte(Memory.ReadLong(Address + 0x50) + 0x130) & 1) == 0;
        private long ComponentList => Memory.ReadLong(Address + 0x8);
        private long ComponentLookup => Memory.ReadLong(Address, 0x48, 0x30);

        public bool HasComponent<T>() where T : Component, new()
        {
            return HasComponent<T>(out long _);
        }

        public T GetComponent<T>() where T : Component, new()
        {
            return HasComponent<T>(out long address) ? ReadObject<T>(ComponentList + Memory.ReadInt(address + 0x18) * 8) : GetObject<T>(0);
        }

        public Dictionary<string, long> GetComponents()
        {
            var dictionary = new Dictionary<string, long>();
            long componentLookup = ComponentLookup;

            // the first address is a base object that doesn't contain a component, so read the first component
            long address = Memory.ReadLong(componentLookup);
            while (address != componentLookup && address != 0 && address != -1)
            {
                string name = Memory.ReadString(Memory.ReadLong(address + 0x10));
                string nameStart = name;
                if (!string.IsNullOrWhiteSpace(name))
                {
                    char[] arr = name.ToCharArray();
                    arr = Array.FindAll(arr, c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c) || c == '-');
                    name = new string(arr);
                }

                if (string.IsNullOrWhiteSpace(name) || name != nameStart)
                {
                    break;
                }

                long componentAddress = Memory.ReadLong(ComponentList + Memory.ReadInt(address + 0x18) * 8);
                if (!dictionary.ContainsKey(name) && !string.IsNullOrWhiteSpace(name))
                {
                    dictionary.Add(name, componentAddress);
                }

                address = Memory.ReadLong(address);
            }

            return dictionary;
        }

        public override string ToString()
        {
            return Path;
        }

        private bool HasComponent<T>(out long address) where T : Component, new()
        {
            string name = typeof(T).Name;
            long componentLookup = ComponentLookup;
            address = Memory.ReadLong(componentLookup);
            int i = 0;
            while (!Memory.ReadString(Memory.ReadLong(address + 0x10)).Equals(name))
            {
                address = Memory.ReadLong(address);
                ++i;
                if (address == componentLookup || address == 0 || address == -1 || i >= 200)
                {
                    return false;
                }
            }

            return true;
        }

        /*
        public string DebugReadComponents()
        {
            string result = string.Empty;

            long componentList = Memory.ReadLong(Address + 0x8);
            result += "ComponentList (EntytaAddr + 0x8): " + Environment.NewLine + componentList.ToString("x") + Environment.NewLine + Environment.NewLine;

            result += "ComponentLookupRead: " + Environment.NewLine + ComponentLookup.ToString("x") + Environment.NewLine;

            long CL_read1 = Memory.ReadLong(Address);
            result += "CL_read1 (EntytaAddr + 0x0): " + Environment.NewLine + CL_read1.ToString("x") + Environment.NewLine + Environment.NewLine;

            long CL_read2 = Memory.ReadLong(CL_read1 + 0x48);
            result += "CL_read2 (CL_read1 + 0x48): " + Environment.NewLine + CL_read2.ToString("x") + Environment.NewLine + Environment.NewLine;

            long CL_read3 = Memory.ReadLong(CL_read2 + 0x30);
            result += "CL_read3 (CL_read2 + 0x30): " + Environment.NewLine + CL_read3.ToString("x") + Environment.NewLine + Environment.NewLine;

            long CL_read4 = Memory.ReadLong(CL_read3 + 0x0);
            result += ">LookUp  (CL_read3 + 0x0): " + Environment.NewLine + CL_read4.ToString("x") + Environment.NewLine + Environment.NewLine;

            result += "ReadingComponents: " + Environment.NewLine;

            var dictionary = new Dictionary<string, long>();

            long componentLookup = ComponentLookup;
            long address = componentLookup;

            do
            {
                result += "addr: " + address.ToString("x") + Environment.NewLine;

                result += "NamePchar at (addr + 0x10): " + (address + 0x10).ToString("x") + Environment.NewLine;
                string name = Memory.ReadString(Memory.ReadLong(address + 0x10));
                result += "name: " + name + Environment.NewLine;

                result += "componentAddress (ComponentList + M.ReadInt(addr + 0x18) * 8): " + (address + 0x10).ToString("x") + Environment.NewLine;
                long componentAddress = Memory.ReadInt(ComponentList + Memory.ReadInt(address + 0x18) * 8);
                result += $"({ComponentList} + M.ReadInt({(address + 0x18):x}) * 8)" + Environment.NewLine;

                result += $"({ComponentList} + {(Memory.ReadInt(address + 0x18)):x} * 8)" + Environment.NewLine;
                result += $"({ComponentList} + {((Memory.ReadInt(address + 0x18)) * 8):x})" + Environment.NewLine;

                result += "FinalComponentAddress: " + componentAddress.ToString("x") + Environment.NewLine;

                if (!dictionary.ContainsKey(name) && !string.IsNullOrWhiteSpace(name))
                {
                    dictionary.Add(name, componentAddress);
                    result += $"AddComponent: {name} : {componentAddress:x}" + Environment.NewLine;
                }
                else
                {
                    result += $"SkipComponent: {name} : {componentAddress:x}. Allready contains or emptyCompName" + Environment.NewLine;
                }

                address = Memory.ReadLong(address);
                result += Environment.NewLine;

            } while (address != componentLookup && address != 0 && address != -1);

            return result;
        }
        */
    }
}
