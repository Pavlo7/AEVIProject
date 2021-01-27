using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AEVIDomain;
using System.Configuration;

namespace AEVIWeb
{
    public static class LocalData
    {

        public static string UserId()
        {
            string msg;
            STUser rd = new STUser();
            CUser clUser = new CUser(null, ConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString,
                     ConfigurationManager.AppSettings["Logpath"]);
            clUser.GetRecordByUserLogin(HttpContext.Current.User.Identity.Name, out rd, out msg);
            return rd.userid;
        }

        public static string CSDbUsers()
        {
            return ConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;
        }

        public static string LogPath()
        {
            return ConfigurationManager.AppSettings["Logpath"];
        }

        public static string ReportMassUploadPath()
        {
            return ConfigurationManager.AppSettings["ReportMassUploadPath"];
        }

        public static string CSDbCards1()
        {
            if (ConfigurationManager.AppSettings["LocalDb"] == "true")
                return ConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;

            return ConfigurationManager.ConnectionStrings["Serv1"].ConnectionString;
        }

        public static string CSDbCards2()
        {
            if (ConfigurationManager.AppSettings["LocalDb"] == "true")
                return ConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;
            
            return ConfigurationManager.ConnectionStrings["Serv2"].ConnectionString;
        }

        public static bool bLocal()
        {
            if (ConfigurationManager.AppSettings["LocalDb"] == "true")
                return true;

                return false;
        }

        public static string ChannelsName()
        {
            return ConfigurationManager.AppSettings["ChannelsName"];
        }

        public static string ChannelsArray()
        {
            return ConfigurationManager.AppSettings["ChannelsArray"];
        }

        public static string SmtpHost()
        {
            return ConfigurationManager.AppSettings["SmtpHost"];
        }

        public static int SmtpPort()
        {
            int ret = -1;
            int.TryParse(ConfigurationManager.AppSettings["SmtpPort"], out ret);
            return ret;
        }

        public static bool SmtpUseSSL()
        {
            bool ret = false;
            bool.TryParse(ConfigurationManager.AppSettings["SmtpUseSSL"], out ret);
            return ret;
        }

        public static string SmtpUserName()
        {
            return ConfigurationManager.AppSettings["SmtpUserName"];
        }

        public static string SmtpPassword()
        {
            return ConfigurationManager.AppSettings["SmtpPassword"];
        }

        public static string SmtpFrom()
        {
            return ConfigurationManager.AppSettings["SmtpFrom"];
        }


        public static string CSDbTransacts1()
        {
            if (ConfigurationManager.AppSettings["LocalDb"] == "true")
                return ConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;

            return ConfigurationManager.ConnectionStrings["Trans1"].ConnectionString;
        }

        public static string CSDbTransacts2()
        {
            if (ConfigurationManager.AppSettings["LocalDb"] == "true")
                return ConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;

            return ConfigurationManager.ConnectionStrings["Trans2"].ConnectionString;
        }

        public static int MaxCntBlockCard()
        {
            int ret = -1;
            int.TryParse(ConfigurationManager.AppSettings["MaxCntBlockCard"], out ret);
            return ret;
        }

        public static int FrameworkCode()
        {
            int ret = -1;
            Log log = new Log(ConfigurationManager.AppSettings["Logpath"]);
            try
            {
                int.TryParse(ConfigurationManager.AppSettings["FrameworkCode"], out ret);
            }
            catch (Exception ex) { log.Write(LogType.Error, ex.Message); }
            return ret;
        }

  
        public static string GetTemplatePath()
        {
            return ConfigurationManager.AppSettings["Template"];
        }
        public static string Images()
        {
            return ConfigurationManager.AppSettings["Images"];
        }

       
        public static int Facility()
        {
            int ret = -1;
            int.TryParse(ConfigurationManager.AppSettings["Facility"], out ret);
            return ret;
        }
        public static string Host()
        {
            return ConfigurationManager.AppSettings["Host"];
        }
        public static string TagId()
        {
            return ConfigurationManager.AppSettings["TagId"];
        }
        public static int Port()
        {
            int ret = -1;
            int.TryParse(ConfigurationManager.AppSettings["Port"], out ret);
            return ret;
        }
        
    }

    public static class LocaParam
    {
        public static STCardVP cardparam;

        public static STCardVP GetCardParam()
        {
            return cardparam;
        }

        public static void SetParam(STCardVP param)
        {
            cardparam = param;
        }

    }

    public static class CheckerField
    {
        public static bool CheckField(string[] arrchar, params string[] prm)
        {
            bool ret = false;
            foreach (string str in prm)
            {
                foreach (string maskchar in arrchar)
                {
                    if (string.IsNullOrEmpty(str)) continue;
                    if (str.Contains(maskchar)) return true;
                }
            }

            return ret;
        }
    }
}