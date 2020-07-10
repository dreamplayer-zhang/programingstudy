namespace RootTools.Control.ACS
{
    public class ACSBitDO : BitDO //forgetACS
    {
        public int m_nModule = -1;
        public int m_nOffset = -1;
        string m_sLog = "";
        public override void Write(bool bOn)
        {
            if (EQ.p_bSimulate)
            {
                p_bOn = bOn;
                return;
            }
            if (p_bOn == bOn) return;
            if (m_nModule < 0) return;
            if (m_nOffset < 0) return;
            string sLog = p_sLongID + ", Write Output : " + bOn.ToString();
            if (sLog != m_sLog) m_log.Info(sLog);
            m_sLog = sLog;
            //CAXD.AxdoWriteOutportBit(m_nModule, m_nOffset, (bOn ? (uint)1 : (uint)0));
        }

    }
}
