namespace Orcus.Administration.Plugins.BuildPlugin
{
    /// <summary>
    ///     The type of an <see cref="OutputFile" />
    /// </summary>
    public enum OutputFileType
    {
        /// <summary>
        ///     It's just a file which is not really needed for the application to work
        /// </summary>
        None,

        /// <summary>
        ///     The file is required for the assembly
        /// </summary>
        RequiredFile,

        /// <summary>
        ///     The file is the assembly. Only one <see cref="OutputFile" /> with this property can exist
        /// </summary>
        MainAssembly
    }
}