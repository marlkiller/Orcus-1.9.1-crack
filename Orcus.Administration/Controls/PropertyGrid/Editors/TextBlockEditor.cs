using System.Windows;
using System.Windows.Controls;

namespace Orcus.Administration.Controls.PropertyGrid.Editors
{
    public class TextBlockEditor : PropertyEditor<TextBlock>
    {
        protected override DependencyProperty GetDependencyProperty()
        {
            return TextBlock.TextProperty;
        }
    }
}