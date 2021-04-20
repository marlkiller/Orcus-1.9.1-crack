namespace Orcus.Administration.Plugins.BuildPlugin
{
    /// <summary>
    ///     Executed when the client file is completely finished. No changes should be made anymore at this point. You can now
    ///     upload it somwewhere or copy it somewhere else
    /// </summary>
    public class ClientBuildCompletedBuilderEvent : BuilderEvent
    {
        /// <summary>
        ///     Executed when the client file is completely finished. No changes should be made anymore at this point. You can now
        ///     upload it somwewhere or copy it somewhere else
        /// </summary>
        /// <param name="builderInformation">The builder settings provide the current properties like file names</param>
        public delegate void ClientBuildCompletedDelegate(IBuilderInformation builderInformation);

        /// <summary>
        ///     Initialize a new instance of <see cref="ClientBuildCompletedBuilderEvent" />
        /// </summary>
        /// <param name="clientBuildCompletedDelegate">The delegate to execute</param>
        public ClientBuildCompletedBuilderEvent(ClientBuildCompletedDelegate clientBuildCompletedDelegate)
        {
            ClientBuildCompleted = clientBuildCompletedDelegate;
        }

        public ClientBuildCompletedDelegate ClientBuildCompleted { get; }
    }
}