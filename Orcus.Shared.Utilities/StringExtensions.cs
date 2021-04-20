using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Orcus.Shared.Utilities
{
    /// <summary>
    ///     Extensions for <see cref="string" />s
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        ///     Check if the provided string is null, empty or only consits of white spaces
        /// </summary>
        /// <param name="value">The string to test</param>
        /// <returns>Return true if the string doesn't contain any visible chars</returns>
        public static bool IsNullOrWhiteSpace(this string value)
        {
            if (value == null) return true;

            for (int i = 0; i < value.Length; i++)
            {
                if (!char.IsWhiteSpace(value[i])) return false;
            }

            return true;
        }

        /// <summary>
        ///     Convert the given hex string to a byte array
        /// </summary>
        /// <param name="hex">The string in hex format</param>
        /// <returns>Return the represented byte array of the given string</returns>
        public static byte[] HexToBytes(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                .ToArray();
        }

        /// <summary>
        ///     Check if the given string is a hex string (only contains chars 0-9a-f)
        /// </summary>
        /// <param name="chars">The string to test</param>
        /// <returns>Return true if the string is a valid hex string</returns>
        //https://stackoverflow.com/questions/223832/check-a-string-to-see-if-all-characters-are-hexadecimal-values
        public static bool IsHex(this IEnumerable<char> chars)
        {
            foreach (var c in chars)
            {
                var isHex = ((c >= '0' && c <= '9') ||
                             (c >= 'a' && c <= 'f') ||
                             (c >= 'A' && c <= 'F'));

                if (!isHex)
                    return false;
            }
            return true;
        }
    }
}