using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PoEHUD.PoE.Components
{
    public class Sockets : Component
    {
        public int LargestLinkSize
        {
            get
            {
                if (Address == 0)
                {
                    return 0;
                }

                long ptrLinkStart = Memory.ReadLong(Address + 0x60);
                long ptrLinkEnd = Memory.ReadLong(Address + 0x68);
                long linkGroupingCount = ptrLinkEnd - ptrLinkStart;
                if (linkGroupingCount <= 0 || linkGroupingCount > 6)
                {
                    return 0;
                }

                int biggestLinkGroupSize = 0;
                for (int i = 0; i < linkGroupingCount; i++)
                {
                    int linkGroupSize = Memory.ReadByte(ptrLinkStart + i);
                    if (linkGroupSize > biggestLinkGroupSize)
                    {
                        biggestLinkGroupSize = linkGroupSize;
                    }
                }

                return biggestLinkGroupSize;
            }
        }

        public IEnumerable<int[]> Links
        {
            get
            {
                var list = new List<int[]>();
                if (Address == 0)
                {
                    return list;
                }

                long ptrLinkStart = Memory.ReadLong(Address + 0x60);
                long ptrLinkEnd = Memory.ReadLong(Address + 0x68);
                long linkGroupingCount = ptrLinkEnd - ptrLinkStart;
                if (linkGroupingCount <= 0 || linkGroupingCount > 6)
                {
                    return list;
                }

                int linkCounter = 0;
                List<int> socketList = SocketList;
                for (int i = 0; i < linkGroupingCount; i++)
                {
                    int linkGroupSize = Memory.ReadByte(ptrLinkStart + i);
                    int[] array = new int[linkGroupSize];
                    for (int j = 0; j < linkGroupSize; j++)
                    {
                        array[j] = socketList[j + linkCounter];
                    }

                    list.Add(array);
                    linkCounter += linkGroupSize;
                }

                return list;
            }
        }

        public List<int> SocketList
        {
            get
            {
                var list = new List<int>();
                if (Address == 0)
                {
                    return list;
                }

                long num = Address + 0x18;
                for (int i = 0; i < 6; i++)
                {
                    int num2 = Memory.ReadInt(num);
                    if (num2 >= 1 && num2 <= 4)
                    {
                        list.Add(Memory.ReadInt(num));
                    }

                    num += 4;
                }

                return list;
            }
        }

        public int NumberOfSockets => SocketList.Count;

        public bool IsRGB
        {
            get
            {
                return Address != 0
                       && Links.Any(current => current.Length >= 3
                                               && current.Contains(1)
                                               && current.Contains(2)
                                               && current.Contains(3));
            }
        }

        public List<string> SocketGroup
        {
            get
            {
                var list = new List<string>();
                foreach (int[] current in Links)
                {
                    var sb = new StringBuilder();
                    foreach (int color in current)
                    {
                        switch (color)
                        {
                            case 1:
                                sb.Append("R");
                                break;
                            case 2:
                                sb.Append("G");
                                break;
                            case 3:
                                sb.Append("B");
                                break;
                            case 4:
                                sb.Append("W");
                                break;
                        }
                    }

                    list.Add(sb.ToString());
                }

                return list;
            }
        }
    }
}
