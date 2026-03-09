using log4net.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RevitDevelop.Utils.Loggers
{
    public static class Logger
    {
        private static ILoggingService _log;

        static Logger()
        {
            try
            {
                string configurationPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty, "log4net.config");
                ConfigureSettings(configurationPath);
            }
            catch (Exception arg)
            {
                Console.WriteLine($"Failed to configure logger : {arg}");
            }
        }

        private static void ConfigureSettings(string configurationPath)
        {
            using (FileStream fileStream = new FileStream(configurationPath, FileMode.Open))
            {
                XmlConfigurator.Configure((Stream)fileStream);
            }

            _log = new LoggingService();
        }

        public static void Debug(object message, Exception exception = null)
        {
            _log?.Debug(message, exception);
        }

        public static void DebugFormat(string format, params object[] args)
        {
            _log?.DebugFormat(format, args);
        }

        public static void Info(object message, Exception exception = null)
        {
            _log?.Info(message, exception);
        }

        public static void InfoFormat(string format, params object[] args)
        {
            _log?.InfoFormat(format, args);
        }

        public static void Warn(object message, Exception exception = null)
        {
            _log?.Warn(message, exception);
        }

        public static void WarnFormat(string format, params object[] args)
        {
            _log?.WarnFormat(format, args);
        }

        public static void Error(object message, Exception exception = null)
        {
            _log?.Error(message, exception);
        }

        public static void ErrorFormat(string format, params object[] args)
        {
            _log?.ErrorFormat(format, args);
        }

        public static void Fatal(object message, Exception exception = null)
        {
            _log?.Fatal(message, exception);
        }

        public static void FatalFormat(string format, params object[] args)
        {
            _log?.FatalFormat(format, args);
        }

        public static bool HasConsoleAppender()
        {
            return _log.HasConsoleAppender();
        }
    }
}
