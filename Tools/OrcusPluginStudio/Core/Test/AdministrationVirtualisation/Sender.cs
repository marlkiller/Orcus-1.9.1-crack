using System;
using System.IO;
using System.Linq;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Shared.Data;

namespace OrcusPluginStudio.Core.Test.AdministrationVirtualisation
{
    public class Sender : ISender
    {
        public void SendCommand(int id, byte[] bytes, PackageCompression packageCompression)
        {
            SendCommandEvent?.Invoke(this, Tuple.Create(BitConverter.ToUInt32(bytes, 0), bytes.Skip(4).ToArray()));
        }

        public void UnsafeSendCommand(int clientId, uint commandId, WriterCall writerCall)
        {
        }

        public void Dispose()
        {
        }

        public void UnsafeSendCommand(int clientId, uint commandId, int length, Action<BinaryWriter> writerCall)
        {
        }

        public event EventHandler<Tuple<uint, byte[]>> SendCommandEvent;
    }
}