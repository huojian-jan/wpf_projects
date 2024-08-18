namespace Huojian.LibraryManagement.Common.Utilities
{
    public class DispatcherDelayInvoker
    {
        private int _delay;
        private readonly Action _invokeAction;
        private DispatcherTimer _timer;
        Dispatcher _dispatcher;

        public DispatcherDelayInvoker(int delay, Action invokeAction, Dispatcher dispatcher)
        {
            _delay = delay;
            _invokeAction = invokeAction;
            _dispatcher = dispatcher;
        }

        public DispatcherDelayInvoker(int delay, Action invokeAction)
        {
            _delay = delay;
            _invokeAction = invokeAction;
            _dispatcher = Dispatcher.CurrentDispatcher;
        }

        public void Invoke()
        {
            if (_timer == null)
                _timer = new DispatcherTimer(new TimeSpan(0, 0, 0, 0, _delay), DispatcherPriority.Normal, TimeoutCallback, _dispatcher);
            _timer.Stop();
            _timer.Start();
        }

        public void Cancel()
        {
            if (_timer == null)
            {
                return;
            }
            _timer.Stop();
            _timer = null;
        }

        private void TimeoutCallback(object _, EventArgs __)
        {
            try
            {
                _timer.Stop();
                _timer = null;
                _invokeAction?.Invoke();
            }
            catch (Exception ex)
            {
                Logging.Error(ex);
            }
        }
    }
}
