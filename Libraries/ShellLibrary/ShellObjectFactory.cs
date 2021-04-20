using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using ShellLibrary.Native;

namespace ShellLibrary
{
    internal static class ShellObjectFactory
    {
        /// <summary>
        /// Creates a ShellObject given a parsing name
        /// </summary>
        /// <param name="parsingName"></param>
        /// <returns>A newly constructed ShellObject object</returns>
        internal static ShellObject Create(string parsingName)
        {
            if (string.IsNullOrEmpty(parsingName))
            {
                throw new ArgumentNullException("parsingName");
            }

            // Create a native shellitem from our path
            IShellItem2 nativeShellItem;
            Guid guid = new Guid("7E9FB0D3-919F-4307-AB2E-9B1860310C93");
            int retCode = ShellNativeMethods.SHCreateItemFromParsingName(parsingName, IntPtr.Zero, ref guid, out nativeShellItem);

            if (retCode != 0)
            {
                throw new Exception("ShellObjectFactoryUnableToCreateItem", Marshal.GetExceptionForHR(retCode));
            }
            return ShellObjectFactory.Create(nativeShellItem);
        }

        /// <summary>
        /// Creates a ShellObject given a native IShellItem interface
        /// </summary>
        /// <param name="nativeShellItem"></param>
        /// <returns>A newly constructed ShellObject object</returns>
        internal static ShellObject Create(IShellItem nativeShellItem)
        {
            // Sanity check
            Debug.Assert(nativeShellItem != null, "nativeShellItem should not be null");

            // Need to make sure we're running on Vista or higher
            if (!(Environment.OSVersion.Version.Major >= 6))
            {
                throw new PlatformNotSupportedException("ShellObjectFactoryPlatformNotSupported");
            }

            // A lot of APIs need IShellItem2, so just keep a copy of it here
            IShellItem2 nativeShellItem2 = nativeShellItem as IShellItem2;

            // Get the System.ItemType property
            string itemType = ShellHelper.GetItemType(nativeShellItem2);

            if (!string.IsNullOrEmpty(itemType)) { itemType = itemType.ToUpperInvariant(); }

            // Get some IShellItem attributes
            ShellNativeMethods.ShellFileGetAttributesOptions sfgao;
            nativeShellItem2.GetAttributes(ShellNativeMethods.ShellFileGetAttributesOptions.FileSystem | ShellNativeMethods.ShellFileGetAttributesOptions.Folder, out sfgao);

            // Is this item a FileSystem item?
            bool isFileSystem = (sfgao & ShellNativeMethods.ShellFileGetAttributesOptions.FileSystem) != 0;

            // Is this item a Folder?
            bool isFolder = (sfgao & ShellNativeMethods.ShellFileGetAttributesOptions.Folder) != 0;

            // Create the right type of ShellObject based on the above information 

            // 1. First check if this is a Shell Link
            if (itemType == ".lnk")
            {
                throw new Exception("No links allowed");
            }
            // 2. Check if this is a container or a single item (entity)
            else if (isFolder)
            {
                throw new Exception("No folders");
            }

            // 6. If this is an entity (single item), check if its filesystem or not
            if (isFileSystem)
            { return new ShellFile(nativeShellItem2); }

            throw new Exception("No other things");
        }
    }
}