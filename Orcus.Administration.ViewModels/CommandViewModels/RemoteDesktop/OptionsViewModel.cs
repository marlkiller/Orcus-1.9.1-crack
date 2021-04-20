using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels.CommandViewModels.RemoteDesktop
{
    public class OptionsViewModel : PropertyChangedBase
    {
        public OptionsViewModel(RemoteDesktopViewModel remoteDesktopViewModel)
        {
            RemoteDesktopViewModel = remoteDesktopViewModel;
        }

        public RemoteDesktopViewModel RemoteDesktopViewModel { get; }
    }
}