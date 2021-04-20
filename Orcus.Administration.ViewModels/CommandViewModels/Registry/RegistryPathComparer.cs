using Orcus.Administration.Commands.Registry;
using Orcus.Administration.FileExplorer.Models;
using Orcus.Administration.ViewModels.CommandViewModels.FileExplorer;

namespace Orcus.Administration.ViewModels.CommandViewModels.Registry
{
    public class RegistryPathComparer : PathComparer, ICompareHierarchy<AdvancedRegistrySubKey>
    {
        public HierarchicalResult CompareHierarchy(AdvancedRegistrySubKey value1, AdvancedRegistrySubKey value2)
        {
            return CompareHierarchy(value1?.Path, value2?.Path);
        }
    }
}