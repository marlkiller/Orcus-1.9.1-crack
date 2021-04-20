using System.Windows;
using Orcus.Shared.DynamicCommands.Conditions;

namespace Orcus.Administration.ViewModels.AddCondition
{
    public class ClientTagConditionViewModel : ConditionViewModel<ClientTagCondition>
    {
        public override string ConditionName { get; } = (string) Application.Current.Resources["ClientTag"];
    }
}