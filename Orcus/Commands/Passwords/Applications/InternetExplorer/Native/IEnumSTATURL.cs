using System.Runtime.InteropServices;

namespace Orcus.Commands.Passwords.Applications.InternetExplorer.Native
{
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("3C374A42-BAE4-11CF-BF7D-00AA006946EE")]
    public interface IEnumSTATURL
    {
        void Next(int celt, ref STATURL rgelt, out int pceltFetched); //Returns the next \"celt\" URLS from the cache
        void Skip(int celt); //Skips the next \"celt\" URLS from the cache. does not work.
        void Reset(); //Resets the enumeration
        void Clone(out IEnumSTATURL ppenum); //Clones this object
        void SetFilter([MarshalAs(UnmanagedType.LPWStr)] string poszFilter, STATURLFLAGS dwFlags);
        //Sets the enumeration filter
    }
}