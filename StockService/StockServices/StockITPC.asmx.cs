using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Script.Serialization;
using System.Web.Script.Services;
using System.Web.Services;
using System.Security.Cryptography;
using System.Text;
using EVOFramework.Database;
using System.Diagnostics;


namespace StockServices
{
    /// <summary>
    /// Summary description for StockITPC
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]

    public class StockITPC : System.Web.Services.WebService
    {

        public string HashUserPassword(string UserCD, string Password, bool ignoreUsernameCaseSensitive)
        {
            byte[] byteUpper = null;
            byte[] byteLower = null;

            if (!ignoreUsernameCaseSensitive)
            {
                byteUpper = Encryption.MD5EncryptString(UserCD);
                byteLower = byteUpper;
            }
            else
            {
                byteUpper = Encryption.MD5EncryptString(UserCD.ToUpper());
                byteLower = Encryption.MD5EncryptString(UserCD.ToLower());
            }

            byte[] bytePassword = Encryption.MD5EncryptString(Password);

            string strEnc = string.Empty;
            for (int i = 0; i < byteUpper.Length; i++)
            {
                if (!ignoreUsernameCaseSensitive)
                    strEnc += String.Format("{0:X2}", (byte)(byteUpper[i] ^ bytePassword[i]));
                else
                {
                    strEnc += String.Format("{0:X2}", (byte)((byteUpper[i] ^ byteLower[i]) ^ bytePassword[i]));
                }
            }

            return strEnc;
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public void GetUserLogin(string username, string password)
        {
            string EncryPassword = HashUserPassword(username, password, true);


            List<UserLogin> userLoginList = new List<UserLogin>();
            Boolean checkLogin = false;
            string cs = ConfigurationManager.ConnectionStrings["FLEX_AMT_AUN"].ConnectionString;
            using (SqlConnection con = new SqlConnection(cs))
            {
                SqlCommand cmd = new SqlCommand("EXEC sp_Security_Login '" + username + "','" + EncryPassword + "'");
                cmd.Connection = con;
                con.Open();
                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    if (rdr["LOWER_USER_ACCOUNT"].ToString() == username && rdr["PASS"].ToString() == EncryPassword)
                    {
                        checkLogin = true;
                        var UserList = new UserLogin
                        {
                            login_status = checkLogin,
                            user_accout = rdr["USER_ACCOUNT"].ToString(),
                            user_pass = rdr["PASS"].ToString(),
                            full_name = rdr["FULL_NAME"].ToString()
                        };
                        userLoginList.Add(UserList);
                    }
                }
                con.Close();
            }

            JavaScriptSerializer js = new JavaScriptSerializer();
            Context.Response.Write(js.Serialize(userLoginList));
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public void GetUserLoginWithBarcode(string user_account)
        {
            List<UserDataBarcode> userBarcodeList = new List<UserDataBarcode>();
            Boolean checkLogin = false;
            string cs = ConfigurationManager.ConnectionStrings["FLEX_AMT_AUN"].ConnectionString;
            using (SqlConnection con = new SqlConnection(cs))
            {
                SqlCommand cmd = new SqlCommand("EXEC sp_Login_Barcode_GetAllUser");
                cmd.Connection = con;
                con.Open();
                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    if (rdr["USER_ACCOUNT"].ToString() == user_account)
                    {
                        checkLogin = true;
                        var userBarcode = new UserDataBarcode
                        {
                            login_status = checkLogin,
                            USER_ACCOUNT = rdr["USER_ACCOUNT"].ToString(),
                            LANG_CD = rdr["LANG_CD"].ToString(),
                            DATE_FORMAT = rdr["DATE_FORMAT"].ToString(),
                            FULL_NAME = rdr["FULL_NAME"].ToString()
                        };
                        userBarcodeList.Add(userBarcode);
                    }
                }
                con.Close();
            }

            JavaScriptSerializer js = new JavaScriptSerializer();
            Context.Response.Write(js.Serialize(userBarcodeList));
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public void GetLocationSite()
        {
            List<LocationSite> locationSites = new List<LocationSite>();
            string cs = ConfigurationManager.ConnectionStrings["FLEX_AMT_AUN"].ConnectionString;
            using (SqlConnection con = new SqlConnection(cs))
            {
                SqlCommand cmd = new SqlCommand("EXEC sp_Combo_GetLocationMs");
                cmd.Connection = con;
                con.Open();
                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    var siteList = new LocationSite
                    {
                        VALUE = rdr["VALUE"].ToString(),
                        DISPLAY = rdr["DISPLAY"].ToString(),
                        LOC_CLS = rdr["LOC_CLS"].ToString(),
                        CODE = rdr["CODE"].ToString()
                    };
                    locationSites.Add(siteList);
                }
                con.Close();
            }

            JavaScriptSerializer js = new JavaScriptSerializer();
            Context.Response.Write(js.Serialize(locationSites));
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public void GetIssueListDetailByIssueNo(string issue_no)
        {
            List<IssueListDetail> issueListDetails = new List<IssueListDetail>();
            string cs = ConfigurationManager.ConnectionStrings["FLEX_AMT_AUN"].ConnectionString;
            using (SqlConnection con = new SqlConnection(cs))
            {
                SqlCommand cmd = new SqlCommand("EXEC sp_TRN070_LoadIssueListDetail '" + issue_no + "'");
                cmd.Connection = con;
                con.Open();
                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    var listIssue = new IssueListDetail
                    {
                        ITEM_CD = rdr["ITEM_CD"].ToString(),
                        ITEM_DESC = rdr["ITEM_DESC"].ToString(),
                        LOT_NO = rdr["LOT_NO"].ToString(),
                        REQUEST_QTY = Convert.ToInt32(rdr["REQUEST_QTY"]),
                        ISSUE_QTY = Convert.ToInt32(rdr["ISSUE_QTY"]),
                        ONHAND_QTY = Convert.ToInt32(rdr["ONHAND_QTY"]),
                        TRANS_ID = rdr["TRANS_ID"].ToString(),
                        ISSUEDID = rdr["ISSUEDID"].ToString(),
                        REF_ISSUEDID = rdr["REF_ISSUEDID"].ToString(),
                        LINEID = rdr["LINEID"].ToString(),
                        IMAGEPROFILE = rdr["IMAGEPROFILE"].ToString()
                    };
                    issueListDetails.Add(listIssue);
                }
                con.Close();
            }

            JavaScriptSerializer js = new JavaScriptSerializer();
            Context.Response.Write(js.Serialize(issueListDetails));
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public void GetIssueList()
        {
            List<IssueList> issueLists = new List<IssueList>();
            string cs = ConfigurationManager.ConnectionStrings["FLEX_AMT_AUN"].ConnectionString;
            using (SqlConnection con = new SqlConnection(cs))
            {
                SqlCommand cmd = new SqlCommand("EXEC sp_TRN070_LoadIssueList");
                cmd.Connection = con;
                con.Open();
                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    var listIssue = new IssueList
                    {
                        ISSUE_NO_FROM = rdr["ISSUE_NO_FROM"].ToString(),
                        ISSUE_NO_TO = rdr["ISSUE_NO_TO"].ToString(),
                        LOC_FROM = rdr["LOC_FROM"].ToString(),
                        LOC_MID = rdr["LOC_MID"].ToString(),
                        LOC_TO = rdr["LOC_TO"].ToString()
                    };

                    issueLists.Add(listIssue);
                }
                con.Close();
            }

            JavaScriptSerializer js = new JavaScriptSerializer();
            Context.Response.Write(js.Serialize(issueLists));
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public void GetInvertoryOnHandBySiteDate(string LocationCD, string CurrentDate)
        {
            List<InventoryONHList> inventoryONHLists = new List<InventoryONHList>();
            string cs = ConfigurationManager.ConnectionStrings["FLEX_AMT_AUN"].ConnectionString;
            using (SqlConnection con = new SqlConnection(cs))
            {
                SqlCommand cmd = new SqlCommand("EXEC S_ONH_LoadInventoryOnHandBySiteAndDate '" + LocationCD + "','" + CurrentDate + "'");
                cmd.Connection = con;
                con.Open();
                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    var inventoryONHList = new InventoryONHList
                    {
                        YEAR_MONTH = rdr["YEAR_MONTH"].ToString(),
                        ITEM_CD = rdr["ITEM_CD"].ToString(),
                        ITEM_DESC = rdr["ITEM_DESC"].ToString(),
                        LOC_CD = rdr["LOC_CD"].ToString(),
                        LOT_NO = rdr["LOT_NO"].ToString(),
                        ON_HAND_QTY = Convert.ToInt32(rdr["ON_HAND_QTY"])
                    };

                    inventoryONHLists.Add(inventoryONHList);
                }
                con.Close();
            }

            JavaScriptSerializer js = new JavaScriptSerializer();
            Context.Response.Write(js.Serialize(inventoryONHLists));
        }

        //---------------------------------------------------------------Note 07/11/2018 12:24am
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public String GetRunning_NO_Format(String id_name, String tb_name, String username, String userMachine)
        {
            //id_name = "ISSUE_SLIP_NO" || "TRAN_ID"
            //Get Format
            List<GetRunning_NO_Format> getFormat_ISSUE_SLIP_NO_list = new List<GetRunning_NO_Format>();
            string cs = ConfigurationManager.ConnectionStrings["FLEX_AMT_AUN"].ConnectionString;
            using (SqlConnection con = new SqlConnection(cs))
            {
                SqlCommand cmd = new SqlCommand("exec sp_GetRunning_NO_Format " +
                    "@ID_NAME =  '" + id_name + "'" +
                    ",@TB_NAME = '" + tb_name + "'");
                cmd.Connection = con;
                con.Open();
                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    var getFormat_ISSUE_SLIP_NO = new GetRunning_NO_Format
                    {
                        ID_NAME = rdr["ID_NAME"].ToString(),
                        TB_NAME = rdr["TB_NAME"].ToString(),
                        DESCRIPTION = rdr["DESCRIPTION"].ToString(),
                        FORMAT = rdr["FORMAT"].ToString(),
                        NEXTVALUE = Convert.ToInt32(rdr["NEXTVALUE"]),
                        LAST_RESET = rdr["LAST_RESET"].ToString(),
                        RESET_FLAG_DAY = rdr["RESET_FLAG_DAY"].ToString(),
                        RESET_FLAG_MONTH = rdr["RESET_FLAG_MONTH"].ToString(),
                        RESET_FLAG_YEAR = rdr["RESET_FLAG_YEAR"].ToString(),
                        CRT_BY = rdr["CRT_BY"].ToString(),
                        CRT_DATE = rdr["CRT_DATE"].ToString(),
                        CRT_MACHINE = rdr["CRT_MACHINE"].ToString(),
                        UPD_BY = rdr["UPD_BY"].ToString(),
                        UPD_DATE = rdr["UPD_DATE"].ToString(),
                        UPD_MACHINE = rdr["UPD_MACHINE"].ToString()
                    };
                    getFormat_ISSUE_SLIP_NO_list.Add(getFormat_ISSUE_SLIP_NO);
                }
                con.Close();
            }
            String returnString = getFormat_ISSUE_SLIP_NO_list[0].FORMAT;
            int number = getFormat_ISSUE_SLIP_NO_list[0].NEXTVALUE;
            if (id_name == "ISSUE_SLIP_NO")
            {
                returnString = returnString.Substring(0, 2);
                returnString = returnString + Convert.ToInt32(DateTime.Now.Year).ToString().Substring(2, 2) + Convert.ToInt32(DateTime.Now.Month).ToString("00") + number.ToString("00000");

            }
            else if (id_name == "TRAN_ID")
            {
                returnString = returnString.Substring(0, 1);
                returnString = returnString + Convert.ToInt32(DateTime.Now.Year).ToString().Substring(2, 2) + Convert.ToInt32(DateTime.Now.Month).ToString("00") + number.ToString("0000000");

            }
            //Update Running_NO NextValue
            cs = ConfigurationManager.ConnectionStrings["FLEX_AMT_AUN"].ConnectionString;
            using (SqlConnection con = new SqlConnection(cs))
            {
                SqlCommand cmd = new SqlCommand("EXEC	[dbo].[sp_UpdateRunning_NO_NextValue]" +
                    " @ID_NAME = '" + id_name + "'" +
                    ", @TB_NAME = '" + tb_name + "'" +
                    ", @NEXTVALUE = " + Convert.ToDecimal(getFormat_ISSUE_SLIP_NO_list[0].NEXTVALUE + 1) + " " +
                    ", @LAST_RESET = '" + getFormat_ISSUE_SLIP_NO_list[0].LAST_RESET + "'" +
                    ", @UPD_BY = '" + username + "'" +
                    ",  @UPD_MACHINE = '" + userMachine + "' ");
                cmd.Connection = con;
                con.Open();
                //cmd.ExecuteReader();
                cmd.ExecuteNonQuery();
                con.Close();
            }

            return returnString;
        }
        //---------------------------------------------------------------Note 07/11/2018 17.01pm
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public LocationInfo GetLocationInfo(String locationCode)
        {
            List<LocationInfo> LocactionList = new List<LocationInfo>();
            string cs = ConfigurationManager.ConnectionStrings["FLEX_AMT_AUN"].ConnectionString;
            using (SqlConnection con = new SqlConnection(cs))
            {
                SqlCommand cmd = new SqlCommand("EXEC S_LOC_LoadLocation '" + locationCode + "'");
                cmd.Connection = con;
                con.Open();
                SqlDataReader rdr = cmd.ExecuteReader();

                rdr.Read();
                var LocationDetail = new LocationInfo
                {
                    LOC_CD = rdr["LOC_CD"].ToString(),
                    LOC_DESC = rdr["LOC_DESC"].ToString(),
                    LOC_CLS = rdr["LOC_CLS"].ToString(),
                    CRT_BY = rdr["CRT_BY"].ToString(),
                    CRT_DATE = rdr["CRT_DATE"].ToString(),
                    CRT_MACHINE = rdr["CRT_MACHINE"].ToString(),
                    UPD_BY = rdr["UPD_BY"].ToString(),
                    UPD_DATE = rdr["UPD_DATE"].ToString(),
                    UPD_MACHINE = rdr["UPD_MACHINE"].ToString(),
                    ALLOW_NEGATIVE = rdr["ALLOW_NEGATIVE"].ToString(),
                    MRP_LOCATION = rdr["MRP_LOCATION"].ToString()
                };
                LocactionList.Add(LocationDetail);
                con.Close();
                return LocationDetail;
            }

        }

        //---------------------------------------------------------------Note 07/16/2018 11.56pm
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public void Generate_IS_NO(String username, String userMachine)
        {
            String issue_slip_no = GetRunning_NO_Format("ISSUE_SLIP_NO", "TB_INV_TRANS_TR", username, userMachine);

            JavaScriptSerializer js = new JavaScriptSerializer();
            Context.Response.Write(js.Serialize(issue_slip_no));
        }
        //---------------------------------------------------------------Note 07/11/2018 16:49pm
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public void ComfirmedIssueListDetail(
            String issue_slip_no_main
            , String pITEM_CD
            , String pLOC_CD_From
            , String pLOC_CD_To
            , String pQTY
            , String username
            , String userMachine
            )
        {
            //----InsertTnventoryTransaction
            String pTRANS_ID_From_out = GetRunning_NO_Format("TRAN_ID", "TB_INV_TRANS_TR", username, userMachine);
            String pTRANS_ID_To_in = GetRunning_NO_Format("TRAN_ID", "TB_INV_TRANS_TR", username, userMachine);
            String pLOT_NO = null;
            String pTRANS_DATE_end_date = GetInventoryPeriod().PERIOD_END_DATE;
            String pTRANS_CLS = "10";       //= Issuing?
            String pIN_OUT_CLS_out = "02";
            String pIN_OUT_CLS_in = "01";
            String pOBJ_ITEM_CD = null;
            String pOBJ_ORDER_QTY = null;
            String pREF_SLIP_CLS = "02";
            String pOTHER_DL_NO = null;
            String pREMARK = null;
            String pUPD_BY = null;
            String pUPD_MACHINE = null;
            String pDEALING_NO = null;
            String pSUPP_LOT_NO = null;
            String pPRICE = null;
            String pFOR_CUSTOMER = null;
            String pFOR_MACHINE = null;
            String pSHIFT_CLS = null;
            String pREF_SLIP_NO2 = null;
            String pNG_QTY = null;
            String pTRAN_SUB_CLS = "01";            //What is this???
            String pSCREEN_TYPE = "ISS";
            String pGROUP_TRANS_ID = null;
            String pRESERVE_QTY = null;
            String pNG_REASON = null;

            //-------UpdateTnsertInventoryOnHand
            String pYEAR_MONTH = GetInventoryPeriod().YEAR_MONTH;
            String pTRANS_TYPE = "10";              //=  Issuing
            String pLAST_RECEIVE_DATE = null;
            String pPERIODSTART = GetInventoryPeriod().PERIOD_BEGIN_DATE;

            //-------InsertOrUpdateIssueEntryDetail
            String p_ISSUEDID = null;
            String p_LINEID = null;
            String p_ITEM_DESC = GetItemDescription(pITEM_CD);
            String p_LASTUPDATEBY = null;
            String p_LASTUPDATEMACHINE = null;
            String p_LASTUPDATEDATE = null;
            String p_DELETEFLAG = "N";
            String p_DELETEUSERID = null;
            String p_DELETEMACHINE = null;
            String p_DELETEDATETIME = null;


            //First Transaction [From_out]-------------
            InsertTnventoryTransaction(
                pTRANS_ID_From_out
                , pITEM_CD
                , pLOC_CD_From         //GetLocationInfo(locationCode_From).LOC_CD  //LOC_CD >>From_out 
                , pLOT_NO               //_null
                , pTRANS_DATE_end_date   //TRANS_DATE
                , pTRANS_CLS                  //TRANS_CLS = Issuing
                , pIN_OUT_CLS_out                  //IN_OUT_CLS = "out"
                , pQTY
                , pOBJ_ITEM_CD          //
                , pOBJ_ORDER_QTY        //
                , pTRANS_ID_To_in             //REF_NO
                , ""                         //REF_SLIP_NO From_out >>
                , pREF_SLIP_CLS                       //REF_SLIP_CLS = Issue
                , pOTHER_DL_NO          //
                , issue_slip_no_main              //SLIP_NO
                , pREMARK               //
                , username                   //CRT_BY
                , userMachine                //CRT_MACHINE
                , pUPD_BY               //
                , pUPD_MACHINE          //
                , pDEALING_NO           //
                , pSUPP_LOT_NO          //
                , pPRICE                //
                , pFOR_CUSTOMER         //
                , pFOR_MACHINE          //
                , pSHIFT_CLS            //
                , pREF_SLIP_NO2         //
                , pNG_QTY               //
                , pTRAN_SUB_CLS                       //TRAN_SUB_CLS = ??
                , pSCREEN_TYPE               //SCREEN_TYPE = Issue Entry
                , pGROUP_TRANS_ID       //
                , pRESERVE_QTY          //
                , pNG_REASON            //
                );
            InsertInventoryTransactionExtend(pTRANS_ID_From_out, "10", "0", "0");
            UpdateTnsertInventoryOnHand(
                 pYEAR_MONTH            //pYEAR_MONT
                , pITEM_CD                                  //pITEM_CD
                , pLOC_CD_From                         //GetLocationInfo(locationCode_From).LOC_CD //pLOC_CD
                , pLOT_NO                              //pLOT_NO
                , pIN_OUT_CLS_out                                      //pIN_OUT_CLS = out
                , pQTY                                      //pQTY
                , username                                  //pUPD_BY
                , userMachine                               //pUPD_MACHINE
                , pLAST_RECEIVE_DATE                   //pLAST_RECEIVE_DATE
                , pPERIODSTART    //pPERIODSTART
                , pTRANS_DATE_end_date      //pPERIODEND
                , pSUPP_LOT_NO                         //pSUPP_LOT_NO
                , pTRANS_TYPE                                      //pTRANS_TYPE = Issuing
                );
            //Second Transaction [To_in]-------------
            InsertTnventoryTransaction(
                pTRANS_ID_To_in
                , pITEM_CD
                , pLOC_CD_To           //GetLocationInfo(locationCode_To).LOC_CD  //LOC_CD >>From_out 
                , pLOT_NO               //_null
                , pTRANS_DATE_end_date   //TRANS_DATE
                , pTRANS_CLS                  //TRANS_CLS
                , pIN_OUT_CLS_in                  //IN_OUT_CLS
                , pQTY
                , pOBJ_ITEM_CD           //_null
                , pOBJ_ORDER_QTY         //_null
                , pTRANS_ID_From_out          //REF_NO
                , issue_slip_no_main              //REF_SLIP_NO
                , pREF_SLIP_CLS                       //REF_SLIP_CLS
                , pOTHER_DL_NO          //_null
                , ""                         //SLIP_NO
                , pREMARK               //_null
                , username                   //CRT_BY
                , userMachine                //CRT_MACHINE
                , pUPD_BY               //
                , pUPD_MACHINE          //
                , pDEALING_NO           //
                , pSUPP_LOT_NO          //
                , pPRICE                //
                , pFOR_CUSTOMER         //
                , pFOR_MACHINE          //
                , pSHIFT_CLS            //
                , pREF_SLIP_NO2         //
                , pNG_QTY               //
                , pTRAN_SUB_CLS                       //TRAN_SUB_CLS
                , pSCREEN_TYPE               //SCREEN_TYPE
                , pGROUP_TRANS_ID       //
                , pRESERVE_QTY          //
                , pNG_REASON            //
                );
            InsertInventoryTransactionExtend(pTRANS_ID_To_in, "10", "0", "0");
            UpdateTnsertInventoryOnHand(
                 pYEAR_MONTH            //pYEAR_MONT
                , pITEM_CD                                  //pITEM_CD
                , pLOC_CD_To                           //GetLocationInfo(locationCode_To).LOC_CD   //pLOC_CD
                , pLOT_NO                            //pLOT_NO
                , pIN_OUT_CLS_in                                      //pIN_OUT_CLS = out
                , pQTY                                      //pQTY
                , username                                  //pUPD_BY
                , userMachine                               //pUPD_MACHINE
                , pLAST_RECEIVE_DATE                   //pLAST_RECEIVE_DATE
                , pPERIODSTART             //pPERIODSTART
                , pTRANS_DATE_end_date      //pPERIODEND
                , pSUPP_LOT_NO                         //pSUPP_LOT_NO
                , pTRANS_TYPE                              //pTRANS_TYPE = Issuing
                );
            InsertOrUpdateIssueEntryDetail(
                issue_slip_no_main                               //p_ISSUE_NO
                , p_ISSUEDID                                //null
                , p_LINEID                                  //null
                , pTRANS_CLS                                      //p_TRANS_CLS = "10" = Issuing
                , pIN_OUT_CLS_out                                      //p_IN_OUT_CLS = "02" = out
                , pTRANS_ID_From_out                         //p_TRANS_ID
                , pITEM_CD                                  //p_ITEM_CD
                , p_ITEM_DESC       //p_ITEM_DESC
                , pLOC_CD_From                         //p_LOC_CD
                , pLOT_NO                              //p_LOT_NO
                , pQTY                                      //p_QTY
                , username                                  //p_CREATEBY
                , userMachine                               //p_CREATEMACHINE
                , DateTime.Now.ToString()//p_CREATEDATE
                , p_LASTUPDATEBY
                , p_LASTUPDATEMACHINE
                , p_LASTUPDATEDATE
                , p_DELETEFLAG
                , p_DELETEUSERID
                , p_DELETEMACHINE
                , p_DELETEDATETIME
                );
            InsertOrUpdateIssueEntryDetail(
                issue_slip_no_main                               //p_ISSUE_NO
                , p_ISSUEDID                                //null
                , p_LINEID                                  //null
                , pTRANS_CLS                                      //p_TRANS_CLS = "10" = Issuing
                , pIN_OUT_CLS_in                                      //p_IN_OUT_CLS = "01" = in
                , pTRANS_ID_To_in                         //p_TRANS_ID
                , pITEM_CD                                  //p_ITEM_CD
                , p_ITEM_DESC       //p_ITEM_DESC
                , pLOC_CD_To                           //p_LOC_CD
                , pLOT_NO                              //p_LOT_NO
                , pQTY                                      //p_QTY
                , username                                  //p_CREATEBY
                , userMachine                               //p_CREATEMACHINE
                , DateTime.Now.ToString()//p_CREATEDATE
                , p_LASTUPDATEBY
                , p_LASTUPDATEMACHINE
                , p_LASTUPDATEDATE
                , p_DELETEFLAG                              //p_DELETEFLAG
                , p_DELETEUSERID
                , p_DELETEMACHINE
                , p_DELETEDATETIME
            );

            //JavaScriptSerializer js = new JavaScriptSerializer();
            //Context.Response.Write(js.Serialize(count_row_TRN));
        }

        //---------------------------------------------------------------Note 07/16/2018 15.07pm
        [WebMethod]
        public void FinishingIssue(
            String issue_slip_no_main
            , String issue_slip_no_FROM
            , String pLOC_CD_From
            , String pLOC_CD_To
            , String username
            , String userMachine
            )
        {

            //--------InsertOrUpdateIssueEntryHeader
            String pTRANS_DATE_end_date = GetInventoryPeriod().PERIOD_END_DATE;
            String p_ISSUE_TYPE = "10";
            String p_ISSUE_SUB_TYPE = "01";                 //= Transfer
            String p_JOBHID = "0";
            String p_JOBDID = "0";
            String p_ISSUE_QTY = "0";
            String pREMARK = null;
            String pSCREEN_TYPE = "ISS";
            String p_JOBORDER_NO = null;
            String p_FOR_MACHINE = null;
            String p_FOR_CUSTOMER = null;
            String p_REFDOC_NO = null;
            String p_ORDER_ITEM_CD = null;
            String p_DELETEFLAG = "N";
            String p_DELETEUSERID = null;
            String p_DELETEMACHINE = null;
            String p_DELETEDATETIME = null;


            InsertOrUpdateIssueEntryHeader(
                issue_slip_no_main                               //p_ISSUE_NO
                , pTRANS_DATE_end_date      //p_ISSUE_DATE
                , p_ISSUE_TYPE                                      //p_ISSUE_TYPE = 10 ?????????????
                , p_ISSUE_SUB_TYPE                                      //p_ISSUE_SUB_TYPE = 01 = Transfer
                , p_JOBORDER_NO
                , p_JOBHID                                       //p_JOBHID
                , p_JOBDID                                       //p_JOBDID
                , p_FOR_MACHINE
                , p_FOR_CUSTOMER
                , p_REFDOC_NO
                , p_ORDER_ITEM_CD
                , pLOC_CD_From                         //p_FR_LOC
                , pLOC_CD_To                           //p_TO_LOC
                , p_ISSUE_QTY                                       //p_ISSUE_QTY
                , pREMARK
                , pSCREEN_TYPE                              //p_SCREEN_TYPE
                , username                                  //p_CREATEBY
                , userMachine                               //p_CREATEMACHINE
                , DateTime.Now.ToString()                   //p_CREATEDATE
                , username                                  //p_LASTUPDATEBY
                , userMachine                               //p_LASTUPDATEMACHINE
                , DateTime.Now.ToString()                   //p_LASTUPDATEDATE
                , p_DELETEFLAG                              //p_DELETEFLAG
                , p_DELETEUSERID
                , p_DELETEMACHINE
                , p_DELETEDATETIME
                , issue_slip_no_FROM
                );
        }
        //---------------------------------------------------------------Note 07/12/2018 14.19pm
        private int? ConvertStringToInt(String text)
        {
            int i = 0;
            return (Int32.TryParse(text, out i) ? i : (int?)null);
        }
        //---------------------------------------------------------------Note 07/12/2018 14.58pm
        private String ThisStringIsNullOrEmpty(String text)
        {
            return (String.IsNullOrEmpty(text) == true) ? null : text;
        }
        //---------------------------------------------------------------Note 07/11/2018 17.58pm
        [WebMethod]
        public Int32 InsertTnventoryTransaction(
            String pTRANS_ID
            , String pITEM_CD
            , String pLOC_CD
            , String pLOT_NO
            , String pTRANS_DATE
            , String pTRANS_CLS
            , String pIN_OUT_CLS
            , String pQTY
            , String pOBJ_ITEM_CD
            , String pOBJ_ORDER_QTY
            , String pREF_NO
            , String pREF_SLIP_NO
            , String pREF_SLIP_CLS
            , String pOTHER_DL_NO
            , String pSLIP_NO
            , String pREMARK
            , String pCRT_BY
            , String pCRT_MACHINE
            , String pUPD_BY
            , String pUPD_MACHINE
            , String pDEALING_NO
            , String pSUPP_LOT_NO
            , String pPRICE
            , String pFOR_CUSTOMER
            , String pFOR_MACHINE
            , String pSHIFT_CLS
            , String pREF_SLIP_NO2
            , String pNG_QTY
            , String pTRAN_SUB_CLS
            , String pSCREEN_TYPE
            , String pGROUP_TRANS_ID
            , String pRESERVE_QTY
            , String pNG_REASON
            )
        {
            Int32 tran_count;
            string cs = ConfigurationManager.ConnectionStrings["FLEX_AMT_AUN"].ConnectionString;
            using (SqlConnection con = new SqlConnection(cs))
            {
                SqlCommand cmd = new SqlCommand("EXEC S_TRN_InsertInventoryTransaction " +
                    "@pTRANS_ID" +
                    ",@pITEM_CD" +
                    ",@pLOC_CD" +
                    ",@pLOT_NO" +
                    ",@pTRANS_DATE" +
                    ",@pTRANS_CLS" +
                    ",@pIN_OUT_CLS" +
                    ",@pQTY" +
                    ",@pOBJ_ITEM_CD" +
                    ",@pOBJ_ORDER_QTY" +
                    ",@pREF_NO" +
                    ",@pREF_SLIP_NO" +
                    ",@pREF_SLIP_CLS" +
                    ",@pOTHER_DL_NO" +
                    ",@pSLIP_NO" +
                    ",@pREMARK" +
                    ",@pCRT_BY" +
                    ",@pCRT_MACHINE" +
                    ",@pUPD_BY" +
                    ",@pUPD_MACHINE" +
                    ",@pDEALING_NO" +
                    ",@pSUPP_LOT_NO" +
                    ",@pPRICE" +
                    ",@pFOR_CUSTOMER" +
                    ",@pFOR_MACHINE" +
                    ",@pSHIFT_CLS" +
                    ",@pREF_SLIP_NO2" +
                    ",@pNG_QTY" +
                    ",@pTRAN_SUB_CLS" +
                    ",@pSCREEN_TYPE" +
                    ",@pGROUP_TRANS_ID" +
                    ",@pRESERVE_QTY" +
                    ",@pNG_REASON"
                );
                cmd.Connection = con;
                cmd.Prepare();
                SetPreparedStatementSQLByAddWithValue(cmd, "@pTRANS_ID", pTRANS_ID);
                SetPreparedStatementSQLByAddWithValue(cmd, "@pITEM_CD", pITEM_CD);
                SetPreparedStatementSQLByAddWithValue(cmd, "@pLOC_CD", pLOC_CD);
                SetPreparedStatementSQLByAddWithValue(cmd, "@pLOT_NO", pLOT_NO);
                SetPreparedStatementSQLByAddWithValue(cmd, "@pTRANS_DATE", pTRANS_DATE);
                SetPreparedStatementSQLByAddWithValue(cmd, "@pTRANS_CLS", pTRANS_CLS);
                SetPreparedStatementSQLByAddWithValue(cmd, "@pIN_OUT_CLS", pIN_OUT_CLS);
                SetPreparedStatementSQLByAddWithValue(cmd, "@pQTY", pQTY, true);
                SetPreparedStatementSQLByAddWithValue(cmd, "@pOBJ_ITEM_CD", pOBJ_ITEM_CD);
                SetPreparedStatementSQLByAddWithValue(cmd, "@pOBJ_ORDER_QTY", pOBJ_ORDER_QTY, true);
                SetPreparedStatementSQLByAddWithValue(cmd, "@pREF_NO", pREF_NO);
                SetPreparedStatementSQLByAddWithValue(cmd, "@pREF_SLIP_NO", pREF_SLIP_NO);
                SetPreparedStatementSQLByAddWithValue(cmd, "@pREF_SLIP_CLS", pREF_SLIP_CLS);
                SetPreparedStatementSQLByAddWithValue(cmd, "@pOTHER_DL_NO", pOTHER_DL_NO);
                SetPreparedStatementSQLByAddWithValue(cmd, "@pSLIP_NO", pSLIP_NO);
                SetPreparedStatementSQLByAddWithValue(cmd, "@pREMARK", pREMARK);
                SetPreparedStatementSQLByAddWithValue(cmd, "@pCRT_BY", pCRT_BY);
                SetPreparedStatementSQLByAddWithValue(cmd, "@pCRT_MACHINE", pCRT_MACHINE);
                SetPreparedStatementSQLByAddWithValue(cmd, "@pUPD_BY", pUPD_BY);
                SetPreparedStatementSQLByAddWithValue(cmd, "@pUPD_MACHINE", pUPD_MACHINE);
                SetPreparedStatementSQLByAddWithValue(cmd, "@pDEALING_NO", pDEALING_NO);
                SetPreparedStatementSQLByAddWithValue(cmd, "@pSUPP_LOT_NO", pSUPP_LOT_NO);
                SetPreparedStatementSQLByAddWithValue(cmd, "@pPRICE", pPRICE, true);
                SetPreparedStatementSQLByAddWithValue(cmd, "@pFOR_CUSTOMER", pFOR_CUSTOMER);
                SetPreparedStatementSQLByAddWithValue(cmd, "@pFOR_MACHINE", pFOR_MACHINE);
                SetPreparedStatementSQLByAddWithValue(cmd, "@pSHIFT_CLS", pSHIFT_CLS);
                SetPreparedStatementSQLByAddWithValue(cmd, "@pREF_SLIP_NO2", pREF_SLIP_NO2);
                SetPreparedStatementSQLByAddWithValue(cmd, "pNG_QTY", pNG_QTY, true);
                SetPreparedStatementSQLByAddWithValue(cmd, "@pTRAN_SUB_CLS", pTRAN_SUB_CLS);
                SetPreparedStatementSQLByAddWithValue(cmd, "@pSCREEN_TYPE", pSCREEN_TYPE);
                SetPreparedStatementSQLByAddWithValue(cmd, "@pGROUP_TRANS_ID", pGROUP_TRANS_ID);
                SetPreparedStatementSQLByAddWithValue(cmd, "pRESERVE_QTY", pRESERVE_QTY, true);
                SetPreparedStatementSQLByAddWithValue(cmd, "@pNG_REASON", pNG_REASON);

                con.Open();

                tran_count = cmd.ExecuteNonQuery();
                con.Close();

            }
            return tran_count;
        }

        //---------------------------------------------------------------Note 07/12/2018 17.20pm
        public void SetPreparedStatementSQLByAddWithValue(SqlCommand cmd, String sqlparameterName, String sqlparameterValue)
        {
            if (String.IsNullOrEmpty(sqlparameterValue))
            {
                cmd.Parameters.AddWithValue(sqlparameterName, DBNull.Value);
            }
            else cmd.Parameters.AddWithValue(sqlparameterName, sqlparameterValue);
        }

        //---------------------------------------------------------------Note 07/12/2018 17.35pm
        public void SetPreparedStatementSQLByAddWithValue(SqlCommand cmd, String sqlparameterName, String sqlparameterValue, bool isNumber)
        {
            if (String.IsNullOrEmpty(sqlparameterValue))
            {
                cmd.Parameters.AddWithValue(sqlparameterName, DBNull.Value);
            }
            else cmd.Parameters.AddWithValue(sqlparameterName, (ConvertStringToInt(sqlparameterValue)));
        }

        //---------------------------------------------------------------Note 07/12/2018 17.35pm
        public void SetPreparedStatementSQLByAddWithValue(SqlCommand cmd, String sqlparameterName, DateTime dateValue)
        {
            cmd.Parameters.AddWithValue(sqlparameterName, dateValue);
        }
        //---------------------------------------------------------------Note 07/12/2018 10.02pm
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public InventoryPeriod GetInventoryPeriod()
        {
            string cs = ConfigurationManager.ConnectionStrings["FLEX_AMT_AUN"].ConnectionString;
            using (SqlConnection con = new SqlConnection(cs))
            {
                SqlCommand cmd = new SqlCommand("EXEC sp_Common_GetInventoryPeriod");
                cmd.Connection = con;
                con.Open();
                SqlDataReader rdr = cmd.ExecuteReader();

                rdr.Read();
                var InventoryPeriod_date = new InventoryPeriod
                {
                    YEAR_MONTH = rdr["YEAR_MONTH"].ToString(),
                    PERIOD_BEGIN_DATE = rdr["PERIOD_BEGIN_DATE"].ToString(),
                    PERIOD_END_DATE = rdr["PERIOD_END_DATE"].ToString()
                };
                con.Close();
                return InventoryPeriod_date;
            }
        }

        //---------------------------------------------------------------Note 07/12/2018 10.54am
        [WebMethod]
        public Int32 InsertInventoryTransactionExtend(String p_TRANSID, String p_TRANS_CLS, String p_TRANS_REF_HID, String p_TRANS_REF_DID)
        {
            Int32 tran_count;
            string cs = ConfigurationManager.ConnectionStrings["FLEX_AMT_AUN"].ConnectionString;
            using (SqlConnection con = new SqlConnection(cs))
            {
                SqlCommand cmd = new SqlCommand("EXEC S_TRN_InsertInventoryTransactionExtend " +
                    "@p_TRANSID" +
                    ",	@p_TRANS_CLS" +
                    ",@p_TRANS_REF_HID" +
                    ",@p_TRANS_REF_DID"
                    );
                cmd.Connection = con;
                cmd.Prepare();
                SetPreparedStatementSQLByAddWithValue(cmd, "@p_TRANSID", p_TRANSID);
                SetPreparedStatementSQLByAddWithValue(cmd, "@p_TRANS_CLS", p_TRANS_CLS);
                SetPreparedStatementSQLByAddWithValue(cmd, "@p_TRANS_REF_HID", p_TRANS_REF_HID, true);
                SetPreparedStatementSQLByAddWithValue(cmd, "@p_TRANS_REF_DID", p_TRANS_REF_DID, true);

                con.Open();
                tran_count = cmd.ExecuteNonQuery();

                con.Close();
            }
            return tran_count;
        }
        //---------------------------------------------------------------Note 07/13/2018 10.17pm
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public void UpdateTnsertInventoryOnHand(
           String pYEAR_MONT
            , String pITEM_CD
            , String pLOC_CD
            , String pLOT_NO
            , String pIN_OUT_CLS
            , String pQTY
            , String pUPD_BY
            , String pUPD_MACHINE
            , String pLAST_RECEIVE_DATE
            , String pPERIODSTART
            , String pPERIODEND
            , String pSUPP_LOT_NO
            , String pTRANS_TYPE
            )
        {
            string cs = ConfigurationManager.ConnectionStrings["FLEX_AMT_AUN"].ConnectionString;
            using (SqlConnection con = new SqlConnection(cs))
            {
                SqlCommand cmd = new SqlCommand("EXEC S_ONH_UpdateInsertInventoryOnHand " +
                    "@pYEAR_MONT" +
                    ",@pITEM_CD" +
                    ",@pLOC_CD" +
                    ",@pLOT_NO" +
                    ",@pIN_OUT_CLS" +
                    ",@pQTY" +
                    ",@pUPD_BY" +
                    ",@pUPD_MACHINE" +
                    ",@pLAST_RECEIVE_DATE" +
                    ",@pPERIODSTART" +
                    ",@pPERIODEND" +
                    ",@pSUPP_LOT_NO" +
                    ",@pTRANS_TYPE"
                    );
                cmd.Connection = con;
                cmd.Prepare();
                SetPreparedStatementSQLByAddWithValue(cmd, "@pYEAR_MONT", pYEAR_MONT);
                SetPreparedStatementSQLByAddWithValue(cmd, "@pITEM_CD", pITEM_CD);
                SetPreparedStatementSQLByAddWithValue(cmd, "@pLOC_CD", pLOC_CD);
                SetPreparedStatementSQLByAddWithValue(cmd, "@pLOT_NO", pLOT_NO);
                SetPreparedStatementSQLByAddWithValue(cmd, "@pIN_OUT_CLS", pIN_OUT_CLS);
                SetPreparedStatementSQLByAddWithValue(cmd, "@pQTY", pQTY, true);
                SetPreparedStatementSQLByAddWithValue(cmd, "@pUPD_BY", pUPD_BY);
                SetPreparedStatementSQLByAddWithValue(cmd, "@pUPD_MACHINE", pUPD_MACHINE);
                SetPreparedStatementSQLByAddWithValue(cmd, "@pLAST_RECEIVE_DATE", pLAST_RECEIVE_DATE);
                SetPreparedStatementSQLByAddWithValue(cmd, "@pPERIODSTART", pPERIODSTART);
                SetPreparedStatementSQLByAddWithValue(cmd, "@pPERIODEND", pPERIODEND);
                SetPreparedStatementSQLByAddWithValue(cmd, "@pSUPP_LOT_NO", pSUPP_LOT_NO);
                SetPreparedStatementSQLByAddWithValue(cmd, "@pTRANS_TYPE", pTRANS_TYPE);

                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
            }

        }

        //---------------------------------------------------------------Note 07/13/2018 11.32am
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public void InsertOrUpdateIssueEntryDetail(
            String p_ISSUE_NO
            , String p_ISSUEDID
            , String p_LINEID
            , String p_TRANS_CLS
            , String p_IN_OUT_CLS
            , String p_TRANS_ID
            , String p_ITEM_CD
            , String p_ITEM_DESC
            , String p_LOC_CD
            , String p_LOT_NO
            , String p_QTY
            , String p_CREATEBY
            , String p_CREATEMACHINE
            , String p_CREATEDATE
            , String p_LASTUPDATEBY
            , String p_LASTUPDATEMACHINE
            , String p_LASTUPDATEDATE
            , String p_DELETEFLAG
            , String p_DELETEUSERID
            , String p_DELETEMACHINE
            , String p_DELETEDATETIME
            )
        {
            string cs = ConfigurationManager.ConnectionStrings["FLEX_AMT_AUN"].ConnectionString;
            using (SqlConnection con = new SqlConnection(cs))
            {
                SqlCommand cmd = new SqlCommand("EXEC sp_TRN170_InsertOrUpdateIssueEntryDetail " +
                    "@p_ISSUE_NO" +
                    ",@p_ISSUEDID" +
                    ",@p_LINEID" +
                    ",@p_TRANS_CLS" +
                    ",@p_IN_OUT_CLS" +
                    ",@p_TRANS_ID" +
                    ",@p_ITEM_CD" +
                    ",@p_ITEM_DESC" +
                    ",@p_LOC_CD" +
                    ",@p_LOT_NO" +
                    ",@p_QTY" +
                    ",@p_CREATEBY" +
                    ",@p_CREATEMACHINE" +
                    ",@p_CREATEDATE" +
                    ",@p_LASTUPDATEBY" +
                    ",@p_LASTUPDATEMACHINE" +
                    ",@p_LASTUPDATEDATE" +
                    ",@p_DELETEFLAG" +
                    ",@p_DELETEUSERID" +
                    ",@p_DELETEMACHINE" +
                    ",@p_DELETEDATETIME"
                    );
                cmd.Connection = con;
                cmd.Prepare();
                SetPreparedStatementSQLByAddWithValue(cmd, "@p_ISSUE_NO", p_ISSUE_NO);
                SetPreparedStatementSQLByAddWithValue(cmd, "@p_ISSUEDID", p_ISSUEDID, true);
                SetPreparedStatementSQLByAddWithValue(cmd, "@p_LINEID", p_LINEID, true);
                SetPreparedStatementSQLByAddWithValue(cmd, "@p_TRANS_CLS", p_TRANS_CLS);
                SetPreparedStatementSQLByAddWithValue(cmd, "@p_IN_OUT_CLS", p_IN_OUT_CLS);
                SetPreparedStatementSQLByAddWithValue(cmd, "@p_TRANS_ID", p_TRANS_ID);
                SetPreparedStatementSQLByAddWithValue(cmd, "@p_ITEM_CD", p_ITEM_CD);
                SetPreparedStatementSQLByAddWithValue(cmd, "@p_ITEM_DESC", p_ITEM_DESC);
                SetPreparedStatementSQLByAddWithValue(cmd, "@p_LOC_CD", p_LOC_CD);
                SetPreparedStatementSQLByAddWithValue(cmd, "@p_LOT_NO", p_LOT_NO);
                SetPreparedStatementSQLByAddWithValue(cmd, "@p_QTY", p_QTY, true);
                SetPreparedStatementSQLByAddWithValue(cmd, "@p_CREATEBY", p_CREATEBY);
                SetPreparedStatementSQLByAddWithValue(cmd, "@p_CREATEMACHINE", p_CREATEMACHINE);
                SetPreparedStatementSQLByAddWithValue(cmd, "@p_CREATEDATE", p_CREATEDATE);
                SetPreparedStatementSQLByAddWithValue(cmd, "@p_LASTUPDATEBY", p_LASTUPDATEBY);
                SetPreparedStatementSQLByAddWithValue(cmd, "@p_LASTUPDATEMACHINE", p_LASTUPDATEMACHINE);
                SetPreparedStatementSQLByAddWithValue(cmd, "@p_LASTUPDATEDATE", p_LASTUPDATEDATE);
                SetPreparedStatementSQLByAddWithValue(cmd, "@p_DELETEFLAG", p_DELETEFLAG);
                SetPreparedStatementSQLByAddWithValue(cmd, "@p_DELETEUSERID", p_DELETEUSERID);
                SetPreparedStatementSQLByAddWithValue(cmd, "@p_DELETEMACHINE", p_DELETEMACHINE);
                SetPreparedStatementSQLByAddWithValue(cmd, "@p_DELETEDATETIME", p_DELETEDATETIME);
                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
            }
        }
        //---------------------------------------------------------------Note 07/13/2018 13.40am
        public String GetItemDescription(String itemCode)
        {
            String itemDesc = null;
            string cs = ConfigurationManager.ConnectionStrings["FLEX_AMT_AUN"].ConnectionString;
            using (SqlConnection con = new SqlConnection(cs))
            {
                SqlCommand cmd = new SqlCommand("EXEC S_ITM_LoadItem '" + itemCode + "'");
                cmd.Connection = con;
                con.Open();
                SqlDataReader rdr = cmd.ExecuteReader();

                rdr.Read();

                itemDesc = rdr["ITEM_DESC"].ToString();
                con.Close();
                return itemDesc;
            }

        }

        //---------------------------------------------------------------Note 07/13/2018 14.32am
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public void InsertOrUpdateIssueEntryHeader(
            String @p_ISSUE_NO
            , String p_ISSUE_DATE
            , String p_ISSUE_TYPE
            , String p_ISSUE_SUB_TYPE
            , String p_JOBORDER_NO
            , String p_JOBHID
            , String p_JOBDID
            , String p_FOR_MACHINE
            , String p_FOR_CUSTOMER
            , String p_REFDOC_NO
            , String p_ORDER_ITEM_CD
            , String p_FR_LOC
            , String p_TO_LOC
            , String p_ISSUE_QTY
            , String p_REMARK
            , String p_SCREEN_TYPE
            , String p_CREATEBY
            , String p_CREATEMACHINE
            , String p_CREATEDATE
            , String p_LASTUPDATEBY
            , String p_LASTUPDATEMACHINE
            , String p_LASTUPDATEDATE
            , String p_DELETEFLAG
            , String p_DELETEUSERID
            , String p_DELETEMACHINE
            , String p_DELETEDATETIME
            , String p_ISSUE_NO_FROM
            )
        {
            string cs = ConfigurationManager.ConnectionStrings["FLEX_AMT_AUN"].ConnectionString;
            using (SqlConnection con = new SqlConnection(cs))
            {
                SqlCommand cmd = new SqlCommand("EXEC sp_TRN170_InsertOrUpdateIssueEntryHeader " +
                      " @p_ISSUE_NO" +
                      ",@p_ISSUE_DATE" +
                      ",@p_ISSUE_TYPE" +
                      ",@p_ISSUE_SUB_TYPE" +
                      ",@p_JOBORDER_NO" +
                      ",@p_JOBHID" +
                      ",@p_JOBDID" +
                      ",@p_FOR_MACHINE" +
                      ",@p_FOR_CUSTOMER" +
                      ",@p_REFDOC_NO" +
                      ",@p_ORDER_ITEM_CD" +
                      ",@p_FR_LOC" +
                      ",@p_TO_LOC" +
                      ",@p_ISSUE_QTY" +
                      ",@p_REMARK" +
                      ",@p_SCREEN_TYPE" +
                      ",@p_CREATEBY" +
                      ",@p_CREATEMACHINE" +
                      ",@p_CREATEDATE" +
                      ",@p_LASTUPDATEBY" +
                      ",@p_LASTUPDATEMACHINE" +
                      ",@p_LASTUPDATEDATE" +
                      ",@p_DELETEFLAG" +
                      ",@p_DELETEUSERID" +
                      ",@p_DELETEMACHINE" +
                      ",@p_DELETEDATETIME" +
                      ",@p_ISSUE_NO_FROM"
                    );
                cmd.Connection = con;
                cmd.Prepare();
                SetPreparedStatementSQLByAddWithValue(cmd, "@p_ISSUE_NO", p_ISSUE_NO);
                SetPreparedStatementSQLByAddWithValue(cmd, "@p_ISSUE_DATE", p_ISSUE_DATE);
                SetPreparedStatementSQLByAddWithValue(cmd, "@p_ISSUE_TYPE", p_ISSUE_TYPE);
                SetPreparedStatementSQLByAddWithValue(cmd, "@p_ISSUE_SUB_TYPE", p_ISSUE_SUB_TYPE);
                SetPreparedStatementSQLByAddWithValue(cmd, "@p_JOBORDER_NO", p_JOBORDER_NO);
                SetPreparedStatementSQLByAddWithValue(cmd, "@p_JOBHID", p_JOBHID, true);
                SetPreparedStatementSQLByAddWithValue(cmd, "@p_JOBDID", p_JOBDID, true);
                SetPreparedStatementSQLByAddWithValue(cmd, "@p_FOR_MACHINE", p_FOR_MACHINE);
                SetPreparedStatementSQLByAddWithValue(cmd, "@p_FOR_CUSTOMER", p_FOR_CUSTOMER);
                SetPreparedStatementSQLByAddWithValue(cmd, "@p_REFDOC_NO", p_REFDOC_NO);
                SetPreparedStatementSQLByAddWithValue(cmd, "@p_ORDER_ITEM_CD", p_ORDER_ITEM_CD);
                SetPreparedStatementSQLByAddWithValue(cmd, "@p_FR_LOC", p_FR_LOC);
                SetPreparedStatementSQLByAddWithValue(cmd, "@p_TO_LOC", p_TO_LOC);
                SetPreparedStatementSQLByAddWithValue(cmd, "@p_ISSUE_QTY", p_ISSUE_QTY, true);
                SetPreparedStatementSQLByAddWithValue(cmd, "@p_REMARK", p_REMARK);
                SetPreparedStatementSQLByAddWithValue(cmd, "@p_SCREEN_TYPE", p_SCREEN_TYPE);
                SetPreparedStatementSQLByAddWithValue(cmd, "@p_CREATEBY", p_CREATEBY);
                SetPreparedStatementSQLByAddWithValue(cmd, "@p_CREATEMACHINE", p_CREATEMACHINE);
                SetPreparedStatementSQLByAddWithValue(cmd, "@p_CREATEDATE", p_CREATEDATE);
                SetPreparedStatementSQLByAddWithValue(cmd, "@p_LASTUPDATEBY", p_LASTUPDATEBY);
                SetPreparedStatementSQLByAddWithValue(cmd, "@p_LASTUPDATEMACHINE", p_LASTUPDATEMACHINE);
                SetPreparedStatementSQLByAddWithValue(cmd, "@p_LASTUPDATEDATE", p_LASTUPDATEDATE);
                SetPreparedStatementSQLByAddWithValue(cmd, "@p_DELETEFLAG", p_DELETEFLAG);
                SetPreparedStatementSQLByAddWithValue(cmd, "@p_DELETEUSERID", p_DELETEUSERID);
                SetPreparedStatementSQLByAddWithValue(cmd, "@p_DELETEMACHINE", p_DELETEMACHINE);
                SetPreparedStatementSQLByAddWithValue(cmd, "@p_DELETEDATETIME", p_DELETEDATETIME);
                SetPreparedStatementSQLByAddWithValue(cmd, "@p_ISSUE_NO_FROM", p_ISSUE_NO_FROM);
                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
            }
        }

        //---------------------------------------------------------------Note 07/18/2018 13.40am
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public void GetInventoryOnhand(
            String p_ITEMCATEGORYID
            , String p_ITEM_TYPE
            , String p_ITEM_CD
            , String p_LOT_NO
            , String p_LOC_CD
            )
        {
            CurrentYearMonth p_YEARMONTH = GetCurrentYearMonth();
            String p_ONHAND_LESS_THAN_ZERO = "N";
            String p_ONHAND_LESS_THAN_SAFTYSTOCK = "N";
            String p_GROUP_BY_ITEM = "Y";
            List<InventoryOnhand> InventoryOnhandList = new List<InventoryOnhand>();
            string cs = ConfigurationManager.ConnectionStrings["FLEX_AMT_AUN"].ConnectionString;
            using (SqlConnection con = new SqlConnection(cs))
            {
                SqlCommand cmd = new SqlCommand("EXEC sp_INV010_GetInventoryOnhand @p_YEARMONTH" +
                    ",@p_ITEMCATEGORYID" +
                    ",@p_ITEM_TYPE" +
                    ",@p_ITEM_CD" +
                    ",@p_LOT_NO" +
                    ",@p_LOC_CD" +
                    ",@p_ONHAND_LESS_THAN_ZERO" +
                    ",@p_ONHAND_LESS_THAN_SAFTYSTOCK" +
                    ",@p_GROUP_BY_ITEM"
                    );
                cmd.Connection = con;
                cmd.Prepare();
                SetPreparedStatementSQLByAddWithValue(cmd, "p_YEARMONTH", p_YEARMONTH.YEAR_MONTH);
                SetPreparedStatementSQLByAddWithValue(cmd, "p_ITEMCATEGORYID", p_ITEMCATEGORYID);
                SetPreparedStatementSQLByAddWithValue(cmd, "@p_ITEM_TYPE", p_ITEM_TYPE, true);
                SetPreparedStatementSQLByAddWithValue(cmd, "@p_ITEM_CD", p_ITEM_CD);
                SetPreparedStatementSQLByAddWithValue(cmd, "@p_LOT_NO", p_LOT_NO);
                SetPreparedStatementSQLByAddWithValue(cmd, "@p_LOC_CD", p_LOC_CD);
                SetPreparedStatementSQLByAddWithValue(cmd, "@p_ONHAND_LESS_THAN_ZERO", p_ONHAND_LESS_THAN_ZERO);
                SetPreparedStatementSQLByAddWithValue(cmd, "@p_ONHAND_LESS_THAN_SAFTYSTOCK", p_ONHAND_LESS_THAN_SAFTYSTOCK);
                SetPreparedStatementSQLByAddWithValue(cmd, "@p_GROUP_BY_ITEM", p_GROUP_BY_ITEM);

                con.Open();
                SqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    var InventoryOnhandDetail = new InventoryOnhand
                    {
                        YEAR_MONTH = rdr["YEAR_MONTH"].ToString(),
                        PERIOD_BEGIN_DATE = rdr["PERIOD_BEGIN_DATE"].ToString(),
                        PERIOD_END_DATE = rdr["PERIOD_END_DATE"].ToString(),
                        LOCATION = rdr["LOCATION"].ToString(),
                        ITEM_CODE = rdr["ITEM_CODE"].ToString(),
                        ITEM_DESCRIPTION = rdr["ITEM_DESCRIPTION"].ToString(),
                        LOT_NO = rdr["LOT_NO"].ToString(),
                        PREVIOUS_BAL = rdr["PREVIOUS_BAL"].ToString(),
                        TOTAL_IN_QTY = rdr["TOTAL_IN_QTY"].ToString(),
                        TOTAL_IN_QTY_NEXT_MONTH = rdr["TOTAL_IN_QTY_NEXT_MONTH"].ToString(),
                        TOTAL_OUT_QTY = rdr["TOTAL_OUT_QTY"].ToString(),
                        TOTAL_ADJ_QTY = rdr["TOTAL_ADJ_QTY"].ToString(),
                        ONHAND_QTY = rdr["ONHAND_QTY"].ToString(),
                        ALLOCATE_QTY = rdr["ALLOCATE_QTY"].ToString(),
                        ALLOCATE_QTY_NEXT_MONTH = rdr["ALLOCATE_QTY_NEXT_MONTH"].ToString(),
                        AVAILABLE_QTY = rdr["AVAILABLE_QTY"].ToString(),
                        SAFTY_STOCK = rdr["SAFTY_STOCK"].ToString(),
                        INV_UM_CLS = rdr["INV_UM_CLS"].ToString(),
                        ITEM_CLS = rdr["ITEM_CLS"].ToString(),
                        MODEL = rdr["MODEL"].ToString(),
                        ITEMCATEGORY = rdr["ITEMCATEGORY"].ToString(),
                        LAST_RECEIVE_DATE = rdr["LAST_RECEIVE_DATE"].ToString(),
                        SUPP_LOT_NO = rdr["SUPP_LOT_NO"].ToString(),
                        QUERY_TYPE = rdr["QUERY_TYPE"].ToString(),
                        SHORT_NAME = rdr["SHORT_NAME"].ToString(),
                        FOR_CUSTOMER = rdr["FOR_CUSTOMER"].ToString()
                    };

                    InventoryOnhandList.Add(InventoryOnhandDetail);
                }
                con.Close();

                JavaScriptSerializer js = new JavaScriptSerializer();
                Context.Response.Write(js.Serialize(InventoryOnhandList));

            }
        }

        //---------------------------------------------------------------Note 07/18/2018 15.42am
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public CurrentYearMonth GetCurrentYearMonth()
        {
            CurrentYearMonth CurrentYearMonthList = new CurrentYearMonth();
            string cs = ConfigurationManager.ConnectionStrings["FLEX_AMT_AUN"].ConnectionString;
            using (SqlConnection con = new SqlConnection(cs))
            {
                SqlCommand cmd = new SqlCommand("EXEC S_INV_LoadCurrentYearMonth");
                cmd.Connection = con;
                con.Open();
                SqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    var CurrentYearMonthDetail = new CurrentYearMonth
                    {
                        YEAR_MONTH = rdr["YEAR_MONTH"].ToString(),
                        PERIOD_BEGIN_DATE = rdr["PERIOD_BEGIN_DATE"].ToString(),
                        PERIOD_END_DATE = rdr["PERIOD_END_DATE"].ToString(),
                        CRT_BY = rdr["CRT_BY"].ToString(),
                        CRT_DATE = rdr["CRT_DATE"].ToString(),
                        CRT_MACHINE = rdr["CRT_MACHINE"].ToString(),
                        UPD_BY = rdr["UPD_BY"].ToString(),
                        UPD_DATE = rdr["UPD_DATE"].ToString(),
                        UPD_MACHINE = rdr["UPD_MACHINE"].ToString(),
                    };
                    CurrentYearMonthList = CurrentYearMonthDetail;
                }
                con.Close();

                JavaScriptSerializer js = new JavaScriptSerializer();
                Context.Response.Write(js.Serialize(CurrentYearMonthList));
                return CurrentYearMonthList;
            }
        }
        //===============================================================================================ห้ามลืมข้างล่างนี้ เดี๋ยวพังนะไอโน้ตตต
    }
}