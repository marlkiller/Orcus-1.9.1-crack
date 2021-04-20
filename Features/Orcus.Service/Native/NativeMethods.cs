using System.Runtime.InteropServices;

namespace Orcus.Service.Native
{
    internal class NativeMethods
    {
        [DllImport("user32.dll", EntryPoint = "BlockInput")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool BlockInput([MarshalAs(UnmanagedType.Bool)] bool fBlockIt);
    }
}