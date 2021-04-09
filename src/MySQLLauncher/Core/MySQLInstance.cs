using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySQLLauncher.Core
{
    public class MySQLInstance : IDisposable
    {
        #region Fields

        private string _dataDir;

        #endregion Fields

        #region Constructors

        public MySQLInstance(string instancePath, string iniPath, Process shellProc, Process coreProc)
        {
            InstancePath = instancePath;
            InstanceIniPath = iniPath;

            ShellProcess = shellProc;
            CoreProcess = coreProc;
        }

        #endregion Constructors

        #region Properties

        public Process CoreProcess { get; private set; }

        public string DataDir
        {
            get => _dataDir;
            set => _dataDir = value;
        }

        public string InstanceIniPath { get; private set; }

        public string InstancePath { get; private set; }

        public Process ShellProcess { get; private set; }

        public MySQLInstanceStatus Status { get; private set; }

        #endregion Properties

        #region Methods

        public void Dispose()
        {
            CoreProcess?.Close();
        }

        public override bool Equals(object obj)
        {
            if (obj is MySQLInstance instance)
            {
                if (instance.InstanceIniPath != InstanceIniPath)
                    return false;

                if (instance.CoreProcess.Id != CoreProcess.Id)
                    return false;

                if (instance.ShellProcess.Id != ShellProcess.Id)
                    return false;

                if (instance.InstancePath != InstancePath)
                    return false;

                return true;
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public void Kill()
        {
            CoreProcess.Kill();
        }

        public override string ToString()
        {
            return $"MySQL Instance [ Shell: {ShellProcess.Id}, Core:{CoreProcess.Id} ]";
        }

        #endregion Methods
    }
}