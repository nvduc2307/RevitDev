using System;
using System.Linq;
using System.Reflection;
using log4net;
using log4net.Appender;
using log4net.Core;

namespace RevitDevelop.Utils.Loggers
{
    public class LoggingService : ILoggingService
    {
        private static ILog _log;

        public LoggingService()
        {
            _log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
        }

        public void Debug(object message, Exception exception = null)
        {
            if (_log.IsDebugEnabled)
            {
                _log.Debug(message, exception);
            }
        }

        public void DebugFormat(string format, params object[] args)
        {
            if (_log.IsDebugEnabled)
            {
                _log.DebugFormat(format, args);
            }
        }

        public void Info(object message, Exception exception = null)
        {
            if (_log.IsInfoEnabled)
            {
                _log.Info(message, exception);
            }
        }

        public void InfoFormat(string format, params object[] args)
        {
            if (_log.IsInfoEnabled)
            {
                _log.InfoFormat(format, args);
            }
        }

        public void Warn(object message, Exception exception = null)
        {
            if (_log.IsWarnEnabled)
            {
                _log.Warn(message, exception);
            }
        }

        public void WarnFormat(string format, params object[] args)
        {
            if (_log.IsWarnEnabled)
            {
                _log.WarnFormat(format, args);
            }
        }

        public void Error(object message, Exception exception = null)
        {
            if (_log.IsErrorEnabled)
            {
                _log.Error(message, exception);
            }
        }

        public void ErrorFormat(string format, params object[] args)
        {
            if (_log.IsErrorEnabled)
            {
                _log.ErrorFormat(format, args);
            }
        }

        public void Fatal(object message, Exception exception = null)
        {
            if (_log.IsFatalEnabled)
            {
                _log.Fatal(message, exception);
            }
        }

        public void FatalFormat(string format, params object[] args)
        {
            if (_log.IsFatalEnabled)
            {
                _log.FatalFormat(format, args);
            }
        }

        public bool HasConsoleAppender()
        {
            IAppender[] appenders = ((ILoggerWrapper)_log).Logger.Repository.GetAppenders();
            return appenders.Any((IAppender x) => x.Name.Contains("Console"));
        }
    }
}
