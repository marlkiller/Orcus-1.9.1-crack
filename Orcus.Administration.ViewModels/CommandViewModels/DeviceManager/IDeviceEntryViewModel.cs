using MahApps.Metro.IconPacks;

namespace Orcus.Administration.ViewModels.CommandViewModels.DeviceManager
{
    public interface IDeviceEntryViewModel
    {
        string Caption { get; }
        bool DisplayWarning { get; }
        string WarningMessage { get; }
        PackIconMaterialKind Icon { get; }
        bool IsCategory { get; }
    }
}