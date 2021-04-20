using System;

namespace Orcus.Utilities.WindowsDesktop
{
    public struct Window
    {
        /// <summary>
        ///     Creates a new window object.
        /// </summary>
        /// <param name="handle">Window handle.</param>
        public Window(IntPtr handle)
        {
            Handle = handle;
        }

        /// <summary>
        ///     Gets the window handle.
        /// </summary>
        public IntPtr Handle { get; }
    }
}