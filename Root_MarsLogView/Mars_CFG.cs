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

        public string p_sCategory { get; set; }

        public string p_sCfgID { get; set; }

        public string p_sValue { get; set; }

        public string p_sUnit { get; set; }

        public string p_sECID { get; set; }

        public string p_sData { get; set; }
        #endregion

        #region Functions
        string[] m_asLog;
        string GetString(int nIndex)
        {
            if (m_asLog.Length <= nIndex) return "";
            return m_asLog[nIndex];
        }
        #endregion

        public Mars_CFG(string[] asLog)
        {
            m_asLog = asLog;
            m_sDate = GetString(0);
            m_sTime = GetString(1);
            p_sModule = GetString(2);
            p_sCategory = GetString(4);
            p_sCfgID = GetString(5);
            p_sValue = GetString(6);
            p_sUnit = GetString(7);
            p_sECID = GetString(8);
            p_sData = GetString(9);
        }
    }
}
