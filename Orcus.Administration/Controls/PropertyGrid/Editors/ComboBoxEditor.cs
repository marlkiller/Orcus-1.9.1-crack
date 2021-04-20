using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Orcus.Administration.Controls.PropertyGrid.Editors
{
    public abstract class ComboBoxEditor : PropertyEditor<ComboBox>
    {
        protected override DependencyProperty GetDependencyProperty()
        {
            return Selector.SelectedValueProperty;
        }

        protected override ComboBox CreateEditor()
        {
            return new PropertyGridEditorComboBox();
        }

        protected abstract IEnumerable CreateItemsSource();

        protected override void InitializeControl()
        {
            base.InitializeControl();
            Editor.ItemsSource = CreateItemsSource();
        }
    }

    public class PropertyGridEditorComboBox : ComboBox
    {
        static PropertyGridEditorComboBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PropertyGridEditorComboBox),
                new FrameworkPropertyMetadata(typeof(PropertyGridEditorComboBox)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var popupBorder = GetTemplateChild("PopupBorder") as Border;
            if (popupBorder != null)
                popupBorder.BorderThickness = new Thickness(1, 1, 1, 1);
        }
    }
}