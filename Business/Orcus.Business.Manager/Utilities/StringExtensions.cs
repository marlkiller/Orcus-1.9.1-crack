using System;
using System.Text;

namespace Orcus.Business.Manager.Utilities
{
    public static class StringExtensions
    {
        public static string Replace(this string source, string oldValue, string newValue,
            StringComparison comparisonType)
        {
            if (source.Length == 0 || oldValue.Length == 0)
                return source;

            var result = new StringBuilder();
            int startingPos = 0;
            int nextMatch;
            while ((nextMatch = source.IndexOf(oldValue, startingPos, comparisonType)) > -1)
            {
                result.Append(source, startingPos, nextMatch - startingPos);
                result.Append(newValue);
                startingPos = nextMatch + oldValue.Length;
            }
            result.Append(source, startingPos, source.Length - startingPos);

            return result.ToString();
        }
    }
}