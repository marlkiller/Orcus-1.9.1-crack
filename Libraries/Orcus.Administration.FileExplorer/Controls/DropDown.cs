using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Orcus.Administration.FileExplorer.Controls
{
    /// <summary>
    ///     Display a ToggleButton and when it's clicked, show it's content as a dropdown.
    /// </summary>
    public class DropDown : HeaderedContentControl
    {
        public static readonly DependencyProperty IsDropDownOpenProperty =
            DependencyProperty.Register("IsDropDownOpen", typeof (bool),
                typeof (DropDown), new UIPropertyMetadata(false,
                    OnIsDropDownOpenChanged,
                    OnIsDropDownOpenCoerce));

        public static readonly DependencyProperty IsDropDownAlignLeftProperty =
            DependencyProperty.Register("IsDropDownAlignLeft", typeof (bool),
                typeof (DropDown), new UIPropertyMetadata(false));

        public static readonly DependencyProperty PlacementTargetProperty =
            Popup.PlacementTargetProperty.AddOwner(typeof (DropDown));

        public static readonly DependencyProperty PlacementProperty =
            Popup.PlacementProperty.AddOwner(typeof (DropDown));

        public static readonly DependencyProperty HeaderButtonTemplateProperty =
            DependencyProperty.Register("HeaderButtonTemplate", typeof (ControlTemplate), typeof (DropDown));

        public static readonly DependencyProperty HorizontalOffsetProperty =
            Popup.HorizontalOffsetProperty.AddOwner(typeof (DropDown));

        public static readonly DependencyProperty VerticalOffsetProperty =
            Popup.VerticalOffsetProperty.AddOwner(typeof (DropDown));

        private ContentPresenter _content;
        private Popup _popup;
        private DateTime _lastLostFocusTime = DateTime.MinValue;

        static DropDown()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (DropDown),
                new FrameworkPropertyMetadata(typeof (DropDown)));
        }

        public bool IsDropDownOpen
        {
            get { return (bool) GetValue(IsDropDownOpenProperty); }
            set { SetValue(IsDropDownOpenProperty, value); }
        }

        public bool IsDropDownAlignLeft
        {
            get { return (bool) GetValue(IsDropDownAlignLeftProperty); }
            set { SetValue(IsDropDownAlignLeftProperty, value); }
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

        public ControlTemplate HeaderButtonTemplate
        {
            get { return (ControlTemplate) GetValue(HeaderButtonTemplateProperty); }
            set { SetValue(HeaderButtonTemplateProperty, value); }
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

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var popup = Template.FindName("PART_Popup", this);
            if (popup is Popup)
            {
                _popup = (Popup) this.Template.FindName("PART_Popup", this);
                _content = (ContentPresenter) this.Template.FindName("PART_Content", this);

                _popup.AddHandler(Popup.LostFocusEvent,
                    new RoutedEventHandler((o, e) =>
                    {
                        //(o as DropDownControl).                   
                        //IsDropDownOpen = false;
                    }));
            }
        }

        private static object OnIsDropDownOpenCoerce(DependencyObject sender, object baseValue)
        {
            DropDown ddc = (DropDown) sender;
            if ((bool) baseValue)
            {
                if (Math.Abs(ddc._lastLostFocusTime.Subtract(DateTime.UtcNow).TotalSeconds) < 0.5)
                    return false;
            }
            else
                ddc._lastLostFocusTime = DateTime.UtcNow;

            return baseValue;
        }


        private static void OnIsDropDownOpenChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            //DropDown ddc = (DropDown)sender;
            //if ((bool)args.NewValue)
            //    if (ddc.lastLostFocusTime.Subtract(DateTime.UtcNow).TotalSeconds < 1)
            //        Isdro
            //    else ddc.lastLostFocusTime = DateTime.UtcNow;

            //if (ddc._popup != null)
            //{
            //    ddc._popup.IsOpen = (bool)args.NewValue;
            //}
            //if (ddc._content != null)
            //{
            //    ddc._content.Focus();
            //}
            //if (((bool)args.NewValue) && ddc._dropDownGrid != null)
            //{
            //    //Setfocu
            //    //ddc._dropDownGrid.
            //    //Debug.WriteLine(ddc._dropDownGrid.IsFocused);
            //}
        }

        //IsDropDownOpen

        //IsHeaderEnabled
    }
}