using System.Collections;

namespace Orcus.Administration.Controls.PropertyGrid.Editors
{
    public class BooleanComboBoxEditor : ComboBoxEditor
    {
        protected override IEnumerable CreateItemsSource()
        {
            return new[] {false, true};
        }
    }
}