using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels
{
    public class InputMultilineTextViewModel : PropertyChangedBase
    {
        private bool _isReadOnly;
        private string _text;

        public InputMultilineTextViewModel()
        {
        }

        public InputMultilineTextViewModel(string text)
        {
            Text = text;
            IsReadOnly = true;
        }

        public bool IsReadOnly
        {
            get { return _isReadOnly; }
            set { SetProperty(value, ref _isReadOnly); }
        }

        public string Text
        {
            get { return _text; }
            set { SetProperty(value, ref _text); }
        }

        private RelayCommand _okCommand;

        public RelayCommand OkCommand
        {
            get
            {
                return _okCommand ?? (_okCommand = new RelayCommand(parameter =>
                {
                    
                }));
            }
        }
    }
}