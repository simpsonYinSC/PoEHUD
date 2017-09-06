using System;
using System.Collections.Generic;

namespace PoEHUD.PoE.Elements
{
    public class ItemsOnGroundLabelElement : Element
    {
        private readonly Lazy<long> labelInfo;

        public ItemsOnGroundLabelElement()
        {
            labelInfo = new Lazy<long>(GetLabelInfo);
        }

        public Entity ItemOnGround => ReadObject<Entity>(Address + 0x18);
        public Element Label => ReadObject<Element>(Address + 0x10);
        public bool CanPickUp => labelInfo.Value == 0;

        public TimeSpan TimeLeft
        {
            get
            {
                if (CanPickUp)
                {
                    return new TimeSpan();
                }

                int futureTime = Memory.ReadInt(labelInfo.Value + 0x20);
                return TimeSpan.FromMilliseconds(futureTime - Environment.TickCount);
            }
        }

        public TimeSpan MaximumTimeForPickUp => !CanPickUp ? TimeSpan.FromMilliseconds(Memory.ReadInt(labelInfo.Value + 0x1C)) : new TimeSpan();
        public new bool IsVisible => Label.IsVisible;

        public new IEnumerable<ItemsOnGroundLabelElement> Children
        {
            get
            {
                long address = Memory.ReadLong(Address + OffsetBuffers + 0x344);

                for (long nextAddress = Memory.ReadLong(address); nextAddress != address; nextAddress = Memory.ReadLong(nextAddress))
                {
                    yield return GetObject<ItemsOnGroundLabelElement>(nextAddress);
                }
            }
        }

        private long GetLabelInfo()
        {
            return Label.Address != 0 ? Memory.ReadLong(Label.Address + OffsetBuffers + 0x45C) : 0; // potential candidates: 0x414, 0x45C, 0x494, 0x4A4, 0x4B4
        }
    }
}
