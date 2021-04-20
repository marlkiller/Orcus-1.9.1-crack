using System.Windows;
using Orcus.Shared.DynamicCommands.Conditions;

namespace Orcus.Administration.ViewModels.AddCondition
{
    public class PasswordDataAvailableConditionViewModel : ConditionViewModel<PasswordDataAvailableCondition>
    {
        public override string ConditionName { get; } = (string) Application.Current.Resources["PasswordDataAvailable"];
    }
}