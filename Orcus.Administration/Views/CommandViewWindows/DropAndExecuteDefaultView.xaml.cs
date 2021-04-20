using System.Windows;
using Orcus.Administration.ViewModels.CommandViewModels;

namespace Orcus.Administration.Views.CommandViewWindows
{
    /// <summary>
    ///     Interaction logic for DropAndExecuteDefaultView.xaml
    /// </summary>
    public partial class DropAndExecuteDefaultView
    {
        public DropAndExecuteDefaultView()
        {
            InitializeComponent();
        }

        private void DragAreaOnPreviewDragEnter(object sender, DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        }

        private void DragAreaOnPreviewDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                ((DropAndExecuteViewModel) DataContext).UploadAndExecute(files);
            }
        }
    }
}