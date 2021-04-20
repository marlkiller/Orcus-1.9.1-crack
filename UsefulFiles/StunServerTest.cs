        static void Main(string[] args)
        {
            var list = @"stun.services.mozilla.com:3478
stunserver.org
stun.stunprotocol.org:3478
stun.l.google.com:19302
stun1.l.google.com:19302
stun2.l.google.com:19302
stun3.l.google.com:19302
stun4.l.google.com:19302
stun01.sipphone.com
stun.ekiga.net
stun.fwdnet.net
stun.ideasip.com
stun.iptel.org
stun.rixtelecom.se
stun.schlund.de
stunserver.org
stun.softjoys.com
stun.voiparound.com
stun.voipbuster.com
stun.voipstunt.com
stun.voxgratia.org
stun.xten.com".Split(new[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries).ToList();
            var socket = new Socket(SocketType.Dgram, ProtocolType.Udp);
            var ipad = GetLanIp();
            socket.Bind(new IPEndPoint(ipad, 3478));
            foreach (var s in list)
            {
                var parts = s.Split(':');
                int port;
                string hostname;
                if (parts.Length == 1)
                {
                    hostname = s;
                    port = 3478;
                }
                else
                {
                    hostname = parts[0];
                    port = int.Parse(parts[1]);
                }
                Console.WriteLine("Test " + hostname);
                try
                {
                    var sw = Stopwatch.StartNew();
                    var testResult = TestStun(hostname, port, socket);
                    Console.WriteLine("==> " + testResult?.NetType + " (" + sw.ElapsedMilliseconds + " ms)");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("==> " + ex.Message);
                }
                Console.WriteLine();
            }
            
            Console.WriteLine("Finished testing hosts");
            Console.ReadKey();
        }