using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using static Shenzhou.Framwork.Models.Enum;
using Oracle.ManagedDataAccess.Client;

namespace Shenzhou.Framwork
{
    /// <summary>
    /// SQL相关工厂方法
    /// </summary>
    public class SqlFactory
    {
        /// <summary>
        /// 类名，用户记录异常时出错位置
        /// </summary>
        private static readonly string strClass = "Report.Framework.DapperFactory";

        /// <summary>
        /// 建立数据库连接
        /// </summary>
        /// <param name="cfgConnStr">连接字符串对应的配置文件中AppSettings的Key</param>
        /// <param name="dbType">连接的数据库类型，目前支持 sqlserver, mysql, oracle</param>
        /// <returns></returns>
        public static DbConnection CreateConnection(string cfgConnStr, ConnType dbType)
        {
            try
            {
                string strConnStr = ConfigurationManager.AppSettings[cfgConnStr].ToString();
                //string strDbType = "mysql";
                if (dbType == ConnType.none)
                {
                    dbType = ConnType.oracle;
                    try
                    {
                        dbType = (ConnType)Enum.Parse(typeof(ConnType), ConfigurationManager.AppSettings["db_type"].ToString());
                    }
                    catch { }
                }
                if (dbType == ConnType.oracle)
                {
                    return CreateOrclConnection(strConnStr);
                }
                else if (dbType == ConnType.sqlserver)
                {
                    return CreateSqlConnection(strConnStr);
                }
                else
                {
                    return null;
                    //return CreateMySqlConnection(strConnStr);
                }
            }
            catch (Exception e)
            {
                throw new Exception(strClass + ".CreateConnection>>" + CommonHelper.GetException(e));
            }
        }

        /// <summary>
        /// 建立my sql连接
        /// </summary>
        /// <returns></returns>
        private static OracleConnection CreateOrclConnection(string connstr)
        {
            var connection = new OracleConnection(connstr);
            connection.Open();
            return connection;
        }

        /// <summary>
        /// 建立sql server连接
        /// </summary>
        /// <param name="strConnStr">连接字符串</param>
        /// <returns></returns>
        private static SqlConnection CreateSqlConnection(string strConnStr)
        {
            var connection = new SqlConnection(strConnStr);
            connection.Open();
            return connection;
        }
    }
}
