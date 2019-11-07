using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ACPLPayProj1.Models
{
    public class ConfigurationSMTP
    {
        //SMTP parameters
        public static string smtpAdress = "smtp.gmail.com";
        public static int portNumber = 587;
        public static bool enableSSL = true;
        //need it for the secured connection
        public static string from = "rongon95@gmail.com";
        public static string password = "rc4420x78";
    }

    public class MessageModel
    {
        public static string adress = "Access Computech Pvt Ltd., Vadodara";
        public static string link = "www.acpl.in.net";
        public static string zip = "90";
        public static string city = "Vadodara, Gujarat";
        public static string phone = "7874078565";
        public static string details = "This is a test Email";
    }
}