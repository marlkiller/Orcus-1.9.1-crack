using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Xceed.Wpf.Toolkit.Primitives;

namespace Orcus.Administration.Controls.PropertyGrid
{
    public abstract class PropertyEditor<T> : IPropertyEditor where T : FrameworkElement, new()
    {
        protected DependencyProperty ValueProperty { get; set; }
        protected T Editor { get; private set; }
        protected PropertyItem PropertyItem { get; private set; }

        public FrameworkElement GetEditor(PropertyItem propertyItem)
        {
            PropertyItem = propertyItem;
            Editor = CreateEditor();
            ValueProperty = GetDependencyProperty();
            InitializeControl();
            ResolveValueBinding(propertyItem);

            return Editor;
        }

        protected virtual T CreateEditor()
        {
            return new T();
        }

        protected virtual void ResolveValueBinding(PropertyItem propertyItem)
        {
            var binding = new Binding("Value")
            {
                Source = propertyItem,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                Mode = propertyItem.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay,
                Converter = CreateValueConverter()
            };

            BindingOperations.SetBinding(Editor, ValueProperty, binding);
        }

        protected virtual IValueConverter CreateValueConverter()
        {
            return new NullAsDefaultConverter(PropertyItem.Property.PropertyType);
        }

        protected virtual void InitializeControl()
        {
        }

        protected abstract DependencyProperty GetDependencyProperty();

        private class NullAsDefaultConverter : IValueConverter
        {
            private readonly Type _type;

            public NullAsDefaultConverter(Type type)
            {
                _type = type;
            }

            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return value;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value == null && _type.IsValueType)
                    return Activator.CreateInstance(_type);

                return value;
            }
        }
    }
}