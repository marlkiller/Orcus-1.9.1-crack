namespace Orcus.Administration.Plugins.BuildPlugin
{
    /// <summary>
    ///     Executed when the client file is completely modified. You can now wrap the file, pack it, etc.
    /// </summary>
    public class ClientFileModifiedBuilderEvent : BuilderEvent
    {
        /// <summary>
        ///     Executed when the client file is completely modified. You can now wrap the file, pack it, etc.
        /// </summary>
        /// <param name="builderInformation">The builder settings provide the current properties like file names</param>
        public delegate void ClientFileModifiedDelegate(IBuilderInformation builderInformation);

        /// <summary>
        ///     Initialize a new instance of <see cref="ClientFileModified" />
        /// </summary>
        /// <param name="clientFileModifiedDelegate">The delegate to execute</param>
        public ClientFileModifiedBuilderEvent(ClientFileModifiedDelegate clientFileModifiedDelegate)
        {
            ClientFileModified = clientFileModifiedDelegate;
        }

        public ClientFileModifiedDelegate ClientFileModified { get; }
    }
}