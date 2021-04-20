using System;
using System.Runtime.InteropServices;

namespace AudioPackBuilder.Native
{
    internal class NativeMethods
    {
        /// <summary>
        ///     Delete a GDI object
        /// </summary>
        /// <param name="o">The poniter to the GDI object to be deleted</param>
        /// <returns></returns>
        [DllImport("gdi32")]
        internal static extern int DeleteObject(IntPtr o);
    }
}