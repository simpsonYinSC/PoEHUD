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

        public Element Label => ReadObjectAt<Element>(0x10); // LabelsOnGround
        public Entity ItemOnGround => ReadObjectAt<Entity>(0x18); // ItemsOnGround
        public Element LabelOnHover => ReadObjectAt<Element>(OffsetBuffers + 0x32C);
        public Entity ItemOnHover => ReadObjectAt<Entity>(OffsetBuffers + 0x334);
        public bool CanPickUp => labelInfo.Value == 0;

        public TimeSpan TimeLeft
        {
            get
            {
                if (CanPickUp)
                {
                    return new TimeSpan();
                }

                int futureTime = Memory.ReadInt(labelInfo.Value + 0x38);
                return TimeSpan.FromMilliseconds(futureTime - Environment.TickCount);
            }
        }

        public TimeSpan MaximumTimeForPickUp => !CanPickUp ? TimeSpan.FromMilliseconds(Memory.ReadInt(labelInfo.Value + 0x34)) : new TimeSpan();
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
            return Label.Address != 0 ? Memory.ReadLong(Label.Address + OffsetBuffers + 0x66C) : 0;
        }
    }
}
