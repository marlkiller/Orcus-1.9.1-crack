using System;
using System.IO;

namespace Orcus.Shared.Utilities
{
    /// <summary>
    ///     Some useful functions to handle files
    /// </summary>
    public static class FileExtensions
    {
        private const string NumberPattern = " ({0})";

        /// <summary>
        ///     Get a free temp file name in form of a Guid
        /// </summary>
        /// <returns>A path to a non existing file located in the temp directory</returns>
        public static string GetFreeTempFileName()
        {
            while (true)
            {
                var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("B"));
                if (!File.Exists(path))
                    return path;
            }
        }

        /// <summary>
        ///     Get a free temp file name in form of a Guid
        /// </summary>
        /// <param name="extension">The extension the file should have. Example: exe (without a dot!)</param>
        public static string GetFreeTempFileName(string extension)
        {
            while (true)
            {
                var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("B") + "." + extension);
                if (!File.Exists(path))
                    return path;
            }
        }

        /// <summary>
        ///     Return the full path of a unique file within the folder with the extension (the extension must not include a dot)
        /// </summary>
        /// <param name="folder">The folder of the file</param>
        /// <param name="extension">The extension of the file without a dot (e. g. exe, jpg). Null will result in no extension</param>
        /// <returns>Return a non existing file from the folder</returns>
        public static string GetUniqueFileName(string folder, string extension)
        {
            while (true)
            {
                var path = Path.Combine(folder, Guid.NewGuid().ToString("N"));
                if (extension != null)
                    path += "." + extension;

                if (!File.Exists(path))
                    return path;
            }
        }

        /// <summary>
        ///     Return the full path of a unique file within the folder with the extension (the extension must not include a dot)
        /// </summary>
        /// <param name="folder">The folder of the file</param>
        /// <returns>Return a non existing file from the folder</returns>
        public static string GetUniqueFileName(string folder)
        {
            return GetUniqueFileName(folder, null);
        }

        /// <summary>
        ///     If the file <see cref="path" /> already exists, add a number at the end to make it non existing
        /// </summary>
        /// <param name="path">The path to a file</param>
        /// <returns>A file which does not exist</returns>
        public static string MakeUnique(string path)
        {
            // Short-cut if already available
            if (!File.Exists(path))
                return path;

            var escapedPath = path.Replace("{", "{{").Replace("}", "}}");
            // If path has extension then insert the number pattern just before the extension and return next filename
            if (Path.HasExtension(path))
                return GetNextFilename(escapedPath.Insert(escapedPath.LastIndexOf(Path.GetExtension(escapedPath)), NumberPattern));

            // Otherwise just append the pattern to the path and return next filename
            return GetNextFilename(escapedPath + NumberPattern);
        }

        private static string GetNextFilename(string pattern)
        {
            string tmp = String.Format(pattern, 1);
            if (tmp == pattern)
                throw new ArgumentException("The pattern must include an index place-holder", "pattern");

            if (!File.Exists(tmp))
                return tmp; // short-circuit if no matches

            int min = 1, max = 2; // min is inclusive, max is exclusive/untested

            while (File.Exists(String.Format(pattern, max)))
            {
                min = max;
                max *= 2;
            }

            while (max != min + 1)
            {
                int pivot = (max + min) / 2;
                if (File.Exists(String.Format(pattern, pivot)))
                    min = pivot;
                else
                    max = pivot;
            }

            return String.Format(pattern, max);
        }

        /// <summary>
        ///     If the directory <see cref="path" /> already exists, add a number at the end to make it non existing
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string MakeDirectoryUnique(string path)
        {
            if (!Directory.Exists(path))
                return path;

            return GetNextDirectory(path + NumberPattern);
        }

        private static string GetNextDirectory(string pattern)
        {
            string tmp = String.Format(pattern, 1);
            if (tmp == pattern)
                throw new ArgumentException("The pattern must include an index place-holder", "pattern");

            if (!Directory.Exists(tmp))
                return tmp; // short-circuit if no matches

            int min = 1, max = 2; // min is inclusive, max is exclusive/untested

            while (Directory.Exists(String.Format(pattern, max)))
            {
                min = max;
                max *= 2;
            }

            while (max != min + 1)
            {
                int pivot = (max + min) / 2;
                if (Directory.Exists(String.Format(pattern, pivot)))
                    min = pivot;
                else
                    max = pivot;
            }

            return String.Format(pattern, max);
        }

        /// <summary>
        ///     Converts the given path into a format which is comparable. That makes it possible that
        ///     C:\Folder1\Folder2\..\test.txt equals C:\Folder1\test.txt
        /// </summary>
        /// <param name="path">The path to normalize</param>
        /// <returns>Returns a normalized form of the input <see cref="path" /></returns>
        public static string NormalizePath(this string path)
        {
            string fullPath;
            if (path.Contains(":"))
            {
                fullPath = path;
            }
            else
                try
                {
                    fullPath = Path.GetFullPath(new Uri(path).LocalPath);
                }
                catch (Exception)
                {
                    fullPath = path;
                }

            return fullPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).ToUpperInvariant();
        }

        /// <summary>
        /// Remove all invalid characters from the path
        /// </summary>
        /// <param name="path">The path</param>
        /// <returns>Return a path without any illegal characters</returns>
        public static string RemoveInvalidCharacters(string path)
        {
            return string.Join(string.Empty, path.Split(Path.GetInvalidFileNameChars()));
        }
    }
}