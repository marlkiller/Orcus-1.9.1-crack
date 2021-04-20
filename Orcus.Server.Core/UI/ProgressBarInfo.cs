using System;

namespace Orcus.Server.Core.UI
{
    public class ProgressBarInfo
    {
        internal ProgressBarInfo(string message)
        {
            Message = message;
        }

        public bool IsClosed { get; private set; }
        public string Message { get; }
        public event EventHandler<double> ProgressChanged;
        public event EventHandler Closed;

        internal void ReportProgress(double progress)
        {
            ProgressChanged?.Invoke(this, progress);
        }

        internal void Close()
        {
            IsClosed = true;
            Closed?.Invoke(this, EventArgs.Empty);
        }
    }
}