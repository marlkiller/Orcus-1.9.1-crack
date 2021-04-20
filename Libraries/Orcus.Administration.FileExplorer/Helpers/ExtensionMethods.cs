namespace Orcus.Administration.FileExplorer.Helpers
{
    public static class ExtensionMethods
    {
        public static ITreeRootSelector<VM, T> AsRoot<VM, T>(this ITreeSelector<VM, T> selector)
        {
            return selector as ITreeRootSelector<VM, T>;
        }
    }
}