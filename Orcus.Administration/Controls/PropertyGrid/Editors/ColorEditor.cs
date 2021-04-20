using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Xceed.Wpf.Toolkit;

namespace Orcus.Administration.Controls.PropertyGrid.Editors
{
    public class ColorEditor : PropertyEditor<ColorPicker>
    {
        protected override DependencyProperty GetDependencyProperty()
        {
            return ColorPicker.SelectedColorProperty;
        }

        protected override ColorPicker CreateEditor()
        {
            return new PropertyGridEditorColorPicker();
        }

        protected override IValueConverter CreateValueConverter()
        {
            return new ColorConverter();
        }

        private class ColorConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                var color = value.GetType().IsValueType ? (System.Drawing.Color) value : (System.Drawing.Color?) value;
                return Color.FromArgb(color.Value.A, color.Value.R, color.Value.G, color.Value.B);
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                var color = (Color?) value;
                return System.Drawing.Color.FromArgb(color.Value.A, color.Value.R, color.Value.G, color.Value.B);
            }
        }
    }

    public class PropertyGridEditorColorPicker : ColorPicker
    {
        static PropertyGridEditorColorPicker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (PropertyGridEditorColorPicker),
                new FrameworkPropertyMetadata(typeof (PropertyGridEditorColorPicker)));
        }
    }
}