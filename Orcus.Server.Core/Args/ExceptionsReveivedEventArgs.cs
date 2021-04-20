using System;
using System.Collections.Generic;
using Orcus.Shared.Commands.ExceptionHandling;

namespace Orcus.Server.Core.Args
{
    public class ExceptionsReveivedEventArgs : EventArgs
    {
        public ExceptionsReveivedEventArgs(List<ExceptionInfo> exceptions)
        {
            Exceptions = exceptions;
        }

        public List<ExceptionInfo> Exceptions { get; }
    }
}