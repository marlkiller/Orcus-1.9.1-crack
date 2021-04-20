using System;

namespace Orcus.Shared.Commands.FileExplorer
{
    [Serializable]
    public class DirectoryPropertiesInfo : PropertiesInfo
    {
        public DirectoryType DirectoryType { get; set; }

        //Special Folder
        public SpecialFolderType SpecialFolderType { get; set; }

        //Drive
        public string DriveFormat { get; set; }
    }

    public enum SpecialFolderType
    {
        Virtual = 1,
        Fixed = 2,
        Common = 3,
        PerUser = 4
    }

    public enum DirectoryType
    {
        Normal,
        SpecialFolder,
        Drive
    }
}