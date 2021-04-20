using System.Collections.Generic;

namespace Orcus.Administration.Core.Utilities
{
    public static class ListExtensions
    {
        public static void Swap<T>(this IList<T> list, T item1, T item2)
        {
            var indexItem1 = list.IndexOf(item1);
            var indexItem2 = list.IndexOf(item2);
            list[indexItem1] = item2;
            list[indexItem2] = item1;
        }
    }
}