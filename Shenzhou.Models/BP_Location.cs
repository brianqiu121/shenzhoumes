using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shenzhou.Models
{
    public class BP_Location
    {
        /// <summary>
        /// 储位代码
        /// </summary>
        public string LOCATION_CODE { get; set; }

        /// <summary>
        /// 储位状态 9:keep
        /// </summary>
        public string BLOC_TYP { get; set; }

        /// <summary>
        /// 区域
        /// </summary>
        public string AREA { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public string UPDATE_TIME { get; set; }

        /// <summary>
        /// 线别
        /// </summary>
        public string LINE { get; set; }

        /// <summary>
        /// 储位是否可用 Y/N
        /// </summary>
        public string STATUS { get; set; }

        /// <summary>
        /// 料车号
        /// </summary>
        public string DOLLY { get; set; }

        /// <summary>
        /// 料车状态
        /// </summary>
        public string DOLLY_STATUS { get; set; }
    }
}
