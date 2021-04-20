using System.Windows;
using Orcus.Shared.DynamicCommands.Conditions;

namespace Orcus.Administration.ViewModels.AddCondition
{
    public class PrivilegesConditionViewModel : ConditionViewModel<PrivilegesCondition>
    {
        public override string ConditionName { get; } = (string) Application.Current.Resources["Privileges"];
    }
}