using System.Collections.Generic;

namespace PoEHUD.PoE.Components
{
    public class Actor : Component
    {
        /// <summary>
        /// Standing still = 2048 =bit 11 set
        /// running = 2178 = bit 11 & 7
        /// Maybe Bit-field : Bit 7 set = running
        /// </summary>
        public int ActionId => Address != 0 ? Memory.ReadInt(Address + 0xD8) : 1;
        public bool IsMoving => (ActionId & 128) > 0;
        public bool IsAttacking => (ActionId & 2) > 0;

        public IEnumerable<long> Minions
        {
            get
            {
                var list = new List<long>();
                if (Address == 0)
                {
                    return list;
                }

                long num = Memory.ReadLong(Address + 0x308);
                long num2 = Memory.ReadLong(Address + 0x310);
                for (long i = num; i < num2; i += 8)
                {
                    long item = Memory.ReadLong(i);
                    list.Add(item);
                }

                return list;
            }
        }

        public bool HasMinion(Entity entity)
        {
            if (Address == 0)
            {
                return false;
            }

            long num = Memory.ReadLong(Address + 0x308);
            long num2 = Memory.ReadLong(Address + 0x310);
            for (long i = num; i < num2; i += 8)
            {
                long num3 = Memory.ReadLong(i);
                if (num3 == entity.Id)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
