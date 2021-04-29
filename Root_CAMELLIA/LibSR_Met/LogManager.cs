using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Root_CAMELLIA.LibSR_Met
{
    public enum LogType
    {
        Error,
        Warning,
        Operating,
        Datas,
        PM,
        System,
        Others
    } 
    public class LogManager
    {
        private Object m_Lock = new Object();
        private StackTrace m_stackTrace = new StackTrace(true);
        public LogManager()
        {
        }

        public void WriteLog(LogType logtype, string sLog, [CallerMemberName] string sCallerName = "")
        {
            string sLogPath = ConstValue.PATH_LOG;
            string sLogPathDate = DateTime.Now.ToShortDateString() + @"\";

            DirectoryInfo info = new DirectoryInfo(sLogPath + sLogPathDate);
            if (info.Exists == false)
            {
                info.Create();
            }

            string sTimeLog = DateTime.Now.ToString("HH:mm:ss") + ":" + DateTime.Now.Millisecond.ToString("000") + " - " + "[" + logtype.ToString() + "] ";
            sTimeLog += sCallerName + "() - ";
            sTimeLog += sLog;

            string sLogFileName = DateTime.Now.ToShortDateString() + @"_LibSR_Met.txt";

            lock (m_Lock)
            {
                string FullPath = sLogPath + sLogPathDate;
                DirectoryInfo di = new DirectoryInfo(FullPath);
                if (di.Exists == false)
                {
                    di.Create();
                }
                FullPath += sLogFileName;
                StreamWriter sw = new StreamWriter(FullPath, true); // append

                sw.WriteLine(sTimeLog);
                sw.Close();
            }
        }
    }
}
