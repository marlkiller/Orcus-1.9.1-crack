using Orcus.Administration.Core;

namespace Orcus.Administration.Views.Licensing.Steps
{
    public class Step3 : View
    {
        private ApplicationTheme _applicationTheme;

        public Step3(Settings settings, LicenseConfig config)
        {
            Settings = settings;

            LicenseConfig = config;
        }

        public ApplicationTheme ApplicationTheme
        {
            get { return _applicationTheme; }
            set
            {
                if (SetProperty(value, ref _applicationTheme))
                    Settings.Theme = value;
            }
        }
    }
}