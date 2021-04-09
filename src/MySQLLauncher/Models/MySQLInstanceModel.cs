using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySQLLauncher.Core;
using MySQLLauncher.Infrastructure;

namespace MySQLLauncher.Models
{
    public class MySQLInstanceModel : ModelBase
    {
        #region Fields

        private int _coreProcessId = 0;
        private string _dataDir;

        private int _shellProcessId = 0;

        private MySQLInstanceStatus _status = MySQLInstanceStatus.None;

        #endregion Fields

        #region Constructors

        public MySQLInstanceModel(string instancePath, string iniPath, string dataDir, int shellProcId, int coreProcId)
        {
            InstancePath = instancePath;
            InstanceIniPath = iniPath;
            DataDir = dataDir;

            ShellProcessId = shellProcId;
            CoreProcessId = coreProcId;
        }

        #endregion Constructors

        #region Properties

        public int CoreProcessId
        {
            get => _coreProcessId;
            set => SetProperty(ref _coreProcessId, value);
        }

        public string DataDir
        {
            get => _dataDir;
            private set => SetProperty(ref _dataDir, value);
        }

        public string InstanceIniPath { get; private set; }

        public string InstancePath { get; private set; }

        public int ShellProcessId
        {
            get => _shellProcessId;
            set => SetProperty(ref _shellProcessId, value);
        }

        public MySQLInstanceStatus Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        #endregion Properties

        #region Methods

        internal void Clear()
        {
            InstancePath = "";
            InstanceIniPath = "";
            ShellProcessId = 0;
            CoreProcessId = 0;
            DataDir = "";
            Status = MySQLInstanceStatus.None;
        }

        #endregion Methods
    }
}