using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Orcus.Shared.Utilities.STUN;

namespace Orcus.Shared.Utilities
{
    //Taken from https://github.com/pruiz/LumiSoft.Net
    //License:
    /*
     General usage terms:

  *) If you use/redistribute compiled binary, there are no restrictions.
     You can use it in any project, commercial and no-commercial.
     Redistributing compiled binary not limited any way.

  *) It's allowed to complile source code parts to your application,
     but then you may not rename class names and namespaces.

  *) Anything is possible, if special agreement between LumiSoft.


THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR 
PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE 
FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, 
ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
                  */

    public class SessionTraversalUtilitiesForNAT
    {
        public const string GoogleStunServer = "stun.l.google.com";
        public const int GoogleStunServerPort = 19302;

        public static bool IsHolePunchingPossible(Socket socket, out IPEndPoint ipEndPoint)
        {
            var stunTest = TestStun(GoogleStunServer, GoogleStunServerPort, socket);
            ipEndPoint = stunTest.PublicEndPoint;
            return IsHolePunchingPossible(stunTest.NetType);
        }

        public static bool IsHolePunchingPossible(STUN_NetType stunNetType)
        {
            switch (stunNetType)
            {
                case STUN_NetType.Symmetric:
                case STUN_NetType.SymmetricUdpFirewall:
                case STUN_NetType.UdpBlocked:
                    return false;
                default:
                    return true;
            }
        }

        public static STUN_Result TestStun(string host, int port, Socket socket)
        {
            if (string.IsNullOrEmpty(host))
                throw new ArgumentNullException(nameof(host));

            if (socket == null)
                throw new ArgumentNullException(nameof(socket));

            if (port < 1 || port > 65535)
                throw new ArgumentException("Port value must be >= 1 and <= 65535!");

            if (socket.ProtocolType != ProtocolType.Udp)
                throw new ArgumentException("Socket must be UDP socket !");

            /*
    In test I, the client sends a STUN Binding Request to a server, without any flags set in the
    CHANGE-REQUEST attribute, and without the RESPONSE-ADDRESS attribute. This causes the server 
    to send the response back to the address and port that the request came from.

    In test II, the client sends a Binding Request with both the "change IP" and "change port" flags
    from the CHANGE-REQUEST attribute set.  

    In test III, the client sends a Binding Request with only the "change port" flag set.

                        +--------+
                        |  Test  |
                        |   I    |
                        +--------+
                             |
                             |
                             V
                            /\              /\
                         N /  \ Y          /  \ Y             +--------+
          UDP     <-------/Resp\--------->/ IP \------------->|  Test  |
          Blocked         \ ?  /          \Same/              |   II   |
                           \  /            \? /               +--------+
                            \/              \/                    |
                                             | N                  |
                                             |                    V
                                             V                    /\
                                         +--------+  Sym.      N /  \
                                         |  Test  |  UDP    <---/Resp\
                                         |   II   |  Firewall   \ ?  /
                                         +--------+              \  /
                                             |                    \/
                                             V                     |Y
                  /\                         /\                    |
   Symmetric  N  /  \       +--------+   N  /  \                   V
      NAT  <--- / IP \<-----|  Test  |<--- /Resp\               Open
                \Same/      |   I    |     \ ?  /               Internet
                 \? /       +--------+      \  /
                  \/                         \/
                  |                           |Y
                  |                           |
                  |                           V
                  |                           Full
                  |                           Cone
                  V              /\
              +--------+        /  \ Y
              |  Test  |------>/Resp\---->Restricted
              |   III  |       \ ?  /
              +--------+        \  /
                                 \/
                                  |N
                                  |       Port
                                  +------>Restricted
*/

            try
            {
                var remoteEndPoint = new IPEndPoint(Dns.GetHostAddresses(host)[0], port);

                var stunTest1 = new STUN_Message {Type = STUN_MessageType.BindingRequest};

                STUN_Message test1response = DoTransaction(stunTest1, socket, remoteEndPoint, 1600);

                // UDP blocked.
                if (test1response == null)
                    return new STUN_Result(STUN_NetType.UdpBlocked, null);

                var stunTest2 = new STUN_Message
                {
                    Type = STUN_MessageType.BindingRequest,
                    ChangeRequest = new STUN_t_ChangeRequest(true, true)
                };

                // No NAT.
                if (socket.LocalEndPoint.Equals(test1response.MappedAddress))
                {
                    STUN_Message test2Response = DoTransaction(stunTest2, socket, remoteEndPoint, 1600);
                    // Open Internet.
                    if (test2Response != null)
                    {
                        return new STUN_Result(STUN_NetType.OpenInternet, test1response.MappedAddress);
                    }
                    // Symmetric UDP firewall.
                    return new STUN_Result(STUN_NetType.SymmetricUdpFirewall, test1response.MappedAddress);
                }
                // NAT
                else
                {
                    STUN_Message test2Response = DoTransaction(stunTest2, socket, remoteEndPoint, 1600);

                    // Full cone NAT.
                    if (test2Response != null)
                        return new STUN_Result(STUN_NetType.FullCone, test1response.MappedAddress);

                    /*
                            If no response is received, it performs test I again, but this time, does so to 
                            the address and port from the CHANGED-ADDRESS attribute from the response to test I.
                        */

                    // Test I(II)
                    STUN_Message test12 = new STUN_Message();
                    test12.Type = STUN_MessageType.BindingRequest;

                    STUN_Message test12Response = DoTransaction(test12, socket, test1response.ChangedAddress, 1600);
                    if (test12Response == null)
                    {
                        throw new Exception("STUN Test I(II) dind't get resonse !");
                    }
                    // Symmetric NAT
                    if (!test12Response.MappedAddress.Equals(test1response.MappedAddress))
                    {
                        return new STUN_Result(STUN_NetType.Symmetric, test1response.MappedAddress);
                    }
                    // Test III
                    STUN_Message test3 = new STUN_Message();
                    test3.Type = STUN_MessageType.BindingRequest;
                    test3.ChangeRequest = new STUN_t_ChangeRequest(false, true);

                    STUN_Message test3Response = DoTransaction(test3, socket, test1response.ChangedAddress, 1600);
                    // Restricted
                    if (test3Response != null)
                    {
                        return new STUN_Result(STUN_NetType.RestrictedCone, test1response.MappedAddress);
                    }
                    // Port restricted
                    return new STUN_Result(STUN_NetType.PortRestrictedCone, test1response.MappedAddress);
                }
            }
            finally
            {
                // Junk all late responses.
                DateTime startTime = DateTime.Now;
                while (startTime.AddMilliseconds(200) > DateTime.Now)
                {
                    // We got response.
                    if (socket.Poll(1, SelectMode.SelectRead))
                    {
                        byte[] receiveBuffer = new byte[512];
                        socket.Receive(receiveBuffer);
                    }
                }
            }
        }

        /// <summary>
        ///     Does STUN transaction. Returns transaction response or null if transaction failed.
        /// </summary>
        /// <param name="request">STUN message.</param>
        /// <param name="socket">Socket to use for send/receive.</param>
        /// <param name="remoteEndPoint">Remote end point.</param>
        /// <param name="timeout">Timeout in milli seconds.</param>
        /// <returns>Returns transaction response or null if transaction failed.</returns>
        private static STUN_Message DoTransaction(STUN_Message request, Socket socket, IPEndPoint remoteEndPoint,
            int timeout)
        {
            byte[] requestBytes = request.ToByteData();
            DateTime startTime = DateTime.UtcNow;
            // Retransmit with 500 ms.
            while (startTime.AddMilliseconds(timeout) > DateTime.UtcNow)
            {
                try
                {
                    socket.SendTo(requestBytes, remoteEndPoint);

                    // We got response.
                    if (socket.Poll(500*1000, SelectMode.SelectRead))
                    {
                        byte[] receiveBuffer = new byte[512];
                        socket.Receive(receiveBuffer);

                        // Parse message
                        STUN_Message response = new STUN_Message();
                        response.Parse(receiveBuffer);

                        // Check that transaction ID matches or not response what we want.
                        if (request.TransactionID.SequenceEqual(response.TransactionID))
                        {
                            return response;
                        }
                    }
                }
                catch
                {
                    // ignored
                }
            }

            return null;
        }
    }
}