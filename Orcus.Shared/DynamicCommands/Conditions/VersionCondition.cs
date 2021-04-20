using System;
using Orcus.Shared.Client;
using Orcus.Shared.Connection;

namespace Orcus.Shared.DynamicCommands.Conditions
{
    /// <summary>
    ///     Condition which filters the clients by their version
    /// </summary>
    [Serializable]
    public class VersionCondition : Condition
    {
        public int MinimumVersion { get; set; }
        public int MaximumVersion { get; set; }

        public override string DisplayString => $"{MaximumVersion} ≥ Client Version ≥ {MinimumVersion}";
        public override string ConditionType => "Version";

        public override bool IsTrue(ClientInformation clientInformation, ClientConfig clientConfig)
        {
            var onlineClientInformation = clientInformation as OnlineClientInformation;

            if (onlineClientInformation == null)
                return false;

            return onlineClientInformation.Version <= MaximumVersion &&
                   onlineClientInformation.Version >= MinimumVersion;
        }
    }
}