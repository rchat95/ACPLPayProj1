using ACPLPayProj1.Models;
using paytm;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Xml;

namespace ACPLPayProj1.Controllers
{
    public class HomeController : Controller
    {
        public static int orderID, cartSrNum = 0;
        public static string totalAmt = "", priceMst = "", getTotalBillAmt = "", getProdType = "", getQty = "", SNumSession = "", SNumStr = "", warrantyFlag = "";
        public static Random random = new Random();
        public static string validDevStr = "", invalidDevStr = "", activeDevStr = "", blockDevStr = "", lifetimeStr = "";
        public static string payEmail = "", payPhNum = "", payName = "", payAmt = "", checksum = "", paytmURL = "", trimpayName = "", payAddress = "", payPinCode = "", payQty = "", payTxnID = "", payState = "", payGSTIN = "", payBillname = "";
        public static List<string> SNumStrArray = new List<string>(), SNumTypeArray = new List<string>(), SNumWarrantyArray = new List<string>();

        public ActionResult Index()
        {
            
            if (Convert.ToInt32(Session["qty"]) == 0)
            {
                Session["qty"] = 0;
            }
            
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";           

            return View();
        }      

        [HttpPost]
        public ActionResult Pricing()
        {
            orderID = random.Next(10000);
            payEmail = Request.Params["email"];
            payPhNum = Request.Params["phNum"];
            payName = Request.Params["name"];
            payAddress = Request.Params["address"];
            payPinCode = Request.Params["pincode"];
            payState = Request.Params["state"];
            payGSTIN = Request.Params["gstNo"];
            payQty = Request.Params["totalqty"];
            payAmt = Request.Params["totalPayAmt"];
            payBillname = Request.Params["billingname"];

            trimpayName = payName.Replace(" ","");
            //TestDB();
            PaytmPayment(payEmail, payPhNum, trimpayName, orderID.ToString(), payAmt, @Url.Action("PaymentResp", "Home", null, Request.Url.Scheme));      
            return View();   
        }

        public ActionResult TestPaytm()
        {
            orderID = random.Next(10000);
            PaytmPayment("rongon95@gmail.com", "9663602883", "Rongon", orderID.ToString(), "118", @Url.Action("PaymentResp", "Home", null, Request.Url.Scheme));
            return View();
        }

        public ActionResult PricingPage()
        {
            ViewBag.cartQty = Session["qty"];
            return View();
        }


        public ActionResult PaymentResp()
        {
            Dictionary<string, string> parameters1 = new Dictionary<string, string>();
            foreach (string key in Request.Form.Keys)
            {
                parameters1.Add(key.Trim(), Request.Form[key].Trim());
            }


            ViewBag.txnNum = parameters1.Values.ElementAt(2);
            payTxnID = parameters1.Values.ElementAt(2);
            ViewBag.totalAmt = "₹" + payAmt;
            ViewBag.orderID = orderID;
            ViewBag.custName = payName;
            ViewBag.email = payEmail;

            if (parameters1.ToList().Count > 0 && parameters1.Values.ElementAt(7).Equals("TXN_SUCCESS"))
            {
                //SUCCESS
                ViewBag.Message = "Payment Successful";
                TestDB();
                getProdType = "-1";
                Session["qty"] = 0;
                cartSrNum = 0;                
                Session["SNumSession"] = "";
                SNumSession = "";
            }
            else
            {
                //FAILURE
                ViewBag.Message = "Payment Failure";
                ViewBag.RespCode = parameters1.Values.ElementAt(7);
                ViewBag.RespMsg = parameters1.Values.ElementAt(7);
            }

           

            return View();
        }

        public ActionResult ComingSoon()
        {
            return View();
        }

        public ActionResult Branches()
        {
            return View();
        }

        public ActionResult FM220UProdPage()
        {
            ViewBag.cartQty = Session["qty"];
            return View();
        }

        public ActionResult IrisProdPage()
        {
            ViewBag.cartQty = Session["qty"];
            return View();
        }

        public ActionResult RDSProdPage()
        {
            ViewBag.cartQty = Session["qty"];
            return View();
        }


        public void PaytmPayment(string EMAIL, string MOBILE_NO, string CUST_ID, string ORDER_ID, string TXN_AMOUNT, string CallBackURL)
        {
            String merchantKey = "k&%PvWAPveA#rCwN";
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("MID", "RCPayS81099062107132");
            parameters.Add("CHANNEL_ID", "WEB");
            parameters.Add("INDUSTRY_TYPE_ID", "Retail");
            parameters.Add("WEBSITE", "WEBSTAGING");
            parameters.Add("EMAIL", EMAIL);
            parameters.Add("MOBILE_NO", MOBILE_NO);
            parameters.Add("CUST_ID", CUST_ID);
            parameters.Add("ORDER_ID", ORDER_ID);
            parameters.Add("TXN_AMOUNT", TXN_AMOUNT);
            parameters.Add("CALLBACK_URL", CallBackURL); //This parameter is not mandatory. Use this to pass the callback url dynamically.

            checksum = CheckSum.generateCheckSum(merchantKey, parameters);

            paytmURL = "https://securegw-stage.paytm.in/theia/processTransaction?orderid=" + ORDER_ID;

            string outputHTML = "<html>";
            outputHTML += "<head>";
            outputHTML += "<title>Merchant Check Out Page</title>";
            outputHTML += "</head>";
            outputHTML += "<body>";
            outputHTML += "<center><h1>Please do not refresh this page...</h1></center>";
            outputHTML += "<form method='post' action='" + paytmURL + "' name='f1'>";
            outputHTML += "<table border='1'>";
            outputHTML += "<tbody>";
            foreach (string key in parameters.Keys)
            {
                outputHTML += "<input type='hidden' name='" + key + "' value='" + parameters[key] + "'>";
            }
            outputHTML += "<input type='hidden' name='CHECKSUMHASH' value='" + checksum + "'>";
            outputHTML += "</tbody>";
            outputHTML += "</table>";
            outputHTML += "<script type='text/javascript'>";
            outputHTML += "document.f1.submit();";
            outputHTML += "</script>";
            outputHTML += "</form>";
            outputHTML += "</body>";
            outputHTML += "</html>";
            Response.Write(outputHTML);
        }

        [HttpPost]
        public ActionResult AddToCart()
        {
            try
            {
                Response.StatusCode = 200;
                SNumStr = Request.Params["SNumClientStr"];
                if (Session["SNumSession"] != null)
                {
                    if (Session["SNumSession"].ToString().Contains(SNumStr))
                    {
                        Response.StatusCode = 400;
                        return View("DeviceSelectPage");
                    }
                }

                String currSNumStr = Request.Params["SNumClientStr"].ToString();
                SNumStrArray.Add(currSNumStr.Substring(0,currSNumStr.Length - 1));
                if (priceMst.Equals("1"))
                {
                    SNumTypeArray.Add("1");
                }
                else if (priceMst.Equals("2"))
                {
                    SNumTypeArray.Add("2");
                }
                else if (priceMst.Equals("3"))
                {
                    SNumTypeArray.Add("3");
                }
                else if (priceMst.Equals("4"))
                {
                    SNumTypeArray.Add("4");
                }
                else if (priceMst.Equals("5"))
                {
                    SNumTypeArray.Add("5");
                }



                //string temp = SNumStrArray.ElementAt(0);
                //string temp2 = priceMst;
                warrantyFlag = Request.Params["warrantyFlag"];
                SNumWarrantyArray.Add(warrantyFlag);


                String s = "";
                Session["SNumSession"] += Request.Params["SNumClientStr"];
                SNumSession = Session["SNumSession"].ToString();




                if (Session["qty"] == null || Convert.ToInt32(Session["qty"]) == 0)
                {

                    Session["qty"] = 1;
                    ViewBag.cartQty = Session["qty"];
                }
                else
                {
                    Session["qty"] = Convert.ToInt32(Session["qty"]) + 1;
                    ViewBag.cartQty = Session["qty"];
                }

                getTotalBillAmt = Request.Params["totalBillAmt"];
                getProdType = priceMst;
                getQty = Request.Params["prodQty"];


                return View("CartPage");
            }
            catch (Exception ex)
            {
                String exception = ex.Message.ToString();
                return View("DeviceSelectPage");
            }
        }       

        [HttpPost]
        public ActionResult SendNewMessage()
        {
            try
            {
                Response.StatusCode = 200;
                //getting useful configuration
                string smtpAddress = ConfigurationSMTP.smtpAdress;
                //it can be a "smtp.office365.com" or whatever,
                //it depends on smtp server of your sender email.
                int portNumber = ConfigurationSMTP.portNumber;   //Smtp port 
                bool enableSSL = ConfigurationSMTP.enableSSL;  //SSL enable

                string emailTo = Request.Params["to"];
                string subject = Request.Params["subject"];

                StringBuilder body = new StringBuilder();

                //building the body of our email
                body.Append("<html><head> </head><body>");
                body.Append("<div style=' font-family: Arial; font-size: 14px; color: black;'>Hi,<br><br>");
                body.Append("Message from: " + Request.Params["name"] + "<br><br>");
                body.Append("Email ID: " + Request.Params["to"] + "<br><br>");
                body.Append("Message:<br><br>");
                body.Append(Request.Params["message"]);
                body.Append("</div><br>");
                //Mail signature
                body.Append(string.Format("<span style='font-size:11px;font-family: Arial;color:#40411E;'>{0} - {1} {2}</span><br>", MessageModel.adress, MessageModel.zip, MessageModel.city));
                body.Append(string.Format("<span style='font-size:11px;font-family: Arial;color:#40411E;'>Mail: <a href=\"mailto:{0}\">{0}</a></span><br>", ConfigurationSMTP.from));
                body.Append(string.Format("<span style='font-size:11px;font-family: Arial;color:#40411E;'>Tel: {0}</span><br>", MessageModel.phone));
                body.Append(string.Format("<span style='font-size:11px;font-family: Arial;'><a href=\"web site\">{0}</a></span><br><br>", MessageModel.link));
                body.Append(string.Format("<span style='font-size:11px; font-family: Arial;color:#40411E;'>{0}</span><br>", MessageModel.details));
                body.Append("</body></html>");

                using (MailMessage mail = new MailMessage())
                {

                    mail.From = new MailAddress(ConfigurationSMTP.from);
                    //destination adress
                    mail.To.Add(emailTo);
                    mail.Subject = subject;
                    mail.Body = body.ToString();
                    //set to true, to specify that we are sending html text.
                    mail.IsBodyHtml = true;
                    // Can set to false, if you are sending pure text.

                    //string localFileName = "~/Content/TestAttachement.txt";
                    ////to send a file in attachment.
                    //mail.Attachments.Add(new Attachment(Server.MapPath(localFileName), "application/pdf"));

                    //Specify the smtp Server and port number to create a new instance of SmtpClient.
                    using (SmtpClient smtp = new SmtpClient(smtpAddress, portNumber))
                    {
                        //passing the credentials for authentification
                        smtp.Credentials = new NetworkCredential(ConfigurationSMTP.from, ConfigurationSMTP.password);
                        //Authentification required
                        smtp.EnableSsl = enableSSL;
                        //sending email.
                        smtp.Send(mail);
                    }
                }
            }
            catch (Exception ex)
            {
                //Error response
                Response.StatusCode = 400;
            }
            return null;
        }        

        [HttpPost]
        public ActionResult AddSrNo()
        {
            try
            {
                string devType = Request.Params["prodFlag"].ToString();
                Response.StatusCode = 200;
                string srNos = Request.Params["inputSrNum"].Trim().Replace(" ","");                

                if (srNos == null || srNos.Equals("") || srNos.Trim().Length < 6)
                {
                    Response.StatusCode = 400;                    
                    return null;
                }

                string req_soap = "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:tem=\"http://AccessComputech.com/MGMT_INFO/\">";
                req_soap = req_soap + Convert.ToString("<soapenv:Header/>");
                req_soap = req_soap + Convert.ToString("<soapenv:Body>");
                req_soap = req_soap + Convert.ToString("<tem:FilterSerialNumbers_MULTI>");
                req_soap = req_soap + Convert.ToString("<tem:argSrNos>");
                req_soap = req_soap + srNos.Trim().Replace("\n", "").Replace("\r", "").Replace("\t", "");
                req_soap = req_soap + Convert.ToString("</tem:argSrNos>");
                req_soap = req_soap + Convert.ToString("</tem:FilterSerialNumbers_MULTI>");
                req_soap = req_soap + Convert.ToString("</soapenv:Body>");
                req_soap = req_soap + Convert.ToString("</soapenv:Envelope>");

                HttpWebRequest webRequest__1 = (HttpWebRequest)(WebRequest.Create("https://www.acpl.in.net/FM220_REGISTRATION_SERVICE/MgmtInfoPage.asmx"));
                webRequest__1.Headers.Add("SOAPAction", "\"http://AccessComputech.com/MGMT_INFO/FilterSerialNumbers_MULTI\"");
                webRequest__1.Headers.Add("To", "https://www.acpl.in.net/FM220_REGISTRATION_SERVICE/MgmtInfoPage.asmx");
                webRequest__1.ContentType = "text/xml;charset=\"utf-8\"";
                webRequest__1.Accept = "text/xml";
                webRequest__1.Method = "POST";
                webRequest__1.Timeout = 60000;

                using (Stream stm = webRequest__1.GetRequestStream())
                {
                    using (StreamWriter stmw = new StreamWriter(stm))
                    {
                        stmw.Write(req_soap);
                    }
                }
                WebResponse response = default(WebResponse);
                response = webRequest__1.GetResponse();
                Stream str = response.GetResponseStream();
                StreamReader sr = new StreamReader(str);
                string finalResponse = sr.ReadToEnd();
                string res;
                using (XmlReader reader = XmlReader.Create(new StringReader(finalResponse)))
                {
                    reader.ReadToFollowing("FilterSerialNumbers_MULTIResult");
                    res = reader.ReadString();
                }
                string respToProc = HttpUtility.HtmlDecode(res);               
                
                using (XmlReader reader = XmlReader.Create(new System.IO.StringReader(respToProc)))
                {
                    reader.ReadToFollowing("ValidDevices");
                    validDevStr = reader.ReadString();

                    reader.ReadToFollowing("InvalidDevices");
                    invalidDevStr = reader.ReadString();

                    reader.ReadToFollowing("ActiveDevices");
                    activeDevStr = reader.ReadString();

                    reader.ReadToFollowing("LifeTimeDevices");
                    lifetimeStr = reader.ReadString();

                    reader.ReadToFollowing("BlockedDevices");
                    blockDevStr = reader.ReadString();
                }

                if (devType.Equals("1") || devType.Equals("2") || devType.Equals("3"))
                {
                    string[] inputStrArray = srNos.Split(',');
                    for (int i = 0; i < inputStrArray.Length; i++)
                    {
                        if (inputStrArray[i].ToUpper().StartsWith("D"))
                        {
                            Response.StatusCode = 400;
                            return null;
                        }
                    }
                }

                if (devType.Equals("4") || devType.Equals("5"))
                {
                    string[] inputStrArray = srNos.Split(',');
                    for (int i = 0; i < inputStrArray.Length; i++)
                    {
                        if (inputStrArray[i].ToUpper().StartsWith("B"))
                        {
                            Response.StatusCode = 400;                   
                            return null;
                        }
                    }
                }

                //if (validDevStr == null || validDevStr.Equals(""))
                //{
                //    //Device is not valid
                //    //ViewBag.Message = "Device is not valid";
                //    Response.StatusCode = 400;
                //    return null;
                //}
                

            }
            catch (Exception ex)
            {
                //Error response
                Response.StatusCode = 400;             
            }
            
            return View("DeviceSelectPage");
        }

        public void TestDB()
        {
            SqlConnection oleDbConnection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ACPLPayProj1Context"].ConnectionString);
            oleDbConnection.Open();

            //SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM TBLDEVNUM", oleDbConnection);
            //DataSet dataset = new DataSet();
            //dataset.Clear();
            //da.Fill(dataset);
            String temp = "";

            
            string SNumStrDB = SNumSession.Substring(0, SNumSession.Length - 1);


            for (int i = 0; i < cartSrNum; i++)
            {
                SqlCommand getRefNum = new SqlCommand("SELECT (NEXT VALUE FOR REF_NO_SEQ_SUB) AS RF_NO", oleDbConnection);
                String nxt_seq_val = getRefNum.ExecuteScalar().ToString();
                Int64 seq_ref_no = Convert.ToInt64(nxt_seq_val);

                if (SNumTypeArray[i].Equals("1") && SNumWarrantyArray[i].Equals("N"))   //Finger (1 year without warranty)
                {
                    string[] fqtyArr = SNumStrArray[i].Split(',');
                    string fqty = fqtyArr.Length.ToString();
                    int paidamt = 118 * Convert.ToInt32(fqty);

                    SqlCommand command = new SqlCommand
                ("INSERT INTO TBL_DEV_SUBSCRIPTION_EXT_MST (REF_NO, CUST_NAME, CUST_PHONE, CUST_MAIL, CUST_ADDR1, PIN_CODE, ENTRY_DATE, DEV_QUANTITY, PAYMENT_DETAILS, PAYMENT_FLG, CANCEL_FLG, CUST_ADDR2, CUST_STATE, GSTIN_NO, REMARK, SRNO, PAID_AMT, SUBSCRIPTION_DUR, ADDED_WARRANTY, BILLING_TO, IRIS_QTY, STARTEK_QTY) VALUES('" + seq_ref_no + "','" + payName.Trim().ToUpper() + "','" + payPhNum + "','" + payEmail + "','" + payAddress.Trim().ToUpper() + "','" + payPinCode + "',getdate(),'" + fqty + "','" + "ONLINE - " + payTxnID + "','" + "N" + "','" + "N" + "','" + "" + "','" + payState + "','" + payGSTIN + "','" + "" + "','" + SNumStrArray[i] + "','"  + paidamt.ToString() + "','" + "1" + "','" + "N" + "','" + payBillname + "','" + "0" + "','" + fqty + "')", oleDbConnection);
                    string ss = "";
                    command.ExecuteNonQuery(); //HANDLE ERROR
                }

                if (SNumTypeArray[i].Equals("1") && SNumWarrantyArray[i].Equals("Y"))   //Finger (1 year with warranty)
                {
                    string[] fqtyArr = SNumStrArray[i].Split(',');
                    string fqty = fqtyArr.Length.ToString();
                    int paidamt = 118 * Convert.ToInt32(fqty) + 177 * Convert.ToInt32(fqty);

                    SqlCommand command = new SqlCommand
                ("INSERT INTO TBL_DEV_SUBSCRIPTION_EXT_MST (REF_NO, CUST_NAME, CUST_PHONE, CUST_MAIL, CUST_ADDR1, PIN_CODE, ENTRY_DATE, DEV_QUANTITY, PAYMENT_DETAILS, PAYMENT_FLG, CANCEL_FLG, CUST_ADDR2, CUST_STATE, GSTIN_NO, REMARK, SRNO, PAID_AMT, SUBSCRIPTION_DUR, ADDED_WARRANTY, BILLING_TO, IRIS_QTY, STARTEK_QTY) VALUES('" + seq_ref_no + "','" + payName.Trim().ToUpper() + "','" + payPhNum + "','" + payEmail + "','" + payAddress.Trim().ToUpper() + "','" + payPinCode + "',getdate(),'" + fqty + "','" + "ONLINE - " + payTxnID + "','" + "N" + "','" + "N" + "','" + "" + "','" + payState + "','" + payGSTIN + "','" + "" + "','" + SNumStrArray[i] + "','" + paidamt.ToString() + "','" + "1" + "','" + "Y" + "','" + payBillname + "','" + "0" + "','" + fqty + "')", oleDbConnection);
                    string ss = "";
                    command.ExecuteNonQuery(); //HANDLE ERROR
                }

                if (SNumTypeArray[i].Equals("2") && SNumWarrantyArray[i].Equals("N"))   //Finger (Lifetime without warranty)
                {
                    string[] fqtyArr = SNumStrArray[i].Split(',');
                    string fqty = fqtyArr.Length.ToString();
                    int paidamt = 354 * Convert.ToInt32(fqty);

                    SqlCommand command = new SqlCommand
                ("INSERT INTO TBL_DEV_SUBSCRIPTION_EXT_MST (REF_NO, CUST_NAME, CUST_PHONE, CUST_MAIL, CUST_ADDR1, PIN_CODE, ENTRY_DATE, DEV_QUANTITY, PAYMENT_DETAILS, PAYMENT_FLG, CANCEL_FLG, CUST_ADDR2, CUST_STATE, GSTIN_NO, REMARK, SRNO, PAID_AMT, SUBSCRIPTION_DUR, ADDED_WARRANTY, BILLING_TO, IRIS_QTY, STARTEK_QTY) VALUES('" + seq_ref_no + "','" + payName.Trim().ToUpper() + "','" + payPhNum + "','" + payEmail + "','" + payAddress.Trim().ToUpper() + "','" + payPinCode + "',getdate(),'" + fqty + "','" + "ONLINE - " + payTxnID + "','" + "N" + "','" + "N" + "','" + "" + "','" + payState + "','" + payGSTIN + "','" + "" + "','" + SNumStrArray[i] + "','" + paidamt.ToString() + "','" + "99" + "','" + "N" + "','" + payBillname + "','" + "0" + "','" + fqty + "')", oleDbConnection);
                    string ss = "";
                    command.ExecuteNonQuery(); //HANDLE ERROR
                }

                if (SNumTypeArray[i].Equals("2") && SNumWarrantyArray[i].Equals("Y"))   //Finger (Lifetime without warranty)
                {
                    string[] fqtyArr = SNumStrArray[i].Split(',');
                    string fqty = fqtyArr.Length.ToString();
                    int paidamt = 354 * Convert.ToInt32(fqty) + 177 * Convert.ToInt32(fqty);

                    SqlCommand command = new SqlCommand
                ("INSERT INTO TBL_DEV_SUBSCRIPTION_EXT_MST (REF_NO, CUST_NAME, CUST_PHONE, CUST_MAIL, CUST_ADDR1, PIN_CODE, ENTRY_DATE, DEV_QUANTITY, PAYMENT_DETAILS, PAYMENT_FLG, CANCEL_FLG, CUST_ADDR2, CUST_STATE, GSTIN_NO, REMARK, SRNO, PAID_AMT, SUBSCRIPTION_DUR, ADDED_WARRANTY, BILLING_TO, IRIS_QTY, STARTEK_QTY) VALUES('" + seq_ref_no + "','" + payName.Trim().ToUpper() + "','" + payPhNum + "','" + payEmail + "','" + payAddress.Trim().ToUpper() + "','" + payPinCode + "',getdate(),'" + fqty + "','" + "ONLINE - " + payTxnID + "','" + "N" + "','" + "N" + "','" + "" + "','" + payState + "','" + payGSTIN + "','" + "" + "','" + SNumStrArray[i] + "','" + paidamt.ToString() + "','" + "99" + "','" + "Y" + "','" + payBillname + "','" + "0" + "','" + fqty + "')", oleDbConnection);
                    string ss = "";
                    command.ExecuteNonQuery(); //HANDLE ERROR
                }

                if (SNumTypeArray[i].Equals("3"))   //Finger (Extended warranty)
                {
                    string[] fqtyArr = SNumStrArray[i].Split(',');
                    string fqty = fqtyArr.Length.ToString();
                    int paidamt = 177 * Convert.ToInt32(fqty);

                    SqlCommand command = new SqlCommand
                ("INSERT INTO TBL_DEV_SUBSCRIPTION_EXT_MST (REF_NO, CUST_NAME, CUST_PHONE, CUST_MAIL, CUST_ADDR1, PIN_CODE, ENTRY_DATE, DEV_QUANTITY, PAYMENT_DETAILS, PAYMENT_FLG, CANCEL_FLG, CUST_ADDR2, CUST_STATE, GSTIN_NO, REMARK, SRNO, PAID_AMT, SUBSCRIPTION_DUR, ADDED_WARRANTY, BILLING_TO, IRIS_QTY, STARTEK_QTY) VALUES('" + seq_ref_no + "','" + payName.Trim().ToUpper() + "','" + payPhNum + "','" + payEmail + "','" + payAddress.Trim().ToUpper() + "','" + payPinCode + "',getdate(),'" + fqty + "','" + "ONLINE - " + payTxnID + "','" + "N" + "','" + "N" + "','" + "" + "','" + payState + "','" + payGSTIN + "','" + "" + "','" + SNumStrArray[i] + "','" + paidamt.ToString() + "','" + "99" + "','" + "Y" + "','" + payBillname + "','" + "0" + "','" + fqty + "')", oleDbConnection);
                    string ss = "";
                    command.ExecuteNonQuery(); //HANDLE ERROR
                }

                if (SNumTypeArray[i].Equals("4"))   //Iris (1 year)
                {
                    string[] iqtyArr = SNumStrArray[i].Split(',');
                    string iqty = iqtyArr.Length.ToString();
                    int paidamt = 472 * Convert.ToInt32(iqty);

                    SqlCommand command = new SqlCommand
                ("INSERT INTO TBL_DEV_SUBSCRIPTION_EXT_MST (REF_NO, CUST_NAME, CUST_PHONE, CUST_MAIL, CUST_ADDR1, PIN_CODE, ENTRY_DATE, DEV_QUANTITY, PAYMENT_DETAILS, PAYMENT_FLG, CANCEL_FLG, CUST_ADDR2, CUST_STATE, GSTIN_NO, REMARK, SRNO, PAID_AMT, SUBSCRIPTION_DUR, ADDED_WARRANTY, BILLING_TO, IRIS_QTY, STARTEK_QTY) VALUES('" + seq_ref_no + "','" + payName.Trim().ToUpper() + "','" + payPhNum + "','" + payEmail + "','" + payAddress.Trim().ToUpper() + "','" + payPinCode + "',getdate(),'" + iqty + "','" + "ONLINE - " + payTxnID + "','" + "N" + "','" + "N" + "','" + "" + "','" + payState + "','" + payGSTIN + "','" + "" + "','" + SNumStrArray[i] + "','" + paidamt.ToString() + "','" + "99" + "','" + "N" + "','" + payBillname + "','" + iqty  + "','" + "0" + "')", oleDbConnection);
                    string ss = "";
                    command.ExecuteNonQuery(); //HANDLE ERROR
                }

                if (SNumTypeArray[i].Equals("5"))   //Iris (Lifetime)
                {
                    string[] iqtyArr = SNumStrArray[i].Split(',');
                    string iqty = iqtyArr.Length.ToString();
                    int paidamt = 1180 * Convert.ToInt32(iqty);

                    SqlCommand command = new SqlCommand
                ("INSERT INTO TBL_DEV_SUBSCRIPTION_EXT_MST (REF_NO, CUST_NAME, CUST_PHONE, CUST_MAIL, CUST_ADDR1, PIN_CODE, ENTRY_DATE, DEV_QUANTITY, PAYMENT_DETAILS, PAYMENT_FLG, CANCEL_FLG, CUST_ADDR2, CUST_STATE, GSTIN_NO, REMARK, SRNO, PAID_AMT, SUBSCRIPTION_DUR, ADDED_WARRANTY, BILLING_TO, IRIS_QTY, STARTEK_QTY) VALUES('" + seq_ref_no + "','" + payName.Trim().ToUpper() + "','" + payPhNum + "','" + payEmail + "','" + payAddress.Trim().ToUpper() + "','" + payPinCode + "',getdate(),'" + iqty + "','" + "ONLINE - " + payTxnID + "','" + "N" + "','" + "N" + "','" + "" + "','" + payState + "','" + payGSTIN + "','" + "" + "','" + SNumStrArray[i] + "','" + paidamt.ToString() + "','" + "99" + "','" + "N" + "','" + payBillname + "','" + iqty + "','" + "0" + "')", oleDbConnection);
                    string ss = "";
                    command.ExecuteNonQuery(); //HANDLE ERROR
                }


            }

            
        }



        public ActionResult DeviceSelectPage()
        {
            ViewBag.cartQty = Session["qty"];
            priceMst = Request.QueryString["id"].ToString();
            ViewBag.prodCat = priceMst;
            if (priceMst.Equals("1")) //Finger RD 1 yr
            {
                ViewBag.priceVal = 118;
                ViewBag.prodType = "1 year (Finger)";
            }
            else if (priceMst.Equals("2")) //Finger RD Lifetime
            {
                ViewBag.priceVal = 354;
                ViewBag.prodType = "Lifetime (Finger)";
            }
            else if (priceMst.Equals("3")) //Extended support
            {
                ViewBag.priceVal = 177;
                ViewBag.prodType = "Extended Support";
            }
            else if (priceMst.Equals("4")) //Iris RD 1 year
            {
                ViewBag.priceVal = 472;
                ViewBag.prodType = "1 year (Iris)";
            }
            else if (priceMst.Equals("5")) //Iris RD Lifetime
            {
                ViewBag.priceVal = 1180;
                ViewBag.prodType = "Lifetime (Iris)";
            }

            if (!validDevStr.Equals("")) //VALID DEVICE
            {                
                ViewBag.validMessage = validDevStr;
            }
            else
            {
                ViewBag.validMessage = "";
            }

            if (!activeDevStr.Equals("")) //ACTIVE DEVICE
            {
                ViewBag.activeMessage = activeDevStr;
            }
            else
            {
                ViewBag.activeMessage = "";
            }


            if (!lifetimeStr.Equals("")) //LIFETIME DEVICE
            {
                ViewBag.lifetimeMessage = lifetimeStr;
            }
            else
            {
                ViewBag.lifetimeMessage = "";
            }

            
            if (!invalidDevStr.Equals("")) //INVALID DEVICE
            {
                ViewBag.invalidMessage = invalidDevStr;
            }
            else
            {
                ViewBag.invalidMessage = "";
            }

            if (!blockDevStr.Equals("")) //INVALID DEVICE
            {
                ViewBag.blockedMessage = blockDevStr;
            }
            else
            {
                ViewBag.blockedMessage = "";
            }


            //string[] life_srnos = lifetimeStr.Split(',');

            //for (int i = 0; i < vld_srnos.Length; i++)
            //{
            //    colonIndex = vld_srnos[i].IndexOf(":");
            //    vld_srnos[i] = vld_srnos[i].Substring(0, colonIndex);
            //}

            //for (int i = 0; i < life_srnos.Length; i++)
            //{
            //    colonIndex = life_srnos[i].IndexOf(":");
            //    life_srnos[i] = life_srnos[i].Substring(0, colonIndex);
            //}

            //ViewBag.Message = vld_srnos[0] + "," + vld_srnos[1];

            return View();
        }

        public ActionResult CartPage()
        {
            try
            {
                ViewBag.cartQty = Session["qty"];
                ViewBag.prodType = getProdType;
                ViewBag.getTotalBillAmt = getTotalBillAmt;
                ViewBag.getQty = getQty;

                //string tempValid = validDevStr;
                //string[] tempValidArr = validDevStr.Split(',');

                //for (int i = 0; i < tempValidArr.Length; i++)
                //{
                //    int tempLength = tempValidArr[i].Length;
                //    tempValidArr[i] = tempValidArr[i].Substring(0, tempLength - 2);
                //}

                //string tempValidStr = string.Join(",", tempValidArr);
                string currentSNum = SNumStr.Substring(0, SNumStr.Length - 1);

                //SNumSession contains updated string of all to be ordered products
                string[] SNumArr = SNumSession.Split(',');
                int fmqty = 0, irisqty = 0;
                for (int i = 0; i < SNumArr.Length - 1; i++)
                {
                    if (SNumArr[i].ToUpper().StartsWith("B"))
                    {
                        fmqty++;
                    }
                    else
                    {
                        irisqty++;
                    }
                }

                ViewBag.fm220qty = fmqty;
                ViewBag.irisqty = irisqty;

                if (getProdType.Equals("1"))
                {
                    cartSrNum++;
                    if (Session["cartEntry"] == null || Session["cartEntry"].Equals(""))
                    {
                        //Session["cartEntry"] = HttpUtility.HtmlDecode("<tr><td>" + cartSrNum + "</td><td><a href='#' style='text-decoration:none; color:black' data-toggle='tooltip' title='" + tempValidStr + "'>RD Service - Finger (1 year)" + "</a></td><td>" + getQty + "</td><td id='fieldCell'>₹" + getTotalBillAmt + "</td><td><a class='delete' href='#'>Remove</a></td></tr>");
                        Session["cartEntry"] = HttpUtility.HtmlDecode("<tr><td>" + cartSrNum + "</td><td><a href='#' style='text-decoration:none; color:black' data-toggle='tooltip' title='" + currentSNum + "'>RD Service - Finger (1 year)" + "</a></td><td id='qtyCell'>" + getQty + "</td><td id='fieldCell'>₹" + getTotalBillAmt + "</td></tr>");
                    }
                    else
                    {
                        //Session["cartEntry"] += HttpUtility.HtmlDecode("<tr><td>" + cartSrNum + "</td><td><a href='#' style='text-decoration:none; color:black' data-toggle='tooltip' title='" + tempValidStr + "'>RD Service - Finger (1 year)" + "</a></td><td>" + getQty + "</td><td id='fieldCell'>₹" + getTotalBillAmt + "</td><td><a class='delete' href='#'>Remove</a></td></tr>");
                        Session["cartEntry"] += HttpUtility.HtmlDecode("<tr><td>" + cartSrNum + "</td><td><a href='#' style='text-decoration:none; color:black' data-toggle='tooltip' title='" + currentSNum + "'>RD Service - Finger (1 year)" + "</a></td><td id='qtyCell'>" + getQty + "</td><td id='fieldCell'>₹" + getTotalBillAmt + "</td></tr>");
                    }
                    getProdType = "0";
                }
                else if (getProdType.Equals("2"))
                {
                    cartSrNum++;
                    if (Session["cartEntry"] == null || Session["cartEntry"].Equals(""))
                    {
                        //Session["cartEntry"] = HttpUtility.HtmlDecode("<tr><td>" + cartSrNum + "</td><td><a href='#' style='text-decoration:none; color:black' data-toggle='tooltip' title='" + tempValidStr + "'>RD Service - Finger (Lifetime)" + "</a></td><td>" + getQty + "</td><td id='fieldCell'>₹" + getTotalBillAmt + "</td><td><a class='delete' href='#'>Remove</a></td></tr>");
                        Session["cartEntry"] = HttpUtility.HtmlDecode("<tr><td>" + cartSrNum + "</td><td><a href='#' style='text-decoration:none; color:black' data-toggle='tooltip' title='" + currentSNum + "'>RD Service - Finger (Lifetime)" + "</a></td><td id='qtyCell'>" + getQty + "</td><td id='fieldCell'>₹" + getTotalBillAmt + "</td></tr>");
                    }
                    else
                    {
                        //Session["cartEntry"] += HttpUtility.HtmlDecode("<tr><td>" + cartSrNum + "</td><td><a href='#' style='text-decoration:none; color:black' data-toggle='tooltip' title='" + tempValidStr + "'>RD Service - Finger (Lifetime)" + "</a></td><td>" + getQty + "</td><td id='fieldCell'>₹" + getTotalBillAmt + "</td><td><a class='delete' href='#'>Remove</a></td></tr>");
                        Session["cartEntry"] += HttpUtility.HtmlDecode("<tr><td>" + cartSrNum + "</td><td><a href='#' style='text-decoration:none; color:black' data-toggle='tooltip' title='" + currentSNum + "'>RD Service - Finger (Lifetime)" + "</a></td><td id='qtyCell'>" + getQty + "</td><td id='fieldCell'>₹" + getTotalBillAmt + "</td></tr>");
                    }
                    getProdType = "0";
                }
                else if (getProdType.Equals("3"))
                {
                    cartSrNum++;
                    if (Session["cartEntry"] == null || Session["cartEntry"].Equals(""))
                    {
                        //Session["cartEntry"] = HttpUtility.HtmlDecode("<tr><td>" + cartSrNum + "</td><td><a href='#' style='text-decoration:none; color:black' data-toggle='tooltip' title='" + tempValidStr + "'>RD Service - Extended Support" + "</a></td><td>" + getQty + "</td><td id='fieldCell'>₹" + getTotalBillAmt + "</td><td><a class='delete' href='#'>Remove</a></td></tr>");
                        Session["cartEntry"] = HttpUtility.HtmlDecode("<tr><td>" + cartSrNum + "</td><td><a href='#' style='text-decoration:none; color:black' data-toggle='tooltip' title='" + currentSNum + "'>RD Service - Extended Support" + "</a></td><td id='qtyCell'>" + getQty + "</td><td id='fieldCell'>₹" + getTotalBillAmt + "</td></tr>");
                    }
                    else
                    {
                        //Session["cartEntry"] += HttpUtility.HtmlDecode("<tr><td>" + cartSrNum + "</td><td><a href='#' style='text-decoration:none; color:black' data-toggle='tooltip' title='" + tempValidStr + "'>RD Service - Extended Support" + "</a></td><td>" + getQty + "</td><td id='fieldCell'>₹" + getTotalBillAmt + "</td><td><a class='delete' href='#'>Remove</a></td></tr>");
                        Session["cartEntry"] += HttpUtility.HtmlDecode("<tr><td>" + cartSrNum + "</td><td><a href='#' style='text-decoration:none; color:black' data-toggle='tooltip' title='" + currentSNum + "'>RD Service - Extended Support" + "</a></td><td id='qtyCell'>" + getQty + "</td><td id='fieldCell'>₹" + getTotalBillAmt + "</td></tr>");
                    }
                    getProdType = "0";
                }
                else if (getProdType.Equals("4"))
                {
                    cartSrNum++;
                    if (Session["cartEntry"] == null || Session["cartEntry"].Equals(""))
                    {
                        //Session["cartEntry"] = HttpUtility.HtmlDecode("<tr><td>" + cartSrNum + "</td><td><a href='#' style='text-decoration:none; color:black' data-toggle='tooltip' title='" + tempValidStr + "'>RD Service - Iris (1 year)" + "</a></td><td>" + getQty + "</td><td id='fieldCell'>₹" + getTotalBillAmt + "</td><td><a class='delete' href='#'>Remove</a></td></tr>");
                        Session["cartEntry"] = HttpUtility.HtmlDecode("<tr><td>" + cartSrNum + "</td><td><a href='#' style='text-decoration:none; color:black' data-toggle='tooltip' title='" + currentSNum + "'>RD Service - Iris (1 year)" + "</a></td><td id='qtyCell'>" + getQty + "</td><td id='fieldCell'>₹" + getTotalBillAmt + "</td></tr>");
                    }
                    else
                    {
                        //Session["cartEntry"] += HttpUtility.HtmlDecode("<tr><td>" + cartSrNum + "</td><td><a href='#' style='text-decoration:none; color:black' data-toggle='tooltip' title='" + tempValidStr + "'>RD Service - Iris (1 year)" + "</a></td><td>" + getQty + "</td><td id='fieldCell'>₹" + getTotalBillAmt + "</td><td><a class='delete' href='#'>Remove</a></td></tr>");
                        Session["cartEntry"] += HttpUtility.HtmlDecode("<tr><td>" + cartSrNum + "</td><td><a href='#' style='text-decoration:none; color:black' data-toggle='tooltip' title='" + currentSNum + "'>RD Service - Iris (1 year)" + "</a></td><td id='qtyCell'>" + getQty + "</td><td id='fieldCell'>₹" + getTotalBillAmt + "</td></tr>");
                    }
                    getProdType = "0";
                }
                else if (getProdType.Equals("5"))
                {
                    cartSrNum++;
                    if (Session["cartEntry"] == null || Session["cartEntry"].Equals(""))
                    {
                        //Session["cartEntry"] = HttpUtility.HtmlDecode("<tr><td>" + cartSrNum + "</td><td><a href='#' style='text-decoration:none; color:black' data-toggle='tooltip' title='" + tempValidStr + "'>RD Service - Iris (Lifetime)" + "</a></td><td>" + getQty + "</td><td id='fieldCell'>₹" + getTotalBillAmt + "</td><td><a class='delete' href='#'>Remove</a></td></tr>");
                        Session["cartEntry"] = HttpUtility.HtmlDecode("<tr><td>" + cartSrNum + "</td><td><a href='#' style='text-decoration:none; color:black' data-toggle='tooltip' title='" + currentSNum + "'>RD Service - Iris (Lifetime)" + "</a></td><td id='qtyCell'>" + getQty + "</td><td id='fieldCell'>₹" + getTotalBillAmt + "</td></tr>");
                    }
                    else
                    {
                        //Session["cartEntry"] += HttpUtility.HtmlDecode("<tr><td>" + cartSrNum + "</td><td><a href='#' style='text-decoration:none; color:black' data-toggle='tooltip' title='" + tempValidStr + "'>RD Service - Iris (Lifetime)" + "</a></td><td>" + getQty + "</td><td id='fieldCell'>₹" + getTotalBillAmt + "</td><td><a class='delete' href='#'>Remove</a></td></tr>");
                        Session["cartEntry"] += HttpUtility.HtmlDecode("<tr><td>" + cartSrNum + "</td><td><a href='#' style='text-decoration:none; color:black' data-toggle='tooltip' title='" + currentSNum + "'>RD Service - Iris (Lifetime)" + "</a></td><td id='qtyCell'>" + getQty + "</td><td id='fieldCell'>₹" + getTotalBillAmt + "</td></tr>");
                    }
                    getProdType = "0";
                }
                else if (getProdType.Equals("-1"))
                {
                    Session["cartEntry"] = "";
                    getProdType = "0";
                }
                
            }
            catch (Exception ex)
            {
                string exMsg = ex.Message.ToString();
            }
            return View();
        }

        public ActionResult RemoveFromCart()
        {
            if (Convert.ToInt32(Session["qty"]) == 0)
            {                
                ViewBag.cartQty = Session["qty"];
                return View("CartPage");
            }            
            else
            {
                Session["qty"] = Convert.ToInt32(Session["qty"]) - 1;
                ViewBag.cartQty = Session["qty"];
            }
            
            return View("CartPage");
        }

        public ActionResult PayTMPage()
        {
            ViewBag.Message1 = payEmail;
            ViewBag.Message2 = payPhNum;
            ViewBag.Message3 = trimpayName;
            ViewBag.Message4 = orderID;
            ViewBag.Message5 = payAmt;
            ViewBag.Message6 = checksum;
            ViewBag.Message7 = paytmURL;
            return View();
        }

        public ActionResult ClearCart()
        {
            cartSrNum = 0;
            getProdType = "-1";
            Session["qty"] = 0;
            Session["SNumSession"] = "";
            SNumSession = "";

            ViewBag.cartQty = Session["qty"];
            CartPage();
            return View("CartPage");
        }

        public ActionResult SupportProdPage()
        {
            return View();
        }
        
    }
}