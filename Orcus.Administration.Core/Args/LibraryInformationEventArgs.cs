using System;
using Orcus.Shared.Connection;

namespace Orcus.Administration.Core.Args
{
    public class LibraryInformationEventArgs : EventArgs
    {
        public LibraryInformationEventArgs(int clientId, PortableLibrary libraries)
        {
            ClientId = clientId;
            Libraries = libraries;
        }

        public int ClientId { get; set; }
        public PortableLibrary Libraries { get; set; }
    }
}