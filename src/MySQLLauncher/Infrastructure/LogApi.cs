using System;
using System.Diagnostics;
using PrismTemplate.Infrastructure.Extensions;

namespace PrismTemplate.Infrastructure
{
    public enum LogLevel
    {
        None = 0,
        Error = 1,
        Warn = 2,
        Info = 3,
        Debug = 4
    }

    public sealed class LogApi
    {
        #region Constructors

        static LogApi()
        {
#if DEBUG
            FilterLevel = LogLevel.Debug;
#endif
        }

        #endregion Constructors

        #region Events

        public static event EventHandler<string> ErrorMessage;

        public static event EventHandler<string> InfoMessage;

        public static event EventHandler<string> WarnMessage;

        #endregion Events

        #region Properties

        public static LogLevel FilterLevel { get; set; } = LogLevel.Info;

        #endregion Properties

        #region Methods

        /// <summary>
        /// </summary>
        /// <param name="content"></param>
        /// <param name="level">1:错误 2：警告 3：信息 4：调试日志</param>
        public static void Log(string content, LogLevel level = LogLevel.Info)
        {
            if (level > FilterLevel)
                return;
            content = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} {content}";
            switch (level)
            {
                case LogLevel.Error:
                    OnErrorMsg(content);
                    break;

                case LogLevel.Warn:
                    OnWarnMsg(content);
                    break;

                case LogLevel.Info:
                    OnInfoMsg(content);
                    break;

                default:
                    Trace.WriteLine(content);
                    break;
            }

#if DEBUG
            if (Enum.TryParse<TraceLevel>(((int)level).ToString(), out var tl))
            {
                LogTrace(content, tl);
            }
#endif
        }

        public static void LogException(Exception ex)
        {
            var msg = $"{ex.Message} {Environment.NewLine} {ex.BuildExLog(true)}";
            Log(msg, LogLevel.Error);
        }

        public static void LogTrace(string content, TraceLevel level = TraceLevel.Info)
        {
            switch (level)
            {
                case TraceLevel.Info:
                    Trace.TraceInformation(content);
                    break;

                case TraceLevel.Verbose:
                    Trace.WriteLine(content);
                    break;

                case TraceLevel.Warning:
                    Trace.TraceWarning(content);
                    break;

                case TraceLevel.Error:
                    Trace.TraceError(content);
                    break;
            }
        }

        private static void OnErrorMsg(string content)
        {
            ErrorMessage?.Invoke(null, content);
        }

        private static void OnInfoMsg(string content)
        {
            InfoMessage?.Invoke(null, content);
        }

        private static void OnWarnMsg(string content)
        {
            WarnMessage?.Invoke(null, content);
        }

        #endregion Methods
    }
}