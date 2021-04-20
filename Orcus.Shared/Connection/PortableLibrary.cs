using System;

namespace Orcus.Shared.Connection
{
    /// <summary>
    ///     Free libraries which get transfered to the client when they are needed
    /// </summary>
    [Flags]
    public enum PortableLibrary
    {
        /// <summary>
        ///     No library
        /// </summary>
        None,

        /// <summary>
        ///     Modified version of SharpDX 3.1.0
        /// </summary>
        [PortableLibraryName("SharpDX.dll")] SharpDX = 1 << 1,

        /// <summary>
        ///     Modified version of SharpDX.Direct3D11 3.1.0
        /// </summary>
        [PortableLibraryName("SharpDX.Direct3D11.dll")] SharpDX_Direct3D11 = 1 << 2,

        /// <summary>
        ///     Modified version of SharpDX.Direct3D9 3.1.0
        /// </summary>
        [PortableLibraryName("SharpDX.Direct3D9.dll")] SharpDX_Direct3D9 = 1 << 3,

        /// <summary>
        ///     Modified version of SharpDX.DXGI 3.1.0
        /// </summary>
        [PortableLibraryName("SharpDX.DXGI.dll")] SharpDX_DXGI = 1 << 4,

        /// <summary>
        ///     CSCore 1.2
        /// </summary>
        [PortableLibraryName("CSCore.dll")] CSCore = 1 << 5,

        /// <summary>
        ///     Modified version of Lidgren.Network (commit a7468f4, version 8.0.30703)
        /// </summary>
        [PortableLibraryName("Lidgren.Network.dll")] LidgrenNetwork = 1 << 6,

        /// <summary>
        ///     Modified version of DirectoryInfoEx
        /// </summary>
        [PortableLibraryName("DirectoryInfoEx.dll")] DirectoryInfoEx = 1 << 7,

        /// <summary>
        ///     Modified version of ShellLibrary
        /// </summary>
        [PortableLibraryName("ShellLibrary.dll")] ShellLibrary = 1 << 8,

        /// <summary>
        ///     AForge.Video version 2.2.5
        /// </summary>
        [PortableLibraryName("AForge.Video.dll")] AForge_Video = 1 << 9,

        /// <summary>
        ///     AForge.Video.DirectShow version 2.2.5
        /// </summary>
        [PortableLibraryName("AForge.Video.DirectShow.dll")] AForge_Video_DirectShow = 1 << 10,

        /// <summary>
        ///     Modified version of TurboJpegWrapper (version 1.4.2)
        /// </summary>
        [PortableLibraryName("TurboJpegWrapper.dll")] TurboJpegWrapper = 1 << 11,

        /// <summary>
        ///     A wrapper for Opus
        /// </summary>
        [PortableLibraryName("OpusWrapper.dll")] OpusWrapper = 1 << 12,

        /// <summary>
        ///     SharpZipLib (version 0.86.0)
        /// </summary>
        [PortableLibraryName("ICSharpCode.SharpZipLib.dll")] SharpZipLib = 1 << 13
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class PortableLibraryNameAttribute : Attribute
    {
        public PortableLibraryNameAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}