using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StockServices
{
    public class InventoryONHList
    {
        public string YEAR_MONTH { get; set; }
        public string ITEM_CD { get; set; }
        public string ITEM_DESC { get; set; }
        public string LOC_CD { get; set; }
        public string LOT_NO { get; set; }
        public int ON_HAND_QTY { get; set; }
    }
}