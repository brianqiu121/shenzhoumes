using Dapper;
using Shenzhou.Framwork.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using static Shenzhou.Framwork.Models.Enum;

namespace Shenzhou.Framwork
{
    /// <summary>
    /// SQL类，包含执行SQL的相关方法
    /// </summary>
    public class SqlHelper
    {
        private static readonly string strClass = "Report.Framework.SqlHelper";

        private static string SetParam(string sql, ConnType dbType = ConnType.none)
        {
            switch (dbType)
            {
                case ConnType.sqlserver:
                    return sql.Replace(":", "@");
                case ConnType.oracle:
                    return sql.Replace("@", ":");
                default:
                    return sql;
            }
        }

        /// <summary>
        /// 执行指定Sql语句
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql">Sql语句</param>
        /// <param name="param">参数</param>
        /// <param name="cfgConnStr">配置文件中连接字符串对应的AppSettings的Key</param>
        /// <param name="dbType">连接的数据库类型，目前支持 sqlserver, mysql, oracle</param>
        /// <returns></returns>
        public static List<T> Query<T>(string sql, object param, string cfgConnStr, ConnType dbType = ConnType.none)
        {
            try
            {
                List<T> list;
                sql = SetParam(sql);

                using (var conn = SqlFactory.CreateConnection(cfgConnStr, dbType))
                {
                    if (conn.State != ConnectionState.Open)
                        conn.Open();

                    list = conn.Query<T>(sql, param).ToList();

                    if (conn.State != ConnectionState.Closed)
                        conn.Close();
                }

                return list;
            }
            catch (Exception e)
            {
                throw new Exception(strClass + ".Query(sql, param, cfgConnStr, dbType)>>" + CommonHelper.GetException(e));
            }

        }

        /// <summary>
        /// 执行指定Sql语句
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql">执行的SQL</param>
        /// <param name="param">参数</param>
        /// <param name="cfgConnStr">配置文件中连接字符串对应的AppSettings的Key</param>
        /// <param name="dbType">要连接的数据库类型</param>
        /// <param name="txn">sql transaction</param>
        /// <param name="buffer">buffer</param>
        /// <param name="timeOut">sql执行超时时间</param>
        /// <returns></returns>
        public static List<T> Query<T>(string sql, object param, string cfgConnStr, int? timeOut, ConnType dbType = ConnType.none, IDbTransaction txn = null, bool buffer = true)
        {
            try
            {
                List<T> list;
                sql = SetParam(sql);

                using (var conn = SqlFactory.CreateConnection(cfgConnStr, dbType))
                {
                    if (conn.State != ConnectionState.Open)
                        conn.Open();

                    list = conn.Query<T>(sql, param, txn, buffer, timeOut, CommandType.Text).ToList();

                    if (conn.State != ConnectionState.Closed)
                        conn.Close();
                }

                return list;
            }
            catch (Exception e)
            {
                throw new Exception(strClass + ".Query(sql, param, cfgConnStr, timeOut, dbType, txn, buffer)>>" + CommonHelper.GetException(e));
            }

        }

        /// <summary>
        /// 执行指定Sql语句
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cmd">Sql语句</param>
        /// <param name="cfgConnStr">配置文件中连接字符串对应的AppSettings的Key</param>
        /// <param name="dbType">连接的数据库类型，目前支持 sqlserver, mysql, oracle</param>
        /// <returns></returns>
        public static List<T> Query<T>(CommandDefinition cmd, string cfgConnStr, ConnType dbType = ConnType.none)
        {
            try
            {
                List<T> list;

                using (var conn = SqlFactory.CreateConnection(cfgConnStr, dbType))
                {
                    if (conn.State != ConnectionState.Open)
                        conn.Open();

                    list = conn.Query<T>(cmd).ToList();

                    if (conn.State != ConnectionState.Closed)
                        conn.Close();
                }

                return list;
            }
            catch (Exception e)
            {
                throw new Exception(strClass + ".Query(CommandDefinition, cfgConnStr, dbType)>>" + CommonHelper.GetException(e));
            }
        }

        /// <summary>
        /// 执行指定Sql语句
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cmd">Sql语句</param>
        /// <param name="cfgConnStr">配置文件中连接字符串对应的AppSettings的Key</param>
        /// <param name="dbType">连接的数据库类型，目前支持 sqlserver, mysql, oracle</param>
        /// <returns></returns>
        public static List<T> Query<T>(SqlCommand cmd, string cfgConnStr, ConnType dbType = ConnType.none)
        {
            try
            {
                List<T> list;

                using (var conn = SqlFactory.CreateConnection(cfgConnStr, dbType))
                {
                    if (conn.State != ConnectionState.Open)
                        conn.Open();

                    list = conn.Query<T>(cmd.CommandDefinition).ToList();

                    if (conn.State != ConnectionState.Closed)
                        conn.Close();
                }

                return list;
            }
            catch (Exception e)
            {
                throw new Exception(strClass + ".Query(SqlCommand, cfgConnStr, dbType)>>" + CommonHelper.GetException(e));
            }
        }

        /// <summary>
        /// 执行插入或更新语句
        /// </summary>
        /// <typeparam name="T">泛型，含需要插入的值的对象类型</typeparam>
        /// <param name="sql">执行的SQL语句</param>
        /// <param name="t">含需要操作的值的对象</param>
        /// <param name="connStr">配置文件中定义数据库连接字符串的key</param>
        /// <returns>影响的行数</returns>
        public static int Execute(string sql, object t, string connStr, ConnType dbType = ConnType.none)
        {
            try
            {
                int result;
                sql = SetParam(sql);

                using (var conn = SqlFactory.CreateConnection(connStr, dbType))
                {
                    if (conn.State != ConnectionState.Open)
                        conn.Open();

                    result = conn.Execute(sql, t, null, 30, CommandType.Text);

                    if (conn.State != ConnectionState.Closed)
                        conn.Close();
                }

                return result;
            }
            catch (Exception e)
            {
                throw new Exception(strClass + ".Execute(sql, t, connstr)>>" + CommonHelper.GetException(e));
            }
        }

        /// <summary>
        /// 多次执行一个插入或删除的语句，每次参数不同，同时保证这些语句在一个transaction中执行
        /// </summary>
        /// <typeparam name="T">泛型，含需要插入的值的对象类型</typeparam>
        /// <param name="list">含需要操作的值的对象集合</param>
        /// <param name="sql">执行的SQL语句</param>
        /// <param name="connStr">配置文件中定义数据库连接字符串的key</param>
        /// <returns>影响的行数</returns>
        public static bool Execute<T>(List<T> list, string sql, string connStr, ConnType dbType = ConnType.none, bool ResultCheck = true)
        {
            try
            {
                bool flagCommit = true;
                var result = -1;
                sql = SetParam(sql);

                using (var conn = SqlFactory.CreateConnection(connStr, dbType))
                {
                    if (conn.State != ConnectionState.Open)
                        conn.Open();


                    using (var trans = conn.BeginTransaction())
                    {
                        foreach (var item in list)
                        {
                            result = conn.Execute(sql, item, trans, 30, CommandType.Text);
                            if (result <= 0 && ResultCheck)
                            {
                                trans.Rollback();
                                flagCommit = false;
                                break;
                            }
                        }

                        if (flagCommit)
                        {
                            trans.Commit();
                        }
                    }

                    if (conn.State != ConnectionState.Closed)
                        conn.Close();
                }

                return flagCommit;
            }
            catch (Exception e)
            {
                throw new Exception(strClass + ".Execute(list, sql, connstr)>>" + CommonHelper.GetException(e));
            }
        }

        /// <summary>
        /// 批量执行一组数据操作语句，保证其在一个事务里
        /// </summary>
        /// <param name="list">数据操作语句集合</param>
        /// <param name="connStr">配置文件中定义数据库连接字符串的key</param>
        /// <param name="check">是否每条sql的影响行数都必需大于零，否则事务失败</param>
        /// <returns></returns>
        public static bool Execute(List<CommandDefinition> list, string connStr, ConnType dbType = ConnType.none, bool ResultCheck = true)
        {
            try
            {
                var result = -1;
                bool flagCommit = true;

                using (var conn = SqlFactory.CreateConnection(connStr, dbType))
                {
                    if (conn.State != ConnectionState.Open)
                        conn.Open();

                    using (var trans = conn.BeginTransaction())
                    {
                        foreach (var item in list)
                        {
                            result = conn.Execute(item);
                            if (result <= 0 && ResultCheck)
                            {
                                trans.Rollback();
                                flagCommit = false;
                                break;
                            }
                        }

                        if (flagCommit)
                        {
                            trans.Commit();
                        }
                    }

                    if (conn.State != ConnectionState.Closed)
                        conn.Close();
                }

                return flagCommit;
            }
            catch (Exception e)
            {
                throw new Exception(strClass + ".Execute(List<CommandDefinition>, connstr, check)>>" + CommonHelper.GetException(e));
            }
        }

        /// <summary>
        /// 批量执行一组数据操作语句，保证其在一个事务里
        /// </summary>
        /// <param name="list">数据操作语句集合</param>
        /// <param name="connStr">配置文件中定义数据库连接字符串的key</param>
        /// <returns></returns>
        public static bool Execute(List<SqlCommand> list, string connStr, ConnType dbType = ConnType.none)
        {
            try
            {
                bool flagCommit = true;
                var result = -1;

                using (var conn = SqlFactory.CreateConnection(connStr, dbType))
                {
                    if (conn.State != ConnectionState.Open)
                        conn.Open();

                    using (var trans = conn.BeginTransaction())
                    {
                        foreach (var item in list)
                        {
                            result = conn.Execute(item.CommandDefinition);
                            if (result <= 0 && item.ResultCheck)
                            {
                                trans.Rollback();
                                flagCommit = false;
                                break;
                            }
                        }

                        if (flagCommit)
                        {
                            trans.Commit();
                        }
                    }

                    if (conn.State != ConnectionState.Closed)
                        conn.Close();
                }

                return flagCommit;
            }
            catch (Exception e)
            {
                throw new Exception(strClass + ".Execute(List<SqlCommond>, connstr)>>" + CommonHelper.GetException(e));
            }
        }

        /// <summary>
        /// 执行插入或更新语句
        /// </summary>
        /// <param name="cmd">数据操作语句对象</param>
        /// <param name="connStr">配置文件中定义数据库连接字符串的key</param>
        /// <returns>影响的行数</returns>
        public static int Execute(CommandDefinition cmd, string connStr, ConnType dbType = ConnType.none)
        {
            try
            {
                var result = -1;

                using (var conn = SqlFactory.CreateConnection(connStr, dbType))
                {
                    if (conn.State != ConnectionState.Open)
                        conn.Open();

                    result = conn.Execute(cmd);

                    if (conn.State != ConnectionState.Closed)
                        conn.Close();
                }

                return result;
            }
            catch (Exception e)
            {
                throw new Exception(strClass + ".Execute(SqlCommond, connstr)>>" + CommonHelper.GetException(e));
            }
        }
    }
}
