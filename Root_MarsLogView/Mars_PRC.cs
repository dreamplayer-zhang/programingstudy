using RootTools;

namespace Root_MarsLogView
{
    public class Mars_PRC : NotifyProperty
    {
        #region Status
        public enum eStatus
        {
            Start,
            End
        }
        eStatus _eStatus = eStatus.Start; 
        public eStatus p_eStatus
        { 
            get { return _eStatus; }
            set
            {
                _eStatus = value;
                if (value == eStatus.End) p_sStatus = "Start, End"; 
            }
        }

        string _sStatus = "Start"; 
        public string p_sStatus
        {
            get { return _sStatus; }
            set
            {
                _sStatus = value;
                OnPropertyChanged(); 
            }
        }
        #endregion

        #region DateTime
        public string[] m_sDate = new string[2];
        public string[] m_sTime = new string[2];

        public string p_sStart
        {
            get { return (m_sDate[0] ?? "") + " " + (m_sTime[0] ?? ""); }
        }

        public string p_sEnd
        {
            get { return (m_sDate[1] ?? "") + " " + (m_sTime[1] ?? ""); }
        }
        #endregion

        #region Property
        public string p_sModule { get; set; }

        public string p_sEvent { get; set; }

        public string p_sMaterial { get; set; }

        public string p_sMaterialType { get; set; }

        public string p_sSlot { get; set; }

        public string p_sLot { get; set; }

        public string p_sRecipe { get; set; }

        public string p_sStepNo { get; set; }

        public string p_sStepSeq { get; set; }

        public string p_sStepName { get; set; }

        public string p_sData { get; set; }
        #endregion

        #region Functions
        public string[] m_asLog;
        public bool IsSame(string[] asLog)
        {
            m_asLog = asLog; 
            if (GetString(2) != p_sModule) return false;
            if (GetString(4) != p_sEvent) return false;
            if (GetString(9) != p_sLot) return false;
            if (GetString(11) != p_sStepNo) return false;
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

        public void End(string[] asLog)
        {
            m_asLog = asLog;
            m_sDate[1] = GetString(0);
            m_sTime[1] = GetString(1);
            p_eStatus = eStatus.End;
            OnPropertyChanged("p_sEnd");
        }

        public string GetEndLog(string[] asLog)
        {
            m_asLog[0] = asLog[0];
            m_asLog[1] = asLog[1];
            m_asLog[5] = "'End'";
            string sLog = "";
            for (int n = 0; n < m_asLog.Length - 1; n++) sLog += m_asLog[n] + '\t';
            return sLog + m_asLog[m_asLog.Length - 1]; 
        }
        #endregion
        public int m_iTCP; 
        public string m_sLog; 
        public Mars_PRC(int iTCP, string sLog, string[] asLog)
        {
            m_iTCP = iTCP; 
            m_sLog = sLog; 
            m_asLog = asLog;
            m_sDate[0] = GetString(0);
            m_sTime[0] = GetString(1); 
            p_sModule = GetString(2);
            p_sEvent = GetString(4);
            p_sMaterial = GetString(6);
            p_sMaterialType = GetString(7);
            p_sSlot = GetString(8);
            p_sLot = GetString(9);
            p_sRecipe = GetString(10);
            p_sStepNo = GetString(11);
            p_sStepSeq = GetString(12);
            p_sStepName = GetString(13);
            p_sData = GetString(14);
        }
    }
}
