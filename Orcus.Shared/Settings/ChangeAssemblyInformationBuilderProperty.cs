using System;
using Orcus.Shared.Core;

namespace Orcus.Shared.Settings
{
    [Serializable]
    public class ChangeAssemblyInformationBuilderProperty : IBuilderProperty
    {
        public bool ChangeAssemblyInformation { get; set; }
        public string AssemblyTitle { get; set; }
        public string AssemblyDescription { get; set; }
        public string AssemblyCompanyName { get; set; }
        public string AssemblyProductName { get; set; }
        public string AssemblyCopyright { get; set; }
        public string AssemblyTrademarks { get; set; }
        public string AssemblyProductVersion { get; set; } = "1.0.0.0";
        public string AssemblyFileVersion { get; set; } = "1.0.0.0";

        public IBuilderProperty Clone()
        {
            return new ChangeAssemblyInformationBuilderProperty
            {
                ChangeAssemblyInformation = ChangeAssemblyInformation,
                AssemblyTitle = AssemblyTitle,
                AssemblyDescription = AssemblyDescription,
                AssemblyCompanyName = AssemblyCompanyName,
                AssemblyProductName = AssemblyProductName,
                AssemblyCopyright = AssemblyCopyright,
                AssemblyTrademarks = AssemblyTrademarks,
                AssemblyProductVersion = AssemblyProductVersion,
                AssemblyFileVersion = AssemblyFileVersion
            };
        }
    }
}