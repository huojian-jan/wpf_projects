using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ShadowBot.Common.Utilities
{
    public sealed class UiDispatcher
    {
        private static SynchronizationContext _context;

        public static void Initialize()
        {
            _context = SynchronizationContext.Current;
        }

        public static void Send(Action action)
        {
            _context.Send(new SendOrPostCallback(_ => action()), null);
        }

        public static void Post(Action action)
        {
            _context.Post(new SendOrPostCallback(_ => action()), null);
        }
    }
}
