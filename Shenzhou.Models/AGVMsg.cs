using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shenzhou.Models
{
    public class AGVMsg
    {
        public string SEND { get; set; }
        public string RCV { get; set; }
        public string TYPE { get; set; }
        public string ORI { get; set; }
        public string DES { get; set; }
        public string STATUS { get; set; }
        public string ID { get; set; }
        public string CONTENTS { get; set; }
        public string CallLocation { get; set; }
    }
}
