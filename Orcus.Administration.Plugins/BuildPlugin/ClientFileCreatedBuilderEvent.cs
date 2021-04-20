namespace Orcus.Administration.Plugins.BuildPlugin
{
    /// <summary>
    ///     Executed directly after the client was written to a file. Make changes to the file like renaming, change
    ///     properties, etc.
    /// </summary>
    public class ClientFileCreatedBuilderEvent : BuilderEvent
    {
        /// <summary>
        ///     Executed directly after the client was written to a file. Make changes to the file like renaming, change
        ///     properties, etc.
        /// </summary>
        /// <param name="builderInformation">The builder settings provide the current properties like file names</param>
        public delegate void ClientFileCreatedDelegate(IBuilderInformation builderInformation);

        /// <summary>
        ///     Initialize a new instance of <see cref="ClientFileCreatedDelegate" />
        /// </summary>
        /// <param name="clientFileCreatedDelegate">The delegate to execute</param>
        public ClientFileCreatedBuilderEvent(ClientFileCreatedDelegate clientFileCreatedDelegate)
        {
            ClientFileCreated = clientFileCreatedDelegate;
        }

        public ClientFileCreatedDelegate ClientFileCreated { get; }
    }
}