using System;

namespace Orcus.Server.Core.Args
{
    public class AddKeyLogEventArgs : EventArgs
    {
        public AddKeyLogEventArgs(byte[] data)
        {
            Data = data;
        }

        public byte[] Data { get; }
    }
}