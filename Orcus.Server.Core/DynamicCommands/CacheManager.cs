using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Orcus.Server.Core.Database;
using Orcus.Shared.DynamicCommands;

namespace Orcus.Server.Core.DynamicCommands
{
    public class CacheManager : IDisposable
    {
        private readonly List<DynamicCommandEvent> _cacheItems;
        private readonly DatabaseManager _databaseManager;
        private readonly object _listLock = new object();
        private bool _isRunning;

        public CacheManager(DatabaseManager databaseManager)
        {
            _databaseManager = databaseManager;
            _cacheItems = new List<DynamicCommandEvent>();
        }

        public void Dispose()
        {
            if (_cacheItems.Count > 0)
                Push();
        }

        public event EventHandler<List<DynamicCommandEvent>> DynamicCommandEventsAdded;

        public void AddCommandEvents(List<DynamicCommandEvent> commandEvents)
        {
            lock (_listLock)
            {
                _cacheItems.AddRange(commandEvents);

                if (_isRunning)
                    return;
            }

            new Thread(Run) { IsBackground = true }.Start();
        }

        public void AddCommandEvent(DynamicCommandEvent commandEvent)
        {
            lock (_listLock)
            {
                _cacheItems.Add(commandEvent);

                if (_isRunning)
                    return;
            }

            new Thread(Run) {IsBackground = true}.Start();
        }

        private void Run()
        {
            if (_isRunning)
                return;

            _isRunning = true;
            Thread.Sleep(1000);
            Push();
        }

        private void Push()
        {
            lock (_listLock)
            {
                _isRunning = false;
                _databaseManager.AddDynamicCommandEvents(_cacheItems);
                if (DynamicCommandEventsAdded != null)
                {
                    var addedEvents = _cacheItems.ToList();
                    new Thread(() => DynamicCommandEventsAdded?.Invoke(this, addedEvents)).Start();
                }

                _cacheItems.Clear();
            }
        }
    }
}