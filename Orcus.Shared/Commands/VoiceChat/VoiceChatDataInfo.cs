using System;
using System.IO;
using Orcus.Shared.Data;

namespace Orcus.Shared.Commands.VoiceChat
{
    public class VoiceChatDataInfo : IDataInfo
    {
        private readonly byte[][] _segments;
        private readonly int[] _segmentLengths;

        public VoiceChatDataInfo(byte[][] segments, int[] segmentLengths)
        {
            _segments = segments;
            _segmentLengths = segmentLengths;

            Length = 0;
            for (int i = 0; i < segmentLengths.Length; i++)
                Length += 4 + segmentLengths[i];
        }

        public int Length { get; }

        public byte[] ToArray()
        {
            var buffer = new byte[Length];
            var position = 0;
            for (int i = 0; i < _segments.Length; i++)
            {
                var segmentLength = _segmentLengths[i];
                Buffer.BlockCopy(BitConverter.GetBytes(segmentLength), 0, buffer, position, 4);
                Buffer.BlockCopy(_segments[i], 0, buffer, position + 4, segmentLength);
                position += 4 + segmentLength;
            }

            return buffer;
        }

        public void WriteToBuffer(byte[] buffer, int index)
        {
            var position = index;
            for (int i = 0; i < _segments.Length; i++)
            {
                var segmentLength = _segmentLengths[i];
                Buffer.BlockCopy(BitConverter.GetBytes(segmentLength), 0, buffer, position, 4);
                Buffer.BlockCopy(_segments[i], 0, buffer, position + 4, segmentLength);
                position += 4 + segmentLength;
            }
        }

        public void WriteIntoStream(Stream outStream)
        {
            for (int i = 0; i < _segments.Length; i++)
            {
                var segmentLength = _segmentLengths[i];
                outStream.Write(BitConverter.GetBytes(segmentLength), 0, 4);
                outStream.Write(_segments[i], 0, segmentLength);
            }
        }
    }
}