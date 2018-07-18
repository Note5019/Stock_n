using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StockServices
{
    public class TransactionProducts //s.[ORDER_NUMBER],[ITEM_CD],sd.[DATETIME],[STATUS],[QTY],[USERNAME]
    {
        public string orderNumber { get; set; }
        public string itemCode { get; set; }
        public string datetime { get; set; }
        public string status { get; set; }
        public string qty { get; set; }
        public string username { get; set; }
    }
}