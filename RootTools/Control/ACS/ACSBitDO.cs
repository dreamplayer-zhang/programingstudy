using SPIIPLUSCOM660Lib;
using System;

namespace RootTools.Control.ACS
{
    public class ACSBitDO : BitDO
    {
        public int m_nByte = 0; 
        public int m_nBit = -1;
        
        public override void Write(bool bOn)
        {
            if (EQ.p_bSimulate)
            {
                p_bOn = bOn;
                return;
            }
            if (p_bOn == bOn) return;
            if (m_nBit < 0) return;
            if (m_acs.p_bConnect == false) return; 
            try
            {
                dynamic d = m_acs.m_channel.ReadVariable(m_listDIO.m_sName, -1, m_nByte, m_nByte);
                uint nDO = (uint)(int)d;
                if (bOn) nDO |= m_listDIO.m_aBitComp[m_nBit];
                else nDO &= (m_listDIO.m_aBitComp[m_nBit] ^ 0xff); 
                m_acs.m_channel.WriteVariable(nDO, m_listDIO.m_sName, -1, m_nByte, m_nByte); 
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
        ACSListDIO m_listDIO; 
        public ACSBitDO(ACS acs, ACSListDIO listDIO)
        {
            m_acs = acs;
            m_listDIO = listDIO; 
        }
    }
}
