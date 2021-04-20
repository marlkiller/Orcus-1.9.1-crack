using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using Orcus.Native;

namespace Orcus.Commands.FileExplorer
{
    public static class FileHelper
    {
        public static string AssocQueryString(AssocStr association, string extension)
        {
            const int S_OK = 0;
            const int S_FALSE = 1;

            uint length = 0;
            uint ret = NativeMethods.AssocQueryString(AssocF.None, association, extension, null, null, ref length);
            if (ret != S_FALSE)
                throw new InvalidOperationException("Could not determine associated string");

            var sb = new StringBuilder((int) length);
            // (length-1) will probably work too as the marshaller adds null termination
            ret = NativeMethods.AssocQueryString(AssocF.None, association, extension, null, sb, ref length);
            if (ret != S_OK)
                throw new InvalidOperationException("Could not determine associated string");

            return sb.ToString();
        }

        public static long GetFileSizeOnDisk(string file)
        {
            return GetFileSizeOnDisk(new FileInfo(file));
        }

        public static long GetFileSizeOnDisk(FileInfo fileInfo)
        {
            uint dummy, sectorsPerCluster, bytesPerSector;
            int result = NativeMethods.GetDiskFreeSpaceW(fileInfo.Directory.Root.FullName, out sectorsPerCluster,
                out bytesPerSector, out dummy, out dummy);

            if (result == 0)
                throw new Win32Exception();

            uint clusterSize = sectorsPerCluster*bytesPerSector;
            uint hosize;
            uint losize = NativeMethods.GetCompressedFileSizeW(fileInfo.FullName, out hosize);

            var size = (long) hosize << 32 | losize;
            return (size + clusterSize - 1)/clusterSize*clusterSize;
        }
    }
}