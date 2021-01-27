using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace AEVIDomain
{
    public struct STUserVP
    {
        public bool isall;
        public string maskusername;
        public string masklogin;
        public string maskemail;
        public int? permission;
        public int? condition;
        public string strdata;
    }

    public struct STPassCache
    {
        public string password;
        public string passwordsalt;
    }

    public struct STUser
    {
        public string userid;
        public string username;
        public string email;
        public string login;
        public string password;
        public string passwordsalt;
        public DateTime creationdate;
        public DateTime modifieddate;
        public int permission;
        public int condition;
        public bool isactivated;
        public DateTime? activateddate;
        public string owneruserid;
        public string comments;
        public string ownerusername;
        public DateTime passvaliddate;
        public bool oldpass;
        public bool islock;
        public DateTime? locktime;
        public int cntmisstry;
        public DateTime? lastmisstime;
        public string newemailkey;
       // public List<STPassCache> passcache;
    }

    public class CUser
    {
        public string UserId;
        public string ConnectionString;
        public string LogPath;

        public CUser(string userid, string connectionstring, string logpath)
        {
            UserId = userid;
            ConnectionString = connectionstring;
            LogPath = logpath;
        }

        public int Insert(STUser data, out string msg)
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
                    string guid = Guid.NewGuid().ToString();
                    string query = "INSERT INTO dbo.Users (UserId,UserName,Email,Login,CreatedDate,LastModifiedDate," +
                        "IsActivated,ActivatedDate,OwnerUserId,Comments,Condition,Permission,PassValidDate,NewEmailKey) " +
                        "VALUES (@1, @2, @3, @4, @5, @6, @7, @8, @9, @10, @11, @12, @13, @14)";
                    SqlCommand cmd = new SqlCommand(query, connect);
                    cmd.Parameters.Add(crp(SqlDbType.VarChar, "@1", guid, false));
                    cmd.Parameters.Add(crp(SqlDbType.VarChar, "@2", data.username, false));
                    cmd.Parameters.Add(crp(SqlDbType.VarChar, "@3", data.email, false));
                    cmd.Parameters.Add(crp(SqlDbType.VarChar, "@4", data.login, false));
                    cmd.Parameters.Add(crp(SqlDbType.DateTime, "@5", data.creationdate, false));
                    cmd.Parameters.Add(crp(SqlDbType.DateTime, "@6", data.modifieddate, false));
                    cmd.Parameters.Add(crp(SqlDbType.Bit, "@7", data.isactivated, true));
                    cmd.Parameters.Add(crp(SqlDbType.DateTime, "@8", data.activateddate, true));
                    cmd.Parameters.Add(crp(SqlDbType.VarChar, "@9", data.owneruserid, true));
                    cmd.Parameters.Add(crp(SqlDbType.VarChar, "@10", data.comments, true));
                    cmd.Parameters.Add(crp(SqlDbType.Int, "@11", data.condition, false));
                    cmd.Parameters.Add(crp(SqlDbType.Int, "@12", data.permission, false));
                    cmd.Parameters.Add(crp(SqlDbType.DateTime, "@13", data.passvaliddate, false));
                    cmd.Parameters.Add(crp(SqlDbType.VarChar, "@14", data.newemailkey, true));
                    SqlDataReader reader = cmd.ExecuteReader();
                    connect.Close();
                }
                else return 1;
            }
            catch (Exception ex) { log.Write(LogType.Error, ex.Message); ret = -1; msg = ex.Message; }
            return ret;
        }

        public int Update(string userid, STUser data, out string msg)
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
                    string query = "UPDATE dbo.Users SET UserName=@1,Email=@2,Login=@3,LastModifiedDate=@4,Comments=@5," +
                        "Condition=@6,Permission=@7 WHERE UserId=@8";
                    SqlCommand cmd = new SqlCommand(query, connect);
                    cmd.Parameters.Add(crp(SqlDbType.VarChar, "@1", data.username, false));
                    cmd.Parameters.Add(crp(SqlDbType.VarChar, "@2", data.email, false));
                    cmd.Parameters.Add(crp(SqlDbType.VarChar, "@3", data.login, false));
                    cmd.Parameters.Add(crp(SqlDbType.DateTime, "@4", data.modifieddate, false));
                    cmd.Parameters.Add(crp(SqlDbType.VarChar, "@5", data.comments, true));
                    cmd.Parameters.Add(crp(SqlDbType.Int, "@6", data.condition, false));
                    cmd.Parameters.Add(crp(SqlDbType.Int, "@7", data.permission, false));
                  //  cmd.Parameters.Add(crp(SqlDbType.DateTime, "@8", data.passvaliddate, false));
                    cmd.Parameters.Add(crp(SqlDbType.VarChar, "@8", userid, false));
                    SqlDataReader reader = cmd.ExecuteReader();
                    connect.Close();
                }
                else return 1;
            }
            catch (Exception ex) { log.Write(LogType.Error, ex.Message); ret = -1; msg = ex.Message; }
            return ret;
        }
        public int UpdatePassword(string userid, string password, string passwordsalt, DateTime passvaliddate, out string msg)
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
                    string query = "UPDATE dbo.Users SET Password=@1,PasswordSalt=@2,PassValidDate=@3 WHERE UserId=@4";
                    SqlCommand cmd = new SqlCommand(query, connect);
                    cmd.Parameters.Add(crp(SqlDbType.VarChar, "@1", password, false));
                    cmd.Parameters.Add(crp(SqlDbType.VarChar, "@2", passwordsalt, false));
                    cmd.Parameters.Add(crp(SqlDbType.DateTime, "@3", passvaliddate, false));
                    cmd.Parameters.Add(crp(SqlDbType.VarChar, "@4", userid, false));
          
                    SqlDataReader reader = cmd.ExecuteReader();
                    connect.Close();
                }
                else return 1;
            }
            catch (Exception ex) { log.Write(LogType.Error, ex.Message); ret = -1; msg = ex.Message; }
            return ret;
        }
       
        public int Delete(string userid, out string msg)
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
                    string query = "UPDATE dbo.Users SET Condition=2 WHERE UserId=@1";
                    SqlCommand cmd = new SqlCommand(query, connect);
                    cmd.Parameters.Add(crp(SqlDbType.VarChar, "@1", userid, false));
                    SqlDataReader reader = cmd.ExecuteReader();
                    connect.Close();
                }
                else return 1;
            }
            catch (Exception ex) { log.Write(LogType.Error, ex.Message); ret = -1; msg = ex.Message; }
            return ret;
        }

        public int Activate(string userid, string password, string salt, out string msg)
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
                    string query = "UPDATE dbo.Users SET IsActivated=@1,ActivatedDate=@2,LastModifiedDate=@3," +
                        "NewEmailKey=@4,Password=@5,PasswordSalt=@6,PassValidDate=@7  WHERE UserId=@8";
                    SqlCommand cmd = new SqlCommand(query, connect);
                    DateTime dt = DateTime.Now;
                    DateTime ad = dt.AddMonths(6);
                    cmd.Parameters.Add(crp(SqlDbType.Bit, "@1", true, false));
                    cmd.Parameters.Add(crp(SqlDbType.DateTime, "@2", dt, false));
                    cmd.Parameters.Add(crp(SqlDbType.DateTime, "@3", dt, false));
                    cmd.Parameters.Add(crp(SqlDbType.VarChar, "@4", null, true));
                    cmd.Parameters.Add(crp(SqlDbType.VarChar, "@5", password, true));
                    cmd.Parameters.Add(crp(SqlDbType.VarChar, "@6", salt, true));
                    cmd.Parameters.Add(crp(SqlDbType.DateTime, "@7", ad, false));
                    cmd.Parameters.Add(crp(SqlDbType.VarChar, "@8", userid, false));
                    SqlDataReader reader = cmd.ExecuteReader();
                    connect.Close();
                }
                else return 1;
            }
            catch (Exception ex) { log.Write(LogType.Error, ex.Message); ret = -1; msg = ex.Message; }
            return ret;
        }
        public int FPS(string userid, string password, string salt, out string msg)
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
                    string query = "UPDATE dbo.Users SET LastModifiedDate=@1,NewEmailKey=@2,Password=@3,PasswordSalt=@4," +
                        "PassValidDate=@5  WHERE UserId=@6";
                    SqlCommand cmd = new SqlCommand(query, connect);
                    DateTime dt = DateTime.Now;
                    DateTime ad = dt.AddMonths(6);
                    cmd.Parameters.Add(crp(SqlDbType.DateTime, "@1", dt, false));
                    cmd.Parameters.Add(crp(SqlDbType.VarChar, "@2", null, true));
                    cmd.Parameters.Add(crp(SqlDbType.VarChar, "@3", password, true));
                    cmd.Parameters.Add(crp(SqlDbType.VarChar, "@4", salt, true));
                    cmd.Parameters.Add(crp(SqlDbType.DateTime, "@5", ad, false));
                    cmd.Parameters.Add(crp(SqlDbType.VarChar, "@6", userid, false));
                    SqlDataReader reader = cmd.ExecuteReader();
                    connect.Close();
                }
                else return 1;
            }
            catch (Exception ex) { log.Write(LogType.Error, ex.Message); ret = -1; msg = ex.Message; }
            return ret;
        }
        public int SetKeyFPS(string userid, string newemailkey, out string msg)
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
                    string query = "UPDATE dbo.Users SET NewEmailKey=@1 WHERE UserId=@2";
                    SqlCommand cmd = new SqlCommand(query, connect);
                    cmd.Parameters.Add(crp(SqlDbType.VarChar, "@1", newemailkey, true));
                    cmd.Parameters.Add(crp(SqlDbType.VarChar, "@2", userid, false));

                    SqlDataReader reader = cmd.ExecuteReader();
                    connect.Close();
                }
                else return 1;
            }
            catch (Exception ex) { log.Write(LogType.Error, ex.Message); ret = -1; msg = ex.Message; }
            return ret;
        }

        public int GetData(STUserVP param, out List<STUser> data, out string msg)
        {
            int ret = 0;
            data = new List<STUser>();
            msg = null;
            STUser stReads;

            string where;
            string owner;
            string query;

            SqlConnection connect;
            Log log = new Log(LogPath);

            try
            {
                connect = new SqlConnection(ConnectionString);
                connect.Open();

                if (connect.State == ConnectionState.Open)
                {
                    
                    STUser stUser;
                    GetRecordByUserId(UserId, out stUser, out msg);

                    where = null;

                    if (param.isall) owner = null;
                    else owner = string.Format("AND U.OwnerUserId LIKE '%{0}%' ", UserId);
                    if (param.maskusername != null)
                        where += string.Format("AND UPPER(U.UserName) LIKE '%{0}%' ", param.maskusername.ToUpper());
                    if (param.masklogin != null)
                        where += string.Format("AND UPPER(U.Login) LIKE '%{0}%' ", param.masklogin.ToUpper());
                    if (param.maskemail != null)
                        where += string.Format("AND UPPER(U.Email) LIKE '%{0}%' ", param.maskemail.ToUpper());
                    if (param.permission != null)
                        where += string.Format("AND U.Permission={0} ", (int)param.permission);
                    if (param.condition != null)
                        where += string.Format("AND U.Condition={0} ", (int)param.condition);
                    
                    if (UserId != null)
                        query = string.Format("SELECT U.UserId,U.UserName,U.Email,U.Login,U.Password," +
                            "U.PasswordSalt,U.CreatedDate,U.LastModifiedDate,U.IsActivated,U.ActivatedDate,U.OwnerUserId," +
                            "U.Comments,U.Condition,U.Permission,U.PassValidDate,U.CntMissTry,U.LockTime,U.LastMissTime," +
                            "U.NewEmailKey,UI.UserName " +
                            "FROM Users U LEFT JOIN (SELECT UserId, UserName FROM Users) UI " +
                           "ON U.OwnerUserId=UI.UserId WHERE U.Permission<>0 AND U.Permission>{0} AND U.UserId<>'{1}' {2} {3} " +
                           "ORDER BY U.CreatedDate DESC", stUser.permission, UserId, where, owner);
                    else
                        query = null;

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

        public int GetRecordByUserId(string userid, out STUser data, out string msg)
        {
            int ret = 0;
            data = new STUser();
            msg = null;

            SqlConnection connect;
            Log log = new Log(LogPath);

            try
            {
                connect = new SqlConnection(ConnectionString);
                connect.Open();

                if (connect.State == ConnectionState.Open)
                {
                    string query = "SELECT U.UserId,U.UserName,U.Email,U.Login,U.Password," +
                                "U.PasswordSalt,U.CreatedDate,U.LastModifiedDate,U.IsActivated,U.ActivatedDate,U.OwnerUserId," +
                                "U.Comments,Condition,U.Permission,U.PassValidDate,U.CntMissTry,U.LockTime,U.LastMissTime," +
                                "U.NewEmailKey,UI.UserName " +
                                "FROM dbo.Users U LEFT JOIN (SELECT UserId, UserName FROM dbo.Users) UI " +
                               "ON U.OwnerUserId=UI.UserId WHERE U.UserId=@1";

                    SqlCommand cmd = new SqlCommand(query, connect);
                    cmd.Parameters.Add(crp(SqlDbType.VarChar, "@1", userid, false));
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            if (!read(reader, out data, out msg))
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
        public int GetRecordByUserLogin(string login, out STUser data, out string msg)
        {
            int ret = 0;
            data = new STUser();
            msg = null;

            SqlConnection connect;
            Log log = new Log(LogPath);

            try
            {
                connect = new SqlConnection(ConnectionString);
                connect.Open();

                if (connect.State == ConnectionState.Open)
                {
                    string query = "SELECT U.UserId,U.UserName,U.Email,U.Login,U.Password," +
                                "U.PasswordSalt,U.CreatedDate,U.LastModifiedDate,U.IsActivated,U.ActivatedDate,U.OwnerUserId," +
                                "U.Comments,Condition,U.Permission,U.PassValidDate,U.CntMissTry,U.LockTime,U.LastMissTime," +
                                "U.NewEmailKey,UI.UserName " +
                                "FROM dbo.Users U LEFT JOIN (SELECT UserId, UserName FROM dbo.Users) UI " +
                               "ON U.OwnerUserId=UI.UserId WHERE U.Login=@1";

                    SqlCommand cmd = new SqlCommand(query, connect);
                    cmd.Parameters.Add(crp(SqlDbType.VarChar, "@1", login, false));
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            if (!read(reader, out data, out msg))
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
        public int GetRecordByUserKey(string key, out STUser data, out string msg)
        {
            int ret = 0;
            data = new STUser();
            msg = null;

            SqlConnection connect;
            Log log = new Log(LogPath);

            try
            {
                connect = new SqlConnection(ConnectionString);
                connect.Open();

                if (connect.State == ConnectionState.Open)
                {
                    string query = "SELECT U.UserId,U.UserName,U.Email,U.Login,U.Password," +
                                "U.PasswordSalt,U.CreatedDate,U.LastModifiedDate,U.IsActivated,U.ActivatedDate,U.OwnerUserId," +
                                "U.Comments,Condition,U.Permission,U.PassValidDate,U.CntMissTry,U.LockTime,U.LastMissTime," +
                                "U.NewEmailKey,UI.UserName " +
                                "FROM dbo.Users U LEFT JOIN (SELECT UserId, UserName FROM dbo.Users) UI " +
                               "ON U.OwnerUserId=UI.UserId WHERE U.NewEmailKey=@1";

                    SqlCommand cmd = new SqlCommand(query, connect);
                    cmd.Parameters.Add(crp(SqlDbType.VarChar, "@1", key, false));
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            if (!read(reader, out data, out msg))
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
              
        public int Lock(string userid, int cntmisstry, DateTime? lastmisstime, DateTime? locktime, out string msg)
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
                    string query = "UPDATE dbo.Users SET CntMissTry=@1,LastMissTime=@2,LockTime=@3 WHERE UserId=@4";
                    SqlCommand cmd = new SqlCommand(query, connect);
                    cmd.Parameters.Add(crp(SqlDbType.Int, "@1", cntmisstry, false));
                    cmd.Parameters.Add(crp(SqlDbType.DateTime, "@2", lastmisstime, true));
                    cmd.Parameters.Add(crp(SqlDbType.DateTime, "@3", locktime, true));
                    cmd.Parameters.Add(crp(SqlDbType.VarChar, "@4", userid, false));
                    SqlDataReader reader = cmd.ExecuteReader();
                    connect.Close();
                }
                else return 1;
            }
            catch (Exception ex) { log.Write(LogType.Error, ex.Message); ret = -1; msg = ex.Message; }
            return ret;
        }

        public int GetPassCache(string login, out List<STPassCache> data, out string msg)
        {
            int ret = 0;
            msg = null;
            data = new List<STPassCache>();

            SqlConnection connect;
            Log log = new Log(LogPath);

            try
            {
                connect = new SqlConnection(ConnectionString);
                connect.Open();

                if (connect.State == ConnectionState.Open)
                {
                    string query = "SELECT PassCache FROM dbo.Users WHERE Login=@1";

                    SqlCommand cmd = new SqlCommand(query, connect);
                    cmd.Parameters.Add(crp(SqlDbType.VarChar, "@1", login, false));
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            string retstring = reader.GetString(0);
                            data = convert_passcache(retstring);
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
        public int AddPassToPassCache(string login, STPassCache pc, out string msg)
        {
            int ret = 0;
            msg = null;
            
            SqlConnection connect;
            Log log = new Log(LogPath);

            string passcache = null;

            try
            {

                List<STPassCache> lstpc = new List<STPassCache>();
                int retvalue = GetPassCache(login, out lstpc, out msg);
             //   lstpc.Add(pc);

                int cnt=1;
                passcache = string.Format("{0}:{1},", pc.password, pc.passwordsalt);

                foreach(STPassCache item in lstpc)
                {
                    if (cnt < 4)
                    {
                        passcache += string.Format("{0}:{1},", item.password, item.passwordsalt);
                        cnt++;
                    }
                }

                connect = new SqlConnection(ConnectionString);
                connect.Open();

                if (connect.State == ConnectionState.Open)
                {
                    string query = "UPDATE dbo.Users SET PassCache=@1 WHERE Login=@2";

                    SqlCommand cmd = new SqlCommand(query, connect);
                    cmd.Parameters.Add(crp(SqlDbType.VarChar, "@1", passcache, false));
                    cmd.Parameters.Add(crp(SqlDbType.VarChar, "@2", login, false));
                    SqlDataReader reader = cmd.ExecuteReader();

                    connect.Close();
                }
                else return 1;
            }
            catch (Exception ex) { log.Write(LogType.Error, ex.Message); ret = -1; msg = ex.Message; }
            return ret;
        }

        private List<STPassCache> convert_passcache(string data)
        {
            List<STPassCache> ret = new List<STPassCache>();
            if (string.IsNullOrEmpty(data)) return ret;

            STPassCache item;
            string[] words = data.Split(',');
            foreach (string cp in words)
            {
                if (cp.Length > 1)
                {
                    string[] cache = cp.Split(':');
                    if (cache.Length == 2)
                    {
                        item = new STPassCache();
                        item.password = cache[0];
                        item.passwordsalt = cache[1];
                        ret.Add(item);
                    }
                }
            }

            return ret;
        }

        private bool read(SqlDataReader reader, out STUser data, out string msg)
        {
            bool ret = true;
            data = new STUser();
            msg = null;
            try
            {
                data.userid = reader.GetString(0);
                data.username = reader.GetString(1);
                data.email = reader.GetString(2);
                data.login = reader.GetString(3);
                if (!reader.IsDBNull(4))
                    data.password = reader.GetString(4);
                else data.password = null;
                if (!reader.IsDBNull(5))
                    data.passwordsalt = reader.GetString(5);
                else data.passwordsalt = null;
                data.creationdate = reader.GetDateTime(6);
                data.modifieddate = reader.GetDateTime(7);
                data.isactivated = reader.GetBoolean(8);
                if (!reader.IsDBNull(9))
                    data.activateddate = reader.GetDateTime(9);
                else data.activateddate = null;
                if (!reader.IsDBNull(10))
                    data.owneruserid = reader.GetString(10);
                else data.owneruserid = null;
                if (!reader.IsDBNull(11))
                    data.comments = reader.GetString(11);
                else data.comments = null;
                data.condition = reader.GetInt32(12);
                data.permission = reader.GetInt32(13);
                data.passvaliddate = reader.GetDateTime(14);
                if (data.passvaliddate <= DateTime.Now)
                    data.oldpass = true;
                else data.oldpass = false;
                if (!reader.IsDBNull(15))
                    data.cntmisstry = reader.GetInt32(15);
                else data.cntmisstry = 0;
                if (!reader.IsDBNull(16))
                    data.locktime = reader.GetDateTime(16);
                else data.locktime = null;
                if (data.locktime != null && data.locktime >= DateTime.Now)
                    data.islock = true;
                else data.islock = false;
                   if (!reader.IsDBNull(17))
                    data.lastmisstime = reader.GetDateTime(17);
                else data.lastmisstime = null;

                   if (!reader.IsDBNull(18))
                       data.newemailkey = reader.GetString(18);
                   else data.newemailkey = null;

                if (!reader.IsDBNull(19))
                    data.ownerusername = reader.GetString(19);
                else data.ownerusername = null;

               
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
    }
}