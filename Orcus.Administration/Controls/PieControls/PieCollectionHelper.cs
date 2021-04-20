using System.Collections.ObjectModel;
using System.Linq;
using Orcus.Administration.ViewModels.Statistics;

namespace Orcus.Administration.Controls.PieControls
{
    /// <summary>
    ///     Taken from http://www.codeproject.com/Articles/442506/Simple-and-Easy-to-Use-Pie-Chart-Controls-in-WPF
    /// </summary>
    public static class PieCollectionHelper
    {
        public static double GetTotal(this ObservableCollection<PieSegment> collection)
        {
            return collection.Sum(a => a.Value);
        }
    }
}