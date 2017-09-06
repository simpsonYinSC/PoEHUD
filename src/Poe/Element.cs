using System.Collections.Generic;
using System.Linq;
using SharpDX;

namespace PoEHUD.PoE
{
    public class Element : RemoteMemoryObject
    {
        public const int OffsetBuffers = 0x6EC;

        // dd id
        // dd (something zero)
        // 16 dup <128-bytes structure>
        // then the rest is
        public int VTable => Memory.ReadInt(Address + 0);
        public long ChildCount => (Memory.ReadLong(Address + 0x44 + OffsetBuffers) - Memory.ReadLong(Address + 0x3c + OffsetBuffers)) / 8;
        public bool IsVisibleLocal => (Memory.ReadInt(Address + 0x94 + OffsetBuffers) & 1) == 1;
        public Element Root => ReadObject<Element>(Address + 0xC4 + OffsetBuffers);
        public Element Parent => ReadObject<Element>(Address + 0xCC + OffsetBuffers);
        public float X => Memory.ReadFloat(Address + 0xD4 + OffsetBuffers);
        public float Y => Memory.ReadFloat(Address + 0xD8 + OffsetBuffers);
        public float Scale => Memory.ReadFloat(Address + 0x1D0 + OffsetBuffers);
        public float Width => Memory.ReadFloat(Address + 0x204 + OffsetBuffers);
        public float Height => Memory.ReadFloat(Address + 0x208 + OffsetBuffers);

        public bool IsVisible
        {
            get { return IsVisibleLocal && GetParentChain().All(current => current.IsVisibleLocal); }
        }

        public List<Element> Children => GetChildren<Element>();

        public Vector2 GetParentPosition()
        {
            float num = 0;
            float num2 = 0;
            foreach (Element current in GetParentChain())
            {
                num += current.X;
                num2 += current.Y;
            }

            return new Vector2(num, num2);
        }

        public virtual RectangleF GetClientRect()
        {
            var parentPosition = GetParentPosition();

            float width = Game.IngameState.Camera.Width;
            float height = Game.IngameState.Camera.Height;
            float ratioFixMult = width / height / 1.6f;
            float xScale = width / 2560f / ratioFixMult;
            float yScale = height / 1600f;
            float num = (parentPosition.X + X) * xScale;
            float num2 = (parentPosition.Y + Y) * yScale;
            return new RectangleF(num, num2, xScale * Width, yScale * Height);
        }

        public Element GetChildFromIndices(params int[] indices)
        {
            Element poeUIElement = this;
            foreach (int index in indices)
            {
                poeUIElement = poeUIElement.GetChildAtIndex(index);
                if (poeUIElement == null)
                {
                    return null;
                }
            }

            return poeUIElement;
        }

        public Element GetChildAtIndex(int index)
        {
            return index >= ChildCount ? null : GetObject<Element>(Memory.ReadLong(Address + 0x24 + OffsetBuffers, index * 8));
        }

        protected List<T> GetChildren<T>() where T : Element, new()
        {
            const int listOffset = 0x3C + OffsetBuffers;
            var list = new List<T>();
            if (Memory.ReadLong(Address + listOffset + 8) == 0 || Memory.ReadLong(Address + listOffset) == 0 || ChildCount > 1000)
            {
                return list;
            }

            for (int i = 0; i < ChildCount; i++)
            {
                list.Add(GetObject<T>(Memory.ReadLong(Address + listOffset, i * 8)));
            }

            return list;
        }

        private IEnumerable<Element> GetParentChain()
        {
            var list = new List<Element>();
            var hashSet = new HashSet<Element>();
            Element root = Root;
            Element parent = Parent;
            while (!hashSet.Contains(parent) && root.Address != parent.Address && parent.Address != 0)
            {
                list.Add(parent);
                hashSet.Add(parent);
                parent = parent.Parent;
            }

            return list;
        }
    }
}
