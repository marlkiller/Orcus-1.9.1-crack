using System;

namespace Orcus.Shared.DataTransferProtocol
{
    public class UnhandledExceptionEventArgs : EventArgs
    {
        public UnhandledExceptionEventArgs(Exception exception)
        {
            Exception = exception;
        }

        public Exception Exception { get; }
    }
}