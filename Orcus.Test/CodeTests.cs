using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Orcus.Test
{
    [TestClass]
    public class CodeTests
    {
        [TestMethod]
        public void AboutViewNoSpaceTest()
        {
            var codeFile = new FileInfo(@"..\..\..\Orcus.Administration\Views\SettingsWindow.xaml");
            Assert.IsTrue(codeFile.Exists);
            var content = File.ReadAllText(codeFile.FullName);
            Assert.IsTrue(content.Contains("<Run Text=\"(\" /><Hyperlink")); //no white space
        }

        [TestMethod]
        public void WindowsStyleTest()
        {
            var directory = new DirectoryInfo(@"..\..\..\Orcus.Administration\Views");
            var targetFiles = directory.GetFiles("*Window.xaml", SearchOption.AllDirectories);
            var invalidFiles = new List<FileInfo>();
            var ignoredFiles = new[] {"MainWindow.xaml", "LanguageCreatorWindow.xaml", "RegisterOrcusWindow.xaml"};

            foreach (var targetFile in targetFiles)
            {
                if (ignoredFiles.Contains(targetFile.Name))
                    continue;

                var content = File.ReadAllText(targetFile.FullName);
                if (!content.Contains("Style=\"{StaticResource NormalWindow}\"") && !content.Contains("BasedOn=\"{StaticResource NormalWindow}\""))
                    invalidFiles.Add(targetFile);
            }

            if (invalidFiles.Count > 0)
                Assert.Fail("The following windows dont have the NormalWindow style:\r\n" +
                            string.Join(Environment.NewLine,
                                invalidFiles.Select(x => x.FullName.Substring(directory.FullName.Length))));
        }

        [TestMethod]
        public void ClientVersionTest()
        {
            var file = new FileInfo(@"..\..\..\Orcus.Administration.ViewModels\ViewInterface\ApplicationInterface.cs");
            Assert.IsTrue(file.Exists);
            var administrationVersion = int.Parse(Regex.Match(File.ReadAllText(file.FullName),
                "int ClientVersion { get; } = (?<version>([0-9]{1,3}));").Groups["version"].Value);

            var clientFile = new FileInfo(@"..\..\..\Orcus\Program.cs");
            Assert.IsTrue(file.Exists);
            var clientVersion =
                int.Parse(
                    Regex.Match(File.ReadAllText(clientFile.FullName), "int Version = (?<version>([0-9]{1,3}));").Groups
                        ["version"].Value);

            if (clientVersion != administrationVersion)
            {
                Assert.Fail(
                    $"The version of ApplicationInterface.cs ({administrationVersion}) is different from the version in Program.cs ({clientVersion})");
            }
        }

        [TestMethod]
        public void MetroWindowTest()
        {
            var file = new FileInfo(@"..\..\..\Orcus.Administration\Resources\Styles\Window.xaml");
            Assert.IsTrue(file.Exists);
            var content = File.ReadAllText(file.FullName);
            var contentIndex = content.IndexOf("<controls:MetroContentControl");
            var overlayIndex = content.IndexOf("x:Name=\"PART_MetroActiveDialogContainer\"");
            Assert.IsTrue(overlayIndex > contentIndex);
        }
    }
}