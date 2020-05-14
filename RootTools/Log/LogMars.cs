using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools
{
     
    public class LogFNC
    {  
        public LogViewX.eLogType m_eFunc = LogViewX.eLogType.FNC;
        public string m_sEvent;
        public string m_sMatType;
        public string m_sMatID;
        public List<LogData> m_datas;
        public LogFNC(string sEvent, LogViewX.eMaterial eMatType, string sMatID, List<LogData> datas)
        {  
            m_sEvent = sEvent;
            m_sMatType = eMatType.ToString().ToUpper();
            m_sMatID = sMatID;
            m_datas = datas;
        }
    }
    public class LogXFR
    {
        public string m_sEvent;
        public string m_sMatType;
        public string m_sMatID;
        public string m_sFrom;
        public string m_sTo;
        public List<LogData> m_datas;
        public LogXFR(string sEvent, LogViewX.eMaterial eMatType, string sMatID, string sFrom, string sTo,  List<LogData> datas)
        {
            m_sEvent = sEvent;
            m_sMatType = eMatType.ToString().ToUpper();
            m_sMatID = sMatID;
            m_sFrom = sFrom;
            m_sTo = sTo;
            m_datas = datas;
        }
    }
    public class LogPRC
    {
        public string m_sEvent;
        public string m_sMatID;
        public string m_sLot;
        public string m_sRecipe;
        public List<LogData> m_datas;
        public LogPRC(string sEvent, string sMatID, string sLot, string sRecipe,  List<LogData> datas)
        {
            m_sEvent = sEvent;
            m_sMatID = sMatID;
            m_sLot = sLot;
            m_sRecipe = sRecipe;
            m_datas = datas;
        }
    }

    public class LogLEH
    {
        public string m_sDeviceID;
        public string m_sEvent;
        public string m_sLot;
        public string m_sRecipe;
        public string m_sCarrierID;
        public List<LogData> m_datas;
        public LogLEH(LogViewX.eLotEvent eEvent, string DeviceID, string sLot, string sRecipe, string sCarrier, List<LogData> datas)
        {
            m_sDeviceID = DeviceID;
            m_sEvent = eEvent.ToString();
            m_sLot = sLot;
            m_sRecipe = sRecipe;
            m_sCarrierID = sCarrier;
            m_datas = datas;
        }
    }

    public class LogALM
    {
        public string m_sDeviceID;
        public string m_sEventID;
        public string m_sALcode;
        public string m_sStateALM;
        public List<LogData> m_datas;
        public LogALM(string sDeviceID, string sEventID, string sALcode, LogViewX.eStateALM state, LogViewX.eEQP_Stop stop, List<LogData> datas)
        {
            m_sDeviceID = sDeviceID;
            m_sEventID=sEventID;
            m_sALcode=sALcode;
            m_sStateALM=state.ToString();
            
            m_datas = datas;
            if (stop != LogViewX.eEQP_Stop.NONE)
            {
                LogData data = new LogData("DESCRIPTION", stop.ToString());
                m_datas.Insert(0, data);
            }
        }
    }

    public class LogCFG
    {
        public string m_sDeviceID;
        public string m_sCfgType;
        public List<LogData> m_datas;
        public LogCFG(string sDeviceID, LogViewX.eCFGType cfgType, List<LogData> datas)
        {
            m_sDeviceID = sDeviceID;
            m_sCfgType = cfgType.ToString();
            m_datas = datas;
        }
    }

    public class LogCOMM
    {
        public string m_sDeviceID;
        public string m_sCommType;
        public List<LogData> m_datas;
        public LogCOMM(string sDeviceID, LogViewX.eCOMMType CommType, List<LogData> datas)
        {
            m_sDeviceID = sDeviceID;
            m_sCommType = CommType.ToString();
            m_datas = datas;
        }
    }

    public class LogData
    {
        public string m_sKey;
        public string m_sValue;

        public LogData(string sKey, int nValue)
        {
            m_sKey = sKey.ToUpper();
            m_sValue = nValue.ToString();
        }

        public LogData(string sKey, double fValue)
        {
            m_sKey = sKey.ToUpper();
            m_sValue = fValue.ToString(".000");
        }

        public LogData(string sKey, object oValue)
        {
            m_sKey = sKey.ToUpper();
            m_sValue = "'" + oValue + "'";
        }

        public string Get()
        {
            return " ('" + m_sKey + "', " + m_sValue + ")";
        }
    }
}
