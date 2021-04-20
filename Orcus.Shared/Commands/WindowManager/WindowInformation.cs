using System;

namespace Orcus.Shared.Commands.WindowManager
{
    [Serializable]
    public class WindowInformation
    {
        public string Caption { get; set; }
        public string ClassName { get; set; }
        public long Handle { get; set; } //I don't know how the serializer serializes an IntPtr so we better use long
        public long ParentHandle { get; set; }
        public int ProcessId { get; set; }
        public string ProcessName { get; set; }
        public bool IsVisible { get; set; }
    }
}