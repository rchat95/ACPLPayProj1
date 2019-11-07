using paytm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ACPL_Payment_Website
{
    public partial class Page_Iris : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string merchantKey = "k&%PvWAPveA#rCwN";

            Dictionary<string, string> parameters1 = new Dictionary<string, string>();
            string paytmChecksum = "";
            foreach (string key in Request.Form.Keys)
            {
                parameters1.Add(key.Trim(), Request.Form[key].Trim());
            }

            String s = "";
            //Response.Write(parameters1.ElementAt(2));
            Response.Write(parameters1.Values.ElementAt(2));
            Response.Write(parameters1.Values.ElementAt(7)); //If this is equal to "TXN_SUCCESS" then display success resp page
            Response.Write(parameters1.Values.ElementAt(8)); //Success code = "01"



            //if (parameters1.ContainsKey("CHECKSUMHASH"))
            //{
            //    paytmChecksum = parameters1["CHECKSUMHASH"];
            //    parameters1.Remove("CHECKSUMHASH");
            //}

            //if (CheckSum.verifyCheckSum(merchantKey, parameters1, paytmChecksum))
            //{
            //    Response.Write("Checksum Matched");
            //}
            //else
            //{
            //    Response.Write("Checksum MisMatch");
            //}
        }
    }
}