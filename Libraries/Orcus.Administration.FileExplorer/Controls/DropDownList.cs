using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Orcus.Administration.FileExplorer.Controls
{
    public class DropDownList : ComboBox
    {
        public static readonly DependencyProperty HeaderProperty =
            HeaderedContentControl.HeaderProperty.AddOwner(typeof (DropDownList));

        public static readonly DependencyProperty PlacementTargetProperty =
            Popup.PlacementTargetProperty.AddOwner(typeof (DropDownList));

        public static readonly DependencyProperty PlacementProperty =
            Popup.PlacementProperty.AddOwner(typeof (DropDownList));

        public static readonly DependencyProperty HorizontalOffsetProperty =
            Popup.HorizontalOffsetProperty.AddOwner(typeof (DropDownList));

        public static readonly DependencyProperty VerticalOffsetProperty =
            Popup.VerticalOffsetProperty.AddOwner(typeof (DropDownList));

        public static readonly DependencyProperty HeaderButtonTemplateProperty =
            DropDown.HeaderButtonTemplateProperty.AddOwner(typeof (DropDownList));

        static DropDownList()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (DropDownList),
                new FrameworkPropertyMetadata(typeof (DropDownList)));
        }

        public object Header
        {
            get { return GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public UIElement PlacementTarget
        {
            get { return (UIElement) GetValue(PlacementTargetProperty); }
            set { SetValue(PlacementTargetProperty, value); }
        }

        public PlacementMode Placement
        {
            get { return (PlacementMode) GetValue(PlacementProperty); }
            set { SetValue(PlacementProperty, value); }
        }

        public double HorizontalOffset
        {
            get { return (double) GetValue(HorizontalOffsetProperty); }
            set { SetValue(HorizontalOffsetProperty, value); }
        }

        public double VerticalOffset
        {
            get { return (double) GetValue(VerticalOffsetProperty); }
            set { SetValue(VerticalOffsetProperty, value); }
        }

        public ControlTemplate HeaderButtonTemplate
        {
            get { return (ControlTemplate) GetValue(HeaderButtonTemplateProperty); }
            set { SetValue(HeaderButtonTemplateProperty, value); }
        }
    }
}