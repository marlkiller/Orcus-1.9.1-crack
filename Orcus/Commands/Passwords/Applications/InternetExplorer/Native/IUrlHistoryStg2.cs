using System;
using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming

namespace Orcus.Commands.Passwords.Applications.InternetExplorer.Native
{
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("AFA0DC11-C313-11D0-831A-00C04FD5AE38")]
    public interface IUrlHistoryStg2 : IUrlHistoryStg
    {
        new void AddUrl(string pocsUrl, string pocsTitle, ADDURL_FLAG dwFlags); //Adds a new history entry
        new void DeleteUrl(string pocsUrl, int dwFlags); //Deletes an entry by its URL. does not work!

        new void QueryUrl([MarshalAs(UnmanagedType.LPWStr)] string pocsUrl, STATURL_QUERYFLAGS dwFlags,
            ref STATURL lpSTATURL);

        //Returns a STATURL for a given URL
        new void BindToObject([In] string pocsUrl, [In] UUID riid, IntPtr ppvOut); //Binds to an object. does not work!
        new object EnumUrls { [return: MarshalAs(UnmanagedType.IUnknown)] get; }

        //Returns an enumerator for URLs
        void AddUrlAndNotify(string pocsUrl, string pocsTitle, int dwFlags, int fWriteHistory, object poctNotify,
            object punkISFolder);

        //does not work!
        void ClearHistory(); //Removes all history items
    }
}