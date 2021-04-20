using System.Collections.Generic;

namespace Orcus.Administration.Plugins.BuildPlugin
{
    /// <summary>
    ///     The current settings of the builder
    /// </summary>
    public interface IBuilderInformation
    {
        /// <summary>
        ///     The path of the assembly from the <see cref="OutputFiles" />
        /// </summary>
        string AssemblyPath { get; }

        /// <summary>
        ///     The files which were created in this builder process
        /// </summary>
        List<OutputFile> OutputFiles { get; }

        /// <summary>
        ///     The builder logger to write messages to the log window
        /// </summary>
        IBuildLogger BuildLogger { get; }
    }
}