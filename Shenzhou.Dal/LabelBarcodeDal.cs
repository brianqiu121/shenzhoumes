using Shenzhou.Framwork;
using Shenzhou.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shenzhou.Dal
{
    public class LabelBarcodeDal
    {
        private string _oracleConnStr;
        private string _mssqlConnStr;

        public LabelBarcodeDal()
        {
            _oracleConnStr = GlobalData.OracleConnString;
            _mssqlConnStr = GlobalData.MSSqlConnString;
        }

        public LabelBarcodeDal(string oracleConnStr, string mssqlConnStr)
        {
            _oracleConnStr = oracleConnStr;
            _mssqlConnStr = mssqlConnStr;
        }

        /// <summary>
        /// 根据barcode更新BP_LABELBARCODE的isexit为1
        /// </summary>
        /// <param name="BARCODE"></param>
        /// <returns></returns>
        public int updateBP_LabelbarcodeToEXIT(string BARCODE)
        {
            try
            {
                string time = DateTime.UtcNow.TimeMillis().ToString();

                string strSql = @"UPDATE BP_LABELBARCODE 
                                    SET IS_EXIT = 1 , 
                                        EXIT_TIME=:EXIT_TIME 
                                    WHERE BARCODE=:BARCODE";

                var condition = new { EXIT_TIME = time, BARCODE = BARCODE };


                return SqlHelper.Execute(strSql, condition, _oracleConnStr);

            }
            catch (Exception e)
            {
                throw new Exception("Shenzhou.Dal.LabelBarcodeDal.updateBP_LabelbarcodeToEXIT>>" + CommonHelper.GetException(e));
            }
        }
    }
}
