using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StockServices
{
    public class LocationInfo
    {
        public String LOC_CD { get; set; }
        public String LOC_DESC { get; set; }
        public String LOC_CLS { get; set; }
        public String CRT_BY { get; set; }
        public String CRT_DATE { get; set; }
        public String CRT_MACHINE { get; set; }
        public String UPD_BY { get; set; }
        public String UPD_DATE { get; set; }
        public String UPD_MACHINE { get; set; }
        public String ALLOW_NEGATIVE { get; set; }
        public String MRP_LOCATION { get; set; }
    }
}