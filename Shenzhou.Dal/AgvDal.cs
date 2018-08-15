using Shenzhou.Framwork;
using Shenzhou.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shenzhou.Dal
{
    public class AgvDal
    {
        private string _oracleConnStr;
        private string _mssqlConnStr;

        public AgvDal()
        {
            _oracleConnStr = GlobalData.OracleConnString;
            _mssqlConnStr = GlobalData.MSSqlConnString;
        }

        public AgvDal(string oracleConnStr, string mssqlConnStr)
        {
            _oracleConnStr = oracleConnStr;
            _mssqlConnStr = mssqlConnStr;
        }

        /// <summary>
        /// 新增From_PLC_CC记录
        /// </summary>
        /// <param name="avgMsg"></param>
        /// <returns></returns>
        public int insertMsgFrom_PLC_CC(AGVMsg agvMsg)
        {
            try
            {
                string strTime = DateTime.UtcNow.TimeMillis().ToString();
                string strSql = @" INSERT INTO FROM_PLC_CC 
                                        (
                                            TIME,SENDER,MESSAGE_TYPE,MESSAGE_COUNT,
                                            MESSAGE,ORIGIN,DEST,MESSAGE_ID
                                        ) 
                                    VALUES 
                                        (
                                            :TIME,:SENDER,:MESSAGE_TYPE,:MESSAGE_COUNT,:MESSAGE,
                                            :ORIGIN,:DEST,:MESSAGE_ID
                                        ) ";
                var condition = new
                {
                    TIME = strTime,
                    SENDER = agvMsg.SEND,
                    MESSAGE_TYPE = agvMsg.TYPE,
                    MESSAGE_COUNT = 0,
                    MESSAGE = agvMsg.CONTENTS,
                    ORIGIN = agvMsg.ORI,
                    DEST = agvMsg.DES,
                    MESSAGE_ID = agvMsg.ID
                };
                return SqlHelper.Execute(strSql, condition, _oracleConnStr);

            }
            catch (Exception e)
            {
                throw new Exception("Shenzhou.Dal.AgvDal.insertMsgFrom_PLC_CC>>" + CommonHelper.GetException(e));
            }
        }

        /// <summary>
        /// 更新指定id的指令发送状态为1
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public int updateCommandSendSuccess(string id, string status)
        {
            try
            {
                string strTime = DateTime.UtcNow.TimeMillis().ToString();
                string strSql = @" UPDATE COMMAND_LIST 
                                    SET SENDSTATUS =1 ,UPDATE_TIME =:UPDATE_TIME ,STATUS=:STATUS 
                                    WHERE  ID = :ID ";
                var condition = new
                {
                    TIME = strTime,
                    STATUS = status,
                    ID = id
                };
                return SqlHelper.Execute(strSql, condition, _oracleConnStr);

            }
            catch (Exception e)
            {
                throw new Exception("Shenzhou.Dal.AgvDal.updateCommandSendSuccess>>" + CommonHelper.GetException(e));
            }
        }

        /// <summary>
        /// 新增command list记录，默认建立时间和更新时间为当前时间
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public int insertCOMMAND_LIST(COMMAND_LIST cmd)
        {
            try
            {
                string time = DateTime.UtcNow.TimeMillis().ToString();
                cmd.Create_Time = time;
                cmd.Update_Time = time;
                string strSql = @" INSERT INTO COMMAND_LIST 
                                    (ID,SENDER,RECIVER,ORIGN,DEST,CONTENTS,TYPE,STATUS,
                                        SENDSTATUS,UPDATE_TIME,CREATE_TIME,CALL_LOCATION) 
                                    VALUES (:ID,:SENDER,:RECIVER,:ORIGN,:DEST,:CONTENTS,:TYPE,:STATUS,
                                        :SENDSTATUS,:UPDATE_TIME,:CREATE_TIME,:CALL_LOCATION) ";

                if (cmd.Type.Equals("99") || cmd.Type.Equals("98"))
                {
                    cmd.SendStatus = "1";
                }
                else
                {
                    cmd.SendStatus = "0";
                }
                return SqlHelper.Execute(strSql, cmd, _oracleConnStr);

            }
            catch (Exception e)
            {
                throw new Exception("Shenzhou.Dal.AgvDal.insertCOMMAND_LIST>>" + CommonHelper.GetException(e));
            }
        }

        /// <summary>
        /// 新增BP_FULLDOLLY_TRANSPORT记录
        /// </summary>
        /// <param name="Dolly"></param>
        /// <param name="Ori"></param>
        /// <param name="Des"></param>
        /// <param name="type"></param>
        /// <param name="Content"></param>
        /// <returns></returns>
        public int insertBP_FULLDOLLY_TRANSPORT(string Dolly, string Ori, string Des, int type, string Content)
        {
            try
            {
                string time = DateTime.UtcNow.TimeMillis().ToString();

                string strSql = @" INSERT INTO BP_FULLDOLLY_TRANSPORT 
                                        (DOLLY,ORI,DES,CONTENT_INFO,CREATE_TIME,TYP) 
                                    VALUES 
                                        (:DOLLY,:ORI,:DES,:CONTENT_INFO,:CREATE_TIME,:TYP) ";

                var condition = new
                {
                    DOLLY = Dolly,
                    ORI = Ori,
                    DES = Des,
                    CONTENT_INFO = Content,
                    CREATE_TIME = time,
                    TYP = type
                };
                return SqlHelper.Execute(strSql, condition, _oracleConnStr);

            }
            catch (Exception e)
            {
                throw new Exception("Shenzhou.Dal.AgvDal.insertBP_FULLDOLLY_TRANSPORT>>" + CommonHelper.GetException(e));
            }
        }

        /// <summary>
        /// 更新指定ID的指令的发送状态为1， 更新状态为指定状态，类型为指定类型
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public int updateCommandChange(string id, string status, string type)
        {
            try
            {
                string strTime = DateTime.UtcNow.TimeMillis().ToString();
                string strSql = @" UPDATE COMMAND_LIST 
                                    SET SENDSTATUS =1 ,UPDATE_TIME =:UPDATE_TIME ,
                                        STATUS=:STATUS ,type=:type 
                                    WHERE  ID = :ID ";
                var condition = new
                {
                    UPDATE_TIME = strTime,
                    STATUS = status,
                    type = type,
                    ID = id
                };
                return SqlHelper.Execute(strSql, condition, _oracleConnStr);

            }
            catch (Exception e)
            {
                throw new Exception("Shenzhou.Dal.AgvDal.updateCommandChange>>" + CommonHelper.GetException(e));
            }
        }

        /// <summary>
        /// 设置command_list中reciver为指定接收者，type为1或5的记录的send status为零
        /// </summary>
        /// <param name="Receiver"></param>
        /// <returns></returns>
        public int ResendCommandReqByAGV(string Receiver)
        {
            try
            {
                string strTime = DateTime.UtcNow.TimeMillis().ToString();
                string strSql = @" update command_list 
                                        set sendstatus = 0 
                                    where (type = 1 or type = 5) 
                                        and reciver = :reciver ";
                var condition = new
                {
                    reciver = Receiver
                };
                return SqlHelper.Execute(strSql, condition, _oracleConnStr);

            }
            catch (Exception e)
            {
                throw new Exception("Shenzhou.Dal.AgvDal.ResendCommandReqByAGV>>" + CommonHelper.GetException(e));
            }
        }

        /// <summary>
        /// 设置command_list中reciver为指定接收者，type为03的记录的type为07
        /// </summary>
        /// <param name="Receiver"></param>
        /// <returns></returns>
        public int ResendCommandReqByAGVBlockCommand(string Receiver)
        {
            try
            {
                string strTime = DateTime.UtcNow.TimeMillis().ToString();
                string strSql = @" update command_list 
                                    set type = '07' 
                                    where type = '03' 
                                      and reciver = :reciver ";
                var condition = new
                {
                    reciver = Receiver
                };
                return SqlHelper.Execute(strSql, condition, _oracleConnStr);

            }
            catch (Exception e)
            {
                throw new Exception("Shenzhou.Dal.AgvDal.ResendCommandReqByAGVBlockCommand>>" + CommonHelper.GetException(e));
            }
        }

        /// <summary>
        /// 根据ID更新send status为零，其他栏位为指定值
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <param name="type"></param>
        /// <param name="origin"></param>
        /// <param name="dest"></param>
        /// <param name="contents"></param>
        /// <returns></returns>
        public int updateCommandTypeWithSend(string id, string status, string type, string origin, string dest, string contents)
        {
            try
            {
                string strTime = DateTime.UtcNow.TimeMillis().ToString();
                string strSql = @" UPDATE COMMAND_LIST 
                                    SET SENDSTATUS =0, UPDATE_TIME =:UPDATE_TIME, 
                                        STATUS=:STATUS, type=:type, orign = :orign, 
                                        dest = :dest, contents =:contents 
                                    WHERE  ID = :ID ";
                var condition = new
                {
                    UPDATE_TIME = strTime,
                    STATUS = status,
                    type = type,
                    orign = origin,
                    dest = dest,
                    contents = contents,
                    ID = id
                };
                return SqlHelper.Execute(strSql, condition, _oracleConnStr);

            }
            catch (Exception e)
            {
                throw new Exception("Shenzhou.Dal.AgvDal.updateCommandTypeWithSend>>" + CommonHelper.GetException(e));
            }
        }

        /// <summary>
        /// 获取command_list中send status为零的记录
        /// </summary>
        /// <returns></returns>
        public List<COMMAND_LIST> getCommandList()
        {
            try
            {
                string strSql = @" select * from COMMAND_LIST where SENDSTATUS=0 ";

                return SqlHelper.Query<COMMAND_LIST>(strSql, null, _oracleConnStr);

            }
            catch (Exception e)
            {
                throw new Exception("Shenzhou.Dal.CutDal.getCommandList>>" + CommonHelper.GetException(e));
            }
        }

        /// <summary>
        /// 保存发送给agv的报文记录
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public int insertMsgToPLC_CC(AGVMsg msg)
        {
            try
            {
                string strTime = DateTime.UtcNow.TimeMillis().ToString();
                string strSql = @" INSERT INTO TO_AGV_CC 
                                    (
                                        TIME,MESSAGE_TYPE,SENDER,RECIVER,PRIORITY_NUM,
                                        ORIGIN,DESTINATION,STATUS,ID
                                    ) 
                                    VALUES 
                                    (
                                        :TIME,:MESSAGE_TYPE,:SENDER,:RECIVER,:PRIORITY_NUM,
                                        :ORIGIN,:DESTINATION,:STATUS,:ID
                                    ) ";
                var condition = new
                {
                    TIME = strTime,
                    MESSAGE_TYPE = msg.TYPE,
                    SENDER = msg.SEND,
                    RECIVER = msg.RCV,
                    PRIORITY_NUM = 0,
                    ORIGIN = msg.ORI,
                    DESTINATION = msg.DES,
                    STATUS = msg.STATUS,
                    ID = msg.ID
                };
                return SqlHelper.Execute(strSql, condition, _oracleConnStr);

            }
            catch (Exception e)
            {
                throw new Exception("Shenzhou.Dal.AgvDal.insertMsgToPLC_CC>>" + CommonHelper.GetException(e));
            }
        }

    }
}
