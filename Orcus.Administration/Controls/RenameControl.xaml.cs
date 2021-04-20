using System.Windows;
using System.Windows.Input;

namespace Orcus.Administration.Controls
{
    /// <summary>
    ///     Interaction logic for RenameControl.xaml
    /// </summary>
    public partial class RenameControl
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text", typeof (string), typeof (RenameControl), new PropertyMetadata(default(string)));

        public static readonly DependencyProperty TextToEditProperty = DependencyProperty.Register(
            "TextToEdit", typeof (string), typeof (RenameControl), new PropertyMetadata(default(string)));

        public string TextToEdit
        {
            get { return (string) GetValue(TextToEditProperty); }
            set { SetValue(TextToEditProperty, value); }
        }

        public static readonly DependencyProperty IsInRenamingModeProperty = DependencyProperty.Register(
            "IsInRenamingMode", typeof (bool), typeof (RenameControl),
            new PropertyMetadata(default(bool), IsInRenamingModeChanged));

        public static readonly DependencyProperty RenameCommandProperty = DependencyProperty.Register(
            "RenameCommand", typeof (ICommand), typeof (RenameControl), new PropertyMetadata(default(ICommand)));

        public static readonly DependencyProperty FileSelectionModeProperty = DependencyProperty.Register(
            "FileSelectionMode", typeof (bool), typeof (RenameControl), new PropertyMetadata(default(bool)));

        public bool FileSelectionMode
        {
            get { return (bool) GetValue(FileSelectionModeProperty); }
            set { SetValue(FileSelectionModeProperty, value); }
        }

        public RenameControl()
        {
            InitializeComponent();
        }

        public string Text
        {
            get { return (string) GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public ICommand RenameCommand
        {
            get { return (ICommand) GetValue(RenameCommandProperty); }
            set { SetValue(RenameCommandProperty, value); }
        }

        public bool IsInRenamingMode
        {
            get { return (bool) GetValue(IsInRenamingModeProperty); }
            set { SetValue(IsInRenamingModeProperty, value); }
        }

        private static void IsInRenamingModeChanged(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var renameControl = (RenameControl) dependencyObject;
            var isRenaming = (bool) dependencyPropertyChangedEventArgs.NewValue;
            renameControl.CONTENT_ON.Visibility = isRenaming ? Visibility.Hidden : Visibility.Visible;
            renameControl.CONTENT_OFF.Visibility = isRenaming ? Visibility.Visible : Visibility.Hidden;
            if (isRenaming)
            {
                renameControl.CONTENT_OFF.Text = renameControl.TextToEdit;
                if (renameControl.FileSelectionMode && renameControl.TextToEdit.Contains("."))
                    renameControl.CONTENT_OFF.Select(0, renameControl.TextToEdit.LastIndexOf('.'));
                else
                    renameControl.CONTENT_OFF.SelectAll();
                renameControl.CONTENT_OFF.Focus();
            }
        }

        private void CONTENT_OFF_OnPreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (!IsInRenamingMode)
                return;

            if (CONTENT_OFF.ContextMenu.IsOpen)
                return;

            if (e.NewFocus == CONTENT_OFF)
                return;

            IsInRenamingMode = false;
            RenameCommand?.Execute(CONTENT_OFF.Text);
        }

        private void CONTENT_OFF_OnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void CONTENT_OFF_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                case Key.Tab:
                    IsInRenamingMode = false;
                    RenameCommand?.Execute(CONTENT_OFF.Text);
                    break;
                case Key.Escape:
                    IsInRenamingMode = false;
                    break;
            }
        }
    }
}