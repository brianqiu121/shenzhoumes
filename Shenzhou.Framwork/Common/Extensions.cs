using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shenzhou.Framwork
{
    public static class Extensions
    {
        private static DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public static long TimeMillis(this DateTime d)
        {
            return (long)((d - Jan1st1970).TotalMilliseconds);
        }
        
    }
}
