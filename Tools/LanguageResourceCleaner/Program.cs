using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace LanguageResourceCleaner
{
    class Program
    {
        static void Main()
        {
            var baseDirectories = new[]
            {
                @"D:\Dokumente\Visual Studio 2015\Projects\Orcus\MainSource\Orcus.Administration",
                @"D:\Dokumente\Visual Studio 2015\Projects\Orcus\MainSource\Orcus.Administration.Commands"
            };

            var resourceFile =
                new FileInfo(
                    @"D:\Dokumente\Visual Studio 2015\Projects\Orcus\MainSource\Orcus.Administration\Resources\Languages\OrcusAdministration.en-us.xaml");

            var validExtensions = new[] {".cs", ".xaml"};

            var resourceFileContent = File.ReadAllText(resourceFile.FullName);
            var resourceKeys =
                Regex.Matches(resourceFileContent, @"(?<=x:Key="").+?(?="")")
                    .Cast<Match>()
                    .Select(match => match.Value)
                    .ToList();
            Console.WriteLine(resourceKeys.Count + " resource keys found");

            var fileList = new List<FileInfo>();
            foreach (var directory in baseDirectories.Select(x => new DirectoryInfo(x)))
            {
                fileList.AddRange(
                    directory.GetFiles("*.*", SearchOption.AllDirectories)
                        .Where(x => validExtensions.Contains(x.Extension.ToLower())));
            }

            Console.WriteLine(fileList.Count + " files found");
            var resourceKeysLeft = resourceKeys.ToList();

            Console.WriteLine();
            Console.Write("Begin...");
            foreach (var fileInfo in fileList)
            {
                Console.Write(
                    $"\rProcessing {fileInfo.Name}({fileList.IndexOf(fileInfo)} of {fileList.Count})\t\t\t\t\t");
                var fileContent = File.ReadAllText(fileInfo.FullName);

                switch (fileInfo.Extension)
                {
                    case ".xaml":
                        for (int i = resourceKeysLeft.Count - 1; i > -1; i--)
                        {
                            var resourceKey = resourceKeysLeft[i];
                            var match = Regex.Match(fileContent, $"{{(StaticResource|DynamicResource) {resourceKey}}}");
                            if (match.Success)
                                resourceKeysLeft.Remove(resourceKey);
                        }
                        break;
                    case ".cs":
                        for (int i = resourceKeysLeft.Count - 1; i > -1; i--)
                        {
                            var resourceKey = resourceKeysLeft[i];
                            var match = Regex.Match(fileContent,
                                $@"(Current\.Resources\[|Current\.FindResource\(|Current\.TryFindResource\()""{resourceKey}""(\]|\))");
                            if (match.Success)
                                resourceKeysLeft.Remove(resourceKey);
                        }
                        break;
                }
            }

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("===========================================================");
            Console.WriteLine("Job Finished");
            Console.WriteLine("===========================================================");
            Console.WriteLine($"Not found keys: {resourceKeysLeft.Count}");
            foreach (var key in resourceKeysLeft)
                Console.WriteLine(key);
            Console.ReadKey();
        }
    }
}