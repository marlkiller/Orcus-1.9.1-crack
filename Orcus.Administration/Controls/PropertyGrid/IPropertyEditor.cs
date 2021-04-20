using System.Windows;

namespace Orcus.Administration.Controls.PropertyGrid
{
    public interface IPropertyEditor
    {
        FrameworkElement GetEditor(PropertyItem propertyItem);
    }
}