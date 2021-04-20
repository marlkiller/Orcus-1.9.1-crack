using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Orcus.Administration.FileExplorer.Utilities;

namespace Orcus.Administration.FileExplorer.Controls
{
    public class ExplorerTextBox : SuggestBox, ICustomFocusLoosingControl
    {
        public static readonly DependencyProperty IsInEditModeProperty = DependencyProperty.Register(
            "IsInEditMode", typeof (bool), typeof (ExplorerTextBox),
            new PropertyMetadata(default(bool), IsInEditModePropertyChangedCallback));

        public static readonly DependencyProperty EnterTextCommandProperty = DependencyProperty.Register(
            "EnterTextCommand", typeof (ICommand), typeof (ExplorerTextBox), new PropertyMetadata(default(ICommand)));

        public static readonly DependencyProperty CurrentPathProperty = DependencyProperty.Register(
            "CurrentPath", typeof (string), typeof (ExplorerTextBox), new PropertyMetadata(default(string)));

        private bool _ignoreNext;

        public ExplorerTextBox()
        {
            AddHandler(PreviewMouseLeftButtonDownEvent,
                new MouseButtonEventHandler(SelectivelyIgnoreMouseButton), true);
            AddHandler(GotKeyboardFocusEvent,
                new RoutedEventHandler(SelectAllText), true);
            AddHandler(MouseDoubleClickEvent,
                new RoutedEventHandler(SelectAllText), true);

            Loaded += OnLoaded;
        }

        public event EventHandler FocusLost;

        protected virtual void OnFocusLost()
        {
            FocusLost?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnWantToLooseFocus()
        {
            OnFocusLost();
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Text = CurrentPath;
            Loaded -= OnLoaded;
        }

        public ICommand EnterTextCommand
        {
            get { return (ICommand) GetValue(EnterTextCommandProperty); }
            set { SetValue(EnterTextCommandProperty, value); }
        }

        public bool IsInEditMode
        {
            get { return (bool) GetValue(IsInEditModeProperty); }
            set { SetValue(IsInEditModeProperty, value); }
        }

        public string CurrentPath
        {
            get { return (string)GetValue(CurrentPathProperty); }
            set { SetValue(CurrentPathProperty, value); }
        }

        private static void IsInEditModePropertyChangedCallback(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var explorerTextBox = (ExplorerTextBox) dependencyObject;
            if (!explorerTextBox.IsLoaded)
                return;

            if ((bool) dependencyPropertyChangedEventArgs.NewValue)
            {
                explorerTextBox.Text = explorerTextBox.CurrentPath;
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            ContextMenuOpening += OnContextMenuClosing;
        }
       
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            switch (e.Key)
            {
                case Key.Escape:
                    OnWantToLooseFocus();
                    break;
                case Key.Enter:
                    EnterTextCommand?.Execute(Text);
                    OnWantToLooseFocus();
                    break;
            }
        }

        protected override void OnPreviewLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnPreviewLostKeyboardFocus(e);

            if (_popup.IsOpen)
            {
                var newElement = e.NewFocus as DependencyObject;
                if (newElement != null)
                {
                    var elementParentPopup = newElement.FindParent<ListBox>();
                    if (_itemList == elementParentPopup || newElement == _itemList)
                    {
                        return;
                    }
                }
            }

            if (ContextMenu.IsOpen)
                return;

            if (e.NewFocus == this)
                return;

            OnFocusLost();
        }

        private void OnContextMenuClosing(object sender, ContextMenuEventArgs contextMenuEventArgs)
        {
            _ignoreNext = true;
        }

        private static void SelectivelyIgnoreMouseButton(object sender,
            MouseButtonEventArgs e)
        {
            // Find the TextBox
            DependencyObject parent = e.OriginalSource as UIElement;
            while (parent != null && !(parent is TextBox))
                parent = VisualTreeHelper.GetParent(parent);

            if (parent != null)
            {
                var textBox = (TextBox) parent;
                if (!textBox.IsKeyboardFocusWithin)
                {
                    // If the text box is not yet focussed, give it the focus and
                    // stop further processing of this click event.
                    textBox.Focus();
                    e.Handled = true;
                }
            }
        }

        private static void SelectAllText(object sender, RoutedEventArgs e)
        {
            var textBox = e.OriginalSource as ExplorerTextBox;
            if (textBox == null)
                return;

            if (textBox._ignoreNext)
            {
                textBox._ignoreNext = false;
                return;
            }
            textBox.SelectAll();
        }
    }
}