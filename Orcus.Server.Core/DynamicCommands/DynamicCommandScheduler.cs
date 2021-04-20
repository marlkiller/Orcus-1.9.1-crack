using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using NLog;
using Orcus.Shared.DynamicCommands;
using Orcus.Shared.DynamicCommands.TransmissionEvents;

namespace Orcus.Server.Core.DynamicCommands
{
    internal class DynamicCommandScheduler : IDisposable
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly object _dictionaryLock = new object();
        private readonly Timer _waiter;

        public DynamicCommandScheduler(IEnumerable<RegisteredDynamicCommand> registeredDynamicCommands)
        {
            DynamicCommands =
                registeredDynamicCommands.Where(
                    x =>
                        x.TransmissionEvent.GetType() == typeof (DateTimeTransmissionEvent) ||
                        x.TransmissionEvent.GetType() == typeof (RepeatingTransmissionEvent))
                    .ToDictionary(x => x, y => GetNextTime(y.TransmissionEvent));

            _waiter = new Timer();
            _waiter.Elapsed += WaiterOnElapsed;
        }

        public void Activate()
        {
            CheckCommands();
            Invalidate();
        }

        public Dictionary<RegisteredDynamicCommand, DateTime> DynamicCommands { get; set; }

        public event EventHandler<ExecuteDynamicCommandEventArgs> ExecuteDynamicCommand;

        public void AddDynamicCommand(RegisteredDynamicCommand registeredDynamicCommand)
        {
            lock (_dictionaryLock)
            {
                DynamicCommands.Add(registeredDynamicCommand, GetNextTime(registeredDynamicCommand.TransmissionEvent));
                Invalidate();
            }
        }

        public void RemoveDynamicCommand(RegisteredDynamicCommand dynamicCommand)
        {
            if (!DynamicCommands.ContainsKey(dynamicCommand))
                return;

            Logger.Debug("Remove dynamic command {0} from scheduler", dynamicCommand.Id);

            lock (_dictionaryLock)
                DynamicCommands.Remove(dynamicCommand);

            Invalidate();
        }

        private void Invalidate()
        {
            Logger.Debug("Invalidate dynamic command scheduler");

            if (DynamicCommands.Count == 0)
                return;

            _waiter.Stop();
            var timeUntilNextCommand = (DynamicCommands.Select(x => x.Value).Min() - DateTime.UtcNow).TotalMilliseconds;

            Logger.Debug("Next command will be executed in {0} min.", timeUntilNextCommand / 60000);

            _waiter.Interval = timeUntilNextCommand;
            _waiter.Start();
        }

        private void WaiterOnElapsed(object sender, ElapsedEventArgs e)
        {
            CheckCommands();
            Invalidate();
        }

        private void CheckCommands()
        {
            lock (_dictionaryLock)
            {
                var commandsToExecute = DynamicCommands.Where(x => (x.Value - DateTime.UtcNow).TotalSeconds < 0).ToList();
                Logger.Debug("{0} commands need to be executed now", commandsToExecute.Count);

                foreach (var pair in commandsToExecute)
                {
                    ExecuteDynamicCommand?.Invoke(this, new ExecuteDynamicCommandEventArgs(pair.Key));
                    DynamicCommands.Remove(pair.Key);
                    if (pair.Key.TransmissionEvent.GetType() == typeof(RepeatingTransmissionEvent))
                        DynamicCommands.Add(pair.Key, GetNextTime(pair.Key.TransmissionEvent));
                }
            }
        }

        private DateTime GetNextTime(TransmissionEvent transmissionEvent)
        {
            var dateTimeEvent = transmissionEvent as DateTimeTransmissionEvent;
            if (dateTimeEvent != null)
                return dateTimeEvent.DateTime;

            var repeatingEvent = transmissionEvent as RepeatingTransmissionEvent;
            if (repeatingEvent != null)
            {
                return
                    repeatingEvent.DayZero.AddMilliseconds(repeatingEvent.TimeSpan.TotalMilliseconds*
                                                           Math.Ceiling(
                                                               (DateTime.UtcNow - repeatingEvent.DayZero).TotalMilliseconds/
                                                               repeatingEvent.TimeSpan.TotalMilliseconds));
            }

            throw new InvalidOperationException(nameof(transmissionEvent));
        }

        public void Dispose()
        {
            _waiter.Stop();
            _waiter.Dispose();
        }
    }
}