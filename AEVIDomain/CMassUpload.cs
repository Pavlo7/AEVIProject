using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace AEVIDomain
{
    public class CMassUpload
    {
        public string UserId;
        public string ConnectionString;
        public string ConnectionStringReserve;
        public string LogPath;

        public CMassUpload(string userid, string connectionstring, string connectionstringreserve, string logpath) 
        {
            UserId = userid;
            ConnectionString = connectionstring;
            ConnectionStringReserve = connectionstringreserve;
            LogPath = logpath;
        }

        public bool Read()
        {
            return true;
            //StreamReader sr = new StreamReader(FilePath);
        }
    }
}
