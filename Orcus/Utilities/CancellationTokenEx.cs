using System;

namespace Orcus.Utilities
{
    public class CancellationTokenEx
    {
        private readonly Action _cancelAction;
        private readonly object _cancelLock = new object();

        public CancellationTokenEx(Action cancelAction)
        {
            _cancelAction = cancelAction;
        }

        public CancellationTokenEx()
        {
        }

        public bool IsCanceled { get; private set; }

        public void Cancel()
        {
            if (IsCanceled)
                return;

            lock (_cancelLock)
            {
                if (IsCanceled)
                    return;

                _cancelAction?.Invoke();
                IsCanceled = true;
            }
        }
    }
}