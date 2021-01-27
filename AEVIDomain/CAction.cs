using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;


namespace AEVIDomain
{
    public enum ActionType
    {
        AddUser,
        EditUser,
        DeleteUser,
        AddCard,
        EditCard,
        DeleteCard,
        LogON,
        LogOFF,
        Upload,
    }

    public struct STAction
    {
        public DateTime dt;
        public string actionname;
        public string value;
        public string username;
    }

    public class CAction
    {
        public string UserId;
        public string ConnectionString;
        public string LogPath;

        public CAction(string userid, string connectionstring, string logpath)
        {
            UserId = userid;
            ConnectionString = connectionstring;
            LogPath = logpath;
        }

        public int AddAction(ActionType type, string value, out string msg)
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
                    string query = "INSERT INTO dbo.Actions (UserId,ActionName,ActionDate,RecValue) VALUES (@1, @2, @3, @4)";
                    SqlCommand cmd = new SqlCommand(query, connect);
                    cmd.Parameters.Add(crp(SqlDbType.VarChar, "@1", UserId, false));
                    cmd.Parameters.Add(crp(SqlDbType.VarChar, "@2", type.ToString(), false));
                    cmd.Parameters.Add(crp(SqlDbType.DateTime, "@3", DateTime.Now, false));
                    cmd.Parameters.Add(crp(SqlDbType.VarChar, "@4", value, true));
                    SqlDataReader reader = cmd.ExecuteReader();
                    connect.Close();
                }
                else return 1;
            }
            catch (Exception ex) { log.Write(LogType.Error, ex.Message); ret = -1; msg = ex.Message; }
            return ret;
        }

        public List<STAction> GetReport(DateTime dtbegin, DateTime dtend)
        {
            string msg = null;
            List<STAction> ret = new List<STAction>();
            STAction item;
            SqlConnection connect;
            Log log = new Log(LogPath);

            CUser clUser = new CUser(UserId, ConnectionString, LogPath);
            STUser stUser;
            

            string where = null;

            try
            {
                clUser.GetRecordByUserId(UserId, out stUser, out msg);

                connect = new SqlConnection(ConnectionString);
                connect.Open();
                if (connect.State == ConnectionState.Open)
                {
                    if (stUser.permission != 2) where = string.Format("AND (T1.UserId=@3 OR T2.Permission>{0})", stUser.permission);
                    else where = string.Format("AND T1.UserId=@3");
                    
                    string query = string.Format("SELECT T1.ActionName,T1.ActionDate,T1.RecValue,T2.UserName FROM dbo.Actions T1 "+
                        "LEFT JOIN dbo.Users T2 ON T1.UserId = T2.UserId " +
                        "WHERE T1.ActionDate>=@1 AND T1.ActionDate<=@2 {0} ORDER BY T1.ActionDate", where);
                    SqlCommand cmd = new SqlCommand(query, connect);
                    cmd.Parameters.Add(crp(SqlDbType.DateTime, "@1", dtbegin, false));
                    cmd.Parameters.Add(crp(SqlDbType.DateTime, "@2", dtend, false));
                    cmd.Parameters.Add(crp(SqlDbType.Char, "@3", UserId, false));
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            item = new STAction();
                            if (!reader.IsDBNull(0))
                                item.actionname = reader.GetString(0);
                            else item.actionname = null;
                            if (!reader.IsDBNull(1))
                                item.dt = reader.GetDateTime(1);
                            if (!reader.IsDBNull(2))
                                item.value = reader.GetString(2);
                            else item.value = null;
                            if (!reader.IsDBNull(3))
                                item.username = reader.GetString(3);
                            else item.username = null;
                            
                            ret.Add(item);
                        }
                    }
                    reader.Dispose();
                    connect.Close();
                }
            }
            catch (Exception ex) { log.Write(LogType.Error, ex.Message);}
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
