using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using Sorzus.Wpf.Toolkit.Native;

namespace Sorzus.Wpf.Toolkit.Images
{
    /// <summary>
    ///     Provides information about a givin icon.
    ///     This class cannot be inherited.
    /// </summary>
    [Serializable]
    internal class IconInfo
    {
        public static int SizeOfIconDir = Marshal.SizeOf(typeof (IconDir));
        public static int SizeOfIconDirEntry = Marshal.SizeOf(typeof (IconDirEntry));
        public static int SizeOfGroupIconDir = Marshal.SizeOf(typeof (GroupIconDir));
        public static int SizeOfGroupIconDirEntry = Marshal.SizeOf(typeof (GroupIconDirEntry));

        private int _bestFitIconIndex;
        private int _bitCount;
        private int _colorCount;

        private string _fileName;
        private GroupIconDir _groupIconDir;
        private List<GroupIconDirEntry> _groupIconDirEntries;
        private int _height;
        private IconDir _iconDir;
        private List<IconDirEntry> _iconDirEntries;
        private List<Icon> _images;
        private int _planes;
        private List<byte[]> _rawData;
        private byte[] _resourceRawData;
        private Icon _sourceIcon;

        private int _width;

        /// <summary>
        ///     Intializes a new instance of TAFactory.IconPack.IconInfo which contains the information about the givin icon.
        /// </summary>
        /// <param name="icon">A System.Drawing.Icon object to retrieve the information about.</param>
        public IconInfo(Icon icon)
        {
            FileName = null;
            LoadIconInfo(icon);
        }

        /// <summary>
        ///     Intializes a new instance of TAFactory.IconPack.IconInfo which contains the information about the icon in the givin
        ///     file.
        /// </summary>
        /// <param name="fileName">A fully qualified name of the icon file, it can contain environment variables.</param>
        public IconInfo(string fileName)
        {
            FileName = FileName;
            LoadIconInfo(new Icon(fileName));
        }

        /// <summary>
        ///     Gets the source System.Drawing.Icon.
        /// </summary>
        public Icon SourceIcon
        {
            get { return _sourceIcon; }
            private set { _sourceIcon = value; }
        }

        /// <summary>
        ///     Gets the icon's file name.
        /// </summary>
        public string FileName
        {
            get { return _fileName; }
            private set { _fileName = value; }
        }

        /// <summary>
        ///     Gets a list System.Drawing.Icon that presents the icon contained images.
        /// </summary>
        public List<Icon> Images
        {
            get { return _images; }
            private set { _images = value; }
        }

        /// <summary>
        ///     Get whether the icon contain more than one image or not.
        /// </summary>
        public bool IsMultiIcon => Images.Count > 1;

        /// <summary>
        ///     Gets icon index that best fits to screen resolution.
        /// </summary>
        public int BestFitIconIndex
        {
            get { return _bestFitIconIndex; }
            private set { _bestFitIconIndex = value; }
        }

        /// <summary>
        ///     Gets icon width.
        /// </summary>
        public int Width
        {
            get { return _width; }
            private set { _width = value; }
        }

        /// <summary>
        ///     Gets icon height.
        /// </summary>
        public int Height
        {
            get { return _height; }
            private set { _height = value; }
        }

        /// <summary>
        ///     Gets number of colors in icon (0 if >=8bpp).
        /// </summary>
        public int ColorCount
        {
            get { return _colorCount; }
            private set { _colorCount = value; }
        }

        /// <summary>
        ///     Gets icon color planes.
        /// </summary>
        public int Planes
        {
            get { return _planes; }
            private set { _planes = value; }
        }

        /// <summary>
        ///     Gets icon bits per pixel (0 if less than 8bpp).
        /// </summary>
        public int BitCount
        {
            get { return _bitCount; }
            private set { _bitCount = value; }
        }

        /// <summary>
        ///     Gets icon bits per pixel.
        /// </summary>
        public int ColorDepth
        {
            get
            {
                if (BitCount != 0)
                    return BitCount;
                if (ColorCount == 0)
                    return 0;
                return (int) Math.Log(ColorCount, 2);
            }
        }

        /// <summary>
        ///     Gets the TAFactory.IconPack.IconDir of the icon.
        /// </summary>
        public IconDir IconDir
        {
            get { return _iconDir; }
            private set { _iconDir = value; }
        }

        /// <summary>
        ///     Gets the TAFactory.IconPack.GroupIconDir of the icon.
        /// </summary>
        public GroupIconDir GroupIconDir
        {
            get { return _groupIconDir; }
            private set { _groupIconDir = value; }
        }

        /// <summary>
        ///     Gets a list of TAFactory.IconPack.IconDirEntry of the icon.
        /// </summary>
        public List<IconDirEntry> IconDirEntries
        {
            get { return _iconDirEntries; }
            private set { _iconDirEntries = value; }
        }

        /// <summary>
        ///     Gets a list of TAFactory.IconPack.GroupIconDirEntry of the icon.
        /// </summary>
        public List<GroupIconDirEntry> GroupIconDirEntries
        {
            get { return _groupIconDirEntries; }
            private set { _groupIconDirEntries = value; }
        }

        /// <summary>
        ///     Gets a list of raw data for each icon image.
        /// </summary>
        public List<byte[]> RawData
        {
            get { return _rawData; }
            private set { _rawData = value; }
        }

        /// <summary>
        ///     Gets the icon raw data as a resource data.
        /// </summary>
        public byte[] ResourceRawData
        {
            get { return _resourceRawData; }
            set { _resourceRawData = value; }
        }

        /// <summary>
        ///     Gets the index of the icon that best fits the current display device.
        /// </summary>
        /// <returns>The icon index.</returns>
        public int GetBestFitIconIndex()
        {
            int iconIndex;
            var resBits = Marshal.AllocHGlobal(ResourceRawData.Length);
            Marshal.Copy(ResourceRawData, 0, resBits, ResourceRawData.Length);
            try
            {
                iconIndex = NativeMethods.LookupIconIdFromDirectory(resBits, true);
            }
            finally
            {
                Marshal.FreeHGlobal(resBits);
            }

            return iconIndex;
        }

        /// <summary>
        ///     Gets the index of the icon that best fits the current display device.
        /// </summary>
        /// <param name="desiredSize">Specifies the desired size of the icon.</param>
        /// <returns>The icon index.</returns>
        public int GetBestFitIconIndex(Size desiredSize)
        {
            return GetBestFitIconIndex(desiredSize, false);
        }

        /// <summary>
        ///     Gets the index of the icon that best fits the current display device.
        /// </summary>
        /// <param name="desiredSize">Specifies the desired size of the icon.</param>
        /// <param name="isMonochrome">Specifies whether to get the monochrome icon or the colored one.</param>
        /// <returns>The icon index.</returns>
        public int GetBestFitIconIndex(Size desiredSize, bool isMonochrome)
        {
            var iconIndex = 0;
            var flags = LookupIconIdFromDirectoryExFlags.LR_DEFAULTCOLOR;
            if (isMonochrome)
                flags = LookupIconIdFromDirectoryExFlags.LR_MONOCHROME;
            var resBits = Marshal.AllocHGlobal(ResourceRawData.Length);
            Marshal.Copy(ResourceRawData, 0, resBits, ResourceRawData.Length);
            try
            {
                iconIndex = NativeMethods.LookupIconIdFromDirectoryEx(resBits, true, desiredSize.Width,
                    desiredSize.Height,
                    flags);
            }
            finally
            {
                Marshal.FreeHGlobal(resBits);
            }

            return iconIndex;
        }

        /// <summary>
        ///     Loads the icon information from the givin icon into class members.
        /// </summary>
        /// <param name="icon">A System.Drawing.Icon object to retrieve the information about.</param>
        private void LoadIconInfo(Icon icon)
        {
            if (icon == null)
                throw new ArgumentNullException(nameof(icon));

            SourceIcon = icon;
            var inputStream = new MemoryStream();
            SourceIcon.Save(inputStream);

            inputStream.Seek(0, SeekOrigin.Begin);
            var dir = Utility.ReadStructure<IconDir>(inputStream);

            IconDir = dir;
            GroupIconDir = dir.ToGroupIconDir();

            Images = new List<Icon>(dir.Count);
            IconDirEntries = new List<IconDirEntry>(dir.Count);
            GroupIconDirEntries = new List<GroupIconDirEntry>(dir.Count);
            RawData = new List<byte[]>(dir.Count);

            var newDir = dir;
            newDir.Count = 1;
            for (var i = 0; i < dir.Count; i++)
            {
                inputStream.Seek(SizeOfIconDir + i*SizeOfIconDirEntry, SeekOrigin.Begin);

                var entry = Utility.ReadStructure<IconDirEntry>(inputStream);

                IconDirEntries.Add(entry);
                GroupIconDirEntries.Add(entry.ToGroupIconDirEntry(i));

                var content = new byte[entry.BytesInRes];
                inputStream.Seek(entry.ImageOffset, SeekOrigin.Begin);
                inputStream.Read(content, 0, content.Length);
                RawData.Add(content);

                var newEntry = entry;
                newEntry.ImageOffset = SizeOfIconDir + SizeOfIconDirEntry;

                var outputStream = new MemoryStream();
                Utility.WriteStructure(outputStream, newDir);
                Utility.WriteStructure(outputStream, newEntry);
                outputStream.Write(content, 0, content.Length);

                outputStream.Seek(0, SeekOrigin.Begin);
                Icon newIcon;
                try
                {
                    newIcon = new Icon(outputStream);
                }
                catch (Exception)
                {
                    return;
                }

                outputStream.Close();

                Images.Add(newIcon);
                if (dir.Count == 1)
                {
                    BestFitIconIndex = 0;

                    Width = entry.Width;
                    Height = entry.Height;
                    ColorCount = entry.ColorCount;
                    Planes = entry.Planes;
                    BitCount = entry.BitCount;
                }
            }
            inputStream.Close();
            ResourceRawData = GetIconResourceData();

            if (dir.Count > 1)
            {
                BestFitIconIndex = GetBestFitIconIndex();

                Width = IconDirEntries[BestFitIconIndex].Width;
                Height = IconDirEntries[BestFitIconIndex].Height;
                ColorCount = IconDirEntries[BestFitIconIndex].ColorCount;
                Planes = IconDirEntries[BestFitIconIndex].Planes;
                BitCount = IconDirEntries[BestFitIconIndex].BitCount;
            }
        }

        /// <summary>
        ///     Returns the icon's raw data as a resource data.
        /// </summary>
        /// <returns>The icon's raw as a resource data.</returns>
        private byte[] GetIconResourceData()
        {
            var outputStream = new MemoryStream();
            Utility.WriteStructure(outputStream, GroupIconDir);
            foreach (var entry in GroupIconDirEntries)
            {
                Utility.WriteStructure(outputStream, entry);
            }

            return outputStream.ToArray();
        }
    }
}