namespace Orcus.Server.Core.UI
{
    public static class UiManager
    {
        internal static IUiImplementation UiImplementation { get; private set; }

        public static void RegisterUiImplementation(IUiImplementation uiImplementation)
        {
            UiImplementation = uiImplementation;
        }
    }
}