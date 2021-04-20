using Orcus.Administration.Resources;

namespace Orcus.Administration.ViewModels.About
{
    public class MITLicenseInfo : LicenseInfo
    {
        public MITLicenseInfo(string copyright)
        {
            Text = string.Format(Licenses.MIT,
                string.IsNullOrEmpty(copyright) ? "Copyright (c) <year> <copyright holders>" : copyright);
            Name = "MIT";
        }
    }
}