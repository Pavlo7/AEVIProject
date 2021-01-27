using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AEVIDomain
{
    public class CPermission
    {
        public CPermission() { }

        public int GetId(string data)
        {
            int ret = -1;
            if (data == "FULL") ret = 0;
            if (data == "PREMIUM") ret = 1;
            if (data == "STANDART") ret = 2;

            return ret;
        }

        public string GetName(int data)
        {
            string ret = null;
            if (data == 0) ret = "FULL";
            if (data == 1) ret = "PREMIUM";
            if (data == 2) ret = "STANDART";

            return ret;
        }

        public List<string> GetList(int tp)
        {
            int type = tp;
            List<string> ret = new List<string>();

            switch (type)
            {
                case 0:
                    ret.Add("PREMIUM");
                    ret.Add("STANDART");
                    break;
                case 1:
                    ret.Add("STANDART");
                    break;
            }

            return ret;
        }
    }
}