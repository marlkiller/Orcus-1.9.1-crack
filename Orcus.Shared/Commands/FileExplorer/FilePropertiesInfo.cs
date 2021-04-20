using System;
using System.Collections.Generic;

namespace Orcus.Shared.Commands.FileExplorer
{
    [Serializable]
    public class FilePropertiesInfo : PropertiesInfo
    {
        public string OpenWithProgramPath { get; set; }
        public string OpenWithProgramName { get; set; }
        public long Size { get; set; }
        public long SizeOnDisk { get; set; }
        public ExecutableProperties ExecutableProperties { get; set; }
        public List<FileProperty> FileProperties { get; set; }
    }

    [Serializable]
    public class FileProperty
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public FilePropertyGroup Group { get; set; }
    }

    [Serializable]
    public class ShellProperty : FileProperty
    {
        public Guid FormatId { get; set; }
        public int PropertyId { get; set; }
    }

    public enum FilePropertyGroup
    {
        FileVersionInfo,
        Executable,
        Details,
        Audio,
        Photo,
        Image,
        Media,
        Document,
        Music,
        Calendar,
        Contact,
        Message,
        Note,
        Task,
        RecordedTV,
        Video
    }
}