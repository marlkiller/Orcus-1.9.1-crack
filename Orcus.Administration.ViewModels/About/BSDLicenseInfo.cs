using Orcus.Administration.Resources;

namespace Orcus.Administration.ViewModels.About
{
    public class BSDLicenseInfo : LicenseInfo
    {
        public BSDLicenseInfo(string copyright)
        {
            Text = string.Format(Licenses.BSD3,
                string.IsNullOrEmpty(copyright) ? "Copyright (c) <year> <copyright holders>" : copyright);
            Name = "BSD 3-Clause";
        }
    }
}