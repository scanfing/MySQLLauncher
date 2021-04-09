using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySQLLauncher.Core
{
    public enum MySQLInstanceStatus
    {
        None,
        Starting,
        Running,
        Stoping,
        Stopped
    }
}