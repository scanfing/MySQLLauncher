using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Commands;

namespace MySQLLauncher.Infrastructure
{
    public class CommandImpl<T> : DelegateCommand<T>
    {
        public CommandImpl(Action<T> executeMethod) : base(executeMethod)

        {
        }

        public CommandImpl(Action<T> executeMethod, Func<T, bool> canExecuteMethod) : base(executeMethod, canExecuteMethod)
        {
        }
    }
}