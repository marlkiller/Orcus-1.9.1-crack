using System;

namespace Orcus.Administration.Core.Args
{
    public class PluginUploadProgressChangedEventArgs : EventArgs
    {
        public PluginUploadProgressChangedEventArgs(double progress, int bytesSent, int totalBytes, string pluginName)
        {
            Progress = progress;
            BytesSent = bytesSent;
            TotalBytes = totalBytes;
            PluginName = pluginName;
        }

        public double Progress { get; }
        public int BytesSent { get; }
        public int TotalBytes { get; }
        public string PluginName { get; }
    }
}