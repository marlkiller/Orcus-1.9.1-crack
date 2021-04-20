namespace Orcus.Plugins.Builder
{
    /// <summary>
    ///     The categories of the builder
    /// </summary>
    public enum BuilderCategory
    {
        /// <summary>
        ///     General settings for the client
        /// </summary>
        GeneralSettings,

        /// <summary>
        ///     Settings which influence the connection of the client
        /// </summary>
        Connection,

        /// <summary>
        ///     Settings about features which protect the client
        /// </summary>
        Protection,

        /// <summary>
        ///     Settings about features which influence the installation of the client
        /// </summary>
        Installation,

        /// <summary>
        ///     Settings about features which influence the output file
        /// </summary>
        Assembly
    }
}