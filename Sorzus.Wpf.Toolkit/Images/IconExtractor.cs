using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using Sorzus.Wpf.Toolkit.Extensions;
using Sorzus.Wpf.Toolkit.Native;

namespace Sorzus.Wpf.Toolkit.Images
{
    /// <summary>
    ///     Get icon resources (RT_GROUP_ICON and RT_ICON) from an executable module (either a .dll or an .exe file).
    /// </summary>
    internal class IconExtractor : IDisposable
    {
        /// <summary>
        ///     Initializes a new IconExtractor and loads the executable module into the address space of the calling process.
        ///     The executable module can be a .dll or an .exe file.
        ///     The specified module can cause other modules to be mapped into the address space.
        /// </summary>
        /// <param name="fileName">
        ///     The name of the executable module (either a .dll or an .exe file). The file name can contain
        ///     environment variables (like %SystemRoot%).
        /// </param>
        public IconExtractor(string fileName)
        {
            LoadLibrary(fileName);
        }

        /// <summary>
        ///     A fully quallified name of the executable module.
        /// </summary>
        public string FileName { get; private set; }

        /// <summary>
        ///     Gets the module handle.
        /// </summary>
        public IntPtr ModuleHandle { get; private set; }

        /// <summary>
        ///     Gets a list of icons resource names RT_GROUP_ICON;
        /// </summary>
        public List<ResourceName> IconNamesList { get; private set; }

        /// <summary>
        ///     Gets number of RT_GROUP_ICON found in the executable module.
        /// </summary>
        public int IconCount => IconNamesList.Count;

        /// <summary>
        ///     Gets or sets the RT_GROUP_ICON cache.
        /// </summary>
        private Dictionary<int, Icon> IconCache { get; set; }

        /// <summary>
        ///     Releases the resources of that object.
        /// </summary>
        public void Dispose()
        {
            if (ModuleHandle != IntPtr.Zero)
            {
                try
                {
                    NativeMethods.FreeLibrary(ModuleHandle);
                }
                catch
                {
                }
                ModuleHandle = IntPtr.Zero;
            }
            IconNamesList?.Clear();
        }

        /// <summary>
        ///     Destructs the IconExtractor object.
        /// </summary>
        ~IconExtractor()
        {
            Dispose();
        }

        /// <summary>
        ///     Gets a System.Drawing.Icon that represents RT_GROUP_ICON at the givin index.
        /// </summary>
        /// <param name="index">The index of the RT_GROUP_ICON in the executable module.</param>
        /// <returns>Returns System.Drawing.Icon.</returns>
        public Icon GetIconAt(int index)
        {
            if (index < 0 || index >= IconCount)
            {
                if (IconCount > 0)
                    throw new ArgumentOutOfRangeException(nameof(index), index,
                        "Index should be in the range (0-" + IconCount + ").");
                throw new ArgumentOutOfRangeException(nameof(index), index, "No icons in the list.");
            }

            if (!IconCache.ContainsKey(index))
                IconCache[index] = GetIconFromLib(index);

            return IconCache[index];
        }

        /// <summary>
        ///     This function maps a specified executable module into the address space of the calling process.
        ///     The executable module can be a .dll or an .exe file.
        ///     The specified module can cause other modules to be mapped into the address space.
        /// </summary>
        /// <param name="fileName">
        ///     The name of the executable module (either a .dll or an .exe file). The file name can contain
        ///     environment variables (like %SystemRoot%).
        /// </param>
        private void LoadLibrary(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException(nameof(fileName));

            FileName = Environment.ExpandEnvironmentVariables(fileName);
            //Load the executable module into memory using LoadLibraryEx API.
            ModuleHandle = NativeMethods.LoadLibraryEx(Environment.ExpandEnvironmentVariables(FileName), IntPtr.Zero,
                LoadLibraryFlags.LOAD_LIBRARY_AS_DATAFILE);
            if (ModuleHandle == IntPtr.Zero)
            {
                int errorNum = Marshal.GetLastWin32Error();
                if (errorNum != 0)
                    throw new Win32Exception(errorNum);
            }

            IconNamesList = new List<ResourceName>();
            IconCache = new Dictionary<int, Icon>();

            //Enumurate the resource names of RT_GROUP_ICON by calling EnumResourcesCallBack function for each resource of that type.
            NativeMethods.EnumResourceNames(ModuleHandle, ResourceTypes.GROUP_ICON, EnumResourcesCallBack, IntPtr.Zero);
        }

        /// <summary>
        ///     The callback function that is being called for each resource (RT_GROUP_ICON, RT_ICON) in the executable module.
        ///     The function stores the resource name of type RT_GROUP_ICON into the GroupIconsList and
        ///     stores the resource name of type RT_ICON into the IconsList.
        /// </summary>
        /// <param name="hModule">The module handle.</param>
        /// <param name="lpszType">Specifies the type of the resource being enumurated (RT_GROUP_ICON, RT_ICON).</param>
        /// <param name="lpszName">
        ///     Specifies the name of the resource being enumurated. For more ifnormation, see the Remarks
        ///     section.
        /// </param>
        /// <param name="lParam">Specifies the application defined parameter passed to the EnumResourceNames function.</param>
        /// <returns>This callback function return true to continue enumuration.</returns>
        /// <remarks>
        ///     If the high bit of lpszName is not set (=0), lpszName specifies the integer identifier of the givin resource.
        ///     Otherwise, it is a pointer to a null terminated string.
        ///     If the first character of the string is a pound sign (#), the remaining characters represent a decimal number that
        ///     specifies the integer identifier of the resource. For example, the string "#258" represents the identifier 258.
        ///     #define IS_INTRESOURCE(_r) ((((ULONG_PTR)(_r)) >> 16) == 0)
        /// </remarks>
        private bool EnumResourcesCallBack(IntPtr hModule, ResourceTypes lpszType, IntPtr lpszName, IntPtr lParam)
        {
            switch (lpszType)
            {
                case ResourceTypes.GROUP_ICON:
                    IconNamesList.Add(new ResourceName(lpszName));
                    break;
            }

            return true;
        }

        /// <summary>
        ///     Gets a System.Drawing.Icon that represents RT_GROUP_ICON at the givin index from the executable module.
        /// </summary>
        /// <param name="index">The index of the RT_GROUP_ICON in the executable module.</param>
        /// <returns>Returns System.Drawing.Icon.</returns>
        private Icon GetIconFromLib(int index)
        {
            byte[] resourceData = GetResourceData(ModuleHandle, IconNamesList[index], ResourceTypes.GROUP_ICON);
            //Convert the resouce into an .ico file image.
            using (MemoryStream inputStream = new MemoryStream(resourceData))
            using (MemoryStream destStream = new MemoryStream())
            {
                //Read the GroupIconDir header.
                GroupIconDir grpDir = Utility.ReadStructure<GroupIconDir>(inputStream);

                int numEntries = grpDir.Count;
                int iconImageOffset = IconInfo.SizeOfIconDir + numEntries*IconInfo.SizeOfIconDirEntry;

                //Write the IconDir header.
                Utility.WriteStructure(destStream, grpDir.ToIconDir());
                for (int i = 0; i < numEntries; i++)
                {
                    //Read the GroupIconDirEntry.
                    GroupIconDirEntry grpEntry = Utility.ReadStructure<GroupIconDirEntry>(inputStream);

                    //Write the IconDirEntry.
                    destStream.Seek(IconInfo.SizeOfIconDir + i*IconInfo.SizeOfIconDirEntry, SeekOrigin.Begin);
                    Utility.WriteStructure(destStream, grpEntry.ToIconDirEntry(iconImageOffset));

                    //Get the icon image raw data and write it to the stream.
                    byte[] imgBuf = GetResourceData(ModuleHandle, grpEntry.ID, ResourceTypes.ICON);
                    destStream.Seek(iconImageOffset, SeekOrigin.Begin);
                    destStream.Write(imgBuf, 0, imgBuf.Length);

                    //Append the iconImageOffset.
                    iconImageOffset += imgBuf.Length;
                }
                destStream.Seek(0, SeekOrigin.Begin);
                return new Icon(destStream);
            }
        }

        /// <summary>
        ///     Extracts the raw data of the resource from the module.
        /// </summary>
        /// <param name="hModule">The module handle.</param>
        /// <param name="resourceName">The name of the resource.</param>
        /// <param name="resourceType">The type of the resource.</param>
        /// <returns>The resource raw data.</returns>
        private static byte[] GetResourceData(IntPtr hModule, ResourceName resourceName, ResourceTypes resourceType)
        {
            //Find the resource in the module.
            IntPtr hResInfo = IntPtr.Zero;
            try
            {
                hResInfo = NativeMethods.FindResource(hModule, resourceName.Value, resourceType);
            }
            finally
            {
                resourceName.Free();
            }
            if (hResInfo == IntPtr.Zero)
            {
                throw new Win32Exception();
            }
            //Load the resource.
            IntPtr hResData = NativeMethods.LoadResource(hModule, hResInfo);
            if (hResData == IntPtr.Zero)
            {
                throw new Win32Exception();
            }
            //Lock the resource to read data.
            IntPtr hGlobal = NativeMethods.LockResource(hResData);
            if (hGlobal == IntPtr.Zero)
            {
                throw new Win32Exception();
            }
            //Get the resource size.
            int resSize = NativeMethods.SizeofResource(hModule, hResInfo);
            if (resSize == 0)
            {
                throw new Win32Exception();
            }
            //Allocate the requested size.
            byte[] buf = new byte[resSize];
            //Copy the resource data into our buffer.
            Marshal.Copy(hGlobal, buf, 0, buf.Length);

            return buf;
        }

        /// <summary>
        ///     Extracts the raw data of the resource from the module.
        /// </summary>
        /// <param name="hModule">The module handle.</param>
        /// <param name="resourceId">The identifier of the resource.</param>
        /// <param name="resourceType">The type of the resource.</param>
        /// <returns>The resource raw data.</returns>
        private static byte[] GetResourceData(IntPtr hModule, int resourceId, ResourceTypes resourceType)
        {
            //Find the resource in the module.
            IntPtr hResInfo = NativeMethods.FindResource(hModule, (IntPtr) resourceId, resourceType);
            if (hResInfo == IntPtr.Zero)
            {
                throw new Win32Exception();
            }
            //Load the resource.
            IntPtr hResData = NativeMethods.LoadResource(hModule, hResInfo);
            if (hResData == IntPtr.Zero)
            {
                throw new Win32Exception();
            }
            //Lock the resource to read data.
            IntPtr hGlobal = NativeMethods.LockResource(hResData);
            if (hGlobal == IntPtr.Zero)
            {
                throw new Win32Exception();
            }
            //Get the resource size.
            int resSize = NativeMethods.SizeofResource(hModule, hResInfo);
            if (resSize == 0)
            {
                throw new Win32Exception();
            }
            //Allocate the requested size.
            byte[] buf = new byte[resSize];
            //Copy the resource data into our buffer.
            Marshal.Copy(hGlobal, buf, 0, buf.Length);

            return buf;
        }
    }
}