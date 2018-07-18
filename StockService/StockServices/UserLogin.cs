using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StockServices
{
    public class UserLogin
    {
        public string user_accout { set; get; }
        public string user_pass { set; get; }
        public string full_name { set; get; }
        public bool login_status { set; get; }
    }
}