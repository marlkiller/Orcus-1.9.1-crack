using Orcus.Administration.Core;

namespace Orcus.Administration.Views.Licensing.Steps
{
    public class Step1 : View
    {
        private LanguageInfo _selectedLanguage;

        public Step1(Settings settings, LicenseConfig config)
        {
            Settings = settings;
            _selectedLanguage = Settings.Language;
            LicenseConfig = config;
        }

        public LanguageInfo SelectedLanguage
        {
            get { return _selectedLanguage; }
            set
            {
                if (SetProperty(value, ref _selectedLanguage))
                {
                    Settings.Language = value;
                    Settings.Language.Load();
                }
            }
        }
    }
}