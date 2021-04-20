namespace Orcus.Administration.Plugins.BuildPlugin
{
    /// <summary>
    ///     Represents an output file of the builder process
    /// </summary>
    public class OutputFile
    {
        /// <summary>
        ///     Initialize a new instance of <see cref="OutputFile" />
        /// </summary>
        /// <param name="path">The path of the file</param>
        /// <param name="outputFileType">The type of the file</param>
        public OutputFile(string path, OutputFileType outputFileType)
        {
            Path = path;
            OutputFileType = outputFileType;
        }

        /// <summary>
        ///     The path of the file
        /// </summary>
        public string Path { get; }

        /// <summary>
        ///     The type of the file
        /// </summary>
        public OutputFileType OutputFileType { get; }
    }
}