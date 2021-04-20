namespace Orcus.Extensions
{
    internal static class StringExtensions
    {
        public static bool IsNullOrWhiteSpace(this string value)
        {
            return string.IsNullOrEmpty(value?.Trim());
        }
    }
}