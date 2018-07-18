using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StockServices
{
    public class UserDataBarcode
    {
        public string USER_ACCOUNT { set; get; }
        public string LANG_CD { set; get; }
        public string DATE_FORMAT { set; get; }
        public string FULL_NAME { set; get; }
        public bool login_status { set; get; }
    }
}