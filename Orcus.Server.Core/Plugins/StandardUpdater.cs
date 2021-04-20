using System;
using System.Threading;
using System.Web.Script.Serialization;
using NLog;

namespace Orcus.Server.Core.Plugins
{
    public abstract class StandardUpdater<TSettings> : IUpdatePlugin
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private bool _isRunning;
        private Timer _updateTimer;

        public TSettings Settings { get; set; }

        public abstract string Name { get; }
        public abstract string Host { get; }

        public void ServerStarted()
        {
            if (_isRunning)
                return;

            _isRunning = true;

            try
            {
                UpdateDns();
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, "Error when trying to update {0}", Name);
            }

            _updateTimer = new Timer(TimerCallback, null, TimeSpan.FromMinutes(10), Timeout.InfiniteTimeSpan);
        }

        public void Stop()
        {
            _isRunning = false;
            _updateTimer?.Dispose();
            _updateTimer = null;
        }

        public abstract bool SetupConsole();

        public virtual string SaveSettings()
        {
            return new JavaScriptSerializer().Serialize(Settings);
        }

        public virtual void LoadSettings(string settings)
        {
            Settings = new JavaScriptSerializer().Deserialize<TSettings>(settings);
        }

        private void TimerCallback(object state)
        {
            try
            {
                UpdateDns();
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, "Error when trying to update {0}", Name);
            }

            if (_isRunning)
                _updateTimer.Change(TimeSpan.FromMinutes(10), Timeout.InfiniteTimeSpan);
        }

        protected abstract void UpdateDns();
    }
}