using System;
using System.Collections.Generic;

namespace Orcus.Server.Core.Extensions
{
    internal static class SortedListExtensions
    {
        public static ushort GetFirstUnusedKey<TValue>(this SortedDictionary<ushort, TValue> dict)
        {
            if (!Equals(dict.Comparer, Comparer<ushort>.Default))
                throw new NotSupportedException("Unsupported comparer");

            using (var enumerator = dict.GetEnumerator())
            {
                if (!enumerator.MoveNext())
                    return 0;

                ushort nextKeyInSequence = (ushort) (enumerator.Current.Key + 1);

                if (nextKeyInSequence < 1)
                    throw new InvalidOperationException("The dictionary contains keys less than 0");

                if (nextKeyInSequence != 1)
                    return 0;

                while (enumerator.MoveNext())
                {
                    var key = enumerator.Current.Key;
                    if (key > nextKeyInSequence)
                        return nextKeyInSequence;

                    ++nextKeyInSequence;
                }

                return nextKeyInSequence;
            }
        }
    }
}