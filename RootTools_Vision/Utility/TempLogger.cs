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
        public static object lockObj = new object();
        public static void Write(string filename, string log)
        {
            lock(lockObj)
            {
                string strTime = DateTime.Now.ToString("yyyyMMdd");
                string filePath = @"D:\" + filename + "_" + strTime + ".txt";
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
                    sw.WriteLine(DateTime.Now.ToString("HHmmss") + "    " + log);
                    sw.Close();
                }
            }
        }
    }
}
