using System.Windows;
using System.Windows.Controls;

namespace Orcus.Administration.Controls.PropertyGrid.Editors
{
    public class TextBoxEditor : PropertyEditor<TextBox>
    {
        protected override TextBox CreateEditor()
        {
            return new PropertyGridEditorTextBox();
        }

        protected override DependencyProperty GetDependencyProperty()
        {
            return TextBox.TextProperty;
        }
    }

    public class PropertyGridEditorTextBox : TextBox
    {
        static PropertyGridEditorTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (PropertyGridEditorTextBox),
                new FrameworkPropertyMetadata(typeof (PropertyGridEditorTextBox)));
        }
    }
}