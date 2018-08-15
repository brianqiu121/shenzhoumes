using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shenzhou.Framwork
{
    /// <summary>
    /// 公共方法
    /// </summary>
    public class CommonHelper
    {
        /// <summary>
        /// 取得异常的的错误信息，循环遍历取得所有错误
        /// </summary>
        /// <param name="e">异常对象</param>
        /// <returns></returns>
        public static string GetException(Exception e)
        {
            Exception eTmp = e;
            string strErr = e.Message;
            while (eTmp.InnerException != null)
            {
                strErr = strErr + ">>inner:" + eTmp.InnerException.Message;
                eTmp = eTmp.InnerException;
            }

            return strErr;
        }

        /// <summary>
        /// 与1970-01-01的utc时间毫秒时间差
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static DateTime GetTime(long time)
        {
            DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return Jan1st1970.AddMilliseconds(time);
        }
    }

}
