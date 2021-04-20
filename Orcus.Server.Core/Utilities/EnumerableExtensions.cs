using System.Collections.Generic;

namespace Orcus.Server.Core.Utilities
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> AddItem<T>(this IEnumerable<T> enumerable, T item)
        {
            return new List<T>(enumerable) {item};
        }

        public static IEnumerable<T> AddItem<T>(this IEnumerable<T> enumerable, params T[] items)
        {
            var list = new List<T>(enumerable);
            list.AddRange(items);
            return list;
        }
    }
}