using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Orcus.Administration.Core.Utilities
{
    public static class FileExtensions
    {
        private const uint FILE_ATTRIBUTE_READONLY = 0x00000001;
        private const uint FILE_ATTRIBUTE_HIDDEN = 0x00000002;
        private const uint FILE_ATTRIBUTE_SYSTEM = 0x00000004;
        private const uint FILE_ATTRIBUTE_DIRECTORY = 0x00000010;
        private const uint FILE_ATTRIBUTE_ARCHIVE = 0x00000020;
        private const uint FILE_ATTRIBUTE_DEVICE = 0x00000040;
        private const uint FILE_ATTRIBUTE_NORMAL = 0x00000080;
        private const uint FILE_ATTRIBUTE_TEMPORARY = 0x00000100;
        private const uint FILE_ATTRIBUTE_SPARSE_FILE = 0x00000200;
        private const uint FILE_ATTRIBUTE_REPARSE_POINT = 0x00000400;
        private const uint FILE_ATTRIBUTE_COMPRESSED = 0x00000800;
        private const uint FILE_ATTRIBUTE_OFFLINE = 0x00001000;
        private const uint FILE_ATTRIBUTE_NOT_CONTENT_INDEXED = 0x00002000;
        private const uint FILE_ATTRIBUTE_ENCRYPTED = 0x00004000;
        private const uint FILE_ATTRIBUTE_VIRTUAL = 0x00010000;

        private const uint SHGFI_ICON = 0x000000100; // get icon
        private const uint SHGFI_DISPLAYNAME = 0x000000200; // get display name
        private const uint SHGFI_TYPENAME = 0x000000400; // get type name
        private const uint SHGFI_ATTRIBUTES = 0x000000800; // get attributes
        private const uint SHGFI_ICONLOCATION = 0x000001000; // get icon location
        private const uint SHGFI_EXETYPE = 0x000002000; // return exe type
        private const uint SHGFI_SYSICONINDEX = 0x000004000; // get system icon index
        private const uint SHGFI_LINKOVERLAY = 0x000008000; // put a link overlay on icon
        private const uint SHGFI_SELECTED = 0x000010000; // show icon in selected state
        private const uint SHGFI_ATTR_SPECIFIED = 0x000020000; // get only specified attributes
        private const uint SHGFI_LARGEICON = 0x000000000; // get large icon
        private const uint SHGFI_SMALLICON = 0x000000001; // get small icon
        private const uint SHGFI_OPENICON = 0x000000002; // get open icon
        private const uint SHGFI_SHELLICONSIZE = 0x000000004; // get shell size icon
        private const uint SHGFI_PIDL = 0x000000008; // pszPath is a pidl
        private const uint SHGFI_USEFILEATTRIBUTES = 0x000000010; // use passed dwFileAttribute

        [DllImport("shell32")]
        private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, out SHFILEINFO psfi,
            uint cbFileInfo, uint flags);

        [DllImport("User32.dll")]
        private static extern int DestroyIcon(IntPtr hIcon);

        /// <summary>
        ///     Get information about the file
        /// </summary>
        /// <param name="fileName">The path to the file</param>
        /// <returns>Information about the file</returns>
        public static FileInformation GetFileTypeDescription(string fileName)
        {
            var extension = Path.GetExtension(fileName);
            if (string.IsNullOrEmpty(extension))
                return null;

            SHFILEINFO shfi;
            if (IntPtr.Zero != SHGetFileInfo(
                fileName,
                FILE_ATTRIBUTE_NORMAL,
                out shfi,
                (uint) Marshal.SizeOf(typeof (SHFILEINFO)),
                SHGFI_USEFILEATTRIBUTES | SHGFI_TYPENAME | SHGFI_ICON | SHGFI_LARGEICON))
            {
                var result = new FileInformation {Description = shfi.szTypeName};
                var icon = (Icon) Icon.FromHandle(shfi.hIcon).Clone();
                DestroyIcon(shfi.hIcon); // Cleanup
                result.Icon = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
                icon.Dispose();

                return result;
            }

            return null;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)] public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)] public string szTypeName;
        }
    }

    public class FileInformation
    {
        public string Description { get; set; }
        public ImageSource Icon { get; set; }
    }
}