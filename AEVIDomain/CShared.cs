using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace AEVIDomain
{
    public static class CShared
    {
        public static bool IsConnect(string connectionstring, out string msg)
        {
            bool ret = false;
            msg = null;
            SqlConnection connect;
            try
            {
                connect = new SqlConnection(connectionstring);
                connect.Open();
                if (connect.State == ConnectionState.Open)
                {
                    ret = true;
                    connect.Close();
                }
                else ret = false;
            }
            catch (Exception ex) { msg = ex.Message; ret = false; }
            return ret;
        }
    }
}
