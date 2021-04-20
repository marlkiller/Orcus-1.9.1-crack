using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels
{
    public class InputTextViewModel : PropertyChangedBase
    {
        private bool? _dialogResult;
        private RelayCommand _okCommand;

        public InputTextViewModel(string defaultText, string watermark, string affirmerButtonText)
        {
            Watermark = watermark;
            AffirmerButtonText = affirmerButtonText;
            Text = defaultText;
        }

        public bool? DialogResult
        {
            get { return _dialogResult; }
            set { SetProperty(value, ref _dialogResult); }
        }

        public string Watermark { get; }
        public string AffirmerButtonText { get; }
        public string Text { get; set; }

        public RelayCommand OkCommand
        {
            get
            {
                return _okCommand ?? (_okCommand = new RelayCommand(parameter =>
                {
                    if (!string.IsNullOrWhiteSpace(Text))
                        DialogResult = true;
                }));
            }
        }
    }
}