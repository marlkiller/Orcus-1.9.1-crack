/*
 * Copyright 2015 Tomi Valkeinen
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace Orcus.Shared.NetSerializer
{
    internal static class Primitives
    {
        public static MethodInfo GetWritePrimitive(Type type)
        {
            return typeof (Primitives).GetMethod("WritePrimitive",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.ExactBinding, null,
                new[] {typeof (Stream), type}, null);
        }

        public static MethodInfo GetReaderPrimitive(Type type)
        {
            return typeof (Primitives).GetMethod("ReadPrimitive",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.ExactBinding, null,
                new[] {typeof (Stream), type.MakeByRefType()}, null);
        }

        private static uint EncodeZigZag32(int n)
        {
            return (uint) ((n << 1) ^ (n >> 31));
        }

        private static ulong EncodeZigZag64(long n)
        {
            return (ulong) ((n << 1) ^ (n >> 63));
        }

        private static int DecodeZigZag32(uint n)
        {
            return (int) (n >> 1) ^ -(int) (n & 1);
        }

        private static long DecodeZigZag64(ulong n)
        {
            return (long) (n >> 1) ^ -(long) (n & 1);
        }

        private static uint ReadVarint32(Stream stream)
        {
            int result = 0;
            int offset = 0;

            for (; offset < 32; offset += 7)
            {
                int b = stream.ReadByte();
                if (b == -1)
                    throw new EndOfStreamException();

                result |= (b & 0x7f) << offset;

                if ((b & 0x80) == 0)
                    return (uint) result;
            }

            throw new InvalidDataException();
        }

        private static void WriteVarint32(Stream stream, uint value)
        {
            for (; value >= 0x80u; value >>= 7)
                stream.WriteByte((byte) (value | 0x80u));

            stream.WriteByte((byte) value);
        }

        private static ulong ReadVarint64(Stream stream)
        {
            long result = 0;
            int offset = 0;

            for (; offset < 64; offset += 7)
            {
                int b = stream.ReadByte();
                if (b == -1)
                    throw new EndOfStreamException();

                result |= (long) (b & 0x7f) << offset;

                if ((b & 0x80) == 0)
                    return (ulong) result;
            }

            throw new InvalidDataException();
        }

        private static void WriteVarint64(Stream stream, ulong value)
        {
            for (; value >= 0x80u; value >>= 7)
                stream.WriteByte((byte) (value | 0x80u));

            stream.WriteByte((byte) value);
        }


        public static void WritePrimitive(Stream stream, bool value)
        {
            stream.WriteByte(value ? (byte) 1 : (byte) 0);
        }

        public static void ReadPrimitive(Stream stream, out bool value)
        {
            var b = stream.ReadByte();
            value = b != 0;
        }

        public static void WritePrimitive(Stream stream, byte value)
        {
            stream.WriteByte(value);
        }

        public static void ReadPrimitive(Stream stream, out byte value)
        {
            value = (byte) stream.ReadByte();
        }

        public static void WritePrimitive(Stream stream, sbyte value)
        {
            stream.WriteByte((byte) value);
        }

        public static void ReadPrimitive(Stream stream, out sbyte value)
        {
            value = (sbyte) stream.ReadByte();
        }

        public static void WritePrimitive(Stream stream, char value)
        {
            WriteVarint32(stream, value);
        }

        public static void ReadPrimitive(Stream stream, out char value)
        {
            value = (char) ReadVarint32(stream);
        }

        public static void WritePrimitive(Stream stream, ushort value)
        {
            WriteVarint32(stream, value);
        }

        public static void ReadPrimitive(Stream stream, out ushort value)
        {
            value = (ushort) ReadVarint32(stream);
        }

        public static void WritePrimitive(Stream stream, short value)
        {
            WriteVarint32(stream, EncodeZigZag32(value));
        }

        public static void ReadPrimitive(Stream stream, out short value)
        {
            value = (short) DecodeZigZag32(ReadVarint32(stream));
        }

        public static void WritePrimitive(Stream stream, uint value)
        {
            WriteVarint32(stream, value);
        }

        public static void ReadPrimitive(Stream stream, out uint value)
        {
            value = ReadVarint32(stream);
        }

        public static void WritePrimitive(Stream stream, int value)
        {
            WriteVarint32(stream, EncodeZigZag32(value));
        }

        public static void ReadPrimitive(Stream stream, out int value)
        {
            value = DecodeZigZag32(ReadVarint32(stream));
        }

        public static void WritePrimitive(Stream stream, ulong value)
        {
            WriteVarint64(stream, value);
        }

        public static void ReadPrimitive(Stream stream, out ulong value)
        {
            value = ReadVarint64(stream);
        }

        public static void WritePrimitive(Stream stream, long value)
        {
            WriteVarint64(stream, EncodeZigZag64(value));
        }

        public static void ReadPrimitive(Stream stream, out long value)
        {
            value = DecodeZigZag64(ReadVarint64(stream));
        }

#if !NO_UNSAFE
        public static unsafe void WritePrimitive(Stream stream, float value)
        {
            uint v = *(uint*) &value;
            WriteVarint32(stream, v);
        }

        public static unsafe void ReadPrimitive(Stream stream, out float value)
        {
            uint v = ReadVarint32(stream);
            value = *(float*) &v;
        }

        public static unsafe void WritePrimitive(Stream stream, double value)
        {
            ulong v = *(ulong*) &value;
            WriteVarint64(stream, v);
        }

        public static unsafe void ReadPrimitive(Stream stream, out double value)
        {
            ulong v = ReadVarint64(stream);
            value = *(double*) &v;
        }
#else
		public static void WritePrimitive(Stream stream, float value)
		{
			WritePrimitive(stream, (double)value);
		}

		public static void ReadPrimitive(Stream stream, out float value)
		{
			double v;
			ReadPrimitive(stream, out v);
			value = (float)v;
		}

		public static void WritePrimitive(Stream stream, double value)
		{
			ulong v = (ulong)BitConverter.DoubleToInt64Bits(value);
			WriteVarint64(stream, v);
		}

		public static void ReadPrimitive(Stream stream, out double value)
		{
			ulong v = ReadVarint64(stream);
			value = BitConverter.Int64BitsToDouble((long)v);
		}
#endif

        public static void WritePrimitive(Stream stream, DateTime value)
        {
            long v = value.ToBinary();
            WritePrimitive(stream, v);
        }

        public static void ReadPrimitive(Stream stream, out DateTime value)
        {
            long v;
            ReadPrimitive(stream, out v);
            value = DateTime.FromBinary(v);
        }

#if NO_UNSAFE
		public static void WritePrimitive(Stream stream, string value)
		{
			if (value == null)
			{
				WritePrimitive(stream, (uint)0);
				return;
			}

			var encoding = new UTF8Encoding(false, true);

			int len = encoding.GetByteCount(value);

			WritePrimitive(stream, (uint)len + 1);

			var buf = new byte[len];

			encoding.GetBytes(value, 0, value.Length, buf, 0);

			stream.Write(buf, 0, len);
		}

		public static void ReadPrimitive(Stream stream, out string value)
		{
			uint len;
			ReadPrimitive(stream, out len);

			if (len == 0)
			{
				value = null;
				return;
			}
			else if (len == 1)
			{
				value = string.Empty;
				return;
			}

			len -= 1;

			var encoding = new UTF8Encoding(false, true);

			var buf = new byte[len];

			int l = 0;

			while (l < len)
			{
				int r = stream.Read(buf, l, (int)len - l);
				if (r == 0)
					throw new EndOfStreamException();
				l += r;
			}

			value = encoding.GetString(buf);
		}
#else
        private sealed class StringHelper
        {
            public StringHelper()
            {
                Encoding = new UnicodeEncoding(false, true);
            }

            public const int BYTEBUFFERLEN = 256;
            public const int CHARBUFFERLEN = 128;

            private Encoder m_encoder;
            private Decoder m_decoder;

            private byte[] m_byteBuffer;
            private char[] m_charBuffer;

            private UnicodeEncoding Encoding { get; }
            public Encoder Encoder => m_encoder ?? (m_encoder = Encoding.GetEncoder());
            public Decoder Decoder => m_decoder ?? (m_decoder = Encoding.GetDecoder());

			public byte[] ReadBuffer { get { if (m_byteBuffer == null) m_byteBuffer = new byte[BYTEBUFFERLEN]; return m_byteBuffer; } }
			public byte[] WriteBuffer { get { if (m_byteBuffer == null) m_byteBuffer = new byte[BYTEBUFFERLEN]; return m_byteBuffer; } }
			public char[] CharBuffer => m_charBuffer ?? (m_charBuffer = new char[CHARBUFFERLEN]);
        }

        [ThreadStatic] private static StringHelper s_stringHelper;

        private static bool? _unsafeEncoderFailed;
		public static unsafe void WritePrimitive(Stream stream, string value)
		{
			if (value == null)
			{
				WritePrimitive(stream, (uint)0);
				return;
			}
			if (value.Length == 0)
			{
				WritePrimitive(stream, (uint)1);
				return;
			}

			var helper = s_stringHelper;
			if (helper == null)
				s_stringHelper = helper = new StringHelper();

			var encoder = helper.Encoder;
			var buf = helper.WriteBuffer;

			int totalChars = value.Length;
			int totalBytes;

			fixed (char* ptr = value)
				totalBytes = encoder.GetByteCount(ptr, totalChars, true);

			WritePrimitive(stream, (uint)totalBytes + 1);
			WritePrimitive(stream, (uint)totalChars);

			int p = 0;
			bool completed = false;

			while (completed == false)
			{
				int charsConverted;
				int bytesConverted;

				if (_unsafeEncoderFailed.HasValue && _unsafeEncoderFailed.Value)
				{
					encoder.Convert(value.ToCharArray(p, totalChars - p), 0, totalChars - p, buf, 0, buf.Length, true,
						out charsConverted, out bytesConverted, out completed);
				}
				else
				{
					fixed (char* src = value)
					fixed (byte* dst = buf)
					{
						encoder.Convert(src + p, totalChars - p, dst, buf.Length, true,
							out charsConverted, out bytesConverted, out completed);
					}
				}

				//The unsafe Encoder.Convert() void fails on some system (leaves the array empty)
				if (_unsafeEncoderFailed == null)
				{
					var alsoSomethingElseThanZero = false;
					for (int i = 0; i < bytesConverted; i++)
					{
						if (buf[i] != 0)
						{
							alsoSomethingElseThanZero = true;
							break;
						}
					}

					var onlyContainsUnicodeZeros = true;
					if (!alsoSomethingElseThanZero)
					{
						for (int i = 0; i < totalChars; i++)
						{
							if (value[i] != '\u0000')
							{
								onlyContainsUnicodeZeros = false;
								break;
							}
						}

						if (onlyContainsUnicodeZeros)
						{
							stream.Write(new byte[totalChars * 2], 0, totalChars * 2);
							return;
						}

						_unsafeEncoderFailed = true;
						completed = false;
						continue;
					}

					_unsafeEncoderFailed = false;
				}

				stream.Write(buf, 0, bytesConverted);
				p += charsConverted;
			}
		}

		public static void ReadPrimitive(Stream stream, out string value)
        {
            uint totalBytes;
            ReadPrimitive(stream, out totalBytes);

            if (totalBytes == 0)
            {
                value = null;
                return;
            }
            else if (totalBytes == 1)
            {
                value = string.Empty;
                return;
            }

            totalBytes -= 1;

            uint totalChars;
            ReadPrimitive(stream, out totalChars);

            var helper = s_stringHelper;
            if (helper == null)
                s_stringHelper = helper = new StringHelper();

            var decoder = helper.Decoder;
            var buf = helper.ReadBuffer;
            var chars = totalChars <= StringHelper.CHARBUFFERLEN ? helper.CharBuffer : new char[totalChars];

            int streamBytesLeft = (int) totalBytes;

            int cp = 0;

            while (streamBytesLeft > 0)
            {
                int bytesInBuffer = stream.Read(buf, 0, Math.Min(buf.Length, streamBytesLeft));
                if (bytesInBuffer == 0)
                    throw new EndOfStreamException();

                streamBytesLeft -= bytesInBuffer;
                bool flush = streamBytesLeft == 0;

                bool completed = false;

                int p = 0;

                while (completed == false)
                {
                    int charsConverted;
                    int bytesConverted;

                    decoder.Convert(buf, p, bytesInBuffer - p,
                        chars, cp, (int) totalChars - cp,
                        flush,
                        out bytesConverted, out charsConverted, out completed);

                    p += bytesConverted;
                    cp += charsConverted;
                }
            }

            value = new string(chars, 0, (int) totalChars);
        }
#endif

        public static void WritePrimitive(Stream stream, byte[] value)
        {
            if (value == null)
            {
                WritePrimitive(stream, (uint) 0);
                return;
            }

            WritePrimitive(stream, (uint) value.Length + 1);

            stream.Write(value, 0, value.Length);
        }

        private static readonly byte[] EmptyByteArray = new byte[0];

        public static void ReadPrimitive(Stream stream, out byte[] value)
        {
            uint len;
            ReadPrimitive(stream, out len);

            if (len == 0)
            {
                value = null;
                return;
            }
            else if (len == 1)
            {
                value = EmptyByteArray;
                return;
            }

            len -= 1;

            value = new byte[len];
            int l = 0;

            while (l < len)
            {
                int r = stream.Read(value, l, (int) len - l);
                if (r == 0)
                    throw new EndOfStreamException();
                l += r;
            }
        }
    }
}