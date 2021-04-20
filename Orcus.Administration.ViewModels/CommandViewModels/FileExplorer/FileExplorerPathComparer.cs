using Orcus.Administration.FileExplorer.Models;
using Orcus.Shared.Commands.FileExplorer;

namespace Orcus.Administration.ViewModels.CommandViewModels.FileExplorer
{
    public class FileExplorerPathComparer : PathComparer, ICompareHierarchy<IFileExplorerEntry>
    {
        public HierarchicalResult CompareHierarchy(IFileExplorerEntry value1, IFileExplorerEntry value2)
        {
            return CompareHierarchy(value1?.Path, value2?.Path);
        }
    }
}