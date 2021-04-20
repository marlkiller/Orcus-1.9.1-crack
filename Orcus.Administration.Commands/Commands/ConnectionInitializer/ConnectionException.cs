using System;
using Orcus.Shared.Commands.ConnectionInitializer;

namespace Orcus.Administration.Commands.ConnectionInitializer
{
    public class ConnectionException : Exception
    {
        public ConnectionException(string message, ConnectionType connectionType)
            : base($"{message} ({connectionType})")
        {
        }
    }
}