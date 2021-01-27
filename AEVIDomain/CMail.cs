using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace AEVIDomain
{
    public struct STMail
    {
        public string recid;
        public string recvalue;
        public string to;
        public string tamplate;
        public string pan;
        public string fleetpwd;
        public string linkkey;
        public string dtcreate;
        public string dtmistsent;
        public string login;
        public string attachment;
    }

    public class CMail
    {
        public string UserId;
        public string ConnectionString;
        public string LogPath;

        public CMail(string userid, string connectionstring, string logpath)
        {
            UserId = userid;
            ConnectionString = connectionstring;
            LogPath = logpath;
        }

        public int GetData(out List<STMail> data, out string msg)
        {
            int ret = 0;
            data = new List<STMail>();
            msg = null;
            STMail stReads;

            string query;

            SqlConnection connect;
            Log log = new Log(LogPath);

            try
            {
                connect = new SqlConnection(ConnectionString);
                connect.Open();

                if (connect.State == ConnectionState.Open)
                {
                    query = string.Format("SELECT CAST(RecId AS varchar(36)),RecValue, " +
                        "dbo.GetTag(RecValue, 'TOA') AS ToAddress," +
                        "dbo.GetTag(RecValue, 'TMP') AS Template," +
                        "dbo.GetTag(RecValue, 'PAN') AS MaskedPan," +
                        "dbo.GetTag(RecValue, 'LIN') AS LinkKey," +
                        "dbo.GetTag(RecValue, 'PWD') AS FleetPwd, " +
                        "dbo.GetTag(RecValue, 'LGN') AS Login, " +
                        "dbo.GetTag(RecValue, 'ATC') AS Attachment " +
                        "FROM dbo.Mails ");
                    
                    SqlCommand cmd = new SqlCommand(query, connect);
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            if (read(reader, out stReads, out msg))
                                data.Add(stReads);
                            else
                            {
                                ret = -1;
                            }
                        }
                    }
                    reader.Dispose();
                    connect.Close();
                }
                else return 1;
            }
            catch (Exception ex) { log.Write(LogType.Error, ex.Message); ret = -1; msg = ex.Message; }
            return ret;
        }

        public int Insert(STMail data, out string msg)
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
                    toTags(data, out data.recvalue, out msg);
                    string query = "INSERT INTO dbo.Mails (RecValue) VALUES (@1)";
                    SqlCommand cmd = new SqlCommand(query, connect);
                    cmd.Parameters.Add(crp(SqlDbType.VarChar, "@1", data.recvalue, false));
                    SqlDataReader reader = cmd.ExecuteReader();
                    connect.Close();
                }
                else return 1;
            }
            catch (Exception ex) { log.Write(LogType.Error, ex.Message); ret = -1; msg = ex.Message; }
            return ret;
        }

        public int Delete(string recid, out string msg)
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
                    string query = "DELETE FROM dbo.Mails WHERE RecId=@1";
                    SqlCommand cmd = new SqlCommand(query, connect);
                    cmd.Parameters.Add(crp(SqlDbType.VarChar, "@1", recid, false));
                    SqlDataReader reader = cmd.ExecuteReader();
                    connect.Close();
                }
                else return 1;
            }
            catch (Exception ex) { log.Write(LogType.Error, ex.Message); ret = -1; msg = ex.Message; }
            return ret;
        }

        public int UpdateMissSentDate(string recid, string recvalue, out string msg)
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
                    TAG tag = new TAG(recvalue);
                    tag.Delete("MDT");
                    tag.SetValue("MDT", DateTime.Now.ToString("yyyyMMddHHmmss"));
                    string retvalue = tag.ToString();

                    string query = "UPDATE dbo.Mails SET RecValue=@1 WHERE RecId=@2";
                    SqlCommand cmd = new SqlCommand(query, connect);
                    cmd.Parameters.Add(crp(SqlDbType.VarChar, "@1", retvalue, false));
                    cmd.Parameters.Add(crp(SqlDbType.VarChar, "@2", recid, false));
                    SqlDataReader reader = cmd.ExecuteReader();
                    connect.Close();
                }
                else return 1;
            }
            catch (Exception ex) { log.Write(LogType.Error, ex.Message); ret = -1; msg = ex.Message; }
            return ret;
        }

        private bool read(SqlDataReader reader, out STMail data, out string msg)
        {
            bool ret = true;
            data = new STMail();
            msg = null;
            try
            {
                data.recid = reader.GetValue(0).ToString();
                if (!reader.IsDBNull(1))
                    data.recvalue = reader.GetString(1);
                else data.recvalue = null;
                if (!reader.IsDBNull(2))
                    data.to = reader.GetString(2);
                else data.to = null;
                if (!reader.IsDBNull(3))
                    data.tamplate = reader.GetString(3);
                else data.tamplate = null;
                if (!reader.IsDBNull(4))
                    data.pan = reader.GetString(4);
                else data.pan = null;
                if (!reader.IsDBNull(5))
                    data.linkkey = reader.GetString(5);
                else data.linkkey = null;
                if (!reader.IsDBNull(6))
                    data.fleetpwd = reader.GetString(6);
                else data.fleetpwd = null;
                if (!reader.IsDBNull(7))
                    data.login = reader.GetString(7);
                else data.login = null;
                if (!reader.IsDBNull(8))
                    data.attachment = reader.GetString(8);
                else data.attachment = null;
            }
            catch (Exception ex) { msg = ex.Message; ret = false; }
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

        private int toTags(STMail data, out string retvalue, out string msg)
        {
            int ret = 0;

            retvalue = null;
            msg = null;

            string tag = null;

            Log log = new Log(LogPath);

            try
            {
                tag = string.Format("<CDT={0}>", data.dtcreate);
                retvalue += tag;

                tag = string.Format("<TOA={0}>", data.to);
                retvalue += tag;

                tag = string.Format("<TMP={0}>", data.tamplate);
                retvalue += tag;

                if (data.pan != null) tag = string.Format("<PAN={0}>", data.pan);
                else tag = null;
                retvalue += tag;

                if (data.linkkey != null) tag = string.Format("<LIN={0}>", data.linkkey);
                else tag = null;
                retvalue += tag;

                if (data.fleetpwd != null) tag = string.Format("<PWD={0}>", data.fleetpwd);
                else tag = null;
                retvalue += tag;

                if (data.login != null) tag = string.Format("<LGN={0}>", data.login);
                else tag = null;
                retvalue += tag;

                if (data.attachment != null) tag = string.Format("<ATC={0}>", data.attachment);
                else tag = null;
                retvalue += tag;
            }
            catch (Exception ex) { log.Write(LogType.Error, ex.Message); }
            return ret;
        }

        /*public bool SendNotice(string host, int port, bool useSsl, string userName, string password, string from, out string msg)
        {
            bool ret = true;
            Log log = new Log(LogPath);
            msg = null;

            List<STMail> list = new List<STMail>();

            try
            {
                GetData(out list, out msg);
                foreach (STMail mail in list)
                {
                    if(!SMTPNotice.SendNotice(host, port, useSsl, userName, password, from, mail.email, mail.subject,
                        mail.text, out msg))
                    {
                        log.Write(LogType.Error, msg);
                        ret = false;
                    }
                    else
                    {
                        if (Delete(mail.id, out msg) != 0)
                            ret = false;
                    }
                }
            }
            catch (Exception ex) { log.Write(LogType.Error, ex.Message); msg = ex.Message; ret = false; }
            return ret;
        }*/
    }
}
