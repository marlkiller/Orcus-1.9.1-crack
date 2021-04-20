using System;
using System.Diagnostics;

namespace Orcus.Shared.Commands.TaskManager
{
    [Serializable]
    public class ProcessInfo
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string CompanyName { get; set; }
        public long WorkingSet { get; set; }
        public long PrivateBytes { get; set; }
        public byte[] IconBytes { get; set; }
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public ProcessPriorityClass PriorityClass { get; set; }
        public bool CanChangePriorityClass { get; set; }
        public int ParentProcess { get; set; }
        public string ProcessOwner { get; set; }
        public ProcessStatus Status { get; set; }
        public string Filename { get; set; }
        public string CommandLine { get; set; }
        public string ProductVersion { get; set; }
        public string FileVersion { get; set; }
        public long MainWindowHandle { get; set; }
    }
}