using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace AEVIDomain
{
    public struct STTransactVP
    {
        public DateTime dtbegin;
        public DateTime dtend;
        public string maskedpan;
        public string maskedpos;
        public string strdata;
    }

    public struct STTransact
    {
        public DateTime ltime;
        public string pan;
        public string country;
        public string pos;
        public string amount;
        public string currency;
        public string product;
        public string quantity;
    }

    public class CTransact
    {
        public string UserId;
        public string ConnectionString;
        public string ConnectionStringReserve;
        public string LogPath;

        public CTransact(string userid, string connectionstring, string connectionstringreserve, string logpath) 
        {
            UserId = userid;
            ConnectionString = connectionstring;
            ConnectionStringReserve = connectionstringreserve;
            LogPath = logpath;
        }

        public int GetData(STTransactVP param, out List<STTransact> data, out string msg)
        {
            int ret = 0;
            data = new List<STTransact>();
            List<STTransact> retdata = new List<STTransact>();

            SqlConnection connect;
            Log log = new Log(LogPath);

            msg = null;

        

            string where;

            STTransact item;
         

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

                    if (param.maskedpan != null)
                        where += string.Format("AND CardNumber LIKE '{0}' ", param.maskedpan.Replace('*', '%'));
                   
                    if (param.maskedpos != null)
                        where += string.Format("AND IssuerTags LIKE '%<POSId=%{0}%>%'", param.maskedpos.Replace('*', '%'));


                    string query = string.Format("SELECT LTime, dbo.getMaskedPAN(CardNumber) as Card, POSCountry as Country, " +
                        "dbo.getTag(IssuerTags,'POSId') as POS, Amount, Currency, Product, Quantity, CardNumber " +
                        "from Keepstorage where LTime>=@1 AND LTime<=@2 AND (POSOwner = '405' or POSOwner = '406') {0} " +
                        "order by LTime", where);

                    SqlCommand cmd = new SqlCommand(query, connect);
                    cmd.Parameters.Add(crp(SqlDbType.DateTime, "@1", param.dtbegin, true));
                    cmd.Parameters.Add(crp(SqlDbType.DateTime, "@2", param.dtend, true));
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            item = new STTransact();

                            item.ltime = reader.GetDateTime(0);
                            if (!reader.IsDBNull(1))
                                item.pan = reader.GetString(1);
                            else item.pan = null;
                            if (!reader.IsDBNull(2))
                                item.country = reader.GetString(2);
                            else item.country = null;
                            if (!reader.IsDBNull(3))
                                item.pos = reader.GetString(3);
                            else item.pos = null;
                            if (!reader.IsDBNull(4))
                                item.amount = reader.GetDecimal(4).ToString();
                            else item.amount = null;
                            if (!reader.IsDBNull(5))
                                item.currency = reader.GetString(5);
                            else item.currency = null;
                            if (!reader.IsDBNull(6))
                                item.product = reader.GetString(6);
                            else item.product = null;
                            if (!reader.IsDBNull(7))
                                item.quantity = reader.GetDecimal(7).ToString();
                            else item.quantity = null;

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
