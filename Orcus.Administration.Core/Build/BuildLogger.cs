using System;
using Orcus.Administration.Plugins.BuildPlugin;

namespace Orcus.Administration.Core.Build
{
    public class BuildLogger : IBuildLogger
    {
        public void Status(string message)
        {
            NewLogMessage?.Invoke(this, new NewBuildLogMessageEventArgs(message, BuildLogType.Status));
        }

        public void Warn(string message)
        {
            NewLogMessage?.Invoke(this, new NewBuildLogMessageEventArgs(message, BuildLogType.Warning));
        }

        public void Error(string message)
        {
            NewLogMessage?.Invoke(this, new NewBuildLogMessageEventArgs(message, BuildLogType.Error));
        }

        public void Success(string message)
        {
            NewLogMessage?.Invoke(this, new NewBuildLogMessageEventArgs(message, BuildLogType.Success));
        }

        public event EventHandler<NewBuildLogMessageEventArgs> NewLogMessage;
    }

    public enum BuildLogType
    {
        Status,
        Warning,
        Error,
        Success
    }

    public class NewBuildLogMessageEventArgs : EventArgs
    {
        public NewBuildLogMessageEventArgs(string content, BuildLogType buildLogType)
        {
            Content = content;
            BuildLogType = buildLogType;
        }

        public BuildLogType BuildLogType { get; }
        public string Content { get; }
    }
}