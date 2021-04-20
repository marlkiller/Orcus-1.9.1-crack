using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Sorzus.Wpf.Toolkit.Native
{
    internal static class NativeMethods
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hReservedNull, LoadLibraryFlags dwFlags);

        [DllImport("kernel32.dll")]
        internal static extern bool EnumResourceNames(IntPtr hModule, ResourceTypes lpszType,
            EnumResNameProcDelegate lpEnumFunc,
            IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true, ExactSpelling = true)]
        internal static extern int LookupIconIdFromDirectory(IntPtr presbits, bool fIcon);

        [DllImport("user32.dll", SetLastError = true, ExactSpelling = true)]
        internal static extern int LookupIconIdFromDirectoryEx(IntPtr presbits, bool fIcon, int cxDesired, int cyDesired,
            LookupIconIdFromDirectoryExFlags Flags);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern IntPtr FindResource(IntPtr hModule, IntPtr lpName, ResourceTypes lpType);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        internal static extern IntPtr LoadResource(IntPtr hModule, IntPtr hResInfo);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        internal static extern IntPtr LockResource(IntPtr hResData);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        internal static extern int SizeofResource(IntPtr hModule, IntPtr hResInfo);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        internal static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("user32.dll")]
        internal static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        internal static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

        [DllImport("user32.dll")]
        internal static extern bool SetWindowPos(IntPtr hwnd, IntPtr hwndInsertAfter, int x, int y, int width,
            int height, uint flags);

        [DllImport("user32.dll")]
        internal static extern bool GetWindowRect(IntPtr hWnd, ref Rectangle lpRect);

        [DllImport("user32.dll")]
        internal static extern int MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32.dll")]
        internal static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);

        [DllImport("user32.dll")]
        internal static extern int UnhookWindowsHookEx(IntPtr idHook);

        [DllImport("user32.dll")]
        internal static extern IntPtr CallNextHookEx(IntPtr idHook, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        internal static extern IntPtr GetCurrentThreadId();

        internal delegate bool EnumResNameProcDelegate(
            IntPtr hModule, ResourceTypes lpszType, IntPtr lpszName, IntPtr lParam);

        internal delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);
    }

    internal enum ResourceTypes
    {
        CURSOR = 1,
        BITMAP = 2,
        ICON = 3,
        MENU = 4,
        DIALOG = 5,
        STRING = 6,
        FONTDIR = 7,
        FONT = 8,
        ACCELERATOR = 9,
        RCDATA = 10,
        MESSAGETABLE = 11,
        GROUP_CURSOR = 12,
        GROUP_ICON = 14,
        VERSION = 16,
        DLGINCLUDE = 17,
        PLUGPLAY = 19,
        VXD = 20,
        ANICURSOR = 21,
        ANIICON = 22,
        HTML = 23,
        MANIFEST = 24
    }


    [Flags]
    internal enum LoadLibraryFlags : uint
    {
        DONT_RESOLVE_DLL_REFERENCES = 0x00000001,
        LOAD_IGNORE_CODE_AUTHZ_LEVEL = 0x00000010,
        LOAD_LIBRARY_AS_DATAFILE = 0x00000002,
        LOAD_LIBRARY_AS_DATAFILE_EXCLUSIVE = 0x00000040,
        LOAD_LIBRARY_AS_IMAGE_RESOURCE = 0x00000020,
        LOAD_WITH_ALTERED_SEARCH_PATH = 0x00000008
    }

    /// <summary>
    ///     Presents an Icon Directory.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = 6)]
    public struct IconDir
    {
        public short Reserved; // Reserved (must be 0)
        public short Type; // Resource Type (1 for icons)
        public short Count; // How many images?

        /// <summary>
        ///     Converts the current TAFactory.IconPack.IconDir into TAFactory.IconPack.GroupIconDir.
        /// </summary>
        /// <returns>TAFactory.IconPack.GroupIconDir</returns>
        public GroupIconDir ToGroupIconDir()
        {
            return new GroupIconDir
            {
                Reserved = Reserved,
                Type = Type,
                Count = Count
            };
        }
    }

    public enum LookupIconIdFromDirectoryExFlags
    {
        LR_DEFAULTCOLOR = 0,
        LR_MONOCHROME = 1
    }

    /// <summary>
    ///     Presents an Icon Directory Entry.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = 16)]
    public struct IconDirEntry
    {
        public byte Width; // Width, in pixels, of the image
        public byte Height; // Height, in pixels, of the image
        public byte ColorCount; // Number of colors in image (0 if >=8bpp)
        public byte Reserved; // Reserved ( must be 0)
        public short Planes; // Color Planes
        public short BitCount; // Bits per pixel
        public int BytesInRes; // How many bytes in this resource?
        public int ImageOffset; // Where in the file is this image?

        /// <summary>
        ///     Converts the current TAFactory.IconPack.IconDirEntry into TAFactory.IconPack.GroupIconDirEntry.
        /// </summary>
        /// <param name="id">The resource identifier.</param>
        /// <returns>TAFactory.IconPack.GroupIconDirEntry</returns>
        public GroupIconDirEntry ToGroupIconDirEntry(int id)
        {
            return new GroupIconDirEntry
            {
                Width = Width,
                Height = Height,
                ColorCount = ColorCount,
                Reserved = Reserved,
                Planes = Planes,
                BitCount = BitCount,
                BytesInRes = BytesInRes,
                ID = (short) id
            };
        }
    }

    /// <summary>
    ///     Presents a Group Icon Directory.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = 6)]
    public struct GroupIconDir
    {
        public short Reserved; // Reserved (must be 0)
        public short Type; // Resource Type (1 for icons)
        public short Count; // How many images?

        /// <summary>
        ///     Converts the current TAFactory.IconPack.GroupIconDir into TAFactory.IconPack.IconDir.
        /// </summary>
        /// <returns>TAFactory.IconPack.IconDir</returns>
        public IconDir ToIconDir()
        {
            return new IconDir
            {
                Reserved = Reserved,
                Type = Type,
                Count = Count
            };
        }
    }

    /// <summary>
    ///     Presents a Group Icon Directory Entry.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = 14)]
    public struct GroupIconDirEntry
    {
        public byte Width; // Width, in pixels, of the image
        public byte Height; // Height, in pixels, of the image
        public byte ColorCount; // Number of colors in image (0 if >=8bpp)
        public byte Reserved; // Reserved ( must be 0)
        public short Planes; // Color Planes
        public short BitCount; // Bits per pixel
        public int BytesInRes; // How many bytes in this resource?
        public short ID; // the ID

        /// <summary>
        ///     Converts the current TAFactory.IconPack.GroupIconDirEntry into TAFactory.IconPack.IconDirEntry.
        /// </summary>
        /// <param name="imageOffset">The resource identifier.</param>
        /// <returns>TAFactory.IconPack.IconDirEntry</returns>
        public IconDirEntry ToIconDirEntry(int imageOffset)
        {
            return new IconDirEntry
            {
                Width = Width,
                Height = Height,
                ColorCount = ColorCount,
                Reserved = Reserved,
                Planes = Planes,
                BitCount = BitCount,
                BytesInRes = BytesInRes,
                ImageOffset = imageOffset
            };
        }
    }
}