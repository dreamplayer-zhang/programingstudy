using System;
using System.Collections.Generic;

namespace RootTools
{
    public class LogWriter
    {
        NLog.Logger m_logger;
        LogViewX m_logview;
        public LogWriter(NLog.Logger logger, LogViewX logview)
        {
            m_logger = logger;
            m_logview = logview;
        }

        public void WriteLog(string sLog)
        {
            m_logger.Info(sLog);
        }
        
        public void WriteErrorLog(string sLog)
        {
            m_logger.Error(sLog);
        }

        public void WriteWarnLog(string sLog)
        {
            m_logger.Warn(sLog);
        }


        public void WriteTraceLog(string sLog)
        {
            m_logger.Trace(sLog);
        }

        public void WriteFatalLog(string sLog)
        {
            m_logger.Fatal(sLog);
        }

        public void WriteDebugLog(string sLog)
        {
            m_logger.Debug(sLog);
        }

        public void WriteLog(LogFNC fnc, LogViewX.eStatus status)
        {
            //NLog.Logger loggg = logview.GetLog(LogView.eLogType.FNC, "class");
            LogWriter loggg = m_logview.GetLog(LogViewX.eLogType.FNC, "class", "");
            string str = Get(fnc.m_eFunc, fnc.m_sEvent, status, fnc.m_sMatID, fnc.m_sMatType) + Get(fnc.m_datas);
            loggg.WriteLog(str);
        }

        public void WriteLog(LogXFR xfr, LogViewX.eStatus status)
        {
            LogWriter loggg = m_logview.GetLog(LogViewX.eLogType.XFR, "class", "");
            string str  = Get(LogViewX.eLogType.XFR, xfr.m_sEvent, status, xfr.m_sMatID, xfr.m_sMatType) + Get(xfr.m_datas);
            loggg.WriteLog(str);
        }

        public void WriteLog(LogPRC prc, LogViewX.eStatus status)
        {
            LogWriter loggg = m_logview.GetLog(LogViewX.eLogType.PRC, "class", "");
            string str = Get(LogViewX.eLogType.PRC, prc.m_sEvent, status, prc.m_sMatID, prc.m_sRecipe) + Get(prc.m_datas);
            loggg.WriteLog(str);
        }

        public void WriteLog(LogLEH lotevent)
        {
            LogWriter loggg = m_logview.GetLog(LogViewX.eLogType.LEH, "class", "");
            string str = Get(LogViewX.eLogType.LEH, lotevent.m_sDeviceID, lotevent.m_sEvent, lotevent.m_sLot, lotevent.m_sRecipe, lotevent.m_sCarrierID) + Get(lotevent.m_datas);
            loggg.WriteLog(str);
        }

        public void WriteLog(LogALM alm)
        {
            LogWriter loggg = m_logview.GetLog(LogViewX.eLogType.ALM, "class", "");
            string str = Get(LogViewX.eLogType.ALM, alm.m_sDeviceID, alm.m_sEventID, alm.m_sALcode, alm.m_sStateALM) + Get(alm.m_datas);
            loggg.WriteLog(str);
        }

        public void WriteLog(LogCFG cfg)
        {
            LogWriter loggg = m_logview.GetLog(LogViewX.eLogType.CFG, "class", "");
            string str = Get(LogViewX.eLogType.CFG, cfg.m_sDeviceID, cfg.m_sCfgType) + Get(cfg.m_datas);
            loggg.WriteLog(str);
        }

        public void WriteLog(LogCOMM comm)
        {
            LogWriter loggg = m_logview.GetLog(LogViewX.eLogType.COMM, "class", "");
            string str = Get(LogViewX.eLogType.COMM, comm.m_sDeviceID, comm.m_sCommType) + Get(comm.m_datas);
            loggg.WriteLog(str);
        }

        public void Info(string str)
        {
            m_logger.Info(str);
        }
        public void Error(string str)
        {
            m_logger.Error(str);
        }
        public void Error(string str1,string str2)
        {
            m_logger.Error(str1,str2);
        }
        public void Error(Exception e, string str2)
        {
            m_logger.Error(e, str2);
        }

        public void Warn(string str)
        {
            m_logger.Warn(str);
        }

        public string Get(params object[] objs)
        {
            string str = "";
            foreach (object obj in objs)
            {
                str += " '" + obj.ToString() + "'";
            }
            return str;
        }

        public string Get(List<LogData> datas)
        {
            string sData = "";
            foreach (LogData data in datas)
                sData += data.Get();
            return sData;
        }
    }
}
