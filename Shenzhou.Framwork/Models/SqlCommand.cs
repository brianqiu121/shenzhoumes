using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Shenzhou.Framwork.Models
{
    /// <summary>
    /// Sql命令
    /// </summary>
    public class SqlCommand
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="parameters"></param>
        /// <param name="resultCheck"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="commandType"></param>
        /// <param name="flags"></param>
        public SqlCommand(string commandText, object parameters = null, bool resultCheck = true, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null, CommandFlags flags = CommandFlags.Buffered)
        {
            CommandDefinition = new CommandDefinition(commandText, parameters, transaction, commandTimeout, commandType, flags);
            ResultCheck = resultCheck;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="commandDefinition"></param>
        /// <param name="resultCheck"></param>
        public SqlCommand(CommandDefinition commandDefinition, bool resultCheck = true)
        {
            CommandDefinition = commandDefinition;
            ResultCheck = resultCheck;
        }

        /// <summary>
        /// sql命令对象
        /// </summary>
        public CommandDefinition CommandDefinition { get; }

        /// <summary>
        /// 是否sql的影响行数必需大于零，否则事务失败
        /// </summary>
        public bool ResultCheck { get; }
    }
}
