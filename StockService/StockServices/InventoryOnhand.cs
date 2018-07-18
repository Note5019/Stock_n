using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StockServices
{
    public class InventoryOnhand
    {
        public String YEAR_MONTH { get; set; }
        public String PERIOD_BEGIN_DATE { get; set; }
        public String PERIOD_END_DATE { get; set; }
        public String LOCATION { get; set; }
        public String ITEM_CODE { get; set; }
        public String ITEM_DESCRIPTION { get; set; }
        public String LOT_NO { get; set; }
        public String PREVIOUS_BAL { get; set; }
        public String TOTAL_IN_QTY { get; set; }
        public String TOTAL_IN_QTY_NEXT_MONTH { get; set; }
        public String TOTAL_OUT_QTY { get; set; }
        public String TOTAL_ADJ_QTY { get; set; }
        public String ONHAND_QTY { get; set; }
        public String ALLOCATE_QTY { get; set; }
        public String ALLOCATE_QTY_NEXT_MONTH { get; set; }
        public String AVAILABLE_QTY { get; set; }
        public String SAFTY_STOCK { get; set; }
        public String INV_UM_CLS { get; set; }
        public String ITEM_CLS { get; set; }
        public String MODEL { get; set; }
        public String ITEMCATEGORY { get; set; }
        public String LAST_RECEIVE_DATE { get; set; }
        public String SUPP_LOT_NO { get; set; }
        public String QUERY_TYPE { get; set; }
        public String SHORT_NAME { get; set; }
        public String FOR_CUSTOMER { get; set; }
    }
}