using Orcus.Administration.Resources;

namespace Orcus.Administration.ViewModels.About
{
    public class LicenseInfo
    {
        public string Text { get; set; }
        public string Name { get; set; }

        public static LicenseInfo Apache2 { get; } = new LicenseInfo
        {
            Name = "Apache 2.0",
            Text = Licenses.Apache2_0
        };

        public static LicenseInfo CreateCommonsLicense3 { get; } = new LicenseInfo
        {
            Name = "CC Attribution 3.0",
            Text = Licenses.CC3
        };

        public static LicenseInfo CreateCommonsLicenseSa3 { get; } = new LicenseInfo
        {
            Name = "CC Attribution-ShareAlike 3.0",
            Text = Licenses.CC3SA
        };

        // ReSharper disable once InconsistentNaming
        public static LicenseInfo MSPL { get; } = new LicenseInfo
        {
            Name = "Ms-PL",
            Text = Licenses.MS_PL
        };

        // ReSharper disable once InconsistentNaming
        public static LicenseInfo MPL2 { get; } = new LicenseInfo
        {
            Name = "MPL-2.0",
            Text = Licenses.MPL2
        };

        // ReSharper disable once InconsistentNaming
        public static LicenseInfo CPOL { get; } = new LicenseInfo
        {
            Name = "CPOL",
            Text = Licenses.CPOL
        };

        // ReSharper disable once InconsistentNaming
        public static LicenseInfo LGPL { get; } = new LicenseInfo
        {
            Name = "LGPL",
            Text = Licenses.LGPL
        };

        // ReSharper disable once InconsistentNaming
        public static LicenseInfo MPL1_1 { get; } = new LicenseInfo
        {
            Name = "MPL-1.1",
            Text = Licenses.MPL1_1
        };
    }
}