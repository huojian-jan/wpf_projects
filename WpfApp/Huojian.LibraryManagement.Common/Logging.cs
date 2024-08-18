using log4net.Appender;
using log4net.Repository.Hierarchy;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Huojian.LibraryManagement.Common
{
    public static class Logging
    {
        private static readonly ILog _log = LogManager.GetLogger("default");

        public static void Debug(object message, bool remoteWrite = false)
        {
            LogWriter("debug", message, null, remoteWrite);
            _log.Debug(message);
        }

        public static void Debug(object message, Exception exception, bool remoteWrite = false)
        {
            LogWriter("debug", message, exception, remoteWrite);
            _log.Debug(message, exception);
        }

        public static void Info(object message, bool remoteWrite = false)
        {
            LogWriter("info", message, null, remoteWrite);
            _log.Info(message);
        }

        public static void Info(object message, Exception exception, bool remoteWrite = false)
        {
            LogWriter("info", message, exception, remoteWrite);
            _log.Info(message, exception);
        }

        public static void Warn(object message, bool remoteWrite = false)
        {
            LogWriter("warn", message, null, remoteWrite);
            _log.Warn(message);
        }

        public static void Warn(object message, Exception exception, bool remoteWrite = false)
        {
            LogWriter("warn", message, exception, remoteWrite);
            _log.Warn(message, exception);
        }

        public static void Error(object message, bool remoteWrite = true)
        {
            LogWriter("error", message, null, remoteWrite);
            _log.Error(message);
        }

        public static void Error(object message, Exception exception, bool remoteWrite = true)
        {
            LogWriter("error", message, exception, remoteWrite);
            _log.Error(message, exception);
        }

        public static void Fatal(object message, bool remoteWrite = true)
        {
            LogWriter("fatal", message, null, remoteWrite);
            _log.Fatal(message);
        }

        public static void Fatal(object message, Exception exception, bool remoteWrite = true)
        {
            LogWriter("fatal", message, exception, remoteWrite);
            _log.Fatal(message, exception);
        }

        private static string _loggingPath;

        public static string LoggingPath
        {
            get
            {
                if (_loggingPath == null)
                {
                    var hierarchy = (Hierarchy)LogManager.GetRepository();
                    var appenders = hierarchy.GetAppenders();

                    foreach (var appender in appenders)
                    {
                        if (appender is FileAppender fileAppender)
                        {
                            _loggingPath = fileAppender.File;
                            break;
                        }
                    }
                }

                return _loggingPath;
            }
        }

        private static void LogWriter(string level, object message, Exception exception, bool remoteWrite)
        {
            if (!remoteWrite) return;

            var exceptionStr = string.Empty;
            if (exception != null)
            {
                exceptionStr = exception.ToString();
            }

            //LogHelper.Write(level, message.ToString(), exceptionStr);
        }
    }
}
