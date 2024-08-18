using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ShadowBot.Common.Utilities
{
    public class DelayInvoker : IDisposable
    {
        private readonly object _lock = new object();
        private readonly int _delay;
        private readonly Action _invokeAction;
        private Timer _timer;

        public DelayInvoker(int delay, Action invokeAction)
        {
            _delay = delay;
            _invokeAction = invokeAction;
        }

        public void Invoke()
        {
            lock (_lock)
            {
                if (_timer == null)
                    _timer = new Timer(TimeoutCallback, null, _delay, Timeout.Infinite);
                else
                    _timer.Change(_delay, Timeout.Infinite);
            }
        }

        public void Cancel()
        {
            if (_timer != null)
            {
                lock (_lock)
                {
                    if (_timer != null)
                        _timer.Change(Timeout.Infinite, Timeout.Infinite);
                }
            }
        }

        private void TimeoutCallback(object state)
        {
            lock (_lock)
            {
                if (_timer != null)
                {
                    _timer.Dispose();
                    _timer = null;
                }
            }
            _invokeAction?.Invoke();
        }

        #region IDisposable Support

        private bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    lock (_lock)
                    {
                        if (_timer != null)
                        {
                            _timer.Dispose();
                            _timer = null;
                        }
                    }
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
