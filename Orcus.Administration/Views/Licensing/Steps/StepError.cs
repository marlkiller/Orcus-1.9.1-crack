using Orcus.Administration.Licensing;

namespace Orcus.Administration.Views.Licensing.Steps
{
    public class StepError : View
    {
        public StepError(LicenseRequestResult licenseRequestResult)
        {
            LicenseRequestResult = licenseRequestResult;
        }

        public LicenseRequestResult LicenseRequestResult { get; }
    }
}