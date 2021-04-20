namespace Orcus.Administration.FileExplorer.Models
{

    public interface ICompareHierarchy<T>
    {
        HierarchicalResult CompareHierarchy(T value1, T value2);
    }
}
