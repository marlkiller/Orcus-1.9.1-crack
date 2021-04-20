using System;
using System.IO;
using Orcus.Shared.Communication;
using Orcus.Shared.Data;

namespace Orcus.Plugins
{
    /// <summary>
    ///     Provides information about the connection
    /// </summary>
    public interface IConnectionInfo
    {
        /// <summary>
        ///     Actions and information of the client
        /// </summary>
        IClientInfo ClientInfo { get; }

        /// <summary>
        ///     Provides initialized connections
        /// </summary>
        IConnectionInitializer ConnectionInitializer { get; }

        /// <summary>
        ///     The id of the administration
        /// </summary>
        ushort AdministrationId { get; }

        /// <summary>
        ///     Fast response. The command failed
        /// </summary>
        void CommandFailed(Command command, byte[] data);

        /// <summary>
        ///     Fast response. The command succeed
        /// </summary>
        void CommandSucceed(Command command, byte[] data);

        /// <summary>
        ///     Fast response. The command succeed, but something went wrong
        /// </summary>
        void CommandWarning(Command command, byte[] data);

        /// <summary>
        ///     Response to the administration the <see cref="data" />
        /// </summary>
        void CommandResponse(Command command, byte[] data,
            PackageCompression packageCompression = PackageCompression.Auto);

        /// <summary>
        ///     Response data and define the <see cref="ResponseType" />
        /// </summary>
        void Response(byte[] package, ResponseType responseType,
            PackageCompression packageCompression = PackageCompression.Auto);

        /// <summary>
        ///     Send data using a BinaryWriter. WARNING: This is extremely dangerous if not the same length gets written into the
        ///     stream like set by parameter. Please use the managed way if you are not sure what you are doing
        /// </summary>
        /// <param name="command">The command you want to send</param>
        /// <param name="length">The size of bytes you want to write in the stream</param>
        /// <param name="writerCall">The delegate you should use to write the bytes</param>
        void UnsafeResponse(Command command, int length, Action<BinaryWriter> writerCall);

        /// <summary>
        ///     Send data using a BinaryWriter. WARNING: This is extremely dangerous if not the same length gets written into the
        ///     stream like set by parameter. Please use the managed way if you are not sure what you are doing
        /// </summary>
        /// <param name="command">The command you want to send</param>
        /// <param name="writerCall">The delegate you should use to write the bytes</param>
        void UnsafeResponse(Command command, WriterCall writerCall);
    }
}