using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using Orcus.Plugins.PropertyGrid;

namespace Orcus.Administration.Controls.PropertyGrid.Editors
{
    public class ConvertingTextBoxEditor : TextBoxEditor
    {
        private readonly TypeConverter _typeConverter;

        public ConvertingTextBoxEditor(TypeConverter typeConverter)
        {
            _typeConverter = typeConverter;
        }

        protected override IValueConverter CreateValueConverter()
        {
            return new StringToTypeConverter(_typeConverter, PropertyItem.Property);
        }

        private class StringToTypeConverter : IValueConverter
        {
            private readonly TypeConverter _typeConverter;
            private readonly IProperty _property;

            public StringToTypeConverter(TypeConverter typeConverter, IProperty property)
            {
                _typeConverter = typeConverter;
                _property = property;
            }

            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return _typeConverter.ConvertToString(value);
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                try
                {
                    return _typeConverter.ConvertFromString((string) value);
                }
                catch (Exception)
                {
                    return _property.Value;
                }
            }
        }
    }
}