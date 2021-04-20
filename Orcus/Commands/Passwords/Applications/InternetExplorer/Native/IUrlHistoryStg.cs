using System;
using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming

namespace Orcus.Commands.Passwords.Applications.InternetExplorer.Native
{
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("3C374A41-BAE4-11CF-BF7D-00AA006946EE")]
    public interface IUrlHistoryStg
    {
        void AddUrl(string pocsUrl, string pocsTitle, ADDURL_FLAG dwFlags); //Adds a new history entry
        void DeleteUrl(string pocsUrl, int dwFlags); //Deletes an entry by its URL. does not work!

        void QueryUrl([MarshalAs(UnmanagedType.LPWStr)] string pocsUrl, STATURL_QUERYFLAGS dwFlags,
            ref STATURL lpSTATURL);

        //Returns a STATURL for a given URL
        void BindToObject([In] string pocsUrl, [In] UUID riid, IntPtr ppvOut); //Binds to an object. does not work!
        object EnumUrls { [return: MarshalAs(UnmanagedType.IUnknown)] get; }

        //Returns an enumerator for URLs
    }
}