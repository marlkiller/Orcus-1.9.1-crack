using System;
using System.IO;
using Orcus.Shared.Resharper;

namespace Orcus.Shared.Data
{
    /// <summary>
    ///     A temporary object which provides information for writing into a stream
    /// </summary>
    public class WriterCall
    {
        private readonly Action<Stream> _writerCall;

        /// <summary>
        ///     Initialize a new instance of <see cref="WriterCall" />
        /// </summary>
        /// <param name="size">The length of the bytes which will be written into the stream</param>
        /// <param name="writerCall">The delegate which will write the bytes of the given <see cref="size" /> into the stream</param>
        public WriterCall(int size, [InstantHandle] Action<BinaryWriter> writerCall)
        {
            _writerCall = stream =>
            {
                //no need to dispose this BinaryWriter, check https://referencesource.microsoft.com/#mscorlib/system/io/binarywriter.cs,100
                writerCall(new BinaryWriter(stream));
            };
            Size = size;
        }

        /// <summary>
        ///     Initialize a new instance of <see cref="WriterCall" />
        /// </summary>
        /// <param name="size">The length of the bytes which will be written into the stream</param>
        /// <param name="writerCall">The delegate which will write the bytes of the given <see cref="size" /> into the stream</param>
        public WriterCall(int size, [InstantHandle] Action<Stream> writerCall)
        {
            _writerCall = writerCall;
            Size = size;
        }

        /// <summary>
        ///     Initialize a new instance of <see cref="WriterCall" />
        /// </summary>
        /// <param name="data">The data to write into the stream</param>
        /// <param name="index">The start index</param>
        /// <param name="count">The length of the bytes which should be written into the stream</param>
        public WriterCall(byte[] data, int index, int count)
        {
            Size = count;
            _writerCall = stream => stream.Write(data, index, count);
        }

        /// <summary>
        ///     Initialize a new instance of <see cref="WriterCall" />
        /// </summary>
        /// <param name="data">The bytes which should be written into the stream</param>
        public WriterCall(byte[] data) : this(data, 0, data.Length)
        {
        }

        /// <summary>
        ///     Initialize a new instance of <see cref="WriterCall" />
        /// </summary>
        /// <param name="dataInfo">The data info which should be sent</param>
        public WriterCall(IDataInfo dataInfo)
        {
            Size = dataInfo.Length;
            _writerCall = dataInfo.WriteIntoStream;
        }

        /// <summary>
        ///     The amount of bytes which will be written into the stream by this delegate
        /// </summary>
        public int Size { get; }

        /// <summary>
        ///     Write bytes into the given stream
        /// </summary>
        /// <param name="stream">The stream which should receive the bytes</param>
        public void WriteIntoStream(Stream stream)
        {
            _writerCall(stream);
        }
    }
}