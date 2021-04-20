using System;
using System.Collections.Generic;
using System.Linq;

namespace Orcus.Shared.Utilities
{
    /// <summary>
    ///     Some useful functions to handle enums
    /// </summary>
    //https://stackoverflow.com/questions/1799370/getting-attributes-of-enums-value
    public static class EnumHelper
    {
        /// <summary>
        ///     Gets an attribute on an enum field value
        /// </summary>
        /// <typeparam name="T">The type of the attribute you want to retrieve</typeparam>
        /// <param name="enumVal">The enum value</param>
        /// <returns>The attribute of type T that exists on the enum value</returns>
        /// <example><code>string desc = myEnumVariable.GetAttributeOfType&gt;DescriptionAttribute>().Description;</code></example>
        public static T GetAttributeOfType<T>(this Enum enumVal) where T : Attribute
        {
            var type = enumVal.GetType();
            var memInfo = type.GetMember(enumVal.ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof (T), false);
            return attributes.Length > 0 ? (T) attributes[0] : null;
        }

        public static IEnumerable<T> GetUniqueFlags<T>(this Enum flags) where T : struct, IConvertible
        {
            ulong flag = 1;
            var flagsInt = Convert.ToInt32(flags);
            foreach (var value in Enum.GetValues(typeof(T)).Cast<Enum>())
            {
                ulong bits = Convert.ToUInt64(value);
                while (flag < bits)
                {
                    flag <<= 1;
                }

                var valueInt = Convert.ToInt32(value);
                if (flag == bits && (flagsInt & valueInt) == valueInt)
                {
                    yield return (T) (object) value;
                }
            }
        }
    }
}