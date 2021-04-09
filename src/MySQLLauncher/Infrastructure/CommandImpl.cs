using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Commands;

namespace MySQLLauncher.Infrastructure
{
    public class CommandImpl : DelegateCommand
    {
        public CommandImpl(Action executeMethod) : base(executeMethod)
        {
        }

        public CommandImpl(Action executeMethod, Func<bool> canExecuteMethod) : base(executeMethod, canExecuteMethod)
        {
        }
    }
}