﻿using System;
using Serilog;
using Serilog.Events;
using Logger = Castle.Core.Logging.ILogger;

namespace Castle.Services.Logging.SerilogIntegration
{
    [Serializable]
    public class SerilogLogger : MarshalByRefObject, Logger
    {
        public SerilogLogger(ILogger logger, SerilogFactory factory)
        {
            Logger = logger;
            Factory = factory;
        }

        internal SerilogLogger() { }

        protected internal ILogger Logger { get; set; }

        protected internal SerilogFactory Factory { get; set; }

        public bool IsDebugEnabled
        {
            get { return Logger.IsEnabled(LogEventLevel.Debug); }
        }

        public bool IsErrorEnabled
        {
            get { return Logger.IsEnabled(LogEventLevel.Error); }
        }

        public bool IsFatalEnabled
        {
            get { return Logger.IsEnabled(LogEventLevel.Fatal); }
        }

        public bool IsInfoEnabled
        {
            get { return Logger.IsEnabled(LogEventLevel.Information); }
        }

        public bool IsWarnEnabled
        {
            get { return Logger.IsEnabled(LogEventLevel.Warning); }
        }

        public override string ToString()
        {
            return Logger.ToString();
        }

        public Logger CreateChildLogger(string loggerName)
        {
            // Serilog calls these sub loggers. We might be able to do something here but for now I'm going leave it like this.
            throw new NotImplementedException("Creating child loggers for Serilog is not supported");
        }

        public void Debug(string message, Exception exception)
        {
            if (IsDebugEnabled)
            {
                Logger.Debug(message, exception);
            }
        }

        public void Debug(Func<string> messageFactory)
        {
            if (IsDebugEnabled)
            {
                Logger.Debug(messageFactory.Invoke());
            }
        }

        public void Debug(string message)
        {
            if (IsDebugEnabled)
            {
                Logger.Debug(message);
            }
        }

        public void DebugFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args)
        {
            if (IsDebugEnabled)
            {
                Logger.Debug(exception, string.Format(formatProvider, format, args));
            }
        }

        public void DebugFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            if (IsDebugEnabled)
            {
                Logger.Debug(string.Format(formatProvider, format, args));
            }
        }

        public void DebugFormat(Exception exception, string format, params object[] args)
        {
            if (IsDebugEnabled)
            {
                Logger.Debug(exception, format, args);
            }
        }

        public void DebugFormat(string format, params object[] args)
        {
            if (IsDebugEnabled)
            {
                Logger.Debug(format, args);
            }
        }

        public void Error(string message, Exception exception)
        {
            if (IsErrorEnabled)
            {
                Logger.Error(message, exception);
            }
        }

        public void Error(Func<string> messageFactory)
        {
            if (IsErrorEnabled)
            {
                Logger.Error(messageFactory.Invoke());
            }
        }

        public void Error(string message)
        {
            if (IsErrorEnabled)
            {
                Logger.Error(message);
            }
        }

        public void ErrorFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args)
        {
            if (IsErrorEnabled)
            {
                Logger.Error(exception, string.Format(formatProvider, format, args));
            }
        }

        public void ErrorFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            if (IsErrorEnabled)
            {
                Logger.Error(string.Format(formatProvider, format, args));
            }
        }

        public void ErrorFormat(Exception exception, string format, params object[] args)
        {
            if (IsErrorEnabled)
            {
                Logger.Error(exception, format, args);
            }
        }

        public void ErrorFormat(string format, params object[] args)
        {
            if (IsErrorEnabled)
            {
                Logger.Error(format, args);
            }
        }

        public void Fatal(string message, Exception exception)
        {
            if (IsFatalEnabled)
            {
                Logger.Fatal(message, exception);
            }
        }

        public void Fatal(Func<string> messageFactory)
        {
            if (IsFatalEnabled)
            {
                Logger.Fatal(messageFactory.Invoke());
            }
        }

        public void Fatal(string message)
        {
            if (IsFatalEnabled)
            {
                Logger.Fatal(message);
            }
        }

        public void FatalFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args)
        {
            if (IsFatalEnabled)
            {
                Logger.Fatal(exception, string.Format(formatProvider, format, args));
            }
        }

        public void FatalFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            if (IsFatalEnabled)
            {
                Logger.Fatal(string.Format(formatProvider, format, args));
            }
        }

        public void FatalFormat(Exception exception, string format, params object[] args)
        {
            if (IsFatalEnabled)
            {
                Logger.Fatal(exception, format, args);
            }
        }

        public void FatalFormat(string format, params object[] args)
        {
            if (IsFatalEnabled)
            {
                Logger.Fatal(format, args);
            }
        }

        public void Info(string message, Exception exception)
        {
            if (IsInfoEnabled)
            {
                Logger.Information(message, exception);
            }
        }

        public void Info(Func<string> messageFactory)
        {
            if (IsInfoEnabled)
            {
                Logger.Information(messageFactory.Invoke());
            }
        }

        public void Info(string message)
        {
            if (IsInfoEnabled)
            {
                Logger.Information(message);
            }
        }

        public void InfoFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args)
        {
            if (IsInfoEnabled)
            {
                Logger.Information(exception, string.Format(formatProvider, format, args));
            }
        }

        public void InfoFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            if (IsInfoEnabled)
            {
                Logger.Information(string.Format(formatProvider, format, args));
            }
        }

        public void InfoFormat(Exception exception, string format, params object[] args)
        {
            if (IsInfoEnabled)
            {
                Logger.Information(exception, format, args);
            }
        }

        public void InfoFormat(string format, params object[] args)
        {
            if (IsInfoEnabled)
            {
                Logger.Information(format, args);
            }
        }

        public void Warn(string message, Exception exception)
        {
            if (IsWarnEnabled)
            {
                Logger.Warning(message, exception);
            }
        }

        public void Warn(Func<string> messageFactory)
        {
            if (IsWarnEnabled)
            {
                Logger.Warning(messageFactory.Invoke());
            }
        }

        public void Warn(string message)
        {
            if (IsWarnEnabled)
            {
                Logger.Warning(message);
            }
        }

        public void WarnFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args)
        {
            if (IsWarnEnabled)
            {
                Logger.Warning(exception, string.Format(formatProvider, format, args));
            }
        }

        public void WarnFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            if (IsWarnEnabled)
            {
                Logger.Warning(string.Format(formatProvider, format, args));
            }
        }

        public void WarnFormat(Exception exception, string format, params object[] args)
        {
            if (IsWarnEnabled)
            {
                Logger.Warning(exception, format, args);
            }
        }

        public void WarnFormat(string format, params object[] args)
        {
            if (IsWarnEnabled)
            {
                Logger.Warning(format, args);
            }
        }
    }
}
