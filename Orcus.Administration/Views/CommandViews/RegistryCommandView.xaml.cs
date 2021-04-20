using Orcus.Administration.FileExplorer.Controls;

namespace Orcus.Administration.Views.CommandViews
{
    /// <summary>
    ///     Interaction logic for RegistryCommandView.xaml
    /// </summary>
    public partial class RegistryCommandView
    {
        public RegistryCommandView()
        {
            InitializeComponent();
            ExplorerTextBox.HierarchyHelper = new PathHierarchyHelper("Parent", "Selection.Value.Path", "AutoCompleteEntries");
        }
    }
}