using System;
using System.Collections.Generic;
using System.Linq;

namespace Orcus.Administration.Core.Utilities
{
    public static class EnumUtilities
    {
        public static IEnumerable<T> GetUniqueFlags<T>(this Enum flags) where T : struct, IConvertible
        {
            ulong flag = 1;
            foreach (var value in Enum.GetValues(flags.GetType()).Cast<Enum>())
            {
                ulong bits = Convert.ToUInt64(value);
                while (flag < bits)
                {
                    flag <<= 1;
                }

                if (flag == bits && flags.HasFlag(value))
                {
                    yield return (T) (object) value;
                }
            }
        }
    }
}