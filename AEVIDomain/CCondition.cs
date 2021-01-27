using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AEVIDomain
{
    public class CCondition
    {
        public CCondition() { }

        public int GetId(string data)
        {
            int ret = -1;
            if (data == "Active") ret = 0;
            if (data == "Blocked") ret = 1;
            if (data == "Deleted") ret = 2;

            return ret;
        }

        public string GetName(int data)
        {
            string ret = null;
            if (data == 0) ret = "Active";
            if (data == 1) ret = "Blocked";
            if (data == 2) ret = "Deleted";

            return ret;
        }

        public List<string> GetList()
        {
            List<string> ret = new List<string>();
            ret.Add("Active");
            ret.Add("Blocked");
            ret.Add("Deleted");
            return ret;
        }
    }
}