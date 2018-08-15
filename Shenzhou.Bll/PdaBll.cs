using Shenzhou.Dal;
using Shenzhou.Framwork;
using Shenzhou.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shenzhou.Bll
{
    public class PdaBll
    {
        /// <summary>
        /// 处理pda发来的报文
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public string ProcessMsg(string msg)
        {
            try
            {
                if (msg == null)
                {
                    return null;
                }
                msg = msg.ToUpper();
                string[] arg = msg.Split(',');
                if (arg == null)
                {
                    return null;
                }

                string type = arg[0];
                string barCode = arg[1].ToUpper().Trim();
                string result = null;

                if (type.Equals("21")) //裁剪工位 拉走空车
                { // call empty agv
                    result = callAgvForMoveEmptyDolly(arg[2]);
                }
                else if (type.Equals("22"))  //for s01 area call agv  裁剪任务单单号登记
                {
                    string orderInfo = null;
                    if (arg.Length > 2)
                    {
                        orderInfo = arg[2];
                    }

                    result = HanderCut(barCode, orderInfo);
                }
                else if (type.Equals("23")) //裁剪无空车呼叫
                { //for call agv , type 0 no empty car type1 have empty call

                    result = callAgvToCut(barCode, arg[2], arg[3]);
                }

                return result;
            }
            catch (Exception e)
            {
                GlobalData.log.Error("Shenzhou.Bll.PdaBll.ProcessMsg>>" + CommonHelper.GetException(e));
                return null;
            }
        }

        #region 报文类型为21的处理，裁剪工位 拉走空车
        /// <summary>
        /// 报文类型为21的处理，裁剪工位 拉走空车
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public string callAgvForMoveEmptyDolly(string location)
        {

            GlobalData.log.Info("IN callAgvForMoveEmptyDolly: 1");
            CutDal cutDal = new CutDal();
            AgvDal agvDal = new AgvDal();
            LocationDal locationDal = new LocationDal();
            GlobalData.log.Info("IN callAgvForMoveEmptyDolly: 2");
            if (cutDal.getLocationOrderIsSendForCJMoveEmpty(location).Count() == 0)
            {

                GlobalData.log.Info("call agv to caray emptyDolly from " + location);
                COMMAND_LIST agv = new COMMAND_LIST()
                                        {
                                            Sender =GlobalData.Sender,
                                            Reciver =GlobalData.Receiver,
                                            Orign = location,
                                            Dest = location,
                                            Status="00",
                                            ID= Guid.NewGuid().ToString(),
                                            Contents= "",
                                            Type="10",
                                            Call_Location= location
                                         };// = origin
                agvDal.insertCOMMAND_LIST(agv);
                locationDal.updateLocationLockStatus(location, 9);
            }

            return "SUCCESS";

        }
        #endregion

        #region 报文类型为22的处理,裁剪工位任务单单号登记
        /// <summary>
        /// 报文类型为22的处理，即是从裁剪工位提交的报文
        /// </summary>
        /// <param name="barCode"></param>
        /// <param name="orderInfo"></param>
        /// <returns></returns>
        private string HanderCut(string barCode, string orderInfo)
        {
            if (barCode == null || barCode.Length < 3)
            {
                return "ERROR,1";
            }

            string result;
            if (barCode.StartsWith("4") || barCode.StartsWith("2"))
            {

                GlobalData.log.Info("IN HANDERcj BARCODE MATCH: AND GO TO - getSongbuOrder(barCode)");
                result = getSongbuOrder(barCode);
            }
            else
            {
                String[] arg = orderInfo.Split('@');
                // orderInfo is not identy
                if (arg.Length != 3)
                {
                    return "ERROR,4";
                }

                String orderID = arg[0];

                result = getSongbuHandler(barCode, orderID);

            }
            GlobalData.log.Info("handerCJ result:" + result + " barocde:" + barCode);
            return result;
        }

        /// <summary>
        /// 取得订单信息，先从MES数据库查询，没有则从博大数据库查询，并同步插入到MES数据库
        /// </summary>
        /// <param name="orderTxm"></param>
        /// <returns></returns>
        private string getSongbuOrder(String orderTxm)
        {
            GlobalData.log.Info("in getSongbuOrder barcode:" + orderTxm);

            CutDal cutDal = new CutDal();
            MssqlDal mssqlDal = new MssqlDal();

            List<BP_ORDER> BPlist = cutDal.getBpOrderByOrderID(orderTxm);
            BP_ORDER BP;
            // long count= ORCLdao.getBPorderIsExsit(orderTxm);
            long count = BPlist.Count();
            if (count == 0)
            {
                BPlist = mssqlDal.getBpOrderByBarCode(orderTxm);
                if (BPlist.Count() == 0)
                {
                    return "ERROR,2";
                }
                else
                {
                    GlobalData.log.Info("RETUEN bp ORDER FOUND IN SQLSERVER ");
                    cutDal.insertBP_ORDER(BPlist);
                    BP = BPlist[0];
                    return getOrderReturnInfo("1", BP.HYH, BP.GH, getOrderReturnInfo(BPlist), BPlist.Count);
                }
            }
            else
            {
                GlobalData.log.Info("RETUEN bp ORDER FOUND IN ORCL ");
                BP = BPlist[0];
                return getOrderReturnInfo("1", BP.HYH, BP.GH, getOrderReturnInfo(BPlist), BPlist.Count);

            }
        }

        /// <summary>
        /// 拼接返回的报文
        /// </summary>
        /// <param name="type"></param>
        /// <param name="hyh"></param>
        /// <param name="gh"></param>
        /// <param name="doneCount"></param>
        /// <param name="totalCount"></param>
        /// <returns></returns>
        public string getOrderReturnInfo(String type, String hyh, String gh, long doneCount, int totalCount)
        {
            String result = "SUCCESS," + type + "," + hyh + "," + gh + "," + doneCount + "," + totalCount;
            // String  result="SUCCESS,"+type+",hyh12345678901,gh12345678890152,95,100";

            return result;
        }

        /// <summary>
        /// 计算status为1的记录数
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private int getOrderReturnInfo(List<BP_ORDER> list)
        {
            int count = 0;

            //foreach (BP_ORDER bp in list)
            //{
            //    if (bp.Status != null && bp.Status == 1)
            //    {
            //        count++;
            //    }
            //}
            count = list.Where(r => r.Status == 1).Count();
            return count;
        }

        public string getSongbuHandler(string songbuTxm, string rwdorderID)
        {
            CutDal cutDal = new CutDal();
            SongbuDal songbuDal = new SongbuDal();

            BP_ORDER BP = cutDal.getBPorderInfoBySongBu(songbuTxm).FirstOrDefault();

            if (BP == null)
            {
                return "ERROR,3";
            }
            string songBUorderID = BP.ORDER_ID;

            if (!songBUorderID.Equals(rwdorderID))
            {
                return "ERROR,5";
            }

            cutDal.updateSongBuToCompete(songbuTxm);
            songbuDal.deleteDollyInfoBygoods(songbuTxm);
            List<BP_ORDER> BPlist = cutDal.getBpOrderByOrderID(songBUorderID);
            return getOrderReturnInfo("2", BP.HYH, BP.GH, getOrderReturnInfo(BPlist), BPlist.Count());
        }
        #endregion

        #region 报文类型为23的处理, 裁剪工位 有/无空车 呼叫
        public string callAgvToCut(string orderInfo, string location, string type)
        {
            if (orderInfo == null)
            {
                return "ERROR,3";
            }
            string[] arg = orderInfo.Split('@');

            if (arg.Length != 3)
            {
                return "ERROR,3";
            }

            string orderID = arg[0];
            string hyh = arg[1];
            string gh = arg[2];

            GlobalData.log.Info("ARG[0]: " + arg[0]);
            GlobalData.log.Info("ARG[1]: " + arg[1]);
            GlobalData.log.Info("ARG[2]: " + arg[2]);

            //从MES系统，根据合约号和缸号，查找料车及储位
            CutDal cutDal = new CutDal();
            LocationDal locationDal = new LocationDal();
            AgvDal agvDal = new AgvDal();
            BP_Location sendinfo = cutDal.getLocationForCJByhyhAndgh(hyh, gh);

            if (sendinfo == null || string.IsNullOrWhiteSpace(sendinfo.LOCATION_CODE))
            {
                sendinfo = cutDal.getLocationForCJByGH(gh); //从MES系统，根据缸号，查找料车及储位
            }

            if (sendinfo == null || string.IsNullOrWhiteSpace(sendinfo.LOCATION_CODE))
            {
                return "ERROR,2";
            }

            string orign = sendinfo.LOCATION_CODE;//sendinfo[0];
            string dolly = sendinfo.DOLLY;//sendinfo[1];

            //  if(dao.getLocationOrderIsSend(orign,location,dolly)==0){
            if (cutDal.getLocationOrderIsSendForCJ(location).Count() == 0)
            {
                GlobalData.log.Info("trans dolly" + dolly);

                GlobalData.log.Info("BEGIN SEND 02 TELE " + type);
                COMMAND_LIST agv = new COMMAND_LIST()
                                    {
                                        Sender = GlobalData.Sender,
                                        Reciver = GlobalData.Receiver,
                                        Orign = orign,
                                        Status = "00",
                                        Dest = location,
                                        ID = Guid.NewGuid().ToString(),
                                        Contents = dolly,
                                        Type = "0" + type,
                                        Call_Location = location
                                    };//= dest (location)
                GlobalData.log.Info("END SEND 02 TELE " + type + " AGV TYPE:" + agv.Type);

                agvDal.insertCOMMAND_LIST(agv);
                //dao.updateLocationLockStatus(orign,9);
                locationDal.updateBPLocationStatus(orign, 9, "keep", 2);
            }
            else  //该工位已存在未完成的呼叫
            {
                return "ERROR,9";
            }

            return "SUCCESS";
        }
        #endregion
    }
}
