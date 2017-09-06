using System;
using System.Globalization;
using System.Linq;

namespace PoEHUD.Models
{
    public struct Pattern
    {
        public byte[] Bytes;
        public string Mask;

        public Pattern(byte[] pattern, string mask)
        {
            Bytes = pattern;
            Mask = mask;
        }

        public Pattern(string pattern, string mask)
        {
            string[] arr = pattern.Split(new[] { "\\x" }, StringSplitOptions.RemoveEmptyEntries);
            Bytes = arr.Select(y => byte.Parse(y, NumberStyles.HexNumber)).ToArray();
            Mask = mask;
        }
    }
}
