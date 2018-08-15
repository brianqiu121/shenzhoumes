using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shenzhou.Models
{
    public class BP_ORDER
    {
        /// <summary>
        /// 单号
        /// </summary>
        public string ORDER_ID { get; set; }

        public string TXM { get; set; }

        public string HH { get; set; }

        /// <summary>
        /// 合约号
        /// </summary>
        public string HYH { get; set; }

        public string SH { get; set; }

        public string SC { get; set; }

        /// <summary>
        /// 缸号
        /// </summary>
        public string GH { get; set; }

        public string PS { get; set; }

        public string PH { get; set; }

        /// <summary>
        /// 状态，标记松布是否完成
        /// </summary>
        public int? Status { get; set; }

        /// <summary>
        /// 建立时间，长整型，从1970-01-01起的毫秒
        /// </summary>
        public string CREATE_TIME { get; set; }

        /// <summary>
        /// 更新时间，长整型，从1970-01-01起的毫秒
        /// </summary>
        public string UPDATE_TIME { get; set; }
    }
}
