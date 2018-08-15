using Shenzhou.Framwork;
using Shenzhou.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shenzhou.Dal
{
    public class LocationDal
    {
        private string _oracleConnStr;
        private string _mssqlConnStr;

        public LocationDal()
        {
            _oracleConnStr = GlobalData.OracleConnString;
            _mssqlConnStr = GlobalData.MSSqlConnString;
        }

        public LocationDal(string oracleConnStr, string mssqlConnStr)
        {
            _oracleConnStr = oracleConnStr;
            _mssqlConnStr = mssqlConnStr;
        }

        /// <summary>
        /// 根据储位代码更新location表的bloc_type为指定状态
        /// </summary>
        /// <param name="location"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public int updateLocationLockStatus(string location, int status)
        {
            try
            {
                string time = DateTime.UtcNow.TimeMillis().ToString();

                string strSql = @"UPDATE LOCATION 
                                SET BLOC_TYP =:BLOC_TYP ,UPDATE_TIME=:UPDATE_TIME 
                                WHERE  LOCATION_CODE = :LOCATION_CODE";

                var condition = new { BLOC_TYP = status, UPDATE_TIME = time, LOCATION_CODE = location };


                return SqlHelper.Execute(strSql, condition, _oracleConnStr);

            }
            catch (Exception e)
            {
                throw new Exception("Shenzhou.Dal.LocationDal.updateLocationLockStatus>>" + CommonHelper.GetException(e));
            }
        }

        /// <summary>
        /// 更新bp_location的状态
        /// </summary>
        /// <param name="location"></param>
        /// <param name="status"></param>
        /// <param name="dolly"></param>
        /// <param name="dolly_status"></param>
        /// <returns></returns>
        public int updateBPLocationStatus(string location, int status, string dolly, int dolly_status)
        {
            try
            {
                string time = DateTime.UtcNow.TimeMillis().ToString();
                string strSql = @"  ";
                var condition = new { BLOC_TYP = status, UPDATE_TIME = time, DOLLY = dolly, DOLLY_STATUS = dolly_status, LOCATION_CODE = location };

                if (dolly.Equals("keep"))
                {
                    strSql = @"UPDATE BP_LOCATION 
                                SET BLOC_TYP =:BLOC_TYP ,UPDATE_TIME=:UPDATE_TIME 
                                WHERE  LOCATION_CODE = :LOCATION_CODE";

                }
                else
                {
                    strSql = @"UPDATE BP_LOCATION 
                                SET BLOC_TYP =:BLOC_TYP ,UPDATE_TIME=:UPDATE_TIME, 
                                    DOLLY = :DOLLY, DOLLY_STATUS = :DOLLY_STATUS 
                                WHERE  LOCATION_CODE = :LOCATION_CODE";
                }

                return SqlHelper.Execute(strSql, condition, _oracleConnStr);

            }
            catch (Exception e)
            {
                throw new Exception("Shenzhou.Dal.CutDal.updateBPLocationStatus>>" + CommonHelper.GetException(e));
            }
        }

        /// <summary>
        /// 更新指定箱号的location，表box_type
        /// </summary>
        /// <param name="boxBarCode"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        public int updateBoxLocation(string boxBarCode, string location)
        {
            try
            {
                string time = DateTime.UtcNow.TimeMillis().ToString();

                string strSql = @"UPDATE BOX_TYPE 
                                    SET LOCATION =:LOCATION ,LAST_UPDATE_TIME=:LAST_UPDATE_TIME 
                                  WHERE  BARCODE = :BARCODE";

                var condition = new { BARCODE = boxBarCode, LAST_UPDATE_TIME = time, LOCATION = location };


                return SqlHelper.Execute(strSql, condition, _oracleConnStr);

            }
            catch (Exception e)
            {
                throw new Exception("Shenzhou.Dal.LocationDal.updateBoxLocation>>" + CommonHelper.GetException(e));
            }
        }

        /// <summary>
        /// 取得location表中bloc_type为0，且location_code以指定区域开头的记录
        /// </summary>
        /// <param name="AREA"></param>
        /// <returns></returns>
        public List<Location> getLocationAvailable(string AREA)
        {
            try
            {
                string time = DateTime.UtcNow.TimeMillis().ToString();

                string strSql = @"select * 
                                    from LOCATION 
                                    WHERE BLOC_TYP = 0 
                                    AND LOCATION_CODE LIKE '" + AREA + "%'";
                
                return SqlHelper.Query<Location>(strSql, null, _oracleConnStr);
            }
            catch (Exception e)
            {
                throw new Exception("Shenzhou.Dal.LocationDal.getLocationAvailable>>" + CommonHelper.GetException(e));
            }
        }

        /// <summary>
        /// 在表BP_LOCATION中，根据area，bloc_typ, dolly_status查找记录，按line排序
        /// </summary>
        /// <param name="area"></param>
        /// <param name="order">排序方式，1ASC，否则DESC</param>
        /// <param name="BLOC_TYP"></param>
        /// <param name="dolly_status"></param>
        /// <returns></returns>
        public List<BP_Location> getBPEmptyDollyLocation(int area, int order, int BLOC_TYP, int dolly_status)
        {
            try
            {
                string time = DateTime.UtcNow.TimeMillis().ToString();
                List<BP_Location> lstResult = new List<BP_Location>();
                string strSql = @"SELECT * 
                                FROM BP_LOCATION 
                                WHERE BLOC_TYP = :BLOC_TYP 
                                        AND DOLLY_STATUS = :DOLLY_STATUS 
                                        AND STATUS = 'Y' 
                                        AND AREA = :AREA 
                                ORDER BY LINE " + (order == 1 ? "ASC" : "DESC");

                var condition = new { BLOC_TYP =BLOC_TYP, DOLLY_STATUS =dolly_status, AREA =area};
                return SqlHelper.Query<BP_Location>(strSql, condition, _oracleConnStr);
            }
            catch (Exception e)
            {
                throw new Exception("Shenzhou.Dal.LocationDal.getBPEmptyDollyLocation>>" + CommonHelper.GetException(e));
            }
        }

        /// <summary>
        /// 在表LOCATION中，根据location_code查找记录，按UPDATE_TIME排序ASC
        /// </summary>
        /// <param name="dest"></param>
        /// <returns></returns>
        public List<Location> SelectLocationAvailableByTimeASC(string dest)
        {
            try
            {
                string time = DateTime.UtcNow.TimeMillis().ToString();

                string strSql = @"SELECT * 
                                    FROM LOCATION 
                                    WHERE LOCATION_CODE LIKE '"+ dest + @"'
                                    AND BLOC_TYP = 0 
                                  ORDER BY UPDATE_TIME ASC";
                
                return SqlHelper.Query<Location>(strSql, null, _oracleConnStr);
            }
            catch (Exception e)
            {
                throw new Exception("Shenzhou.Dal.LocationDal.SelectLocationAvailableByTimeASC>>" + CommonHelper.GetException(e));
            }
        }
    }
}
