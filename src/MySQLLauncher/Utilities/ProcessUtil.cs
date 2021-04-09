using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MySQLLauncher.Utilities
{
    public static class ProcessUtil
    {
        #region Methods

        public static async Task<uint> GetParentProcessId(uint pid)
        {
            var parentid = 0u;
            var wmiQuery = $"select ParentProcessId from Win32_Process where ProcessId='{pid}'";
            using ManagementObjectSearcher searcher = new ManagementObjectSearcher(wmiQuery);
            using ManagementObjectCollection retObjectCollection = await Task.Factory.StartNew(() => searcher.Get());
            foreach (ManagementObject retObject in retObjectCollection)
            {
                parentid = (uint)retObject.GetPropertyValue("ParentProcessId");
            }
            return parentid;
        }

        public static async Task<IList<Win32ProcessInfo>> GetProcesssInfoByName(string exeName)
        {
            var infos = new List<Win32ProcessInfo>();

            var wmiQuery = $"select * from Win32_Process where Name='{exeName}'";
            using ManagementObjectSearcher searcher = new ManagementObjectSearcher(wmiQuery);
            using ManagementObjectCollection retObjectCollection = await Task.Factory.StartNew(() => searcher.Get());
            foreach (ManagementObject retObject in retObjectCollection)
            {
                var pid = (uint)retObject.GetPropertyValue("ProcessId");
                var parentId = (uint)retObject.GetPropertyValue("ParentProcessId");
                var commandline = (string)retObject.GetPropertyValue("CommandLine");
                var creationDatestring = (string)retObject.GetPropertyValue("CreationDate");
                creationDatestring = creationDatestring.Substring(0, 21);
                var creationDate = DateTime.ParseExact(creationDatestring, "yyyyMMddHHmmss.ffffff", System.Globalization.CultureInfo.CurrentCulture);

                var info = new Win32ProcessInfo()
                {
                    ProcessId = pid,
                    ParentProcessId = parentId,
                    CreationDate = creationDate,
                    CommandLine = commandline
                };
                infos.Add(info);
            }
            return infos;
        }

        public static async Task<List<int>> GetSubprocessList(int pid)
        {
            var subids = new List<int>();
            var wmiQuery = $"select ProcessId from Win32_Process where ParentProcessId='{pid}'";
            using ManagementObjectSearcher searcher = new ManagementObjectSearcher(wmiQuery);
            using ManagementObjectCollection retObjectCollection = await Task.Factory.StartNew(() => searcher.Get());
            foreach (ManagementObject retObject in retObjectCollection)
            {
                var subid = (uint)retObject.GetPropertyValue("ProcessId");
                subids.Add(int.Parse(subid.ToString()));
            }
            return subids;
        }

        public static bool IsProcessExist(string pName)
        {
            var ps = Process.GetProcessesByName(pName);
            return ps.Length > 0;
        }

        public static void KillProcessByName(string pName)
        {
            var ps = Process.GetProcessesByName(pName);
            foreach (var p in ps)
            {
                try
                {
                    p.Kill();
                }
                catch (Exception)
                {
                }
            }
        }

        public static async Task<bool> WaitUntilProcessExist(string pName, CancellationToken _ct)
        {
            while (!_ct.IsCancellationRequested)
            {
                await Task.Delay(100);
                var exist = IsProcessExist(pName);
                if (exist)
                    return true;
            }
            return false;
        }

        public static async Task<bool> WaitUntilProcessExist(string pName, long timeout = 3000)
        {
            var dstTime = DateTime.Now.AddMilliseconds(timeout);
            while (DateTime.Now <= dstTime)
            {
                await Task.Delay(100);
                var exist = IsProcessExist(pName);
                if (exist)
                    return true;
            }
            return false; ;
        }

        #endregion Methods
    }
}