using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Shenzhou.Models
{
    public class GlobalData
    {
        public static string OracleConnString = "OracleConnStr";
        public static string MSSqlConnString = "MSSqlConnStr";
        public static ILog log = LogManager.GetLogger("Shenzhou.Mes");

        public static string Sender = "MES";
        public static string Receiver = "AGV01";

        public static string LiveMsg = "99";
        public static string AckMsg = "00";
        public static String CallCarryMsg = "01";
        public static String CallEmptyMsg_Carry = "02"; // FOR CAIJIAN
        public static string ReciverMsg = "03";
        public static string CarryCompleteMsg = "04";
        public static String SetEmptyCarry = "05";
        public static String EmptyCarryMoved = "06";
        public static String Closed_Command = "07";
        public static String AGVRequestResendCommand = "08"; //  for resend command to agv
        public static String CallEmptyDollyAway = "10"; // FOR CAIJIAN
        public static String Arrived_AskDest = "11"; // FOR CAIJIAN
        public static String AssignEmptyDollyDest = "12"; // FOR CAIJIAN
        public static String EmptyDollyAway_CompleteMsg = "13"; // FOR CAIJIAN

        public static int BP_LOCATION_NODOLLY = 0;
        public static int BP_LOCATION_FULLDOLLY = 1;
        public static int BP_LOCATION_EMPTYDOLLY = 2;

        public static int FULLDOLLY_STORE = 1;
        public static int FULLDOLLY_RETRIVEL = 2;

        public static int BP_LOCATION_AREA_01 = 1;
        public static int BP_LOCATION_AREA_02 = 2;
        public static int BP_LOCATION_AREA_03 = 3;

        public static int BP_ORDER_ASC_SPACEFORDOLLY = 1;
        public static int BP_ORDER_DESC_EMPTYDOLLY = 2;

        public static int BP_LOCATION_AVAILABLE = 0;
        public static int BP_LOCATION_OCCUPE = 1;
    }
}
