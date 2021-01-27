using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace AEVIDomain
{
    public struct STCardVP
    {
        public bool bParam;
        public string maskedpan;
        public string expdate;
        public int? loyaltyflag;
        public int? fleetidflag;
        public int? odometerflag;
        public string maskedvrn;
        public string maskeddrivername;
        public string maskedcompanyname;
        public int? intdesignator;
        public string maskedemail;
        public int? blockflag;
        public int? bindflag;
        public string strdata;
        public string maskaccount;
        public string masksubaccount;
    }

    public struct STBind
    {
        public string pan;
        public string flit;
        public string recvalue;
    }

    public struct STCard
    {
        public string pan;
        public string recvalue;
        public string maskedpan;
        public string expdate;
        public int loyaltyflag;
        public int fleetidflag;
        public int odometerflag;
        public string vrn;
        public string drivername;
        public string companyname;
        public string companyname_ir;
        public int intdesignator;
        public string email;
       // public List<STBind> flitdata;
        public string owneruserid;
        public int blockflag;
        public string fleetpwd;
        public int cntbinds;
        public string account;
        public string subaccount;
    }

    public class CCard
    {
        public string UserId;
        public string ConnectionString;
        public string ConnectionStringReserve;
        public string LogPath;

        public CCard(string userid, string connectionstring, string connectionstringreserve, string logpath) 
        {
            UserId = userid;
            ConnectionString = connectionstring;
            ConnectionStringReserve = connectionstringreserve;
            LogPath = logpath;
        }
                
        /*public int GetData(STCardVP param, out List<STCard> data, out string msg)
        {
            int ret = 0;
            data = new List<STCard>();
            List<STCard> retdata = new List<STCard>();

            SqlConnection connect;
            Log log = new Log(LogPath);

            msg = null;

            string recvalue;

            string where;
            
            STCard item;
            bool bFl = false;

            try
            {
                connect = new SqlConnection(ConnectionString);
                connect.Open();

                if (connect.State != ConnectionState.Open)
                {
                    connect = new SqlConnection(ConnectionStringReserve);
                    connect.Open();
                }

                if (connect.State == ConnectionState.Open)
                {
                    where = null;
                  
                    if (param.bParam)
                    {
                        if (!bFl) where = "WHERE ";
                       
                        if (param.maskedpan != null)
                        {
                            if (bFl) where += string.Format("AND X.RecId LIKE '{0}' ", param.maskedpan.Replace('*', '%'));
                            else
                            {
                                bFl = true;
                                where += string.Format("X.RecId LIKE '{0}' ", param.maskedpan.Replace('*', '%'));
                            }
                        }
                        if (param.expdate != null)
                        {
                            if (bFl) where += string.Format("AND UPPER(X.RecValue) LIKE '%<EXP={0}>%' ", param.expdate.ToUpper());
                            else
                            {
                                bFl = true;
                                where += string.Format("UPPER(X.RecValue) LIKE '%<EXP={0}>%' ", param.expdate.ToUpper());
                            }
                        }
                        if (param.fleetidflag != null)
                        {
                            if (bFl) where += string.Format("AND UPPER(X.RecValue) LIKE '%<FFL={0}>%' ", param.fleetidflag);
                            else
                            {
                                bFl = true;
                                where += string.Format("UPPER(X.RecValue) LIKE '%<FFL={0}>%' ", param.fleetidflag);
                            }

                        }
                        if (param.odometerflag != null)
                        {
                            if (bFl) where += string.Format("AND UPPER(X.RecValue) LIKE '%<FKM={0}>%' ", param.odometerflag);
                            else
                            {
                                bFl = true;
                                where += string.Format("UPPER(X.RecValue) LIKE '%<FKM={0}>%' ", param.odometerflag);
                            }
                        }
                        if (param.loyaltyflag != null)
                        {
                            if (bFl) where += string.Format("AND UPPER(X.RecValue) LIKE '%<LOY={0}>%' ", param.loyaltyflag);
                            else
                            {
                                bFl = true;
                                where += string.Format("UPPER(X.RecValue) LIKE '%<LOY={0}>%' ", param.loyaltyflag);
                            }
                        }
                        if (param.maskedvrn != null)
                        {
                            if (bFl) where += string.Format("AND UPPER(X.RecValue) LIKE '%<VRN={0}>%' ", param.maskedvrn.ToUpper());
                            else
                            {
                                bFl = true;
                                where += string.Format("UPPER(X.RecValue) LIKE '%<VRN={0}>%' ", param.maskedvrn.ToUpper());
                            }
                        }
                        if (param.maskeddrivername != null)
                        {
                            if (bFl) where += string.Format("AND UPPER(X.RecValue) LIKE '%<DRV={0}>%' ", param.maskeddrivername.ToUpper());
                            else
                            {
                                bFl = true;
                                where += string.Format("UPPER(X.RecValue) LIKE '%<DRV={0}>%' ", param.maskeddrivername.ToUpper());
                            }
                        }
                        if (param.maskedcompanyname != null)
                        {
                            if (bFl) where += string.Format("AND UPPER(X.RecValue) LIKE '%<CMPI=%{0}%>%' ", param.maskedcompanyname.ToUpper());
                            else
                            {
                                bFl = true;
                                where += string.Format("UPPER(X.RecValue) LIKE '%<CMPI=%{0}%>%' ", param.maskedcompanyname.ToUpper());
                            }
                        }
                        if (param.intdesignator != null)
                        {
                            if (bFl) where += string.Format("AND UPPER(X.RecValue) LIKE '%<INT={0}>%' ", param.intdesignator);
                            else
                            {
                                bFl = true;
                                where += string.Format("UPPER(X.RecValue) LIKE '%<INT={0}>%' ", param.intdesignator);
                            }
                        }
                        if (param.maskedemail != null)
                        {
                            if (bFl) where += string.Format("AND UPPER(X.RecValue) LIKE '%<EML=%{0}%>%' ", param.maskedemail.ToUpper());
                            else
                            {
                                bFl = true;
                                where += string.Format("UPPER(X.RecValue) LIKE '%<EML=%{0}%>%' ", param.maskedemail.ToUpper());
                            }
                        }
                        if (param.blockflag != null)
                        {
                            if (bFl) where += string.Format("AND UPPER(X.RecValue) LIKE '%<BLK={0}>%' ", param.blockflag);
                            else
                            {
                                bFl = true;
                                where += string.Format("UPPER(X.RecValue) LIKE '%<BLK={0}>%' ", param.blockflag);
                            }
                        }

                        if (param.bindflag != null)
                        {
                            if (bFl) where += string.Format("AND X.CNT ");
                            else
                            {
                                bFl = true;
                                where += string.Format("X.CNT>0 ");
                            }
                        }
                    }

                    string query = string.Format("SELECT  X.RecId,X.RecValue,X.I1,X.CNT FROM  ( SELECT T1.RecId,T1.RecValue,T1.I1,Count(T2.RecId) AS CNT FROM dbo.AEVICards T1 " +
                        "LEFT JOIN dbo.AEVIBind T2 ON T1.RecId = SUBSTRING(T2.RecId, 1, len(T1.RecId)) " +
                        " GROUP BY T1.RecId,T1.RecValue,T1.I1) X  {0} ORDER BY X.RecId", where);

                    SqlCommand cmd = new SqlCommand(query, connect);
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            item = new STCard();
                           
                            if (!reader.IsDBNull(1))
                                recvalue = reader.GetString(1);
                            else recvalue = null;
                            ret = getTags(recvalue, out item, out msg);
                            item.pan = reader.GetString(0);
                            item.maskedpan = get_mask_pan(item.pan);
                            if (!reader.IsDBNull(2))
                                item.owneruserid = reader.GetString(2);
                            else item.owneruserid = null;
                            item.recvalue = recvalue;
                            if (!reader.IsDBNull(3))
                                item.cntbinds = reader.GetInt32(3);
                            else item.cntbinds = 0;

                            retdata.Add(item);
                        }
                        data = retdata;
                    }

                    reader.Dispose();
                }
                else return 1;
            }
            catch (Exception ex) { log.Write(LogType.Error, ex.Message); ret = -1; msg = ex.Message; }
            return ret;
        }*/
        public int GetData(STCardVP param, out List<STCard> data, out string msg)
        {
            int ret = 0;
            data = new List<STCard>();
            List<STCard> retdata = new List<STCard>();

            SqlConnection connect;
            Log log = new Log(LogPath);

            msg = null;

            string recvalue;

            string where;

            STCard item;
            bool bFl = false;

            try
            {
                connect = new SqlConnection(ConnectionString);
                connect.Open();

                if (connect.State != ConnectionState.Open)
                {
                    connect = new SqlConnection(ConnectionStringReserve);
                    connect.Open();
                }

                if (connect.State == ConnectionState.Open)
                {
                    where = null;

                    if (param.bParam)
                    {
                        if (!bFl) where = "WHERE ";

                        if (param.maskedpan != null)
                        {
                            if (bFl) where += string.Format("AND X.RecId LIKE '{0}' ", param.maskedpan.Replace('*', '%'));
                            else
                            {
                                bFl = true;
                                where += string.Format("X.RecId LIKE '{0}' ", param.maskedpan.Replace('*', '%'));
                            }
                        }
                        if (param.expdate != null)
                        {
                            if (bFl) where += string.Format("AND UPPER(X.ExpDate) LIKE '%{0}%' ", param.expdate.ToUpper());
                            else
                            {
                                bFl = true;
                                where += string.Format("UPPER(X.ExpDate) LIKE '%{0}%' ", param.expdate.ToUpper());
                            }
                        }
                        if (param.fleetidflag != null)
                        {
                            if (bFl) where += string.Format("AND UPPER(X.Fleetidflag) LIKE '%{0}%' ", param.fleetidflag);
                            else
                            {
                                bFl = true;
                                where += string.Format("UPPER(X.Fleetidflag) LIKE '%{0}%' ", param.fleetidflag);
                            }

                        }
                        if (param.odometerflag != null)
                        {
                            if (bFl) where += string.Format("AND UPPER(X.Odometerflag) LIKE '%{0}%' ", param.odometerflag);
                            else
                            {
                                bFl = true;
                                where += string.Format("UPPER(X.Odometerflag) LIKE '%{0}%' ", param.odometerflag);
                            }
                        }
                        if (param.loyaltyflag != null)
                        {
                            if (bFl) where += string.Format("AND UPPER(X.Loyaltyflag) LIKE '%{0}%' ", param.loyaltyflag);
                            else
                            {
                                bFl = true;
                                where += string.Format("UPPER(X.Loyaltyflag) LIKE '%{0}%' ", param.loyaltyflag);
                            }
                        }
                        if (param.maskedvrn != null)
                        {
                            if (bFl) where += string.Format("AND UPPER(X.Vrn) LIKE '%{0}%' ", param.maskedvrn.ToUpper());
                            else
                            {
                                bFl = true;
                                where += string.Format("UPPER(X.Vrn) LIKE '%{0}%' ", param.maskedvrn.ToUpper());
                            }
                        }
                        if (param.maskeddrivername != null)
                        {
                            if (bFl) where += string.Format("AND UPPER(X.Drivername) LIKE '%{0}%' ", param.maskeddrivername.ToUpper());
                            else
                            {
                                bFl = true;
                                where += string.Format("UPPER(X.Drivername) LIKE '%{0}%' ", param.maskeddrivername.ToUpper());
                            }
                        }
                        if (param.maskedcompanyname != null)
                        {
                            if (bFl) where += string.Format("AND UPPER(X.Companyname_ir) LIKE '%{0}%' ", param.maskedcompanyname.ToUpper());
                            else
                            {
                                bFl = true;
                                where += string.Format("UPPER(X.Companyname_ir) LIKE '%{0}%' ", param.maskedcompanyname.ToUpper());
                            }
                        }
                        if (param.intdesignator != null)
                        {
                            if (bFl) where += string.Format("AND UPPER(X.Intdesignator) LIKE '%{0}%' ", param.intdesignator);
                            else
                            {
                                bFl = true;
                                where += string.Format("UPPER(X.Intdesignator) LIKE '%{0}%' ", param.intdesignator);
                            }
                        }
                        if (param.maskedemail != null)
                        {
                            if (bFl) where += string.Format("AND UPPER(X.Email) LIKE '%{0}%' ", param.maskedemail.ToUpper());
                            else
                            {
                                bFl = true;
                                where += string.Format("UPPER(X.Email) LIKE '%{0}%' ", param.maskedemail.ToUpper());
                            }
                        }
                        if (param.blockflag != null)
                        {
                            if (bFl) where += string.Format("AND UPPER(X.Blockflag) LIKE '%{0}%' ", param.blockflag);
                            else
                            {
                                bFl = true;
                                where += string.Format("UPPER(X.Blockflag) LIKE '%{0}%' ", param.blockflag);
                            }
                        }

                        if (param.bindflag != null)
                        {
                            if (bFl) where += string.Format("AND X.CNT ");
                            else
                            {
                                bFl = true;
                                where += string.Format("X.CNT>0 ");
                            }
                        }

                        if (param.maskaccount != null)
                        {
                            if (bFl) where += string.Format("AND UPPER(X.Account) LIKE '%{0}%' ", param.maskaccount.ToUpper());
                            else
                            {
                                bFl = true;
                                where += string.Format("UPPER(X.Account) LIKE '%{0}%' ", param.maskaccount.ToUpper());
                            }
                        }

                        if (param.masksubaccount != null)
                        {
                            if (bFl) where += string.Format("AND UPPER(X.Subaccount) LIKE '%{0}%' ", param.masksubaccount.ToUpper());
                            else
                            {
                                bFl = true;
                                where += string.Format("UPPER(X.Subaccount) LIKE '%{0}%' ", param.masksubaccount.ToUpper());
                            }
                        }
                    }

                    string query = string.Format("SELECT X.RecId, X.ExpDate, X.Loyaltyflag, X.Fleetidflag, X.Odometerflag, X.Vrn, " +
                        "X.Drivername, X.Companyname, X.Intdesignator, X.Email, X.Blockflag, X.Fleetpwd, X.Companyname_ir, X.CNT, X.I1, X.Account, " +
                        "X.Subaccount " +
                        "FROM (SELECT T1.RecId AS RecId, dbo.GetTag(T1.RecValue, 'EXP') AS ExpDate, " +
                        "dbo.GetTag(T1.RecValue, 'LOY') AS Loyaltyflag ,dbo.GetTag(T1.RecValue, 'FFL') AS Fleetidflag, " +
                        "dbo.GetTag(T1.RecValue, 'FKM') AS Odometerflag ,dbo.GetTag(T1.RecValue, 'VRN') AS Vrn, " +
                        "dbo.GetTag(T1.RecValue, 'DRV') AS Drivername ,dbo.GetTag(T1.RecValue, 'CMP') AS Companyname, " +
                        "dbo.GetTag(T1.RecValue, 'INT') AS Intdesignator ,dbo.GetTag(T1.RecValue, 'EML') AS Email, " +
                        "dbo.GetTag(T1.RecValue, 'BLK') AS Blockflag ,dbo.GetTag(T1.RecValue, 'PSW') AS Fleetpwd, " +
                        "dbo.GetTag(T1.RecValue, 'CMPI') AS Companyname_ir ,COUNT (T2.RecId) AS CNT, T1.I1, " +
                        "dbo.GetTag(T1.RecValue, 'ACC') AS Account, dbo.GetTag(T1.RecValue, 'SACC') AS Subaccount " +
                        "FROM AEVICards T1 LEFT JOIN AEVIBind T2 ON T1.RecId = SUBSTRING(T2.RecId, 1, len(T1.RecId)) " +
                        "GROUP BY T1.RecId,T1.RecValue,T1.I1) X {0} ORDER BY X.RecId", where);

                    SqlCommand cmd = new SqlCommand(query, connect);
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            item = new STCard();

                            string pan = reader.GetString(0);
                            item.pan = CCrypto.EncryptPAN(pan);
                            item.maskedpan = get_mask_pan(pan);
                            if (!reader.IsDBNull(1))
                                item.expdate = reader.GetString(1);
                            else item.expdate = null;
                            if (!reader.IsDBNull(2))
                                int.TryParse(reader.GetString(2), out item.loyaltyflag);
                            else item.loyaltyflag = 0;
                            if (!reader.IsDBNull(3))
                                int.TryParse(reader.GetString(3), out item.fleetidflag);
                            else item.fleetidflag = 0;
                            if (!reader.IsDBNull(4))
                                int.TryParse(reader.GetString(4), out item.odometerflag);
                            else item.odometerflag = 0;
                            if (!reader.IsDBNull(5))
                                item.vrn = reader.GetString(5);
                            else item.vrn = null;
                            if (!reader.IsDBNull(6))
                                item.drivername = reader.GetString(6);
                            else item.drivername = null;
                            if (!reader.IsDBNull(7))
                                item.companyname = reader.GetString(7);
                            else item.companyname = null;
                            if (!reader.IsDBNull(8))
                                int.TryParse(reader.GetString(8), out item.intdesignator);
                            else item.intdesignator = 0;
                            if (!reader.IsDBNull(9))
                                item.email = reader.GetString(9);
                            else item.email = null;
                            if (!reader.IsDBNull(10))
                                int.TryParse(reader.GetString(10), out item.blockflag);
                            else item.blockflag = 0;
                            if (!reader.IsDBNull(11))
                                item.fleetpwd = reader.GetString(11);
                            else item.fleetpwd = null;
                            if (!reader.IsDBNull(12))
                                item.companyname_ir = reader.GetString(12);
                            else item.companyname_ir = null;
                            if (!reader.IsDBNull(13))
                                item.cntbinds = reader.GetInt32(13);
                            else item.cntbinds = 0;
                            if (!reader.IsDBNull(14))
                                item.owneruserid = reader.GetString(14);
                            else item.owneruserid = null;
                            if (!reader.IsDBNull(15))
                                item.account = reader.GetString(15);
                            else item.account = null;
                            if (!reader.IsDBNull(16))
                                item.subaccount = reader.GetString(16);
                            else item.subaccount = null;
                          
                            retdata.Add(item);
                        }
                        data = retdata;
                    }

                    reader.Dispose();
                }
                else return 1;
            }
            catch (Exception ex) { log.Write(LogType.Error, ex.Message); ret = -1; msg = ex.Message; }
            return ret;
        }

        /*public int GetData(STCardVP param, out List<STCard> data, out string msg, int top, int offset)
        {
            int ret = 0;
            data = new List<STCard>();
            List<STCard> retdata = new List<STCard>();

            SqlConnection connect;
            Log log = new Log(LogPath);

            msg = null;

            string recvalue;

            string where;
            
            STCard item;
            bool bFl = false;

            try
            {
                connect = new SqlConnection(ConnectionString);
                connect.Open();

                if (connect.State != ConnectionState.Open)
                {
                    connect = new SqlConnection(ConnectionStringReserve);
                    connect.Open();
                }

                if (connect.State == ConnectionState.Open)
                {
                    where = null;
                  
                    if (param.bParam)
                    {
                        if (!bFl) where = "WHERE ";
                       
                        if (param.maskedpan != null)
                        {
                            if (bFl) where += string.Format("AND X.RecId LIKE '{0}' ", param.maskedpan);
                            else
                            {
                                bFl = true;
                                where += string.Format("X.RecId LIKE '{0}' ", param.maskedpan);
                            }
                        }
                        if (param.expdate != null)
                        {
                            if (bFl) where += string.Format("AND UPPER(X.RecValue) LIKE '%<EXP={0}>%' ", param.expdate.ToUpper());
                            else
                            {
                                bFl = true;
                                where += string.Format("UPPER(X.RecValue) LIKE '%<EXP={0}>%' ", param.expdate.ToUpper());
                            }
                        }
                        if (param.fleetidflag != null)
                        {
                            if (bFl) where += string.Format("AND UPPER(X.RecValue) LIKE '%<FFL={0}>%' ", param.fleetidflag);
                            else
                            {
                                bFl = true;
                                where += string.Format("UPPER(X.RecValue) LIKE '%<FFL={0}>%' ", param.fleetidflag);
                            }

                        }
                        if (param.odometerflag != null)
                        {
                            if (bFl) where += string.Format("AND UPPER(X.RecValue) LIKE '%<FKM={0}>%' ", param.odometerflag);
                            else
                            {
                                bFl = true;
                                where += string.Format("UPPER(X.RecValue) LIKE '%<FKM={0}>%' ", param.odometerflag);
                            }
                        }
                        if (param.loyaltyflag != null)
                        {
                            if (bFl) where += string.Format("AND UPPER(X.RecValue) LIKE '%<LOY={0}>%' ", param.loyaltyflag);
                            else
                            {
                                bFl = true;
                                where += string.Format("UPPER(X.RecValue) LIKE '%<LOY={0}>%' ", param.loyaltyflag);
                            }
                        }
                        if (param.maskedvrn != null)
                        {
                            if (bFl) where += string.Format("AND UPPER(X.RecValue) LIKE '%<VRN={0}>%' ", param.maskedvrn.ToUpper());
                            else
                            {
                                bFl = true;
                                where += string.Format("UPPER(T1.RecValue) LIKE '%<VRN={0}>%' ", param.maskedvrn.ToUpper());
                            }
                        }
                        if (param.maskeddrivername != null)
                        {
                            if (bFl) where += string.Format("AND UPPER(X.RecValue) LIKE '%<DRV={0}>%' ", param.maskeddrivername.ToUpper());
                            else
                            {
                                bFl = true;
                                where += string.Format("UPPER(X.RecValue) LIKE '%<DRV={0}>%' ", param.maskeddrivername.ToUpper());
                            }
                        }
                        if (param.maskedcompanyname != null)
                        {
                            if (bFl) where += string.Format("AND UPPER(X.RecValue) LIKE '%<CMPI={0}>%' ", param.maskedcompanyname.ToUpper());
                            else
                            {
                                bFl = true;
                                where += string.Format("UPPER(X.RecValue) LIKE '%<CMPI={0}>%' ", param.maskedcompanyname.ToUpper());
                            }
                        }
                        if (param.intdesignator != null)
                        {
                            if (bFl) where += string.Format("AND UPPER(X.RecValue) LIKE '%<INT={0}>%' ", param.intdesignator);
                            else
                            {
                                bFl = true;
                                where += string.Format("UPPER(X.RecValue) LIKE '%<INT={0}>%' ", param.intdesignator);
                            }
                        }
                        if (param.maskedemail != null)
                        {
                            if (bFl) where += string.Format("AND UPPER(X.RecValue) LIKE '%<EML={0}>%' ", param.maskedemail.ToUpper());
                            else
                            {
                                bFl = true;
                                where += string.Format("UPPER(X.RecValue) LIKE '%<EML={0}>%' ", param.maskedemail.ToUpper());
                            }
                        }
                        if (param.blockflag != null)
                        {
                            if (bFl) where += string.Format("AND UPPER(X.RecValue) LIKE '%<BLK={0}>%' ", param.blockflag);
                            else
                            {
                                bFl = true;
                                where += string.Format("UPPER(X.RecValue) LIKE '%<BLK={0}>%' ", param.blockflag);
                            }
                        }

                        if (param.bindflag != null)
                        {
                            if (bFl) where += string.Format("AND X.CNT>0 ");
                            else
                            {
                                bFl = true;
                                where += string.Format("X.CNT>0 ");
                            }
                        }
                    }


                    string query1 = string.Format("SELECT TOP({0}) X.RecId,X.RecValue,X.I1,X.CNT FROM  ( SELECT T1.RecId,T1.RecValue,T1.I1,Count(T2.RecId) AS CNT FROM dbo.AEVICards T1 " +
                                "LEFT JOIN dbo.AEVIBind T2 ON T1.RecId = SUBSTRING(T2.RecId, 1, len(T1.RecId)) " +
                                "GROUP BY T1.RecId,T1.RecValue,T1.I1 ORDER BY T1.RecId,T1.RecValue,T1.I1 OFFSET({1}) Rows ) X {2} ORDER BY X.RecId ", top, offset, where) ;

                    string query = string.Format("SELECT T1.RecId,T1.RecValue,T1.I1,Count(T2.RecId) FROM dbo.AEVICards T1 " +
                        "LEFT JOIN dbo.AEVIBind T2 ON T1.RecId = SUBSTRING(T2.RecId, 1, len(T1.RecId)) " +
                        "{0} GROUP BY T1.RecId,T1.RecValue,T1.I1 ORDER BY T1.RecId ", where);

                    SqlCommand cmd = new SqlCommand(query1, connect);
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            item = new STCard();
                           
                            if (!reader.IsDBNull(1))
                                recvalue = reader.GetString(1);
                            else recvalue = null;
                            ret = getTags(recvalue, out item, out msg);
                            item.pan = reader.GetString(0);
                            item.maskedpan = get_mask_pan(item.pan);
                            if (!reader.IsDBNull(2))
                                item.owneruserid = reader.GetString(2);
                            else item.owneruserid = null;
                            item.recvalue = recvalue;
                            if (!reader.IsDBNull(3))
                                item.cntbinds = reader.GetInt32(3);
                            else item.cntbinds = 0;

                            retdata.Add(item);
                        }
                        data = retdata;
                    }

                    reader.Dispose();
                }
                else return 1;
            }
            catch (Exception ex) { log.Write(LogType.Error, ex.Message); ret = -1; msg = ex.Message; }
            return ret;
        }*/

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

        public int GetDataBind(string pan, out List<STBind> data, out string msg)
        {
            int ret = 0;
            data = new List<STBind>();
            STBind item;
            msg = null;
            
            SqlConnection connect;
            Log log = new Log(LogPath);

            try
            {
                connect = new SqlConnection(ConnectionString);
                connect.Open();

                if (connect.State != ConnectionState.Open)
                {
                    connect = new SqlConnection(ConnectionStringReserve);
                    connect.Open();
                }

                if (connect.State == ConnectionState.Open)
                {

                    string query = string.Format("SELECT RecId,RecValue FROM dbo.AEVIBind WHERE RecId LIKE '{0}%'", pan);
                    SqlCommand cmd = new SqlCommand(query, connect);
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            item = new STBind();
                            ret = flit(reader.GetString(0), out item.pan, out item.flit, out msg);
                            if (!reader.IsDBNull(1))
                                item.recvalue = reader.GetString(1);
                            else item.recvalue = null;

                            data.Add(item);
                        }
                    }

                    reader.Dispose();
                }
                else return 1;
            }
            catch (Exception ex) { log.Write(LogType.Error, ex.Message); ret = -1; msg = ex.Message; }
            return ret;
        }

        private int flit(string data, out string pan, out string flit, out string msg)
        {
            int ret = 0;
            pan = null;
            flit = null;
            msg = null;
            Log log = new Log(LogPath);

            try
            {
                if (string.IsNullOrEmpty(data)) { msg = "Empty data."; return -1; }

                if (data.Contains("."))
                {
                    int x1 = data.IndexOf(".");
                    pan = data.Substring(0, x1);
                    flit = data.Substring(x1 + 1);
                }
                else pan = data;
            }
            catch (Exception ex) { log.Write(LogType.Error, ex.Message); ret = -1; msg = ex.Message; }
            return ret;
        }

        private int getTags(string value, out STCard data, out string msg)
        {
            int ret = 0;
            data = new STCard();
            msg = null;

            Log log = new Log(LogPath);

            try
            {
                // Срок действия карты6
                data.expdate = getvaluetag(value, "EXP");
                // Флаг лояльности
                data.loyaltyflag = 0;
                int.TryParse(getvaluetag(value, "LOY"), out data.loyaltyflag);
                // Флаг идентификатора флота
                data.fleetidflag = 0;
                int.TryParse(getvaluetag(value, "FFL"), out data.fleetidflag);
                // Флаг одометра
                data.odometerflag = 0;
                int.TryParse(getvaluetag(value, "FKM"), out data.odometerflag);
                // Регистрационный номер автомобиля
                data.vrn = getvaluetag(value, "VRN");
                // Имя водителя
                data.drivername = getvaluetag(value, "DRV");
                // Наименование компании
                data.companyname = getvaluetag(value, "CMP");
                // Обозначение обмена
                data.intdesignator = 0;
                int.TryParse(getvaluetag(value, "INT"), out data.intdesignator);
                // Адрес электронной почты
                data.email = getvaluetag(value, "EML");
                // Флаг блокировки арты
                data.blockflag = 0;
                int.TryParse(getvaluetag(value, "BLK"), out data.blockflag);
                // Флаг блокировки арты
                data.fleetpwd = getvaluetag(value, "PSW");
                // Наименование компании
                data.companyname_ir = getvaluetag(value, "CMPI");
                // Account
                data.account = getvaluetag(value, "ACC");
                // SubAccount
                data.subaccount = getvaluetag(value, "SACC");
                
            }
            catch (Exception ex) { log.Write(LogType.Error, ex.Message); ret = -1; msg = ex.Message; }
            return ret;
        }

        private string getvaluetag(string value, string tag)
        {
            Log log = new Log(LogPath);
            string ret = null;
            try
            {
                string substr = "<" + tag + "=";
                int pos = -1;
                pos = value.IndexOf(substr);
                if (pos < 0) return ret;

                int pos2 = -1;
                pos2 = value.IndexOf(">", pos);
                if (pos2 < 0) return ret;

                pos = pos + tag.Length + 2;

                ret = value.Substring(pos, pos2 - pos);
            }
            catch (Exception ex) { log.Write(LogType.Error, ex.Message); }
            return ret;
        }

        private int toTags(STCard data, out string retvalue, out string msg)
        {
            int ret = 0;

            retvalue = null;
            msg = null;

            string tag = null;

            Log log = new Log(LogPath);

            try
            {
                tag = string.Format("<EXP={0}>", data.expdate.Trim());
                retvalue += tag;

                tag = string.Format("<LOY={0}>", data.loyaltyflag);
                retvalue += tag;

                tag = string.Format("<FFL={0}>", data.fleetidflag);
                retvalue += tag;

                tag = string.Format("<FKM={0}>", data.odometerflag);
                retvalue += tag;

                if (data.vrn != null) tag = string.Format("<VRN={0}>", data.vrn.Trim());
                else tag = null;
                retvalue += tag;

                if (data.drivername != null) tag = string.Format("<DRV={0}>", data.drivername.Trim());
                else tag = null;
                retvalue += tag;

                tag = string.Format("<CMP={0}>", data.companyname.Trim());
                retvalue += tag;

                tag = string.Format("<INT={0}>", data.intdesignator);
                retvalue += tag;

                tag = string.Format("<EML={0}>", data.email.Trim());
                retvalue += tag;

                tag = string.Format("<BLK={0}>", data.blockflag);
                retvalue += tag;

                tag = string.Format("<PSW={0}>", data.fleetpwd.Trim());
                retvalue += tag;

                tag = string.Format("<CMPI={0}>", data.companyname_ir.Trim());
                retvalue += tag;

                if (data.account != null) tag = string.Format("<ACC={0}>", data.account.Trim());
                else tag = null;
                retvalue += tag;

                if (data.subaccount != null) tag = string.Format("<SACC={0}>", data.subaccount.Trim());
                else tag = null;
                retvalue += tag;
            }
            catch (Exception ex) { log.Write(LogType.Error, ex.Message); }
            return ret;
        }

        private int toUpdateTags(STCard data, string oldvalue, out string retvalue, out string msg)
        {
            int ret = 0;

            retvalue = null;
            msg = null;

          //  string tag = null;

            Log log = new Log(LogPath);

            try
            {
                TAG tag = new TAG(oldvalue);

                tag.Delete("LOY");
                tag.SetValue("LOY", data.loyaltyflag);
                tag.Delete("FFL");
                tag.SetValue("FFL", data.fleetidflag);
                tag.Delete("FKM");
                tag.SetValue("FKM", data.odometerflag);

                if (data.vrn != null)
                {
                    tag.Delete("VRN");
                    tag.SetValue("VRN", data.vrn);
                }
                if (data.drivername != null)
                {
                    tag.Delete("DRV");
                    tag.SetValue("DRV", data.drivername);
                }

                tag.Delete("CMP");
                tag.SetValue("CMP", data.companyname);
                tag.Delete("CMPI");
                tag.SetValue("CMPI", data.companyname_ir);
                tag.Delete("INT");
                tag.SetValue("INT", data.intdesignator);
                tag.Delete("EML");
                tag.SetValue("EML", data.email);
                tag.Delete("BLK");
                tag.SetValue("BLK", data.blockflag);
              //  tag.Delete("PSW");
              //  tag.SetValue("PSW", data.fleetpwd);

                if (data.account != null)
                {
                    tag.Delete("ACC");
                    tag.SetValue("ACC", data.account);
                }

                if (data.subaccount != null)
                {
                    tag.Delete("SACC");
                    tag.SetValue("SACC", data.subaccount);
                }

                retvalue = tag.ToString();
            }
            catch (Exception ex) { log.Write(LogType.Error, ex.Message); }
            return ret;
        }
        private int toUpdateTagsBlock(STCard data, string oldvalue, out string retvalue, out string msg)
        {
            int ret = 0;

            retvalue = null;
            msg = null;

            //  string tag = null;

            Log log = new Log(LogPath);

            try
            {
                TAG tag = new TAG(oldvalue);

                tag.Delete("BLK");
                tag.SetValue("BLK", 1);
                //  tag.Delete("PSW");
                //  tag.SetValue("PSW", data.fleetpwd);

                retvalue = tag.ToString();
            }
            catch (Exception ex) { log.Write(LogType.Error, ex.Message); }
            return ret;
        }

        public int Insert(STCard data, out string msg, bool bLocal, string channelsarray, string channels)
        {
            int ret = 0;
            msg = null;
            string query;
            
            SqlConnection connect;
            Log log = new Log(LogPath);

            try
            {
                string recvalue = null;
                toTags(data, out recvalue, out msg);

                if (bLocal)
                {
                    connect = new SqlConnection(ConnectionString);
                    connect.Open();

                    if (connect.State != ConnectionState.Open)
                    {
                        connect = new SqlConnection(ConnectionStringReserve);
                        connect.Open();
                    }

                    if (connect.State == ConnectionState.Open)
                    {

                        query = string.Format("INSERT INTO dbo.AEVICards (RecId, RecValue, I1) VALUES (@1, @2, @3)");

                        SqlCommand cmd = new SqlCommand(query, connect);
                       
                        cmd.Parameters.Add(crp(SqlDbType.VarChar, "@1", data.pan, false));
                        cmd.Parameters.Add(crp(SqlDbType.VarChar, "@2", recvalue, false));
                        cmd.Parameters.Add(crp(SqlDbType.VarChar, "@3", UserId, false));
                        cmd.ExecuteNonQuery();
                    }
                    else return 1;
                }
                else
                {
                    string resultCode = null;
                    string value = string.Format("{0}|{1}|{2}|{3}|{4}", data.pan, recvalue, UserId, "", "");
                    if (!AEVIRequest(channelsarray, channels, "InsertCard", value, out resultCode, out msg))
                    {
                        ret = -1;
                        log.Write(LogType.Error, resultCode + " " + msg);
                    }
                }
            }
            catch (Exception ex) { log.Write(LogType.Error, ex.Message); ret = -1; msg = ex.Message; }
            return ret;
        }

        public int Update(string pan, STCard data, out string msg, bool bLocal, string channelsarray, string channels)
        {
            int ret = 0;
            msg = null;
            string query;
            
            SqlConnection connect;
            Log log = new Log(LogPath);

            try
            {
                string recvalue = null;
                STCard stold;
                GetRecord(pan, out stold, out msg);
              //  log.Write(LogType.Error, "GetRecord " + msg);
                toUpdateTags(data, stold.recvalue, out recvalue, out msg);
             //   log.Write(LogType.Error, "toUpdateTags " + data.ToString());

                if (bLocal)
                {
                    connect = new SqlConnection(ConnectionString);
                    connect.Open();

                    if (connect.State != ConnectionState.Open)
                    {
                        connect = new SqlConnection(ConnectionStringReserve);
                        connect.Open();
                    }

                    if (connect.State == ConnectionState.Open)
                    {
                        query = string.Format("UPDATE dbo.AEVICards SET RecValue=@1 WHERE RecId=@2");

                        SqlCommand cmd = new SqlCommand(query, connect);
                        cmd.Parameters.Add(crp(SqlDbType.VarChar, "@1", recvalue, false));
                        cmd.Parameters.Add(crp(SqlDbType.VarChar, "@2", CCrypto.DecryptPAN(pan), false));
                        cmd.ExecuteNonQuery();
                    }
                    else return 1;
                }
                else
                {
                    string resultCode = null;
                  
                    string value = string.Format("{0}|{1}|{2}|{3}|{4}", CCrypto.DecryptPAN(pan), recvalue, data.owneruserid, "", "");
                  
                    if (!AEVIRequest(channelsarray, channels, "UpdateCard", value, out resultCode, out msg))
                    {
                        ret = -1;
                        log.Write(LogType.Error, resultCode + " " + msg);
                    }
                }
            }
            catch (Exception ex) { log.Write(LogType.Error, "Update " + ex.Message); ret = -1; msg = ex.Message; }
            return ret;
        }
    
        public int Block(string pan, STCard data, out string msg, bool bLocal, string channelsarray, string channels)
        {
            int ret = 0;
            msg = null;
            string query;

            SqlConnection connect;
            Log log = new Log(LogPath);

            try
            {
                string recvalue = null;
                STCard stold;
                GetRecord(pan, out stold, out msg);
                toUpdateTagsBlock(data, stold.recvalue, out recvalue, out msg);

                if (bLocal)
                {
                    connect = new SqlConnection(ConnectionString);
                    connect.Open();

                    if (connect.State != ConnectionState.Open)
                    {
                        connect = new SqlConnection(ConnectionStringReserve);
                        connect.Open();
                    }

                    if (connect.State == ConnectionState.Open)
                    {
                        query = string.Format("UPDATE dbo.AEVICards SET RecValue=@1 WHERE RecId=@2");

                        SqlCommand cmd = new SqlCommand(query, connect);
                        cmd.Parameters.Add(crp(SqlDbType.VarChar, "@1", recvalue, false));
                        cmd.Parameters.Add(crp(SqlDbType.VarChar, "@2", pan, false));
                        cmd.ExecuteNonQuery();
                    }
                    else return 1;
                }
                else
                {
                    string resultCode = null;
                    string value = string.Format("{0}|{1}|{2}|{3}|{4}", CCrypto.DecryptPAN(data.pan), recvalue, data.owneruserid, "", "");
                    if (!AEVIRequest(channelsarray, channels, "UpdateCard", value, out resultCode, out msg))
                    {
                        ret = -1;
                        log.Write(LogType.Error, resultCode + " " + msg);
                    }
                }
            }
            catch (Exception ex) { log.Write(LogType.Error, ex.Message); ret = -1; msg = ex.Message; }
            return ret;
        }

        public int Delete(string pan, out string msg, bool bLocal, string channelsarray, string channels)
        {
            int ret = 0;
            msg = null;
            string query;

            SqlConnection connect;
            Log log = new Log(LogPath);

            try
            {
                if (bLocal)
                {
                    connect = new SqlConnection(ConnectionString);
                    connect.Open();

                    if (connect.State != ConnectionState.Open)
                    {
                        connect = new SqlConnection(ConnectionStringReserve);
                        connect.Open();
                    }

                    if (connect.State == ConnectionState.Open)
                    {
                        query = string.Format("DELETE FROM dbo.AEVICards WHERE RecId = @1");

                        SqlCommand cmd = new SqlCommand(query, connect);
                        cmd.Parameters.Add(crp(SqlDbType.VarChar, "@1", pan, false));
                        cmd.ExecuteNonQuery();

                        query = string.Format("DELETE FROM dbo.AEVIBind WHERE RecId LIKE '{0}%'", pan);

                        cmd = new SqlCommand(query, connect);
                        cmd.ExecuteNonQuery();
                    }
                    else return 1;
                }
                else
                {
                    string resultCode = null;
                    if (!AEVIRequest(channelsarray, channels, "DeleteCard", CCrypto.DecryptPAN(pan), out resultCode, out msg))
                    {
                        ret = -1;
                        log.Write(LogType.Error, resultCode + " " + msg);
                    }
                }
            }
            catch (Exception ex) { log.Write(LogType.Error, ex.Message); ret = -1; msg = ex.Message; }
            return ret;
        }

        public int Unregistred(string pan, out string msg, bool bLocal, string channelsarray, string channels)
        {
            int ret = 0;
            msg = null;
            string query;

            SqlConnection connect;
            Log log = new Log(LogPath);

            try
            {
                if (bLocal)
                {
                   /*connect = new SqlConnection(ConnectionString);
                    connect.Open();

                    if (connect.State != ConnectionState.Open)
                    {
                        connect = new SqlConnection(ConnectionStringReserve);
                        connect.Open();
                    }

                    if (connect.State == ConnectionState.Open)
                    {
                        query = string.Format("DELETE FROM dbo.AEVICards WHERE RecId = @1");

                        SqlCommand cmd = new SqlCommand(query, connect);
                        cmd.Parameters.Add(crp(SqlDbType.VarChar, "@1", pan, false));
                        cmd.ExecuteNonQuery();

                        query = string.Format("DELETE FROM dbo.AEVIBind WHERE RecId LIKE '{0}%'", pan);

                        cmd = new SqlCommand(query, connect);
                        cmd.ExecuteNonQuery();
                    }
                    else return 1;*/
                }
                else
                {
                    string resultCode = null;
                    if (!AEVIRequest(channelsarray, channels, "UnbindCard", CCrypto.DecryptPAN(pan), out resultCode, out msg))
                    {
                        ret = -1;
                        log.Write(LogType.Error, resultCode + " " + msg);
                    }
                }
            }
            catch (Exception ex) { log.Write(LogType.Error, ex.Message); ret = -1; msg = ex.Message; }
            return ret;
        }

        public int GetRecord(string pan, out STCard data, out string msg)
        {
            int ret = 0;
            data = new STCard();

            msg = null;

            string recvalue;

            SqlConnection connect;
            Log log = new Log(LogPath);

            try
            {
                connect = new SqlConnection(ConnectionString);
                connect.Open();

                if (connect.State != ConnectionState.Open)
                {
                    connect = new SqlConnection(ConnectionStringReserve);
                    connect.Open();
                }

                if (connect.State == ConnectionState.Open)
                {
                    string query = string.Format("SELECT T1.RecId,T1.RecValue,T1.I1,Count(T2.RecId) FROM dbo.AEVICards T1 " +
                        "LEFT JOIN dbo.AEVIBind T2 ON T1.RecId = SUBSTRING(T2.RecId, 1, len(T1.RecId)) " +
                        "WHERE T1.RecId='{0}' GROUP BY T1.RecId,T1.RecValue,T1.I1", CCrypto.DecryptPAN(pan));

                    SqlCommand cmd = new SqlCommand(query, connect);
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        reader.Read();
                        data = new STCard();
                        if (!reader.IsDBNull(1))
                            recvalue = reader.GetString(1);
                        else recvalue = null;
                        ret = getTags(recvalue, out data, out msg);
                        string panw = reader.GetString(0);
                        data.pan = CCrypto.EncryptPAN(panw);
                        data.maskedpan = get_mask_pan(panw);
                        if (!reader.IsDBNull(2))
                            data.owneruserid = reader.GetString(2);
                        else data.owneruserid = null;
                        data.recvalue = recvalue;
                        if (!reader.IsDBNull(3))
                            data.cntbinds = reader.GetInt32(3);
                        else data.cntbinds = 0;
                    }
                    reader.Dispose();
                }
                else return 1;
            }
            catch (Exception ex) { log.Write(LogType.Error, ex.Message); ret = -1; msg = ex.Message; }
            return ret;
        }
        public int GetRecordWithoutDecrypto(string pan, out STCard data, out string msg)
        {
            int ret = 0;
            data = new STCard();

            msg = null;

            string recvalue;

            SqlConnection connect;
            Log log = new Log(LogPath);

            try
            {
                connect = new SqlConnection(ConnectionString);
                connect.Open();

                if (connect.State != ConnectionState.Open)
                {
                    connect = new SqlConnection(ConnectionStringReserve);
                    connect.Open();
                }

                if (connect.State == ConnectionState.Open)
                {
                    string query = string.Format("SELECT T1.RecId,T1.RecValue,T1.I1,Count(T2.RecId) FROM dbo.AEVICards T1 " +
                        "LEFT JOIN dbo.AEVIBind T2 ON T1.RecId = SUBSTRING(T2.RecId, 1, len(T1.RecId)) " +
                        "WHERE T1.RecId='{0}' GROUP BY T1.RecId,T1.RecValue,T1.I1", pan);

                    SqlCommand cmd = new SqlCommand(query, connect);
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        reader.Read();
                        data = new STCard();
                        if (!reader.IsDBNull(1))
                            recvalue = reader.GetString(1);
                        else recvalue = null;
                        ret = getTags(recvalue, out data, out msg);
                        string panw = reader.GetString(0);
                        data.pan = CCrypto.EncryptPAN(panw);
                        data.maskedpan = get_mask_pan(panw);
                        if (!reader.IsDBNull(2))
                            data.owneruserid = reader.GetString(2);
                        else data.owneruserid = null;
                        data.recvalue = recvalue;
                        if (!reader.IsDBNull(3))
                            data.cntbinds = reader.GetInt32(3);
                        else data.cntbinds = 0;
                    }
                    reader.Dispose();
                }
                else return 1;
            }
            catch (Exception ex) { log.Write(LogType.Error, ex.Message); ret = -1; msg = ex.Message; }
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


       // public bool AEVIInsert()

        private bool AEVIRequest(string channelsarray, string channels, string action, string data, 
            out string resultCode, out string msg)
        {
            Log log = new Log(LogPath);
            resultCode = "-1000";
            msg = null;
            try
            {
               

                AEVI.Request(channels, action, data, out resultCode, out msg);
                if (resultCode != "0") { return false; }
            }
            catch (Exception ex) { log.Write(LogType.Error, "AEVIRequest " + ex.Message);  msg = ex.Message; }
            return true;
        }

    }
}
