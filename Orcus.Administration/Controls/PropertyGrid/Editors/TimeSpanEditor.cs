using System;
using System.Windows;
using Xceed.Wpf.Toolkit;

namespace Orcus.Administration.Controls.PropertyGrid.Editors
{
    public class TimeSpanEditor : PropertyEditor<TimeSpanUpDown>
    {
        protected override DependencyProperty GetDependencyProperty()
        {
            return TimeSpanUpDown.ValueProperty;
        }

        protected override TimeSpanUpDown CreateEditor()
        {
            return new PropertyGridEditorTimeSpanUpDown();
        }

        protected override void InitializeControl()
        {
            base.InitializeControl();
            Editor.Minimum = TimeSpan.Zero;
        }
    }

    public class PropertyGridEditorTimeSpanUpDown : TimeSpanUpDown
    {
        static PropertyGridEditorTimeSpanUpDown()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PropertyGridEditorTimeSpanUpDown),
                new FrameworkPropertyMetadata(typeof(PropertyGridEditorTimeSpanUpDown)));
        }
    }
}