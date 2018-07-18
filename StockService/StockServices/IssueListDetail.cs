using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StockServices
{
    public class IssueListDetail
    {
        public string ITEM_CD { get; set; }
        public string ITEM_DESC { get; set; }
        public string LOT_NO { get; set; }
        public int REQUEST_QTY { get; set; }
        public int ISSUE_QTY { get; set; }
        public int ONHAND_QTY { get; set; }
        public string TRANS_ID { get; set; }
        public string ISSUEDID { get; set; }
        public string REF_ISSUEDID { get; set; }
        public string LINEID { get; set; }
        public string IMAGEPROFILE { get; set; }
    }
}