using System.Collections;
using System.Runtime.InteropServices;

namespace Orcus.Commands.Passwords.Applications.InternetExplorer.Native
{
    /// <summary>
    ///     The helper class to sort in ascending order by FileTime(LastVisited).
    /// </summary>
    public class SortFileTimeAscendingHelper : IComparer
    {
        int IComparer.Compare(object a, object b)
        {
            var c1 = (STATURL) a;
            var c2 = (STATURL) b;

            return CompareFileTime(ref c1.ftLastVisited, ref c2.ftLastVisited);
        }

        [DllImport("Kernel32.dll")]
        private static extern int CompareFileTime([In] ref System.Runtime.InteropServices.ComTypes.FILETIME lpFileTime1,
            [In] ref System.Runtime.InteropServices.ComTypes.FILETIME lpFileTime2);

        public static IComparer SortFileTimeAscending()
        {
            return new SortFileTimeAscendingHelper();
        }
    }
}