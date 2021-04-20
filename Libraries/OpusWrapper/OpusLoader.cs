using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace OpusWrapper
{
    public static class OpusLoader
    {
        private static IntPtr _libraryPtr;

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Ansi)]
        internal static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)] string lpFileName);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool FreeLibrary(IntPtr hModule);

        public static void LoadOpus()
        {
            LoadOpus(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
        }

        public static void LoadOpus(string directory)
        {
            if (_libraryPtr != IntPtr.Zero)
                return;

            string nativeLibraryDirectory;
            byte[] resource;

            if (IntPtr.Size == 4)
            {
                nativeLibraryDirectory = Path.Combine(directory, "x86");
                resource = Properties.Resources.opus32;
            }
            else
            {
                nativeLibraryDirectory = Path.Combine(directory, "x64");
                resource = Properties.Resources.opus64;
            }

            Directory.CreateDirectory(nativeLibraryDirectory);

            var libraryFile = new FileInfo(Path.Combine(nativeLibraryDirectory, "opus.dll"));

            if (!libraryFile.Exists || libraryFile.Length != resource.Length)
            {
                if (libraryFile.Exists)
                    try
                    {
                        libraryFile.Delete();
                    }
                    catch (Exception)
                    {
                        libraryFile =
                            new FileInfo(Path.Combine(nativeLibraryDirectory, $"{Guid.NewGuid():N}.dll"));
                    }

                File.WriteAllBytes(libraryFile.FullName, resource);
            }

            IntPtr result = LoadLibrary(libraryFile.FullName);
            if (result == IntPtr.Zero)
            {
                var lastError = Marshal.GetLastWin32Error();
                var error = new Win32Exception(lastError);
                throw error;
            }

            _libraryPtr = result;
        }

        public static void Free()
        {
            if (FreeLibrary(_libraryPtr))
                _libraryPtr = IntPtr.Zero;
        }
    }
}