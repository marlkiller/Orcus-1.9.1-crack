using System;
using Orcus.Shared.Commands.WindowsDrivers;

namespace Orcus.Administration.Commands.WindowsDrivers
{
    public class DriversFileContentReceivedEventArgs : EventArgs
    {
        public DriversFileContentReceivedEventArgs(string content, WindowsDriversFile windowsDriversFile)
        {
            Content = content;
            WindowsDriversFile = windowsDriversFile;
        }

        public string Content { get; }
        public WindowsDriversFile WindowsDriversFile { get; }
    }
}