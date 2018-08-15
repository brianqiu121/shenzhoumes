using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shenzhou.Models
{
    /// <summary>
    /// MES和AGV间的调度命令
    /// </summary>
    public class COMMAND_LIST
    {
        /// <summary>
        /// 唯一标识,GUID
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// 发送者，如 MES
        /// </summary>
        public string Sender { get; set; }

        /// <summary>
        /// 接收者，如 AGV01
        /// </summary>
        public string Reciver { get; set; }

        /// <summary>
        /// 起始位置
        /// </summary>
        public string Orign { get; set; }

        /// <summary>
        /// 目标位置
        /// </summary>
        public string Dest { get; set; }

        /// <summary>
        /// 指令内容
        /// </summary>
        public string Contents { get; set; }

        /// <summary>
        /// 指令类型
        /// 00:新建 
        /// 01:CALLCARRYMSG 无空车呼叫
        /// 02:CALLEMPTYMSG_CARRY 有空车呼叫
        /// 03:RECIVERMSG 
        /// 04:CARRYCOMPLETEMSG 
        /// 05:SETEMPTYCARRY 06:EMPTYCARRYMOVED 07:CLOSED_COMMAND 08:AGVRequestResendCommand 
        /// 09: 10:CAllEMPTYDOLLYAWAY 11:ARRIVED_ASKDEST 12:ASSIGNEMPTYDOLLYDEST 
        /// 13:EMPTYDOLLYAWAY_COMPLETEMSG 99:LIVEMSG
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 发送状态
        /// </summary>
        public string SendStatus { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public string Update_Time { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public string Create_Time { get; set; }

        /// <summary>
        /// 指令状态
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// 呼叫所在位置，发起指令的人所在位置
        /// </summary>
        public string Call_Location { get; set; }
    }
}
