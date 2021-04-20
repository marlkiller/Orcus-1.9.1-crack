using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using MahApps.Metro.Controls;

namespace Orcus.Administration.Controls.PropertyGrid.Editors
{
    public class UpDownEditor<T> : PropertyEditor<NumericUpDown> where T : IConvertible
    {
        private readonly double _minValue;
        private readonly double _maxValue;
        private readonly string _stringFormat;
        private readonly bool _isDecimal;

        public UpDownEditor(double minValue, double maxValue, string stringFormat)
        {
            _minValue = minValue;
            _maxValue = maxValue;
            _stringFormat = stringFormat;
        }

        public UpDownEditor(double minValue, double maxValue, string stringFormat, bool isDecimal)
        {
            _minValue = minValue;
            _maxValue = maxValue;
            _stringFormat = stringFormat;
            _isDecimal = isDecimal;
        }

        public UpDownEditor()
        {
        }

        protected override DependencyProperty GetDependencyProperty()
        {
            return NumericUpDown.ValueProperty;
        }

        protected override NumericUpDown CreateEditor()
        {
            return new PropertyGridEditorNumericUpDown();
        }

        protected override void InitializeControl()
        {
            Editor.Maximum = _maxValue;
            Editor.Minimum = _minValue;
            Editor.BorderThickness = new Thickness(0);
            Editor.TextAlignment = TextAlignment.Left;
            Editor.MinHeight = 20;
            Editor.UpDownButtonsWidth = 20;

            if (_stringFormat != null)
                Editor.StringFormat = _stringFormat;

            if (_isDecimal)
            {
                Editor.HasDecimals = _isDecimal;
                Editor.Interval = .1;
            }
        }

        protected override IValueConverter CreateValueConverter()
        {
            return new BasicTypeConverter<T>();
        }

        private class BasicTypeConverter<TResult> : IValueConverter where  TResult : IConvertible
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return value;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return value == null ? default(TResult) : (TResult) System.Convert.ChangeType(value, typeof (TResult));
            }
        }
    }

    public class PropertyGridEditorNumericUpDown : NumericUpDown
    {
        static PropertyGridEditorNumericUpDown()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PropertyGridEditorNumericUpDown),
                new FrameworkPropertyMetadata(typeof(PropertyGridEditorNumericUpDown)));
        }
    }
}