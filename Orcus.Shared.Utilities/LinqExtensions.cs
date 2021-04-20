using System;
using System.Collections.Generic;
using System.Linq;

namespace Orcus.Shared.Utilities
{
    /// <summary>
    ///     Some useful extensions for linq
    /// </summary>
    public static class LinqExtensions
    {
        /// <summary>
        ///     Return a specific number of contiguous elements from the end of a sequence
        /// </summary>
        /// <param name="collection">The sequence to return elements from.</param>
        /// <param name="n">The number of elements to return</param>
        /// <returns>
        ///     An <see cref="IEnumerable{T}" /> that contains the specified number of elements from the start of the input
        ///     sequence.
        /// </returns>
        public static IEnumerable<T> TakeLast<T>(this IEnumerable<T> collection,
            int n)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));
            if (n < 0)
                throw new ArgumentOutOfRangeException(nameof(n), "n must be 0 or greater");

            LinkedList<T> temp = new LinkedList<T>();
            foreach (var value in collection)
            {
                temp.AddLast(value);
                if (temp.Count > n)
                    temp.RemoveFirst();
            }

            return temp;
        }

        /// <summary>
        ///     Compare two lists ignoring the order of the items
        /// </summary>
        /// <typeparam name="T">The type of the items</typeparam>
        /// <param name="list1">The first list to compare</param>
        /// <param name="list2">The second list to compare</param>
        /// <returns>Return true if both lists contain the same items</returns>
        public static bool ScrambledEquals<T>(this IEnumerable<T> list1, IEnumerable<T> list2)
        {
            var cnt = new Dictionary<T, int>();
            foreach (T s in list1)
            {
                if (cnt.ContainsKey(s))
                {
                    cnt[s]++;
                }
                else
                {
                    cnt.Add(s, 1);
                }
            }
            foreach (T s in list2)
            {
                if (cnt.ContainsKey(s))
                {
                    cnt[s]--;
                }
                else
                {
                    return false;
                }
            }
            return cnt.Values.All(c => c == 0);
        }
    }
}