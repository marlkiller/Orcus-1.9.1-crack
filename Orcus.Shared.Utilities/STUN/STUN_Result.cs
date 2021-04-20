using System.Net;

namespace Orcus.Shared.Utilities.STUN
{
    /// <summary>
    ///     This class holds STUN_Client.Query method return data.
    /// </summary>
    public class STUN_Result
    {
        /// <summary>
        ///     Default constructor.
        /// </summary>
        /// <param name="netType">Specifies UDP network type.</param>
        /// <param name="publicEndPoint">Public IP end point.</param>
        public STUN_Result(STUN_NetType netType, IPEndPoint publicEndPoint)
        {
            NetType = netType;
            PublicEndPoint = publicEndPoint;
        }

        /// <summary>
        ///     Gets UDP network type.
        /// </summary>
        public STUN_NetType NetType { get; }

        /// <summary>
        ///     Gets public IP end point. This value is null if failed to get network type.
        /// </summary>
        public IPEndPoint PublicEndPoint { get; }
    }
}