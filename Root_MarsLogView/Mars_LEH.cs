using RootTools;
using System;

namespace Root_MarsLogView
{
    public class Mars_LEH : NotifyProperty
    {
        #region Event
        public enum eEvent
        {
            CarrierLoad,
            ProcessJobStart,
            ProcessJobEnd,
            CarrierUnload
        }
        eEvent _eEvent = eEvent.CarrierLoad; 
        public eEvent p_eEvent
        { 
            get { return _eEvent; }
            set
            {
                if (_eEvent == value) return; 
                _eEvent = value;
                p_sEvent += ", " + value.ToString();
            }
        }

        string _sEvent = eEvent.CarrierLoad.ToString(); 
        public string p_sEvent
        {
            get { return _sEvent; }
            set
            {
                _sEvent = value;
                OnPropertyChanged(); 
            }
        }

        public static eEvent GetEvent(string sEvent)
        {
            foreach (eEvent eEvent in Enum.GetValues(typeof(eEvent)))
            {
                if (sEvent == eEvent.ToString()) return eEvent; 
            }
            return eEvent.CarrierLoad;
        }
        #endregion

        #region DateTime
        public string[] m_sDate = new string[4];
        public string[] m_sTime = new string[4];

        public string p_sTimeCarrierLoad
        {
            get { return (m_sDate[0] ?? "") + " " + (m_sTime[0] ?? ""); }
        }

        public string p_sTimeProcessJobStart
        {
            get { return (m_sDate[1] ?? "") + " " + (m_sTime[1] ?? ""); }
        }

        public string p_sTimeProcessJobEnd
        {
            get { return (m_sDate[2] ?? "") + " " + (m_sTime[2] ?? ""); }
        }

        public string p_sTimeCarrierUnload
        {
            get { return (m_sDate[3] ?? "") + " " + (m_sTime[3] ?? ""); }
        }
        #endregion

        #region Property
        public string p_sModule { get; set; }

        public string p_sLot { get; set; }

        public string p_sRecipe { get; set; }

        public string p_sFlowInfo { get; set; }

        public string p_sCarrierID { get; set; }

        public string p_sData { get; set; }
        #endregion

        #region Functions
        public string[] m_asLog;
        public bool IsSame(string[] asLog)
        {
            m_asLog = asLog;
            if (GetString(2) != p_sModule) return false;
            if (GetString(5) != p_sLot) return false;
            if (GetString(6) != p_sRecipe) return false;
            if (GetString(8) != p_sCarrierID) return false;
            return true;
        }

        string GetString(int nIndex)
        {
            if (m_asLog.Length <= nIndex) return "";
            string sLog = m_asLog[nIndex];
            if (sLog == "") return "";
            if (sLog[sLog.Length - 1] == '\'') sLog = sLog.Substring(0, sLog.Length - 1);
            if (sLog[0] == '\'') sLog = sLog.Substring(1, sLog.Length - 1);
            return sLog;
        }

        public string GetEndLog(eEvent eEvent, string[] asLog)
        {
            m_asLog[0] = asLog[0];
            m_asLog[1] = asLog[1];
            m_asLog[4] = '\'' + eEvent.ToString() + '\'';
            m_sDate[(int)eEvent] = asLog[0];
            m_sTime[(int)eEvent] = asLog[1];
            p_eEvent = eEvent; 
            switch (eEvent)
            {
                case eEvent.CarrierLoad: OnPropertyChanged("p_sTimeCarrierLoad"); break;
                case eEvent.ProcessJobStart: OnPropertyChanged("p_sTimeProcessJobStart"); break;
                case eEvent.ProcessJobEnd: OnPropertyChanged("p_sTimeProcessJobEnd"); break;
                case eEvent.CarrierUnload: OnPropertyChanged("p_sTimeCarrierUnload"); break;
            }
            string sLog = "";
            for (int n = 0; n < m_asLog.Length - 1; n++) sLog += m_asLog[n] + '\t';
            return sLog + m_asLog[m_asLog.Length - 1];
        }
        #endregion
        public int m_iTCP;
        public string m_sLog; 
        public Mars_LEH(int iTCP, string sLog, string[] asLog)
        {
            m_iTCP = iTCP; 
            m_sLog = sLog; 
            m_asLog = asLog;
            m_sDate[0] = GetString(0);
            m_sTime[0] = GetString(1);
            p_sModule = GetString(2);
            p_sLot = GetString(5);
            p_sRecipe = GetString(6);
            p_sFlowInfo = GetString(7);
            p_sCarrierID = GetString(8);
            p_sData = GetString(9);
        }
    }
}
