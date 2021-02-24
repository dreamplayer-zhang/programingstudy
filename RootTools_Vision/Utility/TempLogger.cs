using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    public class TempLogger
    {
        public static string tempLogRootPath = @"D:\TempLog";

        public static object lockObj = new object();
        public static void Write(string filename, string log)
        {
            Directory.CreateDirectory(tempLogRootPath);

            lock(lockObj)
            {
                string strTime = DateTime.Now.ToString("yyyyMMdd");
                string filePath = tempLogRootPath + "\\" + filename + "_" + strTime + ".txt";
                FileInfo file = new FileInfo(filePath);
                StreamWriter sw;
                if (!file.Exists)
                {
                    sw = file.CreateText();

                }
                else
                {
                    sw = file.AppendText();
                }

                lock (sw)
                {
                    sw.WriteLine(DateTime.Now.ToString("HH:mm:ss") + "    " + log);
                    sw.Close();
                }
            }
        }

        public static void Write(string filename, Exception e, string log = "")
        {
            Directory.CreateDirectory(tempLogRootPath);

            lock (lockObj)
            {
                string strTime = DateTime.Now.ToString("yyyyMMdd");
                string filePath = tempLogRootPath + "\\" + filename + "_" + strTime + ".txt";
                FileInfo file = new FileInfo(filePath);
                StreamWriter sw;
                if (!file.Exists)
                {
                    sw = file.CreateText();

                }
                else
                {
                    sw = file.AppendText();
                }

                lock (sw)
                {
                    string eLog = "Exception : " + e.Message + "\n";
                    if (e.StackTrace != null)
                        eLog += "Stacktrace : " + e.StackTrace + "\n";
                    if (e.Source != null)
                        eLog += "Source : " + e.Source + "\n";
                    if (e.TargetSite != null)
                        eLog += "TargetSite : " + e.TargetSite + "\n";

                    if (e.InnerException != null)
                    {
                        eLog += "InnerException : " + e.InnerException.Message + "\n";
                        if (e.InnerException.StackTrace != null)
                            eLog += "InnerException StackTrace : " + e.InnerException.StackTrace + "\n";
                        if (e.InnerException.Source != null)
                            eLog += "InnerException Source : " + e.InnerException.Source + "\n";
                        if (e.InnerException.TargetSite != null)
                            eLog += "InnerException TargetSite : " + e.InnerException.TargetSite + "\n";
                    }

                    sw.WriteLine(DateTime.Now.ToString("HH:mm:ss") + "\n"
                         + log + "\n" + eLog
                         );
                    sw.Close();
                }
            }
        }
    }
}
