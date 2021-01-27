using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace AEVIDomain
{
    public struct STSession
    {
        public string recid;
        public DateTime dlogon;
        public DateTime? dlogoff;
        public string recvalue;
    }
  
    public class CSession
    {
        public string UserId;
        public string ConnectionString;
        public string LogPath;

        public CSession(string userid, string connectionstring, string logpath) 
        {
            UserId = userid;
            ConnectionString = connectionstring;
            LogPath = logpath;
        }

        public int OpenSession(out STSession data, out string msg)
        {
            int ret = 0;

            Log log = new Log(LogPath);

            data = new STSession();
            msg = null;

            STSession lastsession;

            int numses = 1;

            try
            {
                ret = getlastsession(UserId, out lastsession, out msg);
                if (ret != 0) return ret;

                if (lastsession.recid != null)
                {
                    int x1 = lastsession.recid.IndexOf(".");
                    string s = lastsession.recid.Substring(x1 + 1);
                    int.TryParse(s, out numses);
                    numses++;
                }

                data.recid = string.Format("{0}.{1}", UserId, numses);
                data.dlogon = DateTime.Now;

                ret = insert(data, out msg);
                if (ret == 0) log.Write(LogType.Info, string.Format("Open session: {0}, {1} ",
                     data.recid, data.dlogon.ToLongDateString()));
            }
            catch (Exception ex) { log.Write(LogType.Error, ex.Message); ret = -1; msg = ex.Message; }
            return ret;
        }

        public int CloseSession(STSession data, out string msg)
        {
            int ret = 0;
            msg = null;
            Log log = new Log(LogPath);

            try
            {
                data.dlogoff = DateTime.Now;
                ret = update(data.recid, data, out msg);
                if (ret == 0) log.Write(LogType.Info, string.Format("Close session: {0}, {1} ",
                     data.recid, data.dlogon.ToLongDateString()));
            }
            catch (Exception ex) { log.Write(LogType.Error, ex.Message); ret = -1; msg = ex.Message; }
            return ret;
        }

        private int insert(STSession data, out string msg)
        {
            int ret = 0;
            msg = null;

            SqlConnection connect;
            Log log = new Log(LogPath);

            try
            {
                connect = new SqlConnection(ConnectionString);
                connect.Open();
                if (connect.State == ConnectionState.Open)
                {
                    string query = "INSERT INTO Session (RecId, DateLogOn) VALUES (@1,@2)";
                    SqlCommand cmd = new SqlCommand(query, connect);
                    cmd.Parameters.Add(crp(SqlDbType.VarChar, "@1", data.recid, false));
                    cmd.Parameters.Add(crp(SqlDbType.DateTime, "@2", data.dlogon, false));
                    SqlDataReader reader = cmd.ExecuteReader();
                    connect.Close();
                }
                else return 1;
            }
            catch (Exception ex) { log.Write(LogType.Error, ex.Message); ret = -1; msg = ex.Message; }
            return ret;
        }

        private int update(string id, STSession data, out string msg)
        {
            int ret = 0;
            msg = null;

            SqlConnection connect;
            Log log = new Log(LogPath);

            try
            {
                connect = new SqlConnection(ConnectionString);
                connect.Open();
                if (connect.State == ConnectionState.Open)
                {
                    string query = "UPDATE Session SET DateLogOff=@1,RecValue=@2 WHERE RecId=@3";
                    SqlCommand cmd = new SqlCommand(query, connect);
                    cmd.Parameters.Add(crp(SqlDbType.DateTime, "@1", data.dlogoff, false));
                    cmd.Parameters.Add(crp(SqlDbType.VarChar, "@2", data.recvalue, false));
                    cmd.Parameters.Add(crp(SqlDbType.VarChar, "@", id, false));
                    SqlDataReader reader = cmd.ExecuteReader();
                    connect.Close();
                }
                else return 1;
            }
            catch (Exception ex) { log.Write(LogType.Error, ex.Message); ret = -1; msg = ex.Message; }
            return ret;
        }

        private int getlastsession(string iduser, out STSession data, out string msg)
        {
            int ret = 0;
            msg = null;
            data = new STSession();

            SqlConnection connect;
            Log log = new Log(LogPath);

            try
            {
                connect = new SqlConnection(ConnectionString);
                connect.Open();
                if (connect.State == ConnectionState.Open)
                {
                    string query = string.Format("SELECT RecId,DateLogOn,DateLogOff,RecValue FROM Sessions " +
                        "WHERE RecId LIKE '{0}%' AND DateLogOn=(SELECT MAX(DateLogOn) FROM Sessions WHERE RecId LIKE '{0}%')", iduser);

                    SqlCommand cmd = new SqlCommand(query, connect);
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            if (!read(reader, out data))
                            {
                                ret = -1;
                                msg = "Detailed information can be found in the log file";
                            }
                        }
                    }
                    reader.Dispose();
                }
                else return 1;
            }
            catch (Exception ex) { log.Write(LogType.Error, ex.Message); ret = -1; msg = ex.Message; }
            return ret;
        }

        private bool read(SqlDataReader reader, out STSession data)
        {
            bool ret = true;
            data = new STSession();
            Log log = new Log(LogPath);

            try
            {
                data.recid = reader.GetString(0);
                data.dlogon = reader.GetDateTime(1);
                if (!reader.IsDBNull(2))
                    data.dlogoff = reader.GetDateTime(2);
                else data.dlogoff = null;
                if (!reader.IsDBNull(3))
                    data.recvalue = reader.GetString(3);
                else data.recvalue = null;

            }
            catch (Exception ex) { log.Write(LogType.Error, ex.Message); ret = false; }
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
}
