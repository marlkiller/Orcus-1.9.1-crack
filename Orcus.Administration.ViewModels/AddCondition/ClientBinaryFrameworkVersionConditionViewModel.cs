using System.Windows;
using Orcus.Shared.DynamicCommands.Conditions;

namespace Orcus.Administration.ViewModels.AddCondition
{
    public class ClientBinaryFrameworkVersionConditionViewModel : ConditionViewModel<ClientBinaryFrameworkCondition>
    {
        public override string ConditionName { get; } =
            (string) Application.Current.Resources["ClientNetFrameworkVersion"];
    }
}