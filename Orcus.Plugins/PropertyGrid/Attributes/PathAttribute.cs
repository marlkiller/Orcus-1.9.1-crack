using System;

namespace Orcus.Plugins.PropertyGrid.Attributes
{
    /// <summary>
    ///     The string represents a local path, the user can select it using a dialog
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class PathAttribute : Attribute
    {
        /// <summary>
        ///     Determines the path mode
        /// </summary>
        public PathMode PathMode { get; set; }

        /// <summary>
        ///     The filter for the file dialog
        /// </summary>
        public string Filter { get; set; }
    }

    /// <summary>
    ///     The path mode
    /// </summary>
    public enum PathMode
    {
        /// <summary>
        ///     The path targets a file
        /// </summary>
        File,

        /// <summary>
        ///     The path targets a directory
        /// </summary>
        Directory
    }
}