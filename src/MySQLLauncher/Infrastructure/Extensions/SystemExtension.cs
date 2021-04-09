using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PrismTemplate.Infrastructure.Extensions
{
    public static class SystemExtension
    {
        #region Methods

        /// <summary>
        /// 生成异常的日志信息
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="inludeStackTrace">是否包含堆栈信息</param>
        /// <returns></returns>
        public static string BuildExLog(this Exception ex, bool inludeStackTrace = false)
        {
            var exsb = new StringBuilder();
            exsb.AppendLine(ex.Message);
            if (inludeStackTrace)
            {
                exsb.Append(ex.StackTrace);
            }
            if (ex.InnerException != null)
            {
                exsb.AppendLine(ex.InnerException.BuildExLog(inludeStackTrace));
            }

            return exsb.ToString();
        }

        #endregion Methods
    }
}