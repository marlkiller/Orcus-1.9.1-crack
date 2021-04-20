using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Orcus.Administration.FileExplorer.Controls
{
    [TemplatePart(Name = "hotTrackGrid", Type = typeof (Grid))]
    [TemplatePart(Name = "selected", Type = typeof (Rectangle))]
    [TemplatePart(Name = "background", Type = typeof (Rectangle))]
    [TemplatePart(Name = "highlight", Type = typeof (Rectangle))]
    [TemplateVisualState(Name = "Normal", GroupName = "CommonStates")]
    [TemplateVisualState(Name = "MouseOver", GroupName = "CommonStates")]
    [TemplateVisualState(Name = "Dragging", GroupName = "CommonStates")]
    [TemplateVisualState(Name = "Selected", GroupName = "CommonStates")]
    public class HotTrack : ContentControl
    {
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof (bool),
                typeof (HotTrack), new UIPropertyMetadata(false,
                    OnIsSelectedChanged));

        public static readonly DependencyProperty SelectedBorderBrushProperty =
            DependencyProperty.Register("SelectedBorderBrush", typeof (Brush),
                typeof (HotTrack), new UIPropertyMetadata(Brushes.Transparent));

        public static readonly DependencyProperty BackgroundBrushProperty =
            DependencyProperty.Register("BackgroundBrush", typeof (Brush),
                typeof (HotTrack), new UIPropertyMetadata(SystemColors.HotTrackBrush));

        public static readonly DependencyProperty SelectedBrushProperty =
            DependencyProperty.Register("SelectedBrush", typeof (Brush),
                typeof (HotTrack), new UIPropertyMetadata(SystemColors.ActiveCaptionBrush));

        public static readonly DependencyProperty HighlightBrushProperty =
            DependencyProperty.Register("HighlightBrush", typeof (Brush),
                typeof (HotTrack), new UIPropertyMetadata(new SolidColorBrush(Color.FromArgb(117, 255, 255, 255))));

        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register("CornerRadius", typeof (CornerRadius),
                typeof (HotTrack), new UIPropertyMetadata(new CornerRadius(0)));

        public static readonly DependencyProperty IsDraggingProperty =
            DependencyProperty.Register("IsDragging", typeof (bool),
                typeof (HotTrack), new UIPropertyMetadata(false, OnIsSelectedChanged));

        public static readonly DependencyProperty IsDraggingOverProperty =
            DependencyProperty.Register("IsDraggingOver", typeof (bool),
                typeof (HotTrack), new UIPropertyMetadata(false, OnIsSelectedChanged));

        public static readonly DependencyProperty FillFullRowProperty =
            DependencyProperty.Register("FillFullRow", typeof (bool),
                typeof (HotTrack), new UIPropertyMetadata(false));

        static HotTrack()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (HotTrack),
                new FrameworkPropertyMetadata(typeof (HotTrack)));
        }

        public bool IsSelected
        {
            get { return (bool) GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        public Brush SelectedBorderBrush
        {
            get { return (Brush) GetValue(SelectedBorderBrushProperty); }
            set { SetValue(SelectedBorderBrushProperty, value); }
        }

        public Brush BackgroundBrush
        {
            get { return (Brush) GetValue(BackgroundBrushProperty); }
            set { SetValue(BackgroundBrushProperty, value); }
        }

        public Brush SelectedBrush
        {
            get { return (Brush) GetValue(SelectedBrushProperty); }
            set { SetValue(SelectedBrushProperty, value); }
        }

        public Brush HighlightBrush
        {
            get { return (Brush) GetValue(HighlightBrushProperty); }
            set { SetValue(HighlightBrushProperty, value); }
        }


        public CornerRadius CornerRadius
        {
            get { return (CornerRadius) GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        public bool IsDragging
        {
            get { return (bool) GetValue(IsDraggingProperty); }
            set { SetValue(IsDraggingProperty, value); }
        }

        public bool IsDraggingOver
        {
            get { return (bool) GetValue(IsDraggingOverProperty); }
            set { SetValue(IsDraggingOverProperty, value); }
        }

        /// <summary>
        ///     For TreeView, create a mirror to completely highlight the whole row.
        /// </summary>
        public bool FillFullRow
        {
            get { return (bool) GetValue(FillFullRowProperty); }
            set { SetValue(FillFullRowProperty, value); }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            //UITools.AddValueChanged<HotTrack>(this, IsFocused, 
            UpdateStates(false);
        }

        //protected override void OnGotFocus(RoutedEventArgs e)
        //{
        //    base.OnGotFocus(e);
        //    UpdateStates(true);
        //}

        //protected override void OnLostFocus(RoutedEventArgs e)
        //{
        //    base.OnLostFocus(e);
        //    UpdateStates(true);
        //}

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);

            UpdateStates(true);
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseEnter(e);

            UpdateStates(true);
        }

        private void UpdateStates(bool useTransition)
        {
            if (IsSelected)
                VisualStateManager.GoToState(this, "Selected", useTransition);
            else if (IsDragging)
                VisualStateManager.GoToState(this, "Dragging", useTransition);
            else if (IsDraggingOver)
                VisualStateManager.GoToState(this, "DraggingOver", useTransition);
            else if (IsMouseOver)
                VisualStateManager.GoToState(this, IsEnabled ? "MouseOver" : "MouseOverGrayed", useTransition);
            else VisualStateManager.GoToState(this, "Normal", useTransition);

            //if (IsFocused)
            //    VisualStateManager.GoToState(this, "Focused", useTransition);
            //else VisualStateManager.GoToState(this, "Unfocused", useTransition);
        }

        private static void OnIsSelectedChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            HotTrack ht = (HotTrack) sender;
            if (!(args.NewValue.Equals(args.OldValue)))
                ht.UpdateStates(true);
        }
    }
}