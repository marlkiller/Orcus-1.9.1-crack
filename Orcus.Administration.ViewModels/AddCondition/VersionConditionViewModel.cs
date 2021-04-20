using System.Windows;
using Orcus.Administration.ViewModels.ViewInterface;
using Orcus.Shared.DynamicCommands.Conditions;

namespace Orcus.Administration.ViewModels.AddCondition
{
    public class VersionConditionViewModel : ConditionViewModel<VersionCondition>
    {
        public VersionConditionViewModel()
        {
            Condition.MinimumVersion = 10;
            Condition.MaximumVersion = ApplicationInterface.ClientVersion;
        }

        public override string ConditionName { get; } = (string) Application.Current.Resources["ClientVersion"];

        public override bool Validate(IWindow window)
        {
            if (Condition.MinimumVersion < 0 && Condition.MaximumVersion < 0)
                return false;

            if (Condition.MinimumVersion > Condition.MaximumVersion)
            {
                window.ShowMessageBox((string) Application.Current.Resources["MinimumVersionCantBeGreaterThanMaximum"],
                    (string) Application.Current.Resources["Error"], MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }
    }
}