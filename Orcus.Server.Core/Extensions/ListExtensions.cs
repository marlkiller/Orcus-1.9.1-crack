using System.Collections.Generic;

namespace Orcus.Server.Core.Extensions
{
    public static class ListExtensions
    {
        public static int AddEx<T>(this List<T> list, T item)
        {
            list.Add(item);
            return list.IndexOf(item);
        }
    }
}