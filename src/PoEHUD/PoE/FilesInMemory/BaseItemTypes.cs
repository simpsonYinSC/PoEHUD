using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using PoEHUD.Framework;
using PoEHUD.Models;

namespace PoEHUD.PoE.FilesInMemory
{
    public class BaseItemTypes : FileInMemory
    {
        public Dictionary<string, BaseItemType> Contents = new Dictionary<string, BaseItemType>();

        public BaseItemTypes(Memory memory, long address) : base(memory, address)
        {
            LoadItemTypes();
        }

        public BaseItemType Translate(string metadata)
        {
            if (!Contents.ContainsKey(metadata))
            {
                LoadItemTypes();
            }

            if (Contents.ContainsKey(metadata))
            {
                return Contents[metadata];
            }

            Console.WriteLine("Key not found in BaseItemTypes: " + metadata);
            return null;
        }

        private void LoadItemTypes()
        {
            foreach (long i in RecordAddresses())
            {
                string key = Memory.ReadStringU(Memory.ReadLong(i));
                var baseItemType = new BaseItemType
                {
                    ClassName = Memory.ReadStringU(Memory.ReadLong(i + 0x10, 0)),

                    Width = Memory.ReadInt(i + 0x18),
                    Height = Memory.ReadInt(i + 0x1C),
                    BaseName = Memory.ReadStringU(Memory.ReadLong(i + 0x20)),
                    DropLevel = Memory.ReadInt(i + 0x30),
                    Tags = new string[Memory.ReadLong(i + 0xA8)]
                };

                long ta = Memory.ReadLong(i + 0xB0);
                for (int k = 0; k < baseItemType.Tags.Length; k++)
                {
                    long ii = ta + 0x8 + 0x10 * k;
                    baseItemType.Tags[k] = Memory.ReadStringU(Memory.ReadLong(ii, 0), 255);
                }

                string[] tmpTags = key.Split('/');
                if (tmpTags.Length > 3)
                {
                    baseItemType.MoreTagsFromPath = new string[tmpTags.Length - 3];
                    for (int k = 2; k < tmpTags.Length - 1; k++)
                    {
                        // This Regex and if condition change Item Path Category e.g. TwoHandWeapons
                        // To tag strings type e.g. two_hand_weapon
                        string tmpKey = Regex.Replace(tmpTags[k], @"(?<!_)([A-Z])", "_$1").ToLower().Remove(0, 1);
                        if (tmpKey[tmpKey.Length - 1] == 's')
                        {
                            tmpKey = tmpKey.Remove(tmpKey.Length - 1);
                        }

                        baseItemType.MoreTagsFromPath[k - 2] = tmpKey;
                    }
                }
                else
                {
                    baseItemType.MoreTagsFromPath = new string[1];
                    baseItemType.MoreTagsFromPath[0] = string.Empty;
                }

                if (!Contents.ContainsKey(key))
                {
                    Contents.Add(key, baseItemType);
                }
            }
        }
    }
}
