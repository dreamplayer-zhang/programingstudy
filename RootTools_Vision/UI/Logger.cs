using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    public static class Logger
    {
        public static void AddMsg(LOG_MESSAGE_TYPE type, string msg)
        {
            WorkEventManager.OnAddLog(null, new LogArgs(type, msg));

            TempLogger.Write(type.ToString(), msg);
        }

        public static void AddMsg(LOG_MESSAGE_TYPE type, Exception e)
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

            WorkEventManager.OnAddLog(null, new LogArgs(type, eLog));

            TempLogger.Write(type.ToString(), eLog);
        }
    }
}
