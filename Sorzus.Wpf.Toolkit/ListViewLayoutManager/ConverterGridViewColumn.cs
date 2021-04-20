// -- FILE ------------------------------------------------------------------
// name       : ConverterGridViewColumn.cs
// created    : Jani Giannoudis - 2008.03.27
// language   : c#
// environment: .NET 3.0
// copyright  : (c) 2008-2012 by Itenso GmbH, Switzerland
// --------------------------------------------------------------------------

using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace Sorzus.Wpf.Toolkit.ListViewLayoutManager
{
    // ------------------------------------------------------------------------
    public abstract class ConverterGridViewColumn : GridViewColumn, IValueConverter
    {
        // ----------------------------------------------------------------------
        protected ConverterGridViewColumn(Type bindingType)
        {
            if (bindingType == null)
            {
                throw new ArgumentNullException(nameof(bindingType));
            }

            _bindingType = bindingType;

            // binding
            var binding = new Binding
            {
                Mode = BindingMode.OneWay,
                Converter = this
            };
            DisplayMemberBinding = binding;
        } // ConverterGridViewColumn

        // ----------------------------------------------------------------------
        public Type BindingType => _bindingType;
        // BindingType

        // ----------------------------------------------------------------------
        protected abstract object ConvertValue(object value);

        // ----------------------------------------------------------------------
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!_bindingType.IsInstanceOfType(value))
            {
                throw new InvalidOperationException();
            }
            return ConvertValue(value);
        } // IValueConverter.Convert

        // ----------------------------------------------------------------------
        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        } // IValueConverter.ConvertBack

        // ----------------------------------------------------------------------
        // members
        private readonly Type _bindingType;
    } // class ConverterGridViewColumn
} // namespace Itenso.Windows.Controls.ListViewLayout
// -- EOF -------------------------------------------------------------------