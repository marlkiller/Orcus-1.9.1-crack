using System;
using System.IO;
using System.Text.RegularExpressions;

namespace ConsoleApplication13
{
    class Program
    {
        static void Main(string[] args)
        {
            foreach (
                var fileInfo in
                    new DirectoryInfo(
                        @"D:\Dokumente\Visual Studio 2015\Projects\Orcus\Source\Orcus.Administration\Views").GetFiles(
                            "*.xaml", SearchOption.AllDirectories))
            {
                var fileContent = File.ReadAllText(fileInfo.FullName);

                var changedContent = Regex.Replace(fileContent,
                    @"<MenuItem (?<stuff>(.*?))>\s*?<MenuItem\.Icon>\s*<ContentPresenter Content=""(?<iconResource>(.*?))"" \/>\s*<\/MenuItem\.Icon>\s*<\/MenuItem>",
                    "<MenuItem ${stuff} Icon=\"${iconResource}\" />", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                if (changedContent != fileContent)
                {
                    File.WriteAllText(fileInfo.FullName, changedContent);
                    Console.WriteLine(fileInfo.FullName);
                }
            }
            Console.ReadKey();
        }
    }
}