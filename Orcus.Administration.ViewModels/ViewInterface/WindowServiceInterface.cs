using System.ComponentModel;

namespace Orcus.Administration.ViewModels.ViewInterface
{
    public class WindowServiceInterface
    {
        internal static IWindowServiceInterface Current { get; private set; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void Initialize(IWindowServiceInterface windowServiceInterface)
        {
            Current = windowServiceInterface;
        }
    }
}