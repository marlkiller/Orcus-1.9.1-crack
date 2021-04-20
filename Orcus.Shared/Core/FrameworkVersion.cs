using System.ComponentModel;

namespace Orcus.Shared.Core
{
    /// <summary>
    ///     The framework version of the client
    /// </summary>
    public enum FrameworkVersion
    {
        /// <summary>
        ///     .Net Framework 3.5
        /// </summary>
        [Description(".Net Framework 3.5")] NET35,

        /// <summary>
        ///     .Net Framework 4.0
        /// </summary>
        [Description(".Net Framework 4.0")] NET40,

        /// <summary>
        ///     .Net Framework 4.5
        /// </summary>
        [Description(".Net Framework 4.5")] NET45
    }
}