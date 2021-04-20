using System.Windows;
using System.Windows.Controls;

namespace Orcus.Administration.Controls.PropertyGrid.Editors
{
    public class MultilineStringEditor : PropertyEditor<ExpandableTextBox>
    {
        protected override DependencyProperty GetDependencyProperty()
        {
            return TextBox.TextProperty;
        }
    }

    public class ExpandableTextBox : TextBox
    {
        public static readonly DependencyProperty IsDropDownOpenProperty = DependencyProperty.Register(
            "IsDropDownOpen", typeof (bool), typeof (ExpandableTextBox), new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty DropDownHeightProperty = DependencyProperty.Register(
            "DropDownHeight", typeof (double), typeof (ExpandableTextBox), new PropertyMetadata(300d));

        public static readonly DependencyProperty DropDownWidthProperty = DependencyProperty.Register(
            "DropDownWidth", typeof (double), typeof (ExpandableTextBox), new PropertyMetadata(400d));

        static ExpandableTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (ExpandableTextBox),
                new FrameworkPropertyMetadata(typeof (ExpandableTextBox)));
        }

        public bool IsDropDownOpen
        {
            get { return (bool) GetValue(IsDropDownOpenProperty); }
            set { SetValue(IsDropDownOpenProperty, value); }
        }

        public double DropDownHeight
        {
            get { return (double) GetValue(DropDownHeightProperty); }
            set { SetValue(DropDownHeightProperty, value); }
        }

        public double DropDownWidth
        {
            get { return (double) GetValue(DropDownWidthProperty); }
            set { SetValue(DropDownWidthProperty, value); }
        }
    }
}