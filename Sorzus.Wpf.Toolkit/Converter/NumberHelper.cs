using System;

namespace Sorzus.Wpf.Toolkit.Converter
{
    /// <summary>
    ///     Helper for parsing an <see cref="object" /> to a number
    /// </summary>
    public static class NumberHelper
    {
        /// <summary>
        ///     Try to cast the object to any integer value type
        /// </summary>
        /// <param name="value">The value to cast</param>
        /// <param name="tryParse">True if parsing should be tried</param>
        /// <returns>Return null if it could not be casted to any integer value type</returns>
        public static long? ToLong(this object value, bool tryParse)
        {
            if (value is byte)
                return (byte) value;
            if (value is sbyte)
                return (sbyte) value;
            if (value is short)
                return (short) value;
            if (value is ushort)
                return (ushort) value;
            if (value is int)
                return (int) value;
            if (value is uint)
                return (uint) value;
            if (value is long)
                return (long) value;

            long longValue;
            if (tryParse && long.TryParse(value.ToString(), out longValue))
                return longValue;

            return null;
        }

        /// <summary>
        ///     Try to cast the object to any integer value type
        /// </summary>
        /// <param name="value">The value to cast</param>
        /// <param name="tryParse">True if parsing should be tried</param>
        /// <returns>Return null if it could not be casted to any integer value type</returns>
        public static int? ToInteger(this object value, bool tryParse)
        {
            if (value is byte)
                return (byte) value;
            if (value is sbyte)
                return (sbyte) value;
            if (value is short)
                return (short) value;
            if (value is ushort)
                return (ushort) value;
            if (value is int)
                return (int) value;

            int integerValue;
            if (tryParse && int.TryParse(value.ToString(), out integerValue))
                return integerValue;

            return null;
        }

        /// <summary>
        ///     Try to cast the object to any floating value type
        /// </summary>
        /// <param name="value">The value to cast</param>
        /// <param name="tryParse">True if parsing should be tried</param>
        /// <returns>Return null if it could not be casted to any integer value type</returns>
        public static double? ToDouble(this object value, bool tryParse)
        {
            var integerValue = ToLong(value, false);
            if (integerValue != null)
                return integerValue;

            if (value is double)
                return (double)value;
            if (value is float)
                return (float)value;
            if (value is decimal)
                return (double) (decimal) value;

            double doubleValue;
            if (tryParse && double.TryParse(value.ToString(), out doubleValue))
                return doubleValue;

            return null;
        }

        /// <summary>
        ///     Try to cast the object to any integer value type. Throw exception if that was not possible
        /// </summary>
        /// <param name="value">The value to cast</param>
        /// <returns>Return the casted value</returns>
        /// <example cref="ArgumentException">Thrown when the <see cref="value" /> could not be converted</example>
        public static int ToInteger(this object value)
        {
            var integerValue = value.ToInteger(true);
            if (integerValue == null)
                throw new ArgumentException(nameof(value));
            return integerValue.Value;
        }

        /// <summary>
        ///     Try to cast the object to any integer value type. Throw exception if that was not possible
        /// </summary>
        /// <param name="value">The value to cast</param>
        /// <returns>Return the casted value</returns>
        /// <example cref="ArgumentException">Thrown when the <see cref="value" /> could not be converted</example>
        public static long ToLong(this object value)
        {
            var longValue = value.ToLong(true);
            if (longValue == null)
                throw new ArgumentException(nameof(value));

            return longValue.Value;
        }

        /// <summary>
        ///     Try to cast the object to any floating value type. Throw exception if that was not possible
        /// </summary>
        /// <param name="value">The value to cast</param>
        /// <returns>Return the casted value</returns>
        /// <example cref="ArgumentException">Thrown when the <see cref="value" /> could not be converted</example>
        public static double ToDouble(this object value)
        {
            var doubleValue = value.ToDouble(true);
            if (doubleValue == null)
                throw new ArgumentException(nameof(value));
            return doubleValue.Value;
        }
    }
}