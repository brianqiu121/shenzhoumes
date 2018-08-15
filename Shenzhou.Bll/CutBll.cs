using Shenzhou.Dal;
using Shenzhou.Framwork;
using Shenzhou.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shenzhou.Bll
{
    public class CutBll
    {
        private CutDal _dal;

        public CutBll()
        {
            _dal = new CutDal();
        }

        public CutBll(string oracleConnStr, string mssqlConnStr)
        {
            _dal = new CutDal(oracleConnStr, mssqlConnStr);
        }

        
    }
}
