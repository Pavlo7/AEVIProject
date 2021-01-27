using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace AEVIDomain
{
    public enum LogType
    {
        Info,
        Warn,
        Error,
        Fatal
    }

    public class Log
    {
        string FullPath;

        public Log(string path)
        {
            try
            {
                FullPath = path;
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
        }

        public void Write(LogType type, string msg)
        {
            string fullText = null;

            try
            {
                if (!Directory.Exists(FullPath)) Directory.CreateDirectory(FullPath);

                using (StreamWriter writer = new StreamWriter(Path.Combine(FullPath, get_filename(type)), true))
                {
                    fullText = string.Format("{2:10} [{0: HH:mm:ss}] {1}", DateTime.Now, msg, type.ToString());

                    writer.WriteLine(fullText);
                    writer.Flush();
                }
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
        }

        private static string get_filename(LogType type)
        {
            string ret = null;

            try { ret = string.Format("{0}.log", DateTime.Now.ToString("yyyy-MM-dd")); }
            catch (Exception ex) { }

            return ret;
        }
    }
}
