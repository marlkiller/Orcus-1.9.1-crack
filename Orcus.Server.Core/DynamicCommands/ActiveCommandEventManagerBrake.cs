using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Orcus.Server.Core.DynamicCommands
{
    public class ActiveCommandEventManagerBrake
    {
        private readonly List<ActiveCommandEvent> _activeCommandEvents;
        private readonly object _listLock = new object();
        private readonly Timer _waitTimer;
        private bool _isTimerRunning;
        private readonly TimeSpan _pushFrequency = TimeSpan.FromMilliseconds(300);

        public ActiveCommandEventManagerBrake(ActiveCommandEventManager activeCommandEventManager)
        {
            _activeCommandEvents = new List<ActiveCommandEvent>();

            activeCommandEventManager.ActiveCommandAdded += ActiveCommandEventManagerOnActiveCommandAdded;
            activeCommandEventManager.ActiveCommandRemoved += ActiveCommandEventManagerOnActiveCommandRemoved;
            activeCommandEventManager.ClientAdded += ActiveCommandEventManagerOnClientsUpdated;
            activeCommandEventManager.ClientRemoved += ActiveCommandEventManagerOnClientsUpdated;

            _waitTimer = new Timer(TimerCallback, null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        }

        public event EventHandler<List<ActiveCommandEvent>> PushChanges;

        private void TimerCallback(object state)
        {
            lock (_listLock)
            {
                PushChanges?.Invoke(this, _activeCommandEvents.ToList());
                _activeCommandEvents.Clear();
                _isTimerRunning = false;
            }
        }

        private void ActiveCommandEventManagerOnClientsUpdated(object sender,
            ActiveCommandClientEventArgs activeCommandClientEventArgs)
        {
            lock (_listLock)
            {
                for (int i = _activeCommandEvents.Count - 1; i >= 0; i--)
                {
                    var activeCommandEvent = _activeCommandEvents[i];
                    if (activeCommandEvent.ActiveCommandInfo.DynamicCommand.Id ==
                        activeCommandClientEventArgs.ActiveCommandInfo.DynamicCommand.Id)
                    {
                        //if there is already an updated queued, we just return
                        if (activeCommandEvent.ActiveCommandEventType == ActiveCommandEventType.Updated)
                            return;
                    }
                }

                _activeCommandEvents.Add(new ActiveCommandEvent
                {
                    ActiveCommandInfo = activeCommandClientEventArgs.ActiveCommandInfo,
                    ActiveCommandEventType = ActiveCommandEventType.Updated
                });

                if (!_isTimerRunning)
                {
                    _isTimerRunning = true;
                    _waitTimer.Change(_pushFrequency, Timeout.InfiniteTimeSpan);
                }
            }
        }

        private void ActiveCommandEventManagerOnActiveCommandRemoved(object sender, ActiveCommandInfo activeCommandInfo)
        {
            lock (_listLock)
            {
                for (int i = _activeCommandEvents.Count - 1; i >= 0; i--)
                {
                    var activeCommandEvent = _activeCommandEvents[i];
                    if (activeCommandEvent.ActiveCommandInfo.DynamicCommand.Id == activeCommandInfo.DynamicCommand.Id)
                    {
                        if (activeCommandEvent.ActiveCommandEventType == ActiveCommandEventType.Added)
                            return;

                        _activeCommandEvents.Remove(activeCommandEvent);
                    }
                }

                _activeCommandEvents.Add(new ActiveCommandEvent
                {
                    ActiveCommandInfo = activeCommandInfo,
                    ActiveCommandEventType = ActiveCommandEventType.Removed
                });

                if (!_isTimerRunning)
                {
                    _isTimerRunning = true;
                    _waitTimer.Change(_pushFrequency, Timeout.InfiniteTimeSpan);
                }
            }
        }

        private void ActiveCommandEventManagerOnActiveCommandAdded(object sender, ActiveCommandInfo activeCommandInfo)
        {
            lock (_listLock)
            {
                //remove all previous event
                for (int i = _activeCommandEvents.Count - 1; i >= 0; i--)
                {
                    var activeCommandEvent = _activeCommandEvents[i];
                    if (activeCommandEvent.ActiveCommandInfo.DynamicCommand.Id == activeCommandInfo.DynamicCommand.Id)
                        _activeCommandEvents.Remove(activeCommandEvent);
                }

                _activeCommandEvents.Add(new ActiveCommandEvent
                {
                    ActiveCommandInfo = activeCommandInfo,
                    ActiveCommandEventType = ActiveCommandEventType.Added
                });

                if (!_isTimerRunning)
                {
                    _isTimerRunning = true;
                    _waitTimer.Change(_pushFrequency, Timeout.InfiniteTimeSpan);
                }
            }
        }
    }

    public class ActiveCommandEvent
    {
        public ActiveCommandInfo ActiveCommandInfo { get; set; }
        public ActiveCommandEventType ActiveCommandEventType { get; set; }
    }

    public enum ActiveCommandEventType
    {
        Added,
        Removed,
        Updated
    }
}