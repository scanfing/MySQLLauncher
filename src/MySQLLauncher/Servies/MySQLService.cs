using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MySQLLauncher.Core;
using MySQLLauncher.Models;
using MySQLLauncher.Utilities;
using PrismTemplate.Infrastructure;

namespace MySQLLauncher.Servies
{
    public class MySQLService
    {
        #region Fields

        private ConcurrentBag<MySQLInstance> _MySQLInstaces;

        private CancellationTokenSource _watcherCTS;

        #endregion Fields

        #region Constructors

        public MySQLService()
        {
            _MySQLInstaces = new ConcurrentBag<MySQLInstance>();
        }

        #endregion Constructors

        #region Events

        public event EventHandler<MySQLInstance> InstanceLost;

        public event EventHandler<MySQLInstance> NewInstaceFound;

        #endregion Events

        #region Properties

        public string DefaultMySQLExeName { get; private set; } = "mysqld.exe";
        public string DefaultMySQLProcessName { get; private set; } = "mysqld";

        public bool IsRunning => _watcherCTS != null;

        #endregion Properties

        #region Methods

        public string InitMySQLDataDir(string insPath, string iniPath, bool insecure = false)
        {
            LogApi.Log($"Init MySQL Data Dir , --{iniPath}");

            var dataDir = AnalyzeMySQLDataDirFromIni(iniPath);
            if (Directory.GetFiles(dataDir).Length > 0 || Directory.GetDirectories(dataDir).Length > 0)
            {
                LogApi.Log($"Init Failed，MySQL Data Dir : {dataDir} Is Not Empty.");
                return "";
            }

            Directory.CreateDirectory(dataDir);

            var p = new Process();
            var info = p.StartInfo;
            info.FileName = insPath;
            info.Arguments += $" --defaults-file=\"{iniPath}\" {(insecure ? "--initialize" : "--initialize-insecure")}";
            info.UseShellExecute = false;
            info.CreateNoWindow = true;
            info.RedirectStandardInput = true;
            info.RedirectStandardOutput = true;
            info.RedirectStandardError = true;
            p.OutputDataReceived += MySQLProcess_OutputDataReceived;
            p.ErrorDataReceived += MySQLProcess_ErrorDataReceived;
            p.Start();
            p.WaitForExit();
            if (!insecure)
                return "";
            var dstErr = Path.Combine(dataDir, Environment.GetEnvironmentVariable("ComputerName") + ".err");
            var pd = AnalyzeInitPasswordFromErr(dstErr);
            return pd;
        }

        public void Start()
        {
            LogApi.Log("Start MySQL Service.");
            _watcherCTS?.Cancel();
            _watcherCTS = new CancellationTokenSource();
            Task.Factory.StartNew(WatcherLoop, _watcherCTS.Token);
        }

        public async Task<bool> StartMySQL(string instancePath, string iniPath)
        {
            var p = new Process();
            var info = p.StartInfo;
            info.FileName = instancePath;
            info.Arguments += $" --defaults-file=\"{iniPath}\" --console";
            info.UseShellExecute = false;
            info.CreateNoWindow = true;
            info.RedirectStandardInput = true;
            info.RedirectStandardOutput = true;
            info.RedirectStandardError = true;
            p.OutputDataReceived += MySQLProcess_OutputDataReceived;
            p.ErrorDataReceived += MySQLProcess_ErrorDataReceived;
            LogApi.Log($"Starting MySQL：{instancePath} {info.Arguments}");
            p.Start();
            p.BeginOutputReadLine();
            var success = await ProcessUtil.WaitUntilProcessExist(DefaultMySQLProcessName);
            if (!success)
                return false;
            await Task.Delay(500);
            var pids = await ProcessUtil.GetSubprocessList(p.Id);
            if (pids.Count < 2)
            {
                return false;
            }

            return true;
        }

        public void StopMySQL(MySQLInstanceModel instance)
        {
            if (instance == null)
                return;

            LogApi.Log($"Stoping MySQL Instance [Shell: {instance.ShellProcessId}, Core: {instance.CoreProcessId}]");
            try
            {
                var p = Process.GetProcessById(instance.CoreProcessId);
                p.Kill();
            }
            catch (Exception) { };
            try
            {
                var p = Process.GetProcessById(instance.ShellProcessId);
                p?.Kill();
            }
            catch (Exception) { };
        }

        public void StopWatcher()
        {
            LogApi.Log("Stop MySQL Service.");
            _watcherCTS.Cancel();
            _watcherCTS = null;
        }

        private string AnalyzeInitPasswordFromErr(string errFilePath)
        {
            if (!File.Exists(errFilePath))
                return "";

            var preStr = "A temporary password is generated for root@localhost:";
            var contents = File.ReadAllLines(errFilePath);
            foreach (var str in contents)
            {
                if (str.Contains(preStr))
                {
                    var strs = str.Split(' ');
                    return strs.LastOrDefault();
                }
            }
            return "";
        }

        private string AnalyzeMySQLDataDirFromIni(string iniFilePath)
        {
            var dataDir = string.Empty;
            if (!File.Exists(iniFilePath))
                return dataDir;

            var lines = File.ReadAllLines(iniFilePath);
            foreach (var line in lines)
            {
                var str = line.ToLower();
                if (line.StartsWith("datadir="))
                {
                    dataDir = line.Replace("datadir=", "").Trim().Trim('\"');
                    break;
                }
                else
                    continue;
            }

            return dataDir;
        }

        private async Task<IList<MySQLInstance>> FindMySQLInstances(string exeName = "mysqld.exe")
        {
            var lst = new List<MySQLInstance>();
            if (string.IsNullOrWhiteSpace(exeName))
                exeName = DefaultMySQLExeName;
            var pinfos = await ProcessUtil.GetProcesssInfoByName(exeName);
            foreach (var pinfo in pinfos)
            {
                var ppinfo = pinfos.FirstOrDefault(p => p.ProcessId == pinfo.ParentProcessId && p.CreationDate < pinfo.CreationDate);
                if (ppinfo != null && !string.IsNullOrWhiteSpace(pinfo.CommandLine))
                {
                    var args = pinfo.CommandLine.Split(' ');
                    var instancepath = pinfo.CommandLine.Substring(0, pinfo.CommandLine.IndexOf(" ")).Trim('\"');
                    var inipath = args.FirstOrDefault(str => str.StartsWith("--defaults-file="));
                    if (string.IsNullOrEmpty(inipath))
                    {
                        inipath = "my.ini";
                    }
                    else
                    {
                        inipath = inipath.Replace("--defaults-file=", "").Trim('\"');
                    }
                    var shellProcess = Process.GetProcessById((int)ppinfo.ProcessId);
                    var coreProcess = Process.GetProcessById((int)pinfo.ProcessId);
                    var instance = new MySQLInstance(instancepath, inipath, shellProcess, coreProcess);
                    lst.Add(instance);
                }
            }
            return lst;
        }

        private void MySQLProcess_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            LogApi.Log(e.Data, LogLevel.Error);
        }

        private void MySQLProcess_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            LogApi.Log(e.Data);
        }

        private void RaiseOnInstanceLost(MySQLInstance instance)
        {
            LogApi.Log($"{instance} Lost.");
            InstanceLost?.Invoke(this, instance);
        }

        private void RaiseOnNewInstanceFound(MySQLInstance instance)
        {
            var dataDir = AnalyzeMySQLDataDirFromIni(instance.InstanceIniPath);
            instance.DataDir = dataDir;
            LogApi.Log($"{instance} Found.");
            NewInstaceFound?.Invoke(this, instance);
        }

        private async void WatcherLoop()
        {
            while (!_watcherCTS.Token.IsCancellationRequested)
            {
                var ins = await FindMySQLInstances();
                var oldIns = _MySQLInstaces.ToList(); ;
                foreach (var instance in ins)
                {
                    var pMatched = oldIns.FirstOrDefault(p => p.Equals(instance));
                    if (pMatched is null)
                    {
                        RaiseOnNewInstanceFound(instance);
                        _MySQLInstaces.Add(instance);
                        continue;
                    }

                    oldIns.Remove(pMatched);
                }

                if (oldIns.Count > 0)
                {
                    foreach (var oldInstance in oldIns)
                        RaiseOnInstanceLost(oldInstance);
                }
                _MySQLInstaces = new ConcurrentBag<MySQLInstance>(ins);

                await Task.Delay(500);
            }
        }

        #endregion Methods
    }
}