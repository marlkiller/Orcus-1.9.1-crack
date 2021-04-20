using System;
using System.IO;
using System.Linq;

namespace PluginLibraryUpdater
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine("Path to the Orcus libraries folder");
            var librariesFolder = Console.ReadLine();
            var baseDirectory = new DirectoryInfo(string.IsNullOrEmpty(librariesFolder) ? @"..\..\..\..\Orcus.Administration\bin\Release" : librariesFolder);
            if (!baseDirectory.Exists)
            {
                Console.WriteLine("The directory does not exist");
                return;
            }

            Console.WriteLine("Folder with plugin projects:");
            var projectsFolder = Console.ReadLine();
            var pluginFolder = new DirectoryInfo(string.IsNullOrEmpty(projectsFolder) ? @"..\..\..\..\..\Source2" : projectsFolder);
            if (!pluginFolder.Exists)
            {
                Console.WriteLine("The directory does not exist");
                return;
            }

            var baseDirectoryFiles = baseDirectory.GetFiles();

            foreach (var pluginDirectory in pluginFolder.GetDirectories())
            {
                var referenceDirectory = pluginDirectory.GetDirectories("references", SearchOption.TopDirectoryOnly);
                if (referenceDirectory.Length == 0)
                {
                    Console.WriteLine($"No References folder found in \"{pluginDirectory.Name}\"");
                    continue;
                }

                var folder = referenceDirectory[0];
                var replacedFiles = 0;
                foreach (var fileInfo in folder.GetFiles())
                {
                    var baseFile = baseDirectoryFiles.FirstOrDefault(x => x.Name == fileInfo.Name);
                    if (baseFile != null)
                    {
                        baseFile.CopyTo(fileInfo.FullName, true);
                        replacedFiles++;
                    }
                }

                Console.WriteLine($"Replaced {replacedFiles} files in {pluginDirectory.Name}");
            }

            Console.ReadKey();
        }
    }
}