using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Orcus.Administration.FileExplorer.Controls
{
    /// <summary>
    ///     User update Suggestions when TextChangedEvent raised.
    /// </summary>
    public class SuggestBoxBase : TextBox
    {
        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent("ValueChanged",
            RoutingStrategy.Bubble, typeof (RoutedEventHandler), typeof (SuggestBoxBase));

        public static readonly DependencyProperty DisplayMemberPathProperty = DependencyProperty.Register(
            "DisplayMemberPath", typeof (string), typeof (SuggestBoxBase), new PropertyMetadata("Header"));

        public static readonly DependencyProperty ValuePathProperty = DependencyProperty.Register(
            "ValuePath", typeof (string), typeof (SuggestBoxBase), new PropertyMetadata("Value"));

        public static readonly DependencyProperty SuggestionsProperty = DependencyProperty.Register(
            "Suggestions", typeof (IList<object>), typeof (SuggestBoxBase),
            new PropertyMetadata(null, OnSuggestionsChanged));

        public static readonly DependencyProperty HeaderTemplateProperty =
            HeaderedItemsControl.HeaderTemplateProperty.AddOwner(typeof (SuggestBoxBase));

        public static readonly DependencyProperty IsPopupOpenedProperty =
            DependencyProperty.Register("IsPopupOpened", typeof (bool),
                typeof (SuggestBoxBase), new UIPropertyMetadata(false));

        public static readonly DependencyProperty DropDownPlacementTargetProperty =
            DependencyProperty.Register("DropDownPlacementTarget", typeof (object), typeof (SuggestBoxBase));

        public static readonly DependencyProperty HintProperty =
            DependencyProperty.Register("Hint", typeof (string), typeof (SuggestBoxBase), new PropertyMetadata(""));

        public static readonly DependencyProperty IsHintVisibleProperty =
            DependencyProperty.Register("IsHintVisible", typeof (bool), typeof (SuggestBoxBase),
                new PropertyMetadata(true));

        protected ScrollViewer _host;
        protected ListBox _itemList;
        protected Popup _popup;
        protected bool _prevState;
        protected Grid _root;
        protected UIElement _textBoxView;

        static SuggestBoxBase()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (SuggestBoxBase),
                new FrameworkPropertyMetadata(typeof (SuggestBoxBase)));
        }

        public string DisplayMemberPath
        {
            get { return (string) GetValue(DisplayMemberPathProperty); }
            set { SetValue(DisplayMemberPathProperty, value); }
        }

        public string ValuePath
        {
            get { return (string) GetValue(ValuePathProperty); }
            set { SetValue(ValuePathProperty, value); }
        }

        public IList<object> Suggestions
        {
            get { return (IList<object>) GetValue(SuggestionsProperty); }
            set { SetValue(SuggestionsProperty, value); }
        }

        public DataTemplate HeaderTemplate
        {
            get { return (DataTemplate) GetValue(HeaderTemplateProperty); }
            set { SetValue(HeaderTemplateProperty, value); }
        }

        public bool IsPopupOpened
        {
            get { return (bool) GetValue(IsPopupOpenedProperty); }
            set { SetValue(IsPopupOpenedProperty, value); }
        }

        public object DropDownPlacementTarget
        {
            get { return GetValue(DropDownPlacementTargetProperty); }
            set { SetValue(DropDownPlacementTargetProperty, value); }
        }

        public string Hint
        {
            get { return (string) GetValue(HintProperty); }
            set { SetValue(HintProperty, value); }
        }

        public bool IsHintVisible
        {
            get { return (bool) GetValue(IsHintVisibleProperty); }
            set { SetValue(IsHintVisibleProperty, value); }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _popup = Template.FindName("PART_Popup", this) as Popup;
            _itemList = Template.FindName("PART_ItemList", this) as ListBox;
            _host = Template.FindName("PART_ContentHost", this) as ScrollViewer;
            _textBoxView = LogicalTreeHelper.GetChildren(_host).OfType<UIElement>().First();
            _root = Template.FindName("root", this) as Grid;

            GotKeyboardFocus += (o, e) =>
            {
                popupIfSuggest();
                IsHintVisible = false;
            };
            LostKeyboardFocus += (o, e) =>
            {
                if (!IsKeyboardFocusWithin) hidePopup();
                IsHintVisible = String.IsNullOrEmpty(Text);
            };

            //09-04-09 Based on SilverLaw's approach 
            _popup.CustomPopupPlacementCallback += (popupSize, targetSize, offset) => new[]
            {
                new CustomPopupPlacement(new Point((0.01 - offset.X),
                    (_root.ActualHeight - offset.Y)), PopupPrimaryAxis.None)
            };

            _itemList.MouseDoubleClick += (o, e) =>
            {
                updateValueFromListBox();
                AcceptUpdate();
            };

            _itemList.PreviewMouseUp += (o, e) =>
            {
                if (_itemList.SelectedValue != null)
                    updateValueFromListBox();
            };

            _itemList.PreviewKeyDown += (o, e) =>
            {
                if (e.OriginalSource is ListBoxItem)
                {
                    ListBoxItem lbItem = e.OriginalSource as ListBoxItem;

                    e.Handled = true;
                    switch (e.Key)
                    {
                        case Key.Enter:
                            //Handle in OnPreviewKeyDown
                            break;
                        case Key.Oem5:
                            updateValueFromListBox(false);
                            SetValue(TextProperty, Text + "\\");
                            break;
                        case Key.Escape:
                            Focus();
                            hidePopup();
                            break;
                        default:
                            e.Handled = false;
                            break;
                    }

                    if (e.Handled)
                    {
                        Keyboard.Focus(this);
                        hidePopup();
                        Select(Text.Length, 0); //Select last char
                    }
                }
            };

            Window parentWindow = Window.GetWindow(this);
            if (parentWindow != null)
            {
                parentWindow.Deactivated += delegate
                {
                    _prevState = IsPopupOpened;
                    IsPopupOpened = false;
                };
                parentWindow.Activated += delegate { IsPopupOpened = _prevState; };
            }
        }

        private void updateValueFromListBox(bool updateSrc = true)
        {
            SetValue(TextProperty, _itemList.SelectedValue);

            if (updateSrc)
                updateSource();
            hidePopup();
            Focus();
            CaretIndex = Text.Length;
            AcceptUpdate();
        }

        protected virtual void AcceptUpdate()
        {
        }

        protected virtual void updateSource()
        {
            var txtBindingExpr = GetBindingExpression(TextProperty);
            if (txtBindingExpr == null)
                return;

            if (txtBindingExpr != null)
                txtBindingExpr.UpdateSource();
            RaiseEvent(new RoutedEventArgs(ValueChangedEvent));
        }

        protected void popupIfSuggest()
        {
            if (IsFocused)
                if (Suggestions != null && Suggestions.Count > 0)
                    IsPopupOpened = true;
                else IsPopupOpened = false;
        }

        protected void hidePopup()
        {
            IsPopupOpened = false;
        }

        protected static string getDirectoryName(string path)
        {
            if (path.EndsWith("\\"))
                return path;
            //path = path.Substring(0, path.Length - 1); //Remove ending slash.

            int idx = path.LastIndexOf('\\');
            if (idx == -1)
                return "";
            return path.Substring(0, idx);
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            switch (e.Key)
            {
                case Key.Up:
                case Key.Down:
                case Key.Prior:
                case Key.Next:
                    if (Suggestions != null && Suggestions.Count > 0 && !(e.OriginalSource is ListBoxItem))
                    {
                        popupIfSuggest();
                        _itemList.Focus();
                        _itemList.SelectedIndex = 0;
                        ListBoxItem lbi = _itemList.ItemContainerGenerator
                            .ContainerFromIndex(0) as ListBoxItem;
                        lbi?.Focus();
                        e.Handled = true;
                    }
                    break;
                case Key.Tab:
                case Key.Return:
                case Key.Space:
                    if (_itemList.IsKeyboardFocusWithin)
                        updateValueFromListBox();
                    hidePopup();
                    updateSource();
                    e.Handled = true;
                    break;
                case Key.Back:
                    if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                    {
                        if (Text.EndsWith("\\"))
                            SetValue(TextProperty, Text.Substring(0, Text.Length - 1));
                        else
                            SetValue(TextProperty, getDirectoryName(Text) + "\\");

                        Select(Text.Length, 0);
                        e.Handled = true;
                    }
                    break;
            }
        }

        protected static void OnSuggestionsChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            SuggestBoxBase sbox = sender as SuggestBoxBase;
            if (args.OldValue != args.NewValue)
                sbox.popupIfSuggest();
        }

        public event RoutedEventHandler ValueChanged
        {
            add { AddHandler(ValueChangedEvent, value); }
            remove { RemoveHandler(ValueChangedEvent, value); }
        }
    }
}