using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace Orcus.Administration.Core.Utilities
{
    public static class StringExtensions
    {
        public static string RemoveSpecialCharacters(this string str)
        {
            var sb = new StringBuilder(str.Length);
            foreach (char c in str)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '.' || c == '_')
                    sb.Append(c);
            }
            return sb.ToString();
        }

        public static byte[] HexToBytes(this string hex)
        {
            return Enumerable.Range(0, hex.Length)
                .Where(x => x%2 == 0)
                .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                .ToArray();
        }

        public static string StripWhiteSpace(this string value)
        {
            if (value == null)
                return null;
            if (value.Length == 0 || value.Trim().Length == 0)
                return String.Empty;
            var sb = new StringBuilder(value.Length);
            foreach (var t in value)
                if (!Char.IsWhiteSpace(t))
                    sb.Append(t);
            return sb.ToString();
        }

        public static string SecureStringToString(SecureString value)
        {
            IntPtr valuePtr = IntPtr.Zero;
            try
            {
                valuePtr = Marshal.SecureStringToGlobalAllocUnicode(value);
                return Marshal.PtrToStringUni(valuePtr);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(valuePtr);
            }
        }

        //Source: https://stackoverflow.com/questions/244531/is-there-an-alternative-to-string-replace-that-is-case-insensitive

        public static string Replace(this string str, string oldValue, string newValue, StringComparison comparison)
        {
            if (String.IsNullOrWhiteSpace(str))
                return str;

            // skip the loop entirely if oldValue and newValue are the same
            if (String.Compare(oldValue, newValue, comparison) == 0) return str;

            if (oldValue.Length > str.Length)
                return str;

            var sb = new StringBuilder();

            int previousIndex = 0;
            int index = str.IndexOf(oldValue, comparison);

            while (index != -1)
            {
                sb.Append(str.Substring(previousIndex, index - previousIndex));
                sb.Append(newValue);
                index += oldValue.Length;

                previousIndex = index;
                index = str.IndexOf(oldValue, index, comparison);
            }
            sb.Append(str.Substring(previousIndex));

            return sb.ToString();
        }
    }
}