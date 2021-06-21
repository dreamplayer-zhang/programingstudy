using RootTools;

namespace Root_MarsLogView
{
    public class Mars_CFG : NotifyProperty
    {
        #region DateTime
        public string m_sDate;
        public string m_sTime;

        public string p_sTime
        {
            get { return (m_sDate ?? "") + " " + (m_sTime ?? ""); }
        }
        #endregion

        #region Property
        public string p_sModule { get; set; }
   
        public string p_sEvent { get; set; }

        public string p_sCfgID { get; set; }

        public string p_sValue { get; set; }

        public string p_sUnit { get; set; }

        public string p_sECID { get; set; }

        public string p_sData { get; set; }
        #endregion

        #region Functions
        public string[] m_asLog;
        public bool IsSame(string[] asLog)
        {
            m_asLog = asLog;
            if (GetString(2) != p_sModule) return false;
            if (GetString(4) != p_sEvent) return false;
            if (GetString(5) != p_sCfgID) return false;
            if (GetString(6) != p_sValue) return false;
            return true;
        }

        string GetString(int nIndex)
        {
            if (m_asLog.Length <= nIndex) return "";
            string sLog = m_asLog[nIndex];
            if (sLog.Length == 0) return "";
            if (sLog[sLog.Length - 1] == '\'') sLog = sLog.Substring(0, sLog.Length - 1);
            if (sLog[0] == '\'') sLog = sLog.Substring(1, sLog.Length - 1);
            return sLog;
        }

        #endregion
        public string m_sLog;
        public int m_iTCP; //210602 nscho
        public Mars_CFG(int iTCP, string sLog, string[] asLog)
        {
            m_iTCP = iTCP;
            m_sLog = sLog; 
            m_asLog = asLog;
            m_sDate = GetString(0);
            m_sTime = GetString(1);
            p_sModule = GetString(2);//EFEM or Vision
            p_sEvent = GetString(4);//Version
            p_sCfgID = GetString(5);//'Software Version'
            p_sValue = GetString(6);
            p_sUnit = GetString(7);
            p_sECID = GetString(8);
            p_sData = GetString(9);
        }
    }
}
