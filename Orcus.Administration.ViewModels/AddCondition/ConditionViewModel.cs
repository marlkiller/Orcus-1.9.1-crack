using Orcus.Administration.ViewModels.ViewInterface;
using Orcus.Shared.DynamicCommands;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels.AddCondition
{
    public abstract class ConditionViewModel<TCondition> : PropertyChangedBase, IConditionViewModel
        where TCondition : Condition, new()
    {
        private TCondition _condition;

        protected ConditionViewModel()
        {
            Condition = new TCondition();
        }

        public TCondition Condition
        {
            get { return _condition; }
            set { SetProperty(value, ref _condition); }
        }

        Condition IConditionViewModel.Condition => Condition;
        public abstract string ConditionName { get; }

        public void Initialize(Condition condition)
        {
            Condition = (TCondition) condition;
            Initialize(Condition);
        }

        public virtual bool Validate(IWindow window)
        {
            return true;
        }

        protected virtual void Initialize(TCondition condition)
        {
        }
    }
}