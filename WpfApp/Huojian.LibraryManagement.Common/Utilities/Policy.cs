using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShadowBot.Common.Utilities
{
    // TODO: 考虑迁移实现到https://github.com/App-vNext/Polly
    public static class Policy
    {
        public static bool Retry(Func<bool> func, int timeout, int times = -1, int interval = 200, int delay = 0)
        {
            if (delay > 0)
                System.Threading.Thread.Sleep(delay);
            var sw = Stopwatch.StartNew();
            do
            {
                if (func())
                    return true;
                if (times > 0 && --times == 0) break; // 默认times -1表示忽略重试次数，直至超时
                System.Threading.Thread.Sleep(interval);
            } while (sw.ElapsedMilliseconds < timeout);
            sw.Stop();
            return false;
        }
    }
}
