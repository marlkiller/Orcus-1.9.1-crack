using System;
using Orcus.Plugins;
using Orcus.Shared.Connection;

namespace Orcus.Administration.Plugins.CommandViewPlugin
{
    /// <summary>
    ///     Can be set on a <see cref="Command" /> to transmit and load the assembly before executing the command
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ProvideLibraryAttribute : Attribute
    {
        /// <summary>
        ///     Initialize a new instance of <see cref="ProvideLibraryAttribute" />
        /// </summary>
        /// <param name="library">The library to transmit</param>
        public ProvideLibraryAttribute(PortableLibrary library)
        {
            Library = library;
        }

        /// <summary>
        ///     The library to transmit
        /// </summary>
        public PortableLibrary Library { get; }
    }
}