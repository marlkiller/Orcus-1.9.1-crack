using System;
using Orcus.Administration.FileExplorer.Models;

namespace Orcus.Administration.ViewModels.CommandViewModels.FileExplorer
{
    public abstract class PathComparer
    {
        private const char Separator = '\\';
        private readonly StringComparison _stringComparison = StringComparison.OrdinalIgnoreCase;

        protected HierarchicalResult CompareHierarchy(string path1, string path2)
        {
            if (path1 == null || path2 == null)
                return HierarchicalResult.Unrelated;

            path1 = path1.TrimEnd(Separator);
            path2 = path2.TrimEnd(Separator);

            if (path1.Equals(path2, _stringComparison))
                return HierarchicalResult.Current;

            var path1Split = path1.Split(new[] {Separator}, StringSplitOptions.RemoveEmptyEntries);
            var path2Split = path2.Split(new[] {Separator}, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < Math.Min(path1Split.Length, path2Split.Length); i++)
            {
                if (!path1Split[i].Equals(path2Split[i], _stringComparison))
                    return HierarchicalResult.Unrelated;
            }

            return path1Split.Length > path2Split.Length ? HierarchicalResult.Parent : HierarchicalResult.Child;
        }
    }
}