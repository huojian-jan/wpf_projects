using log4net;
using log4net.Config;
using log4net.Repository.Hierarchy;

namespace log4net_setup
{
    public static class Logging
    {
        private static readonly ILog _log = LogManager.GetLogger("default");

        static Logging()
        {
        }
        public static void Debug(object message, Exception exception=null)
        {
            _log.Debug(message, exception);

            var hierarchy = (Hierarchy)LogManager.GetRepository();
            var appenders = hierarchy.GetAppenders();
            foreach (var appender in appenders)
            {
                
            }
        }

        public static void Info(object message)
        {
            _log.Info(message);
        }

        public static void Warn(object message)
        {
            _log.Warn(message);
        }

        public static void Error(object message, Exception exception=null)
        {
            _log.Error(message, exception);
        }

        public static void Fatal(object message, Exception exception=null)
        {
            _log.Fatal(message, exception);
        }

        public static void Trace(object message,Exception exception=null)
        {
            _log.Fatal(message,exception);
        }
    }
}
