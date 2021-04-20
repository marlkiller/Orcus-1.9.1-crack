using System.Net;
using System.Net.Sockets;

namespace Orcus.Server.Core.Connection
{
    public class BlockedIpAddresses
    {
        private readonly byte[] _lowerBytes;
        private readonly byte[] _upperBytes;

        private BlockedIpAddresses(byte[] lowerBytes, byte[] upperBytes)
        {
            _lowerBytes = lowerBytes;
            _upperBytes = upperBytes;
        }

        //Source: http://stackoverflow.com/questions/4172677/c-enumerate-ip-addresses-in-a-range
        public static bool TryParseIpAddressRange(string ipRange, out BlockedIpAddresses blockedIpAddresses)
        {
            blockedIpAddresses = null;

            var ipParts = ipRange.Split('.');

            var beginIp = new byte[4];
            var endIp = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                string[] rangeParts = ipParts[i].Split('-');

                if (rangeParts.Length < 1 || rangeParts.Length > 2)
                    return false;

                if (!byte.TryParse(rangeParts[0], out beginIp[i]))
                    return false;

                endIp[i] = (rangeParts.Length == 1) ? beginIp[i] : byte.Parse(rangeParts[1]);
            }

            blockedIpAddresses = new BlockedIpAddresses(beginIp, endIp);
            return true;
        }

        //Source: http://stackoverflow.com/questions/4172677/c-enumerate-ip-addresses-in-a-range
        public static bool TryParseCIDRNotation(string ipRange, out BlockedIpAddresses blockedIpAddresses)
        {
            blockedIpAddresses = null;

            string[] x = ipRange.Split('/');

            if (x.Length != 2)
                return false;

            byte bits = byte.Parse(x[1]);
            uint ip = 0;
            string[] ipParts0 = x[0].Split('.');
            for (int i = 0; i < 4; i++)
            {
                ip = ip << 8;
                ip += uint.Parse(ipParts0[i]);
            }

            byte shiftBits = (byte) (32 - bits);
            uint ip1 = (ip >> shiftBits) << shiftBits;

            if (ip1 != ip) // Check correct subnet address
                return false;

            uint ip2 = ip1 >> shiftBits;
            for (int k = 0; k < shiftBits; k++)
            {
                ip2 = (ip2 << 1) + 1;
            }

            var beginIp = new byte[4];
            var endIp = new byte[4];

            for (int i = 0; i < 4; i++)
            {
                beginIp[i] = (byte) ((ip1 >> (3 - i)*8) & 255);
                endIp[i] = (byte) ((ip2 >> (3 - i)*8) & 255);
            }

            blockedIpAddresses = new BlockedIpAddresses(beginIp, endIp);
            return true;
        }

        //Source: http://stackoverflow.com/questions/2138706/how-to-check-a-input-ip-fall-in-a-specific-ip-range
        public bool IsBlocked(IPAddress address)
        {
            if (address.AddressFamily != AddressFamily.InterNetwork)
                return false;

            byte[] addressBytes = address.GetAddressBytes();

            bool lowerBoundary = true, upperBoundary = true;

            for (int i = 0;
                i < _lowerBytes.Length &&
                (lowerBoundary || upperBoundary);
                i++)
            {
                if ((lowerBoundary && addressBytes[i] < _lowerBytes[i]) ||
                    (upperBoundary && addressBytes[i] > _upperBytes[i]))
                {
                    return false;
                }

                lowerBoundary &= (addressBytes[i] == _lowerBytes[i]);
                upperBoundary &= (addressBytes[i] == _upperBytes[i]);
            }

            return true;
        }
    }
}