using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using PoEHUD.Framework.Enums;
using PoEHUD.Models;
using PoEHUD.PoE;

namespace PoEHUD.Framework
{
    public class Memory : IDisposable
    {
        public readonly long AddressOfProcess;
        public string DebugString = string.Empty;
        public Offset Offset;
        private readonly Dictionary<string, int> modules;
        private bool closed;
        private IntPtr procHandle;

        public Memory(Offset offset, int pId)
        {
            try
            {
                Offset = offset;
                Process = Process.GetProcessById(pId);
                AddressOfProcess = Process.MainModule.BaseAddress.ToInt64();
                procHandle = WindowsAPI.OpenProcess(Process, ProcessAccessFlags.All);
                modules = new Dictionary<string, int>();
            }
            catch (Win32Exception ex)
            {
                throw new Exception("You should run program as an administrator", ex);
            }
        }

        ~Memory()
        {
            Close();
        }

        public Process Process { get; }

        public void Dispose()
        {
            Close();
        }

        public int GetModule(string name)
        {
            if (modules.ContainsKey(name))
            {
                return modules[name];
            }

            int num = Process.Modules.Cast<ProcessModule>().First(m => m.ModuleName == name).BaseAddress.ToInt32();
            modules.Add(name, num);
            return num;
        }

        public bool IsInvalid()
        {
            return Process.HasExited || closed;
        }

        public int ReadInt(long address)
        {
            return BitConverter.ToInt32(ReadMem(address, 4), 0);
        }

        /* Infinite recursive call.
        public int ReadInt(int address, params int[] offsets)
        {
            int num = ReadInt(address);
            return offsets.Aggregate(num, (current, num2) => ReadInt(current + num2));
        }
        */

        public int ReadInt(long address, params long[] offsets)
        {
            long num = ReadLong(address);
            return (int)offsets.Aggregate(num, (current, num2) => ReadLong(current + num2));
        }

        public float ReadFloat(long address)
        {
            return BitConverter.ToSingle(ReadMem(address, 4), 0);
        }

        public long ReadLong(long address)
        {
            return BitConverter.ToInt64(ReadMem(address, 8), 0);
        }

        public long ReadLong(long address, params long[] offsets)
        {
            long num = ReadLong(address);
            return offsets.Aggregate(num, (current, num2) => ReadLong(current + num2));
        }

        public uint ReadUInt(long address)
        {
            return BitConverter.ToUInt32(ReadMem(address, 4), 0);
        }

        /// <summary>
        /// Read string as ASCII
        /// </summary>
        /// <param name="address"></param>
        /// <param name="length"></param>
        /// <param name="replaceNull"></param>
        /// <returns></returns>
        public string ReadString(long address, int length = 256, bool replaceNull = true)
        {
            if (address <= 65536 && address >= -1)
            {
                return string.Empty;
            }

            string @string = Encoding.ASCII.GetString(ReadMem(address, length));
            return replaceNull ? RTrimNull(@string) : @string;
        }

        /// <summary>
        /// Read string as Unicode
        /// </summary>
        /// <param name="address"></param>
        /// <param name="length"></param>
        /// <param name="replaceNull"></param>
        /// <returns></returns>
        public string ReadStringU(long address, int length = 256, bool replaceNull = true)
        {
            if (address <= 65536 && address >= -1)
            {
                return string.Empty;
            }

            byte[] mem = ReadMem(address, length);
            if (mem.Length == 0)
            {
                return string.Empty;
            }

            if (mem[0] == 0 && mem[1] == 0)
            {
                return string.Empty;
            }

            string @string = Encoding.Unicode.GetString(mem);
            return replaceNull ? RTrimNull(@string) : @string;
        }

        public byte ReadByte(long address)
        {
            return ReadBytes(address, 1).FirstOrDefault();
        }

        public byte[] ReadBytes(long address, int length)
        {
            return ReadMem(address, length);
        }

        public long[] FindPatterns(params Pattern[] patterns)
        {
            byte[] exeImage = ReadBytes(AddressOfProcess, 0x2000000); // 33mb
            var address = new long[patterns.Length];

            for (int patternIndex = 0; patternIndex < patterns.Length; patternIndex++)
            {
                Pattern pattern = patterns[patternIndex];
                byte[] patternData = pattern.Bytes;
                int patternLength = patternData.Length;

                bool found = false;

                for (int offset = 0; offset < exeImage.Length - patternLength; offset++)
                {
                    if (!CompareData(pattern, exeImage, offset))
                    {
                        continue;
                    }

                    found = true;
                    address[patternIndex] = offset;
                    DebugString += "Pattern " + patternIndex + " is found at " + (AddressOfProcess + offset).ToString("X") + " offset: " + offset.ToString("X") + Environment.NewLine;
                    break;
                }

                if (!found)
                {
                    // System.Windows.Forms.MessageBox.Show("Pattern " + iPattern + " is not found!");
                    DebugString += "Pattern " + patternIndex + " is not found!" + Environment.NewLine;
                }
            }

            return address;
        }

        private static string RTrimNull(string text)
        {
            int num = text.IndexOf('\0');
            return num > 0 ? text.Substring(0, num) : text;
        }

        private static bool CompareData(Pattern pattern, IReadOnlyList<byte> data, int offset)
        {
            return !pattern.Bytes.Where((t, i) => pattern.Mask[i] == 'x' && t != data[offset + i]).Any();
        }

        private void Close()
        {
            if (closed)
            {
                return;
            }

            closed = true;
            WindowsAPI.CloseHandle(procHandle);
        }

        private byte[] ReadMem(long address, int size)
        {
            var array = new byte[size];
            WindowsAPI.ReadProcessMemory(procHandle, (IntPtr)address, array);
            return array;
        }
    }
}
