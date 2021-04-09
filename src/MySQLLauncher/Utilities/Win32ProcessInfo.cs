using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySQLLauncher.Utilities
{
    public class Win32ProcessInfo
    {
        #region Properties

        public string CommandLine { get; set; }

        public DateTime CreationDate { get; set; }

        public uint ParentProcessId { get; set; }

        public uint ProcessId { get; set; }

        #endregion Properties
    }
}