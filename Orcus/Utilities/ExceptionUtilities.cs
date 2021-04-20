using System;

namespace Orcus.Utilities
{
    public static class ExceptionUtilities
    {
        public static T EatExceptions<T>(Func<T> func)
        {
            try
            {
                return func();
            }
            catch
            {
                return default(T);
            }
        }

        public static T? EatExceptionsNull<T>(Func<T> func) where T : struct
        {
            try
            {
                return func();
            }
            catch
            {
                return null;
            }
        }

        public static void EatExceptions(Action action)
        {
            try
            {
                action();
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}