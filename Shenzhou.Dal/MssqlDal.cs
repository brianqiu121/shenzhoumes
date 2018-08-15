using Shenzhou.Framwork;
using Shenzhou.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shenzhou.Dal
{
    public class MssqlDal
    {
        private string _oracleConnStr;
        private string _mssqlConnStr;

        public MssqlDal()
        {
            _oracleConnStr = GlobalData.OracleConnString;
            _mssqlConnStr = GlobalData.MSSqlConnString;
        }

        public MssqlDal(string oracleConnStr, string mssqlConnStr)
        {
            _oracleConnStr = oracleConnStr;
            _mssqlConnStr = mssqlConnStr;
        }

        /// <summary>
        /// 更新sql server的AGV_CallTask表，TaskState为0，PlaceNo为指定工位的记录，设置TaskState为指定值
        /// </summary>
        /// <param name="CallLocation"></param>
        /// <param name="Stage">0 for pending; 1 for Arrive; 2 for Done; 3 for delete; 4 for overtime remove</param>
        /// <returns></returns>
        public int updateAGVCALLTASK_setArrive(string CallLocation, int Stage)
        {
            try
            {
                string strSql = @" update AGV_CallTask 
                                        Set TaskState=@TaskState 
                                    where PlaceNo=@PlaceNo and TaskState=0";
                var condition = new
                {
                    TaskState = Stage,
                    PlaceNo = CallLocation
                };
                return SqlHelper.Execute(strSql, condition, _mssqlConnStr, Framwork.Models.Enum.ConnType.sqlserver);

            }
            catch (Exception e)
            {
                throw new Exception("Shenzhou.Dal.MssqlDal.updateAGVCALLTASK_setArrive>>" + CommonHelper.GetException(e));
            }
        }

        /// <summary>
        /// 从博大系统取得订单信息
        /// </summary>
        /// <param name="orderID"></param>
        /// <returns></returns>
        public List<BP_ORDER> getBpOrderByBarCode(string barCode)
        {
            try
            {
                List<BP_ORDER> lstResult = new List<BP_ORDER>();
                string strSql = @" SELECT * 
                                 FROM [sz_bd].[zy_bd].[dbo].[zy_lbjhgl] a 
                                   inner join [sz_bd].[zy_bd].[dbo].[zy_lbjhgl_txm] b on a.dh=b.dh 
                                 WHERE a.dh =@barCode ";
                var condition = new { barCode = barCode };
                lstResult = SqlHelper.Query<BP_ORDER>(strSql, condition, _mssqlConnStr, Framwork.Models.Enum.ConnType.sqlserver);
                return lstResult;
            }
            catch (Exception e)
            {
                throw new Exception("Shenzhou.Dal.MssqlDal.getBpOrderByBarCode>>" + CommonHelper.GetException(e));
            }
        }

        /// <summary>
        /// 更新AGV_CallTask表中TaskState为1，PlaceNo为指定工位的记录，设置TaskState为指定值
        /// </summary>
        /// <param name="CallLocation"></param>
        /// <param name="Stage">0 for pending; 1 for Arrive; 2 for Done; 3 for delete; 4 for overtime remove</param>
        /// <returns></returns>
        public int updateAGVCALLTASK_seDone(string CallLocation, int Stage)
        {//0 for pending; 1 for Arrive; 2 for Done; 3 for delete; 4 for overtime remove;
            try
            {
                string strSql = @" update AGV_CallTask 
                                      Set TaskState=@TaskState 
                                    where PlaceNo=@PlaceNo and TaskState=1";
                var condition = new
                {
                    TaskState = Stage,
                    PlaceNo = CallLocation
                };
                return SqlHelper.Execute(strSql, condition, _mssqlConnStr, Framwork.Models.Enum.ConnType.sqlserver);

            }
            catch (Exception e)
            {
                throw new Exception("Shenzhou.Dal.MssqlDal.updateAGVCALLTASK_setArrive>>" + CommonHelper.GetException(e));
            }
        }
    }
}
