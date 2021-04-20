using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;

namespace Orcus.Administration.Controls.PropertyGrid.Controls
{
    [TemplatePart(Name = "PART_OpenDialog", Type = typeof(Button))]
    public class PathBox : TextBox
    {
        public PathBox()
        {
            AllowDrop = true;
        }

        protected override void OnPreviewDragOver(DragEventArgs e)
        {
            base.OnPreviewDragOver(e);
            e.Handled = true;

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[]) e.Data.GetData(DataFormats.FileDrop);
                e.Effects = files?.Length == 1 ? DragDropEffects.Copy : DragDropEffects.None;
            }
            else
                e.Effects = DragDropEffects.None;
        }

        protected override void OnPreviewDrop(DragEventArgs e)
        {
            base.OnPreviewDrop(e);
            e.Handled = true;

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[]) e.Data.GetData(DataFormats.FileDrop);
                if (files?.Length > 0)
                    SetValue(TextProperty, files[0]);
            }
        }

        public static readonly DependencyProperty IsSelectingFileProperty = DependencyProperty.Register(
            "IsSelectingFile", typeof (bool), typeof (PathBox), new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty FilterProperty = DependencyProperty.Register(
            "Filter", typeof(string), typeof(PathBox), new PropertyMetadata(default(string)));

        public bool IsSelectingFile
        {
            get { return (bool) GetValue(IsSelectingFileProperty); }
            set { SetValue(IsSelectingFileProperty, value); }
        }

        public string Filter
        {
            get { return (string) GetValue(FilterProperty); }
            set { SetValue(FilterProperty, value); }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var openDialogButton = Template.FindName("PART_OpenDialog", this) as Button;
            if (openDialogButton != null)
            {
                openDialogButton.Click += OpenDialogButtonOnClick;
            }
        }

        private void OpenDialogButtonOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            if (IsSelectingFile)
            {
                var ofd = new OpenFileDialog {Filter = Filter, Multiselect = false};
                if (ofd.ShowDialog(Window.GetWindow(this)) == true)
                    Text = ofd.FileName;
            }
            else
            {
                var folderBrowser = new VistaFolderBrowserDialog();
                if (folderBrowser.ShowDialog(Window.GetWindow(this)) == true)
                    Text = folderBrowser.SelectedPath;
            }
        }

        static PathBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PathBox), new FrameworkPropertyMetadata(typeof(PathBox)));
        }
    }
}