using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace LanguageResourceComparer
{
    class Program
    {
        static void Main()
        {
            var resourceFiles = new[]
            {
                @"D:\Dokumente\Visual Studio 2015\Projects\Orcus\MainSource\Orcus.Administration\Resources\Languages\OrcusAdministration.de-de.xaml",
                @"D:\Dokumente\Visual Studio 2015\Projects\Orcus\MainSource\Orcus.Administration\Resources\Languages\OrcusAdministration.en-us.xaml"
            };

            var fileContent = resourceFiles.ToDictionary(resourceFile => resourceFile, GetResourceKeys);

            for (int i = 0; i < resourceFiles.Length; i++)
            {
                for (int j = 0; j < resourceFiles.Length; j++)
                {
                    if (i == j)
                        continue;

                    Console.WriteLine(
                        $"Comparing dictionary #{i} (\"{Path.GetFileName(resourceFiles[i])}\") to dictionary #{j} (\"{Path.GetFileName(resourceFiles[j])}\")");
                    var offsetKeys = fileContent[resourceFiles[i]].Except(fileContent[resourceFiles[j]]);
                    Console.WriteLine("===========================================================");
                    Console.WriteLine($"Keys only in file #{i} found:");
                    foreach (var offsetKey in offsetKeys)
                    {
                        Console.WriteLine(offsetKey);
                    }
                    Console.WriteLine("===========================================================");
                }
            }

            Console.ReadKey();
        }

        private static string[] GetResourceKeys(string file)
        {
            return
                Regex.Matches(File.ReadAllText(file), @"(?<=x:Key="").+?(?="")")
                    .Cast<Match>()
                    .Select(match => match.Value)
                    .ToArray();
        }
    }
}