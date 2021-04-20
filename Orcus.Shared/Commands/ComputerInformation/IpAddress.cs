using System;
using System.Net.Sockets;

namespace Orcus.Shared.Commands.ComputerInformation
{
    [Serializable]
    public class IpAddress
    {
        public AddressFamily AddressFamily { get; set; }
        public string Value { get; set; }
    }
}