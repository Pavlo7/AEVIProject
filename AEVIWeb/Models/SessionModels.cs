using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using AEVIDomain;

namespace AEVIWeb.Models
{
    public class SessionModels
    {
        public static SessionModels Instance = new SessionModels();

        public int OpenSession()
        {
            int ret = 0;
            string msg;

            STSession st = new STSession();
            CSession clSession =  new CSession(LocalData.UserId(), LocalData.CSDbUsers(), LocalData.LogPath());

            int retvalue = clSession.OpenSession(out st, out msg);

            return ret;
        }
    }
}