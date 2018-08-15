using Shenzhou.Framwork;
using Shenzhou.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shenzhou.Dal
{
    public class CutDal
    {
        private string _oracleConnStr;
        private string _mssqlConnStr;

        public CutDal()
        {
            _oracleConnStr = GlobalData.OracleConnString;
            _mssqlConnStr = GlobalData.MSSqlConnString;
        }

        public CutDal(string oracleConnStr, string mssqlConnStr)
        {
            _oracleConnStr = oracleConnStr;
            _mssqlConnStr = mssqlConnStr;
        }

        /// <summary>
        /// 根据Order ID从MES系统取得订单信息
        /// </summary>
        /// <param name="orderID"></param>
        /// <returns></returns>
        public List<BP_ORDER> getBpOrderByOrderID(string orderID)
        {
            try
            {
                List<BP_ORDER> lstResult = new List<BP_ORDER>();
                string strSql = @" select ORDER_ID, HYH, HH, SH, SC, GH, TXM, 
                                          PS, PH, complete status, CREATE_TIME 
                                    from SEL1_CSS_DBA.BP_ORDER 
                                    WHERE  ORDER_ID = :Order_ID ";
                var condition = new { Order_ID = orderID };
                lstResult = SqlHelper.Query<BP_ORDER>(strSql, condition, _oracleConnStr);
                return lstResult;
            }
            catch (Exception e)
            {
                throw new Exception("Shenzhou.Dal.CutDal.getBpOrderByOrderID>>" + CommonHelper.GetException(e));
            }
        }

        /// <summary>
        /// 根据TXM从MES系统取得订单信息
        /// </summary>
        /// <param name="orderTxm"></param>
        /// <returns></returns>
        public List<BP_ORDER> getBPorderInfoBySongBu(string orderTxm)
        {
            try
            {
                List<BP_ORDER> lstResult = new List<BP_ORDER>();
                string strSql = @" select ORDER_ID, HYH, HH, SH, SC, GH, TXM, 
                                          PS, PH, complete status, CREATE_TIME 
                                    from SEL1_CSS_DBA.BP_ORDER 
                                    WHERE  TXM = :TXM ";
                var condition = new { TXM = orderTxm };
                lstResult = SqlHelper.Query<BP_ORDER>(strSql, condition, _oracleConnStr);
                return lstResult;
            }
            catch (Exception e)
            {
                throw new Exception("Shenzhou.Dal.CutDal.getBPorderInfoBySongBu>>" + CommonHelper.GetException(e));
            }
        }

        

        /// <summary>
        /// 在MES系统中新增BP order记录
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public bool insertBP_ORDER(List<BP_ORDER> list)
        {
            try
            {
                string strSql = @" Insert into SEL1_CSS_DBA.BP_ORDER 
                                        (ORDER_ID, HYH, HH, SH, SC, GH, TXM, PS, PH, CREATE_TIME) 
                                 VALUES (:ORDER_ID, :HYH, :HH, :SH, :SC, :GH, :TXM, :PS, :PH, " + DateTime.Now.TimeMillis() + ") ";

                return SqlHelper.Execute(list, strSql, _oracleConnStr);
            }
            catch (Exception e)
            {
                throw new Exception("Shenzhou.Dal.CutDal.insertBP_ORDER>>" + CommonHelper.GetException(e));
            }
        }

        /// <summary>
        /// 在MES系统中，根据txm将bp order的complete置为1
        /// </summary>
        /// <param name="txm"></param>
        /// <returns></returns>
        public bool updateSongBuToCompete(string txm)
        {
            try
            {
                string strSql = @" update SEL1_CSS_DBA.BP_ORDER  
                                    set COMPLETE = 1 , UPDATE_TIME=:UPDATE_TIME
                                    WHERE  TXM = :TXM ";
                var condition = new { UPDATE_TIME = DateTime.UtcNow.TimeMillis().ToString(), TXM = txm };
                return SqlHelper.Execute(strSql, condition, _oracleConnStr) > 0;

            }
            catch (Exception e)
            {
                throw new Exception("Shenzhou.Dal.CutDal.updateSongBuToCompete>>" + CommonHelper.GetException(e));
            }
        }
        
        /// <summary>
        /// 从MES系统，根据合约号和缸号，查找料车及储位
        /// </summary>
        /// <param name="HYH"></param>
        /// <param name="GH"></param>
        /// <returns></returns>
        public BP_Location getLocationForCJByhyhAndgh(string HYH, string GH)
        {
            try
            {
                BP_Location result = new BP_Location();
                string strSql = @" SELECT LOCATION_CODE,DOLLY_NO DOLLY 
                                    from V_BP_LOCATION_DOLLY_REF 
                                    WHERE HYH = :HYH and GH= :GH  
                                    ORDER BY UPDATE_TIME ASC ";
                var condition = new { HYH = HYH, GH = GH };
                result = SqlHelper.Query<BP_Location>(strSql, condition, _oracleConnStr).FirstOrDefault();
                return result;
            }
            catch (Exception e)
            {
                throw new Exception("Shenzhou.Dal.CutDal.getLocationForCJByhyhAndgh>>" + CommonHelper.GetException(e));
            }
        }

        /// <summary>
        /// 从MES系统，根据缸号，查找料车及储位
        /// </summary>
        /// <param name="GH"></param>
        /// <returns></returns>
        public BP_Location getLocationForCJByGH(string GH)
        {
            try
            {
                BP_Location result = new BP_Location();
                string strSql = @" select LOCATION_CODE,DOLLY_NO DOLLY 
                                    from V_BP_LOCATION_DOLLY_REF 
                                    where GH= :GH  
                                    order by UPDATE_TIME ";
                var condition = new { GH = GH };
                result = SqlHelper.Query<BP_Location>(strSql, condition, _oracleConnStr).FirstOrDefault();
                return result;
            }
            catch (Exception e)
            {
                throw new Exception("Shenzhou.Dal.CutDal.getLocationForCJByGH>>" + CommonHelper.GetException(e));
            }
        }

        /// <summary>
        /// 取得送到指定裁剪工位的未完成的AGV指令
        /// </summary>
        /// <param name="dest">指定裁剪工位</param>
        /// <returns></returns>
        public List<COMMAND_LIST> getLocationOrderIsSendForCJ(string dest)
        {
            try
            {
                List<COMMAND_LIST> result = new List<COMMAND_LIST>();
                string strSql = @" select * 
                                   from COMMAND_LIST 
                                   where TYPE in('01','02','03') 
                                    and  DEST =:dest ";
                var condition = new { dest = dest };
                result = SqlHelper.Query<COMMAND_LIST>(strSql, condition, _oracleConnStr);
                return result;
            }
            catch (Exception e)
            {
                throw new Exception("Shenzhou.Dal.CutDal.getLocationOrderIsSendForCJ>>" + CommonHelper.GetException(e));
            }
        }

        
        
        /// <summary>
        /// 取得状态为10，且起点为指定位置的指令记录
        /// </summary>
        /// <param name="orign"></param>
        /// <returns></returns>
        public List<COMMAND_LIST> getLocationOrderIsSendForCJMoveEmpty(string orign)
        {
            try
            {
                List<COMMAND_LIST> result = new List<COMMAND_LIST>();
                string strSql = @" select * 
                                   from COMMAND_LIST 
                                   where TYPE ='10' and  ORIGN =:ORIGN ";
                var condition = new { ORIGN = orign };
                result = SqlHelper.Query<COMMAND_LIST>(strSql, condition, _oracleConnStr);
                return result;
            }
            catch (Exception e)
            {
                throw new Exception("Shenzhou.Dal.CutDal.getLocationOrderIsSendForCJ>>" + CommonHelper.GetException(e));
            }
        }
        
    }
}
