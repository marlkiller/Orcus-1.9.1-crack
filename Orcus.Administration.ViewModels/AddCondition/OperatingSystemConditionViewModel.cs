using System.Windows;
using Orcus.Administration.ViewModels.ViewInterface;
using Orcus.Shared.DynamicCommands.Conditions;

namespace Orcus.Administration.ViewModels.AddCondition
{
    public class OperatingSystemConditionViewModel : ConditionViewModel<OperatingSystemCondition>
    {
        public override string ConditionName { get; } = (string)Application.Current.Resources["OperatingSystem"];

        public override bool Validate(IWindow window)
        {
            if (Condition.MinimumOsVersion < 0 && Condition.MaximumOsVersion < 0)
                return false;

            if (Condition.MinimumOsVersion > Condition.MaximumOsVersion)
            {
                window.ShowMessageBox((string) Application.Current.Resources["MinimumVersionCantBeGreaterThanMaximum"],
                    (string) Application.Current.Resources["Error"], MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }
    }
}