using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace AEVIDomain
{
    public struct STEventVP
    {
        public DateTime dtbegin;
        public DateTime dtend;
        public string maskedpan;
        public string strdata;
    }

    public struct STEvent
    {
        public DateTime ltime;
        public string country;
        public string msg;
        public string proccode;
        public string maskedpan;
        public string ifsfcode;
        public string utdcode;
        public string text;
    }

    public class CEvent
    {
        public string UserId;
        public string ConnectionString;
        public string ConnectionStringReserve;
        public string LogPath;

        public CEvent(string userid, string connectionstring, string connectionstringreserve, string logpath) 
        {
            UserId = userid;
            ConnectionString = connectionstring;
            ConnectionStringReserve = connectionstringreserve;
            LogPath = logpath;
        }

        public int GetData(STEventVP param, out List<STEvent> data, out string msg)
        {
            int ret = 0;
            data = new List<STEvent>();
            List<STEvent> retdata = new List<STEvent>();

            SqlConnection connect;
            SqlCommand cmd;
            SqlDataReader reader;

            Log log = new Log(LogPath);

            msg = null;

            string where;

            STEvent item;

            try
            {
                where = null;

                if (param.maskedpan != null)
                    where += string.Format("WHERE T.PAN LIKE '{0}' ", param.maskedpan.Replace('*', '%'));

                string query = string.Format(
                    "SELECT T.LTime,T.Country,T.Msg,T.ProcCode,T.PAN,T.IFSFCode,T.UTDCode,T.Text FROM " +
                    "( " +
                    "SELECT LTime, Country, " +
                    "dbo.getTag(Tags, 'Msg') as Msg, " +
                    "dbo.getTag(Tags, 'ProcCode') as ProcCode, " +
                    "dbo.getTag(Tags, 'PAN') as PAN, " +
                    "dbo.getTag(Tags, 'IFSFCode') as IFSFCode, " +
                    "dbo.getTag(Tags, 'UTDCode') as UTDCode, " +
                    "dbo.getTag(Tags, 'Text') as Text " +
                    "from AEVIEvents where LTime>=@1 AND LTime<=@2) T  {0} ", where);

                connect = new SqlConnection(ConnectionString);
                connect.Open();

                if (connect.State == ConnectionState.Open)
                {
                    cmd = new SqlCommand(query, connect);
                    cmd.Parameters.Add(crp(SqlDbType.DateTime, "@1", param.dtbegin, true));
                    cmd.Parameters.Add(crp(SqlDbType.DateTime, "@2", param.dtend, true));
                    reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            item = read(reader);
                            retdata.Add(item);
                        }

                    }

                    reader.Dispose();
                }

                if (ConnectionStringReserve != ConnectionString)
                {
                    connect = new SqlConnection(ConnectionStringReserve);
                    connect.Open();

                    if (connect.State == ConnectionState.Open)
                    {
                        cmd = new SqlCommand(query, connect);
                        cmd.Parameters.Add(crp(SqlDbType.DateTime, "@1", param.dtbegin, true));
                        cmd.Parameters.Add(crp(SqlDbType.DateTime, "@2", param.dtend, true));
                        reader = cmd.ExecuteReader();

                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                item = read(reader);
                                retdata.Add(item);
                            }

                        }

                        reader.Dispose();
                    }
                }

                if (retdata.Count > 0)
                {
                    data = retdata;
                    ListCompareEventData clComp = new ListCompareEventData();
                    data.Sort(clComp);
                    
                }
            }
            catch (Exception ex) { log.Write(LogType.Error, ex.Message); ret = -1; msg = ex.Message; }
            return ret;
        }

        private STEvent read(SqlDataReader reader)
        {
            STEvent item = new STEvent();
            Log log = new Log(LogPath);
            string pan;

            try
            {
                item.ltime = reader.GetDateTime(0);
                if (!reader.IsDBNull(1))
                    item.country = reader.GetString(1);
                else item.country = null;
                if (!reader.IsDBNull(2))
                    item.msg = reader.GetString(2);
                else item.msg = null;
                if (!reader.IsDBNull(3))
                    item.proccode = reader.GetString(3);
                else item.proccode = null;
                if (!reader.IsDBNull(4))
                {
                    pan = reader.GetString(4);
                    item.maskedpan = get_mask_pan(pan);
                }
                else pan = null;
                if (!reader.IsDBNull(5))
                    item.ifsfcode = reader.GetString(5);
                else item.ifsfcode = null;
                if (!reader.IsDBNull(6))
                    item.utdcode = reader.GetString(6);
                else item.utdcode = null;
                if (!reader.IsDBNull(7))
                    item.text = reader.GetString(7);
                else item.text = null;
            }
            catch (Exception ex) { log.Write(LogType.Error, ex.Message);  }

            return item;
        }

        public string get_mask_pan(string data)
        {
            Log log = new Log(LogPath);

            string ret = null;
            try
            {
                if (data.Length < 11) return "The PAN length less than 11";
                for (int i = 0; i < data.Length; i++)
                {
                    if (i <= 5 || i >= data.Length - 4) ret += data[i];
                    else ret += "*";
                }
            }
            catch (Exception ex) { log.Write(LogType.Error, ex.Message); }
            return ret;
        }
        
        private SqlParameter crp(SqlDbType type, string pname, object val, bool isn)
        {
            SqlParameter param = new SqlParameter();
            param.ParameterName = pname;
            param.SqlDbType = type;
            param.IsNullable = isn;
            if (val != null)
                param.Value = val;
            else
                param.Value = DBNull.Value;

            return param;
        }

    }


    public class ListCompareEventData : IComparer<STEvent>
    {

        public int Compare(STEvent x, STEvent y)
        {
            if (x.ltime < y.ltime) return -1;
            if (x.ltime > y.ltime) return 1;
           
            return 0;
        }

    };
}
