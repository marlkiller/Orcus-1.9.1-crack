using System;

namespace Orcus.Plugins.PropertyGrid.Attributes
{
    /// <summary>
    ///     Some options for numeric values like <see cref="byte" />, <see cref="short" />, <see cref="int" />,
    ///     <see cref="long" />, <see cref="double" />, etc.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class NumericValueAttribute : Attribute
    {
        /// <summary>
        ///     The maximum value for the property
        /// </summary>
        public double Maximum { get; set; } = double.MaxValue;

        /// <summary>
        ///     The minimum value for the property
        /// </summary>
        public double Minimum { get; set; } = double.MinValue;

        /// <summary>
        ///     The dispalyed string format
        /// </summary>
        public string StringFormat { get; set; }
    }
}