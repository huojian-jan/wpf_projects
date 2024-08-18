using System;
using System.Diagnostics;

namespace ShadowBot.Common.Utilities
{
    public class NamedStopWatch : IDisposable
    {
        private Stopwatch _stopwatch = null;
        private string _name = string.Empty;
        private bool _remoteWrite = false;

        public NamedStopWatch(string name, bool remoteWrite=false)
        {
            _name = name;
            _remoteWrite = remoteWrite;
            _stopwatch = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            if (_stopwatch != null)
            {
                _stopwatch.Stop();
                Logging.Info($"Watch: {_name} cast : {_stopwatch.ElapsedMilliseconds} ms", remoteWrite: _remoteWrite);
            }
        }
    }
}
