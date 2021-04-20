using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Orcus.Administration.Controls.PropertyGrid.Editors
{
    public class CheckBoxEditor : PropertyEditor<CheckBox>
    {
        protected override DependencyProperty GetDependencyProperty()
        {
            return ToggleButton.IsCheckedProperty;
        }

        protected override void InitializeControl()
        {
            base.InitializeControl();

            Editor.Margin = new Thickness(5, 0, 0, 0);
            Editor.VerticalAlignment = VerticalAlignment.Center;
        }
    }
}