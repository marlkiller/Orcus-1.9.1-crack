using System.Linq;
using System.Windows.Controls;
using Orcus.Administration.FileExplorer.Controls;
using Orcus.Administration.ViewModels.CommandViewModels;
using Orcus.Administration.ViewModels.CommandViewModels.FileExplorer;

namespace Orcus.Administration.Views.CommandViews
{
    /// <summary>
    ///     Interaction logic for FileExplorerCommandView.xaml
    /// </summary>
    public partial class FileExplorerCommandView
    {
        public FileExplorerCommandView()
        {
            InitializeComponent();
            ExplorerTextBox.HierarchyHelper = new PathHierarchyHelper("Parent", "Selection.Value.Path",
                "AutoCompleteEntries");
        }

        private void EntriesListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ((FileExplorerViewModel) DataContext).SelectedEntriesChanged(
                ((ListView) sender).SelectedItems.Cast<IEntryViewModel>().ToList());
        }
    }
}