using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ResourceUnpacker
{
    using System;

    internal static class Program
    {
        public static void WriteResourceToFile(string resourceName)
        {
            string filePath = Regex.Replace(resourceName.Replace("ResourceUnpacker.Resources.", string.Empty), @"\.(?=.*\.)", "\\");
            FileInfo fileInfoOutputFile = new FileInfo(filePath);

            using (Stream resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                fileInfoOutputFile.Directory?.Create();
                using (var file = fileInfoOutputFile.Create())
                {
                    resourceStream?.CopyTo(file);
                }
            }
        }

        private static void Main()
        {
            Console.WriteLine("We are unpacking resources, please be patient...");
            Console.WriteLine("This window will close when we end.");

            string[] resourceNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            foreach (string resourceName in resourceNames)
            {
                WriteResourceToFile(resourceName);
            }
        }
    }
}
