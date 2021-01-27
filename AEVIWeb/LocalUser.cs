using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AEVIDomain;
using System.Configuration;

namespace AEVIWeb
{
    public static class LocalUser
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
    }
}