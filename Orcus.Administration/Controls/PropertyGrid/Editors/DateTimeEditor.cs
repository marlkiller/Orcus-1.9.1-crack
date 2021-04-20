using System;
using System.Windows;
using Xceed.Wpf.Toolkit;

namespace Orcus.Administration.Controls.PropertyGrid.Editors
{
    public class DateTimeEditor : PropertyEditor<DateTimeUpDown>
    {
        protected override DependencyProperty GetDependencyProperty()
        {
            return DateTimeUpDown.ValueProperty;
        }

        protected override DateTimeUpDown CreateEditor()
        {
            return new PropertyGridEditorDateTimeUpDown();
        }

        protected override void InitializeControl()
        {
            base.InitializeControl();

            if ((PropertyItem.Property.PropertyType.IsValueType && (DateTime) PropertyItem.Value == default(DateTime)) ||
                PropertyItem.Value == null)
                PropertyItem.Value = DateTime.Now;
        }
    }

    public class PropertyGridEditorDateTimeUpDown : DateTimeUpDown
    {
        static PropertyGridEditorDateTimeUpDown()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PropertyGridEditorDateTimeUpDown),
                new FrameworkPropertyMetadata(typeof(PropertyGridEditorDateTimeUpDown)));
        }
    }
}