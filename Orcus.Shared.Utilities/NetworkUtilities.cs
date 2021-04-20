using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Orcus.Shared.Utilities
{
    public static class NetworkUtilities
    {
        public static IPAddress GetLanIp()
        {
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                GatewayIPAddressInformation gatewayAddress = ni.GetIPProperties().GatewayAddresses.FirstOrDefault();
                if (gatewayAddress != null) //exclude virtual physical nic with no default gateway
                {
                    if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 ||
                        ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet &&
                        ni.OperationalStatus == OperationalStatus.Up)
                    {
                        foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                        {
                            if (ip.Address.AddressFamily != AddressFamily.InterNetwork ||
                                ip.AddressPreferredLifetime == uint.MaxValue) // exclude virtual network addresses
                                continue;

                            return ip.Address;
                        }
                    }
                }
            }

            return IPAddress.Any;
        }
    }
}