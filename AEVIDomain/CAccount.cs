using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AEVIDomain
{
    public class CAccount
    {
        public string UserId;
        public string ConnectionString;
        public string LogPath;

        public CAccount(string userid, string connectionstring, string logpath)
        {
            UserId = userid;
            ConnectionString = connectionstring;
            LogPath = logpath;
        }
    }
}
