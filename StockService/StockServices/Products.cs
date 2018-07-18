using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StockServices
{
    public class Products
    {
        public string itemCode { get; set; }
        public string itemName { get; set; }
        public string itemLot { get; set; }
        public string itemQty { get; set; }
        public string itemCateId { get; set; }
    }
}