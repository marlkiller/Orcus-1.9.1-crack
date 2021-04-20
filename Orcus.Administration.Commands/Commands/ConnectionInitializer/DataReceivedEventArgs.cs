using System;

namespace Orcus.Administration.Commands.ConnectionInitializer
{
    public class DataReceivedEventArgs : EventArgs
    {
        public DataReceivedEventArgs(byte[] buffer) : this(buffer, 0, buffer.Length)
        {
        }

        public DataReceivedEventArgs(byte[] buffer, int index, int length)
        {
            Buffer = buffer;
            Index = index;
            Length = length;
        }

        public byte[] Buffer { get; }
        public int Index { get; }
        public int Length { get; }

        public byte[] CopyToTruncatedBuffer()
        {
            if (Index == 0 && Length == Buffer.Length)
                return Buffer;

            var buffer = new byte[Length];
            System.Buffer.BlockCopy(Buffer, Index, buffer, 0, Length);
            return buffer;
        }
    }
}