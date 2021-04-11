using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PrismTemplate.Infrastructure;

namespace MySQLLauncher.Servies
{
    public class LogService
    {
        #region Constructors

        public LogService()
        {
            LogDir = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), "Log");

            LogApi.InfoMessage += LogApi_InfoMessage;
            LogApi.WarnMessage += LogApi_WarnMessage;
            LogApi.ErrorMessage += LogApi_ErrorMessage;
        }

        #endregion Constructors

        #region Events

        public event EventHandler<string> LogContentChanged;

        #endregion Events

        #region Properties

        public string LogDir { get; private set; }

        #endregion Properties

        #region Methods

        public void Log(string content, string dstModule = "Launcher")
        {
            LogContentChanged?.Invoke(this, content);
            Directory.CreateDirectory(LogDir);
            var file = $"{dstModule}.{DateTime.Now:yyyy-MM-dd}.log";
            var logFile = Path.Combine(LogDir, file);
            File.AppendAllText(logFile, content + Environment.NewLine);
        }

        private void LogApi_ErrorMessage(object sender, string e)
        {
            Log(e, "error");
        }

        private void LogApi_InfoMessage(object sender, string e)
        {
            Log(e);
        }

        private void LogApi_WarnMessage(object sender, string e)
        {
            Log(e);
        }

        #endregion Methods
    }
}