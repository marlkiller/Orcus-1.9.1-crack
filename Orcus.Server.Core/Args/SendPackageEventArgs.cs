using System;
using Orcus.Shared.Data;
using Orcus.Shared.Server;

namespace Orcus.Server.Core.Args
{
    public class SendPackageEventArgs : EventArgs
    {
        public SendPackageEventArgs()
        {
        }

        public SendPackageEventArgs(byte command, WriterCall writerCall)
        {
            Command = command;
            WriterCall = writerCall;
        }

        public byte Command { get; protected set; }
        public WriterCall WriterCall { get; protected set; }
    }
}