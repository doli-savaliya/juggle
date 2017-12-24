using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace juggle
{
    public class Logfile
    {
        public static void WriteCDNLog(string Message, string functionname)
        {
            StreamWriter sw = null;
            try
            {
                // sw = new StreamWriter(@"C:\JuggleLog\JuggleLog.txt", true);
                string filepath = (HttpContext.Current.Server.MapPath("/Jugglelog/Jugglelog.txt"));
                sw = new StreamWriter(filepath, true);
                sw.WriteLine(DateTime.Now.ToString() + ": " + Message.ToString().Trim());
                sw.Flush();
                sw.Close();
            }
            catch(Exception ex)
            {
                ex.Message.ToString();
            }
        }
    }
}