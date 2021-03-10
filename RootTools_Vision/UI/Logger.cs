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
    }
}
