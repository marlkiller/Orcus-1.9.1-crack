using System;

namespace Orcus.Administration.Views.Licensing.Steps
{
    public class Step4 : View
    {
        private string _licenseKey;

        public Step4(LicenseConfig licenseConfig)
        {
            CanGoForward = false;
            LicenseConfig = licenseConfig;
        }

        public string LicenseKey
        {
            get { return _licenseKey; }
            set
            {
                if (SetProperty(value, ref _licenseKey) && value != null)
                {
                    Guid key;
                    CanGoForward = Guid.TryParse(value, out key);
                    if (CanGoForward)
                        LicenseConfig.LicenseKey = key;
                }
            }
        }
    }
}