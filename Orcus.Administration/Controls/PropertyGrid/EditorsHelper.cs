using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Orcus.Administration.Controls.PropertyGrid.Editors;
using Orcus.Plugins.PropertyGrid.Attributes;

namespace Orcus.Administration.Controls.PropertyGrid
{
    public static class EditorsHelper
    {
        public static IPropertyEditor CreateDefaultEditor(Type propertyType, List<Attribute> customAttributes)
        {
            if (propertyType == typeof (string))
            {
                var pathAttribute = customAttributes.OfType<PathAttribute>().FirstOrDefault();
                if (pathAttribute != null)
                    return new PathEditor(pathAttribute.PathMode, pathAttribute.Filter);

                var multilineAttribute = customAttributes.OfType<MultilineStringAttribute>().FirstOrDefault();
                if (multilineAttribute != null)
                    return new MultilineStringEditor();

                return new TextBoxEditor();
            }
            if (propertyType == typeof (bool) || propertyType == typeof (bool?))
                return new BooleanComboBoxEditor();
            if (propertyType == typeof (DateTime) || propertyType == typeof (DateTime?))
                return new DateTimeEditor();
            if (propertyType == typeof (TimeSpan) || propertyType == typeof (TimeSpan?))
                return new TimeSpanEditor();
            if (propertyType == typeof (System.Drawing.Color) || propertyType == typeof (System.Drawing.Color?))
                return new ColorEditor();

            if (propertyType.IsEnum)
                return new EnumComboBoxEditor();

            var numericAttribute = customAttributes.OfType<NumericValueAttribute>().FirstOrDefault();
            if (propertyType == typeof (byte) || propertyType == typeof (byte?))
                return new UpDownEditor<byte>(numericAttribute?.Minimum ?? byte.MinValue,
                    numericAttribute?.Maximum ?? byte.MaxValue, numericAttribute?.StringFormat);
            if (propertyType == typeof (sbyte) || propertyType == typeof (sbyte?))
                return new UpDownEditor<sbyte>(numericAttribute?.Minimum ?? sbyte.MinValue,
                    numericAttribute?.Maximum ?? sbyte.MaxValue, numericAttribute?.StringFormat);
            if (propertyType == typeof (short) || propertyType == typeof (short?))
                return new UpDownEditor<short>(numericAttribute?.Minimum ?? short.MinValue,
                    numericAttribute?.Maximum ?? short.MaxValue, numericAttribute?.StringFormat);
            if (propertyType == typeof (ushort) || propertyType == typeof (ushort?))
                return new UpDownEditor<ushort>(numericAttribute?.Minimum ?? ushort.MinValue,
                    numericAttribute?.Maximum ?? ushort.MaxValue, numericAttribute?.StringFormat);
            if (propertyType == typeof (int) || propertyType == typeof (int?))
                return new UpDownEditor<int>(numericAttribute?.Minimum ?? int.MinValue,
                    numericAttribute?.Maximum ?? int.MaxValue, numericAttribute?.StringFormat);
            if (propertyType == typeof (uint) || propertyType == typeof (uint?))
                return new UpDownEditor<uint>(numericAttribute?.Minimum ?? uint.MinValue,
                    numericAttribute?.Maximum ?? uint.MaxValue, numericAttribute?.StringFormat);
            if (propertyType == typeof (long) || propertyType == typeof (long?))
                return new UpDownEditor<long>(numericAttribute?.Minimum ?? long.MinValue,
                    numericAttribute?.Maximum ?? long.MaxValue, numericAttribute?.StringFormat);
            if (propertyType == typeof (ulong) || propertyType == typeof (ulong?))
                return new UpDownEditor<ulong>(numericAttribute?.Minimum ?? ulong.MinValue,
                    numericAttribute?.Maximum ?? ulong.MaxValue, numericAttribute?.StringFormat);

            if (propertyType == typeof (float) || propertyType == typeof (float?))
                return new UpDownEditor<float>(numericAttribute?.Minimum ?? float.MinValue,
                    numericAttribute?.Maximum ?? float.MaxValue, numericAttribute?.StringFormat, true);
            if (propertyType == typeof (double) || propertyType == typeof (double?))
                return new UpDownEditor<double>(numericAttribute?.Minimum ?? double.MinValue,
                    numericAttribute?.Maximum ?? double.MaxValue, numericAttribute?.StringFormat, true);
            if (propertyType == typeof (decimal) || propertyType == typeof (decimal?))
                return new UpDownEditor<decimal>(numericAttribute?.Minimum ?? decimal.ToDouble(decimal.MaxValue),
                    numericAttribute?.Maximum ?? decimal.ToDouble(decimal.MinValue), numericAttribute?.StringFormat,
                    true);

            var typeConverter = TypeDescriptor.GetConverter(propertyType);
            if (typeConverter.CanConvertFrom(typeof (string)))
                return new ConvertingTextBoxEditor(typeConverter);

            return new TextBlockEditor();
        }
    }
}