using Shenzhou.Framwork;
using Shenzhou.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shenzhou.Dal
{
    public class SongbuDal
    {
        private string _oracleConnStr;
        private string _mssqlConnStr;

        public SongbuDal()
        {
            _oracleConnStr = GlobalData.OracleConnString;
            _mssqlConnStr = GlobalData.MSSqlConnString;
        }

        public SongbuDal(string oracleConnStr, string mssqlConnStr)
        {
            _oracleConnStr = oracleConnStr;
            _mssqlConnStr = mssqlConnStr;
        }

        /// <summary>
        /// 根据物料条码清空料车
        /// </summary>
        /// <param name="goods"></param>
        /// <returns></returns>
        public bool deleteDollyInfoBygoods(string goods)
        {
            try
            {
                string strSql = @" delete from SONG_BU  WHERE  GOODS = :goods ";
                var condition = new { goods = goods };
                return SqlHelper.Execute(strSql, condition, _oracleConnStr) > 0;

            }
            catch (Exception e)
            {
                throw new Exception("Shenzhou.Dal.SongbuDal.deleteDollyInfoBygoods>>" + CommonHelper.GetException(e));
            }
        }

        /// <summary>
        /// 根据料车号查找song_bu表记录
        /// </summary>
        /// <param name="dollyNo">料车号</param>
        /// <returns></returns>
        public List<Songbu> getSongbuListByDollyNo(string dollyNo)
        {
            try
            {
                string strSql = @" select GOODS from SONG_BU where DOLLY_NO = :DOLLY_NO ";
                var condition = new { DOLLY_NO = dollyNo };
                return SqlHelper.Query<Songbu>(strSql, condition, _oracleConnStr);

            }
            catch (Exception e)
            {
                throw new Exception("Shenzhou.Dal.SongbuDal.getSongbuListByDollyNo>>" + CommonHelper.GetException(e));
            }
        }
    }
}
