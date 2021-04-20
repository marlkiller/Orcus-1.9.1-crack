using System;
using System.Windows;
using System.Windows.Controls;

namespace Orcus.Administration.FileExplorer.Controls
{
    [TemplateVisualState(Name = "ShowCaption", GroupName = "CaptionStates")]
    [TemplateVisualState(Name = "HideCaption", GroupName = "CaptionStates")]
    public class BreadcrumbTreeItem : TreeViewItem
    {
        public static DependencyProperty OverflowItemCountProperty = OverflowableStackPanel.OverflowItemCountProperty
            .AddOwner(typeof (BreadcrumbTreeItem), new PropertyMetadata(OnOverflowItemCountChanged));

        public static DependencyProperty IsOverflowedProperty = DependencyProperty.Register("IsOverflowed",
            typeof (bool),
            typeof (BreadcrumbTreeItem), new PropertyMetadata(false));

        public static readonly DependencyProperty OverflowedItemContainerStyleProperty =
            BreadcrumbTree.OverflowedItemContainerStyleProperty.AddOwner(typeof (BreadcrumbTreeItem));

        public static readonly DependencyProperty SelectedChildProperty =
            DependencyProperty.Register("SelectedChild", typeof (object), typeof (BreadcrumbTreeItem),
                new UIPropertyMetadata(null));

        public static readonly DependencyProperty ValuePathProperty =
            DependencyProperty.Register("ValuePath", typeof (string), typeof (BreadcrumbTreeItem),
                new UIPropertyMetadata(""));


        public static readonly DependencyProperty IsChildSelectedProperty =
            DependencyProperty.Register("IsChildSelected", typeof (bool), typeof (BreadcrumbTreeItem),
                new UIPropertyMetadata());

        public static readonly DependencyProperty IsCurrentSelectedProperty =
            DependencyProperty.Register("IsCurrentSelected", typeof (bool), typeof (BreadcrumbTreeItem),
                new UIPropertyMetadata(false, PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var x = dependencyPropertyChangedEventArgs;
        }

        public static readonly DependencyProperty IsCaptionVisibleProperty =
            DependencyProperty.Register("IsCaptionVisible", typeof (bool), typeof (BreadcrumbTreeItem),
                new UIPropertyMetadata(true, OnIsCaptionVisibleChanged));

        public static readonly DependencyProperty MenuItemTemplateProperty =
            BreadcrumbTree.MenuItemTemplateProperty.AddOwner(typeof (BreadcrumbTreeItem));

        static BreadcrumbTreeItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (BreadcrumbTreeItem),
                new FrameworkPropertyMetadata(typeof (BreadcrumbTreeItem)));
        }

        public int OverflowItemCount
        {
            get { return (int) GetValue(OverflowItemCountProperty); }
            set { SetValue(OverflowItemCountProperty, value); }
        }

        public bool IsOverflowed
        {
            get { return (bool) GetValue(IsOverflowedProperty); }
            set { SetValue(IsOverflowedProperty, value); }
        }

        public Style OverflowedItemContainerStyle
        {
            get { return (Style) GetValue(OverflowedItemContainerStyleProperty); }
            set { SetValue(OverflowedItemContainerStyleProperty, value); }
        }

        public object SelectedChild
        {
            get { return (object) GetValue(SelectedChildProperty); }
            set { SetValue(SelectedChildProperty, value); }
        }

        public string ValuePath
        {
            get { return (string) GetValue(ValuePathProperty); }
            set { SetValue(ValuePathProperty, value); }
        }

        public bool IsChildSelected
        {
            get { return (bool) GetValue(IsChildSelectedProperty); }
            set { SetValue(IsChildSelectedProperty, value); }
        }

        public bool IsCurrentSelected
        {
            get { return (bool) GetValue(IsCurrentSelectedProperty); }
            set { SetValue(IsCurrentSelectedProperty, value); }
        }

        /// <summary>
        ///     Display Caption
        /// </summary>
        public bool IsCaptionVisible
        {
            get { return (bool) GetValue(IsCaptionVisibleProperty); }
            set { SetValue(IsCaptionVisibleProperty, value); }
        }

        public DataTemplate MenuItemTemplate
        {
            get { return (DataTemplate) GetValue(MenuItemTemplateProperty); }
            set { SetValue(MenuItemTemplateProperty, value); }
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new BreadcrumbTreeItem();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.AddHandler(Button.ClickEvent, (RoutedEventHandler) ((o, e) =>
            {
                if (e.Source is Button)
                {
                    this.SetValue(IsCurrentSelectedProperty, true);
                    e.Handled = true;
                }
            }));

            //this.AddHandler(OverflowItem.SelectedEvent, (RoutedEventHandler)((o, e) =>
            //    {
            //        if (e.Source is OverflowItem)
            //        {
            //            IsExpanded = false;
            //        }
            //    }));
        }

        public static void OnIsCaptionVisibleChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            (sender as BreadcrumbTreeItem).UpdateStates(true);
        }

        public static void OnOverflowItemCountChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            (sender as BreadcrumbTreeItem).SetValue(IsOverflowedProperty, ((int) args.NewValue) > 0);
        }

        private void UpdateStates(bool useTransition)
        {
            if (IsCaptionVisible)
                VisualStateManager.GoToState(this, "ShowCaption", useTransition);
            else VisualStateManager.GoToState(this, "HideCaption", useTransition);
        }
    }
}