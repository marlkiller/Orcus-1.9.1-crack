using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Orcus.Test
{
    [TestClass]
    public class LanguageResourceInspection
    {
        [TestMethod]
        public void FileComparison()
        {
            var resourceFiles = new[]
            {
                @"..\..\..\Orcus.Administration\Resources\Languages\OrcusAdministration.de-de.xaml",
                @"..\..\..\Orcus.Administration\Resources\Languages\OrcusAdministration.en-us.xaml"
            };

            var failed = false;
            var fileContent = resourceFiles.ToDictionary(resourceFile => resourceFile, GetResourceKeys);

            for (int i = 0; i < resourceFiles.Length; i++)
            {
                for (int j = 0; j < resourceFiles.Length; j++)
                {
                    if (i == j)
                        continue;

                    Trace.WriteLine(
                        $"Comparing dictionary #{i} (\"{Path.GetFileName(resourceFiles[i])}\") to dictionary #{j} (\"{Path.GetFileName(resourceFiles[j])}\")");
                    var offsetKeys = fileContent[resourceFiles[i]].Except(fileContent[resourceFiles[j]]);
                    Trace.WriteLine("===========================================================");
                    Trace.WriteLine($"Keys only in file #{i} found:");
                    foreach (var offsetKey in offsetKeys)
                    {
                        Trace.WriteLine(offsetKey);
                        failed = true;
                    }
                    Trace.WriteLine("===========================================================");
                }
            }

            if (failed)
                Assert.Fail();
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