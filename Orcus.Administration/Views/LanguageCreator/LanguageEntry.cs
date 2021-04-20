using System.ComponentModel;

namespace Orcus.Administration.Views.LanguageCreator
{
    public class LanguageEntry : INotifyPropertyChanged
    {
        private string _value;

        public string Key { get; set; }
        public string EnglishWord { get; set; }
        public string GermanWord { get; set; }

        public string Value
        {
            get { return _value; }
            set
            {
                if (value == _value) return;
                _value = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Value"));
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
    }
}