using Newtonsoft.Json;
using Shenzhou.Dal;
using Shenzhou.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Shenzhou.Bll
{
    public class AgvBll
    {
        /// <summary>
        /// 处理收到的Agv报文
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public byte[] ProcessMsg(string msg)
        {
            AGVMsg agvMsg = JsonConvert.DeserializeObject<AGVMsg>(msg);
            AgvDal agvDal = new AgvDal();
            SongbuDal songbuDal = new SongbuDal();
            LabelBarcodeDal labelBarcodeDal = new LabelBarcodeDal();
            MssqlDal mssqlDal = new MssqlDal();

            LocationDal locDal = new LocationDal();

            if (agvMsg == null)
            {
                return null;
            }
            if (!agvMsg.TYPE.Equals(GlobalData.LiveMsg))
            {
                agvDal.insertMsgFrom_PLC_CC(agvMsg);
            }
            else
            {
                return null;
            }

            string type = agvMsg.TYPE;
            if (type.Equals(GlobalData.AckMsg))
            {
                agvDal.updateCommandSendSuccess(agvMsg.ID, agvMsg.STATUS);
                string status = agvMsg.STATUS;
                if (status.Equals("00"))
                {
                    string orin = agvMsg.ORI;
                    string commadType = orin.Substring(3, 1);

                    if (!commadType.Equals("R"))
                    {
                        locDal.updateLocationLockStatus(orin, 1);
                        string content = agvMsg.CONTENTS;
                        updateBoxLocationBycontent(content, orin);

                        GlobalData.log.Info(orin + "LOCK");
                    }
                }
                else
                {
                    GlobalData.log.Info(agvMsg.ID + "ERROR CODE:" + status);
                }

                return null;//无需返回信息给agv
            }
            else if (type.Equals(GlobalData.ReciverMsg))
            {
                string orin = agvMsg.ORI;

                if (orin.Substring(3, 1).Equals("K"))
                {
                    locDal.updateBPLocationStatus(orin, 0, "", GlobalData.BP_LOCATION_NODOLLY);
                    if (agvMsg.DES.Substring(3, 1).Equals("C"))
                    {
                        agvDal.insertBP_FULLDOLLY_TRANSPORT(agvMsg.CONTENTS, orin, agvMsg.DES, GlobalData.FULLDOLLY_RETRIVEL, agvMsg.CONTENTS);
                    }
                }
                else if (orin.Substring(3, 1).Equals("S"))
                {
                    locDal.updateLocationLockStatus(orin, 0);
                    GlobalData.log.Info(orin + "UNLOCK");
                }
                else
                {
                    locDal.updateLocationLockStatus(orin, 0);

                    GlobalData.log.Info(orin + "UNLOCK");
                    string content = agvMsg.CONTENTS;
                    updateBoxLocationBycontent(content, "TRANSFERRING");
                }
                agvDal.updateCommandChange(agvMsg.ID, agvMsg.STATUS, type);

                mssqlDal.updateAGVCALLTASK_setArrive(orin, 1);
            }
            else if (type.Equals(GlobalData.CarryCompleteMsg))
            {
                string dest = agvMsg.DES;
                string orin = agvMsg.ORI;

                string[] sendBoxArg;
                string Dolly = "";

                if (orin.Substring(3, 1).Equals("K"))
                {
                    //dao.updateBPLocationStatus(orin, 0,"",Helper.BP_LOCATION_NODOLLY);
                }
                else if (orin.Substring(3, 1).Equals("S")) //松布
                {
                    if (orin.Substring(6, 2).Equals("05"))
                    {
                        if (locDal.getLocationAvailable(orin).Count() != 0)
                        {
                            string FromOrigin = "999";

                            FromOrigin = GetDESTforSBArea(GlobalData.BP_LOCATION_AREA_01, GlobalData.BP_LOCATION_EMPTYDOLLY);  //dolly_status 0 for emptyspace, 1 for fulldolly, 2 for emptydolly
                            //AGVMsg agv = null;
                            if (!FromOrigin.Equals("999"))
                            {

                                COMMAND_LIST agv = new COMMAND_LIST()
                                {
                                    Sender = GlobalData.Sender,
                                    Reciver = GlobalData.Receiver,
                                    Status = "00",
                                    Orign = FromOrigin,
                                    Dest = orin,
                                    ID = Guid.NewGuid().ToString(),
                                    Contents = " ",
                                    Type = "01",
                                    Call_Location = orin
                                };
                                agvDal.insertCOMMAND_LIST(agv);

                                locDal.updateLocationLockStatus(orin, 9);
                                locDal.updateBPLocationStatus(FromOrigin, 9, "keep", 2);
                                GlobalData.log.Info("Info: ASSIGN AN EMPTY DOLLY FOR LOCATIN:" + orin);
                            }
                            else
                            {
                                GlobalData.log.Info("Info: there is no more emptydolly for songbu location:" + orin);
                            }
                        }
                    }
                    else if (orin.Substring(6, 2).Equals("01") || orin.Substring(6, 2).Equals("02"))
                    {
                        if (locDal.getLocationAvailable(orin).Count() != 0)
                        {
                            string FromOrigin = "999";
                            FromOrigin = GetDESTforSBArea(GlobalData.BP_LOCATION_AREA_01, GlobalData.BP_LOCATION_EMPTYDOLLY);  //dolly_status 0 for emptyspace, 1 for fulldolly, 2 for emptydolly

                            if (!FromOrigin.Equals("999"))
                            {
                                COMMAND_LIST agv = new COMMAND_LIST()
                                {
                                    Sender = GlobalData.Sender,
                                    Reciver = GlobalData.Receiver,
                                    Orign = FromOrigin,
                                    Status = "00",
                                    Dest = orin,
                                    ID = Guid.NewGuid().ToString(),
                                    Contents = " ",
                                    Type = "01",
                                    Call_Location = orin
                                };
                                agvDal.insertCOMMAND_LIST(agv);

                                locDal.updateLocationLockStatus(orin, 9);
                                locDal.updateBPLocationStatus(FromOrigin, 9, "keep", 2);
                            }
                            else
                            {
                                GlobalData.log.Info("Info: there is no more emptydolly for songbu location:" + orin);
                            }
                        }
                    }
                }
                else if (dest.Substring(3, 1).Equals("K"))
                {

                    if (agvMsg.CONTENTS != null)
                    {
                        if (agvMsg.CONTENTS.Length > 3)
                        {
                            sendBoxArg = Regex.Split(agvMsg.CONTENTS.Trim(), "|#|");
                            Dolly = sendBoxArg[0];
                        }
                    }

                    locDal.updateBPLocationStatus(dest, 1, Dolly, 1);
                    if (orin.Substring(3, 1).Equals("S"))
                    {
                        agvDal.insertBP_FULLDOLLY_TRANSPORT(Dolly, orin, agvMsg.DES, GlobalData.FULLDOLLY_STORE, agvMsg.CONTENTS);
                    }
                }
                else if (dest.Substring(3, 1).Equals("C"))
                {
                    if (agvMsg.CONTENTS != null)
                    {
                        if (agvMsg.CONTENTS.Length > 3)
                        {
                            Dolly = agvMsg.CONTENTS;
                        }
                    }
                    List<Songbu> result_BP_LABEL = songbuDal.getSongbuListByDollyNo(Dolly);
                    for (int i = 0; i < result_BP_LABEL.Count(); i++)
                    {
                        labelBarcodeDal.updateBP_LabelbarcodeToEXIT(result_BP_LABEL[i].Goods);
                    }
                }
                else
                {
                    locDal.updateLocationLockStatus(dest, 1);
                }
                GlobalData.log.Info(dest + "LOCK");
                string content = agvMsg.CONTENTS;
                updateBoxLocationBycontent(content, dest);
                agvDal.updateCommandChange(agvMsg.ID, agvMsg.STATUS, type);

                mssqlDal.updateAGVCALLTASK_seDone(orin, 2);
            }
            else if (type.Equals(GlobalData.SetEmptyCarry))// 05  tele
            {

            }
            else if (type.Equals(GlobalData.EmptyCarryMoved))//06 tele
            {
                String orin = agvMsg.ORI;
                locDal.updateLocationLockStatus(orin, 0);
                GlobalData.log.Info("EMPTY CAR  " + orin + "UNLOCK");
                agvDal.updateCommandChange(agvMsg.ID, agvMsg.STATUS, type);
            }
            else if (type.Equals(GlobalData.Closed_Command))//07 tele
            {
                String orin = agvMsg.ORI;
                String dest = agvMsg.DES;


                if ((dest.Substring(3, 1).Equals("K")))
                {
                    locDal.updateLocationLockStatus(orin, 0);
                    locDal.updateBPLocationStatus(dest, 0, "keep", 0);
                }
                else
                {
                    locDal.updateLocationLockStatus(orin, 0);
                    locDal.updateBPLocationStatus(dest, 0, "keep", 0);
                }

                if ((orin.Substring(3, 1).Equals("K")))
                {

                }
                else
                {

                }
            }
            else if (type.Equals(GlobalData.AGVRequestResendCommand))//08 tele
            {
                string Sender = agvMsg.SEND;
                if (agvDal.ResendCommandReqByAGV(Sender) > 0 && agvDal.ResendCommandReqByAGVBlockCommand(Sender) > 0)
                {
                    //dao.EmptyLocationTYPE();
                    GlobalData.log.Info("Done : AGVPLC is online and request Server resend command.");
                }
                else
                {
                    GlobalData.log.Info("Fail : AGVPLC is online but resend command fail");
                }
            }
            else if (type.Equals(GlobalData.CallEmptyDollyAway))//10 tele 
            {

            }
            else if (type.Equals(GlobalData.Arrived_AskDest))//11 tele 
            {
                String orin = agvMsg.DES;
                String dest = "999";

                List<Location> loc = locDal.SelectLocationAvailableByTimeASC(orin.Substring(0, 3).ToUpper() + "S01%");
                if (loc.Count() > 0)
                {
                    dest = loc[0].Location_Code;
                }
                GlobalData.log.Info(" assign empty dolly to songbu location order by asc - dest: " + dest);

                if (!dest.Equals("999"))
                {
                    agvDal.updateCommandTypeWithSend(agvMsg.ID, agvMsg.STATUS, "12", orin, dest, " ");
                    locDal.updateBPLocationStatus(dest, 9, "keep", 0);
                }

                if (dest.Equals("999"))
                {
                    dest = GetDESTforSBArea(GlobalData.BP_LOCATION_AREA_01, GlobalData.BP_LOCATION_NODOLLY);
                }

                if (!dest.Equals("999"))
                {
                    agvDal.updateCommandTypeWithSend(agvMsg.ID, agvMsg.STATUS, "12", orin, dest, " ");
                    if ((dest.Substring(3, 1).Equals("K")))
                    {
                        locDal.updateBPLocationStatus(dest, 9, "keep", 0);
                    }
                    else
                    {
                        locDal.updateLocationLockStatus(dest, 9);
                    }
                }
                else
                {
                    agvDal.updateCommandChange(agvMsg.ID, agvMsg.STATUS, type);
                }
            }
            else if (type.Equals(GlobalData.AssignEmptyDollyDest))//12 tele 
            {

            }
            else if (type.Equals(GlobalData.EmptyDollyAway_CompleteMsg))//13 tele 
            {
                string dest = agvMsg.DES;

                if (dest.Substring(3, 1).Equals("K"))
                {
                    locDal.updateBPLocationStatus(dest, 1, "", 2);
                }
                else
                {
                    locDal.updateLocationLockStatus(dest, 1);
                }

                GlobalData.log.Info(dest + "LOCK");
                string content = agvMsg.CONTENTS;
                updateBoxLocationBycontent(content, dest);
                agvDal.updateCommandChange(agvMsg.ID, agvMsg.STATUS, type);
            }

            if (type.Equals(GlobalData.AGVRequestResendCommand))
            {
                agvMsg.TYPE = GlobalData.AGVRequestResendCommand;
                agvMsg.RCV = agvMsg.SEND;
                agvMsg.SEND = "MES";
            }
            else
            {
                agvMsg.TYPE = GlobalData.AckMsg;
            }

            string strMsg = JsonConvert.SerializeObject(agvMsg);
            return Encoding.UTF8.GetBytes(strMsg);
        }

        /// <summary>
        /// 将content中的box对应的location都更新
        /// </summary>
        /// <param name="centent"></param>
        /// <param name="orin"></param>
        private void updateBoxLocationBycontent(string centent, string orin)
        {

            if (centent == null)
            {
                GlobalData.log.Error(orin + "contentisnull");
                return;
            }
            LocationDal locationDal = new LocationDal();
            GlobalData.log.Info("CONTENTS TEXT: " + centent.Trim());
            string[] sendBoxArg = Regex.Split(centent.Trim(), "|#|", RegexOptions.IgnoreCase);// centent.Trim().Split("\\|#\\|");

            foreach (string box in sendBoxArg)
            {
                locationDal.updateBoxLocation(box.Trim(), orin);
                GlobalData.log.Info(box.Trim() + " to " + orin);
            }
        }

        private string GetDESTforSBArea(int Area, int Dolly_Status)
        {  //dolly_status 0 for emptyspace, 1 for fulldolly, 2 for emptydolly
           //area 1 for empty
            string dest = "999";
            int areaNext = Area;

            if (Area == 1)
            {
                areaNext = 2;
            }
            else if (Area == 2)
            {
                areaNext = 1;
            }
            else
            {
                areaNext = Area;
            }

            if (Dolly_Status == GlobalData.BP_LOCATION_EMPTYDOLLY)
            {
                GlobalData.log.Info("go to area:" + Area + " dolly_status:" + GlobalData.BP_LOCATION_EMPTYDOLLY);
                dest = GetBPLocation(Area,
                        GlobalData.BP_ORDER_DESC_EMPTYDOLLY,
                        GlobalData.BP_LOCATION_OCCUPE,
                        GlobalData.BP_LOCATION_EMPTYDOLLY);//order 1 for asc 2 for desc, bloc_tyep 1 for fulldoly, 2 for emptydolly

                if (dest == null)
                {
                    GlobalData.log.Info("trans boxNos" + " dest null");
                }
                else
                {
                    GlobalData.log.Info("trans boxNos dest:" + dest);
                }

                if (dest.Equals("999"))
                {
                    GlobalData.log.Info("go to area next:" + areaNext + " dolly_status:" + GlobalData.BP_LOCATION_EMPTYDOLLY);
                    dest = GetBPLocation(areaNext,
                            GlobalData.BP_ORDER_DESC_EMPTYDOLLY,
                            GlobalData.BP_LOCATION_OCCUPE,
                            GlobalData.BP_LOCATION_EMPTYDOLLY);
                }

                if (dest.Equals("999"))
                {
                    GlobalData.log.Info("go to area last:" + GlobalData.BP_LOCATION_AREA_03 + " dolly_status:" + GlobalData.BP_LOCATION_EMPTYDOLLY);
                    dest = GetBPLocation(GlobalData.BP_LOCATION_AREA_03,
                            GlobalData.BP_ORDER_DESC_EMPTYDOLLY,
                            GlobalData.BP_LOCATION_OCCUPE,
                            GlobalData.BP_LOCATION_EMPTYDOLLY);
                }

            }
            else if (Dolly_Status == GlobalData.BP_LOCATION_NODOLLY)
            {
                GlobalData.log.Info("go to area:" + Area + " dolly_status:" + GlobalData.BP_LOCATION_NODOLLY);
                dest = GetBPLocation(Area,
                        GlobalData.BP_ORDER_ASC_SPACEFORDOLLY,
                        GlobalData.BP_LOCATION_AVAILABLE,
                        GlobalData.BP_LOCATION_NODOLLY);//order 1 for asc 2 for desc, bloc_tyep 1 for fulldoly, 2 for emptydolly

                if (dest.Equals("999"))
                {
                    GlobalData.log.Info("go to area nest:" + areaNext + " dolly_status:" + GlobalData.BP_LOCATION_NODOLLY);
                    dest = GetBPLocation(areaNext,
                            GlobalData.BP_ORDER_ASC_SPACEFORDOLLY,
                            GlobalData.BP_LOCATION_AVAILABLE,
                            GlobalData.BP_LOCATION_NODOLLY);
                }

                if (dest.Equals("999"))
                {
                    GlobalData.log.Info("go to area last:" + GlobalData.BP_LOCATION_AREA_03 + " dolly_status:" + GlobalData.BP_LOCATION_NODOLLY);
                    dest = GetBPLocation(GlobalData.BP_LOCATION_AREA_03,
                            GlobalData.BP_ORDER_ASC_SPACEFORDOLLY,
                            GlobalData.BP_LOCATION_AVAILABLE,
                            GlobalData.BP_LOCATION_NODOLLY);
                }
            }

            return dest;
        }

        private string GetBPLocation(int area, int order, int BLOC_TYP, int dolly_status)
        {
            string dest = "999";
            LocationDal locationDal = new LocationDal();
            List<BP_Location> emptyDollyLocationList = locationDal.getBPEmptyDollyLocation(area, order, BLOC_TYP, dolly_status); //order 1 for asc 2 for desc, bloc_tyep 1 for fulldoly, 2 for emptydolly
            if (emptyDollyLocationList.Count() > 0)
            {
                dest = emptyDollyLocationList[0].LOCATION_CODE;
            }

            if (dest == null)
            {
                dest = "999";
            }

            return dest;
        }

        /// <summary>
        /// 获取需要发送给agv的指令
        /// </summary>
        /// <returns></returns>
        public List<byte[]> SendMsg()
        {
            AgvDal agvDal = new AgvDal();
            List<byte[]> lstSend = new List<byte[]>();
            List<COMMAND_LIST> lstCmd = agvDal.getCommandList();
            foreach (var cmd in lstCmd)
            {
                AGVMsg tmp = getsendMsgInfo(cmd);
                byte[] sendMsg = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(tmp));
                lstSend.Add(sendMsg);
            }
            return lstSend;
        }

        /// <summary>
        /// 将command_list记录转换成发送给agv的指令
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        private AGVMsg getsendMsgInfo(COMMAND_LIST cmd)
        {
            AGVMsg avgMsg = new AGVMsg()
            {
                DES = cmd.Dest,
                ORI = cmd.Orign,
                STATUS = cmd.Type,
                SEND = cmd.Sender,
                RCV = cmd.Reciver,
                ID = cmd.ID,
                CONTENTS = cmd.Contents,
                CallLocation = cmd.Call_Location
            };
            AgvDal agvDal = new AgvDal();
            string type = cmd.Type;
            int intType = -1;
            bool isLiveMsg = false;
            if (type.Equals(GlobalData.CallCarryMsg))
            {
                intType = 1;
            }
            else if (type.Equals(GlobalData.CallEmptyMsg_Carry))
            { //CALLEMPTYMSG
                intType = 2;
            }
            else if (type.Equals(GlobalData.SetEmptyCarry))
            {
                intType = 5;
            }
            else if (type.Equals(GlobalData.Closed_Command))
            {
                intType = 7;
            }
            else if (type.Equals(GlobalData.CallEmptyDollyAway))
            {
                intType = 10;
            }
            else if (type.Equals(GlobalData.AssignEmptyDollyDest))
            {
                intType = 12;
            }
            else if (type.Equals(GlobalData.LiveMsg))
            {
                isLiveMsg = true;
            }
            else
            {
                GlobalData.log.Error("not know type " + type);
                return null;
            }
            if (!isLiveMsg)
            {
                avgMsg.TYPE = intType.ToString();
                //addAgvStatus(avgMsg, intType);
                agvDal.insertMsgToPLC_CC(avgMsg);
            }
            return avgMsg;
        }
    }
}
