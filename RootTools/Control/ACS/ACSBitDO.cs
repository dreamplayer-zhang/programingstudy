using SPIIPLUSCOM660Lib;
using System;

namespace RootTools.Control.ACS
{
    public class ACSBitDO : BitDO
    {
        public int m_nPort = -1;
        public int m_nBit = -1;
        public override void Write(bool bOn)
        {
            if (EQ.p_bSimulate)
            {
                p_bOn = bOn;
                return;
            }
            if (p_bOn == bOn) return;
            if (m_nPort < 0) return;
            if (m_nBit < 0) return;
            if (m_acs.p_bConnect == false) return; 
            try
            {
                m_acs.m_channel.SetOutput(m_nPort, m_nBit, (bOn ? (int)1 : (int)0));
                Log(p_sLongID + ", Write Output : " + bOn.ToString());
            }
            catch (Exception e) { Log("SetOutput Error : " + e.Message); }
        }

        string m_sLog = "";
        void Log(string sLog)
        {
            if (sLog == m_sLog) return;
            m_log.Info(sLog);
            m_sLog = sLog;
        }

        ACS m_acs; 
        public ACSBitDO(ACS acs)
        {
            m_acs = acs; 
        }
    }
}
