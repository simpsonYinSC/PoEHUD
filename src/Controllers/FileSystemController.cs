using System;
using System.Collections.Generic;
using System.Windows.Forms;
using PoEHUD.Framework;
using PoEHUD.Models;
using PoEHUD.PoE.FilesInMemory;

namespace PoEHUD.Controllers
{
    public class FileSystemController
    {
        public readonly BaseItemTypes BaseItemTypes;
        public readonly ItemClasses ItemClasses;
        public readonly ModsDat Mods;
        public readonly StatsDat Stats;
        public readonly TagsDat Tags;
        private readonly Dictionary<string, long> files;
        private readonly Memory memory;
        //// private bool isLoaded;

        public FileSystemController(Memory memory)
        {
            this.memory = memory;
            files = GetAllFiles();

            ItemClasses = new ItemClasses();
            BaseItemTypes = new BaseItemTypes(memory, FindFile("Data/BaseItemTypes.dat"));
            Tags = new TagsDat(memory, FindFile("Data/Tags.dat"));
            Stats = new StatsDat(memory, FindFile("Data/Stats.dat"));
            Mods = new ModsDat(memory, FindFile("Data/Mods.dat"), Stats, Tags);
        }

        public Dictionary<string, long> GetAllFiles()
        {
            var fileList = new Dictionary<string, long>();
            long fileRoot = memory.AddressOfProcess + memory.Offset.FileRoot;
            long start = memory.ReadLong(fileRoot + 0x8);

            for (long currentFile = memory.ReadLong(start); currentFile != start && currentFile != 0; currentFile = memory.ReadLong(currentFile))
            {
                string str = memory.ReadStringU(memory.ReadLong(currentFile + 0x10), 512);

                if (!fileList.ContainsKey(str))
                {
                    fileList.Add(str, memory.ReadLong(currentFile + 0x18));
                }
            }

            return fileList;
        }

        public long FindFile(string name)
        {
            try
            {
                return files[name];
            }
            catch (KeyNotFoundException)
            {
                const string messageFormat = "Couldn't find the file in memory: {0}\nTry to restart the game.";
                MessageBox.Show(string.Format(messageFormat, name), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            }

            return 0;
        }
    }
}
