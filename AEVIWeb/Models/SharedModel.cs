using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using AEVIDomain;

namespace AEVIWeb.Models
{
    public static class SharedModel
    {
        public static bool IsConnect(string cs, out string msg)
        {
            bool ret = false;
            msg = null;
            
            try
            {
               ret =  CShared.IsConnect(cs, out msg);
                
            }
            catch (Exception ex) { msg = ex.Message;  ret = false; }
            return ret;
        }
    }
}