namespace Orcus.Administration.Plugins.CommandViewPlugin
{
    /// <summary>
    ///     Define the compression of the package
    /// </summary>
    public enum PackageCompression
    {
        /// <summary>
        ///     Automatically decide if the package should be compressed.
        /// </summary>
        Auto,

        /// <summary>
        ///     Force compression
        /// </summary>
        Compress,

        /// <summary>
        ///     Don't compress the package
        /// </summary>
        DoNotCompress
    }
}