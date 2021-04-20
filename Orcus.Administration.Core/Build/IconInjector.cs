using System;
using System.IO;
using System.Runtime.InteropServices;
using Orcus.Administration.Core.Native;
using Vestris.ResourceLib;

namespace Orcus.Administration.Core.Build
{
    public static class IconInjector
    {
        // ReSharper disable InconsistentNaming
        private const uint RT_ICON = 3u;
        private const uint RT_GROUP_ICON = 14u;
        // ReSharper restore InconsistentNaming

        public static void InjectIcon(string exeFileName, string iconFileName)
        {
            InjectIcon(exeFileName, iconFileName, 1, 1);
        }

        public static void InjectIcon(string exeFileName, string iconFileName, uint iconGroupId, uint iconBaseId)
        {
            IconFile iconFile = IconFile.FromFile(iconFileName);
            var hUpdate = NativeMethods.BeginUpdateResource(exeFileName, false);
            var data = iconFile.CreateIconGroupData(iconBaseId);
            NativeMethods.UpdateResource(hUpdate, new IntPtr(RT_GROUP_ICON), new IntPtr(iconGroupId), 0, data,
                data.Length);
            for (int i = 0; i <= iconFile.ImageCount - 1; i++)
            {
                var image = iconFile.ImageData(i);
                NativeMethods.UpdateResource(hUpdate, new IntPtr(RT_ICON), new IntPtr(iconBaseId + i), 0, image,
                    image.Length);
            }
            NativeMethods.EndUpdateResource(hUpdate, false);
        }

        private class IconFile
        {
            private ICONDIR _iconDir;
            private ICONDIRENTRY[] _iconEntry;

            private byte[][] _iconImage;

            public int ImageCount => _iconDir.Count;

            public byte[] ImageData(int index)
            {
                return _iconImage[index];
            }

            public static IconFile FromFile(string filename)
            {
                IconFile instance = new IconFile();
                byte[] fileBytes = File.ReadAllBytes(filename);
                GCHandle pinnedBytes = GCHandle.Alloc(fileBytes, GCHandleType.Pinned);
                instance._iconDir = (ICONDIR) Marshal.PtrToStructure(pinnedBytes.AddrOfPinnedObject(), typeof (ICONDIR));
                instance._iconEntry = new ICONDIRENTRY[instance._iconDir.Count];
                instance._iconImage = new byte[instance._iconDir.Count][];
                int offset = Marshal.SizeOf(instance._iconDir);
                var iconDirEntryType = typeof (ICONDIRENTRY);
                var size = Marshal.SizeOf(iconDirEntryType);
                for (int i = 0; i <= instance._iconDir.Count - 1; i++)
                {
                    var entry =
                        (ICONDIRENTRY)
                            Marshal.PtrToStructure(new IntPtr(pinnedBytes.AddrOfPinnedObject().ToInt64() + offset),
                                iconDirEntryType);
                    instance._iconEntry[i] = entry;
                    instance._iconImage[i] = new byte[entry.BytesInRes];
                    Buffer.BlockCopy(fileBytes, entry.ImageOffset, instance._iconImage[i], 0, entry.BytesInRes);
                    offset += size;
                }
                pinnedBytes.Free();
                return instance;
            }

            public byte[] CreateIconGroupData(uint iconBaseId)
            {
                // This will store the memory version of the icon.
                int sizeOfIconGroupData = Marshal.SizeOf(typeof (ICONDIR)) +
                                          Marshal.SizeOf(typeof (GRPICONDIRENTRY))*ImageCount;
                byte[] data = new byte[sizeOfIconGroupData];
                var pinnedData = GCHandle.Alloc(data, GCHandleType.Pinned);
                Marshal.StructureToPtr(_iconDir, pinnedData.AddrOfPinnedObject(), false);
                var offset = Marshal.SizeOf(_iconDir);
                for (int i = 0; i <= ImageCount - 1; i++)
                {
                    GRPICONDIRENTRY grpEntry = new GRPICONDIRENTRY();
                    BITMAPINFOHEADER bitmapheader = new BITMAPINFOHEADER();
                    var pinnedBitmapInfoHeader = GCHandle.Alloc(bitmapheader, GCHandleType.Pinned);
                    Marshal.Copy(ImageData(i), 0, pinnedBitmapInfoHeader.AddrOfPinnedObject(),
                        Marshal.SizeOf(typeof (Gdi32.BITMAPINFOHEADER)));
                    pinnedBitmapInfoHeader.Free();
                    grpEntry.Width = _iconEntry[i].Width;
                    grpEntry.Height = _iconEntry[i].Height;
                    grpEntry.ColorCount = _iconEntry[i].ColorCount;
                    grpEntry.Reserved = _iconEntry[i].Reserved;
                    grpEntry.Planes = bitmapheader.Planes;
                    grpEntry.BitCount = bitmapheader.BitCount;
                    grpEntry.BytesInRes = _iconEntry[i].BytesInRes;
                    grpEntry.ID = Convert.ToUInt16(iconBaseId + i);
                    Marshal.StructureToPtr(grpEntry, new IntPtr(pinnedData.AddrOfPinnedObject().ToInt64() + offset),
                        false);
                    offset += Marshal.SizeOf(typeof (GRPICONDIRENTRY));
                }
                pinnedData.Free();
                return data;
            }
        }
    }
}