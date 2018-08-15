using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shenzhou.Framwork.Models
{
    /// <summary>
    /// 定义枚举类型
    /// </summary>
    public class Enum
    {
        /// <summary>
        /// 数据库类型
        /// </summary>
        public enum ConnType
        {
            /// <summary>
            /// Sql Server
            /// </summary>
            sqlserver,
            /// <summary>
            /// Oracle
            /// </summary>
            oracle,
            /// <summary>
            /// My Sql
            /// </summary>
            mysql,
            /// <summary>
            /// 根据配置文件获得
            /// </summary>
            none
        }

        /// <summary>
        /// 进制
        /// </summary>
        public enum Decimals
        {
            /// <summary>
            /// 十进制
            /// </summary>
            Decimalization,

            /// <summary>
            /// 二进制
            /// </summary>
            Binary,

            /// <summary>
            /// 十六进制
            /// </summary>
            Hexadecimal
        }
    }
}
