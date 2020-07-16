using RootTools.Trees;
using System;
using System.Collections.Generic;

namespace RootTools.Control.ACS
{
    public class ACSListDIO : ListDIO
    {
        #region Port
        public int m_lPort = 1;
        public List<int> m_aPort = new List<int>();
        List<uint> m_aRead = new List<uint>();
        uint[] m_aComp = new uint[16];
        void InitModule()
        {
            while (m_aPort.Count < m_lPort)
            {
                for (int n = 0, nID = 16 * m_aPort.Count; n < 16; n++, nID++) AddBit(NewBit(nID));
                m_aPort.Add(-1);
                m_aRead.Add(0);
            }
        }

        BitDI NewBit(int nID)
        {
            switch (m_eDIO)
            {
                case eDIO.Input:
                    BitDI bitDI = new BitDI();
                    bitDI.Init(nID, m_log);
                    return bitDI;
                case eDIO.Output:
                    ACSBitDO bitDO = new ACSBitDO(m_acs);
                    bitDO.Init(nID, m_log);
                    return bitDO;
            }
            return null;
        }

        void SetModuleOffset()
        {
            if (m_eDIO != eDIO.Output) return;
            int nID = 0;
            for (int nPort = 0; nPort < m_lPort; nPort++)
            {
                for (int nBit = 0; nBit < 16; nBit++, nID++)
                {
                    ((ACSBitDO)m_aDIO[nID]).m_nPort = m_aPort[nPort];
                    ((ACSBitDO)m_aDIO[nID]).m_nBit = nBit;
                }
            }
        }
        #endregion

        #region Read IO
        public void ReadIO()
        {
            switch (m_eDIO)
            {
                case eDIO.Input: ReadInput(); break;
                case eDIO.Output: ReadOutput(); break;
            }
        }

        void ReadInput()
        {
            if (EQ.p_bSimulate) return;
            if (m_acs.p_bConnect == false) return; 
            try
            {
                for (int n = 0; n < m_lPort; n++)
                {
                    if (m_aPort[n] >= 0) m_aRead[n] = (uint)m_acs.m_channel.GetInputPort(n);
                    for (int m = 0, nID = 16 * n; m < 16; m++, nID++) m_aDIO[nID].p_bOn = ((m_aRead[n] & m_aComp[m]) > 0);
                }
            }
            catch (Exception e) { LogError("GetInputPort Error : " + e.Message); }
        }

        void ReadOutput()
        {
            if (EQ.p_bSimulate) return;
            if (m_acs.p_bConnect == false) return;
            try
            {
                for (int n = 0; n < m_lPort; n++)
                {
                    if (m_aPort[n] >= 0) m_aRead[n] = (uint)m_acs.m_channel.GetOutputPort(n);
                    for (int m = 0, nID = 16 * n; m < 16; m++, nID++) m_aDIO[nID].p_bOn = ((m_aRead[n] & m_aComp[m]) > 0);
                }
            }
            catch (Exception e) { LogError("GetOutputPort Error : " + e.Message); }
        }

        StopWatch m_swError = new StopWatch();
        void LogError(string sError)
        {
            if (m_swError.ElapsedMilliseconds < 5000) return;
            m_swError.Restart();
            m_log.Error(m_id + " " + sError);
        }
        #endregion

        string m_id;
        ACS m_acs; 
        Log m_log;
        public void Init(eDIO dio, ACS acs)
        {
            m_id = dio.ToString();
            m_eDIO = dio;
            m_acs = acs; 
            m_log = acs.m_log;
            m_aComp[0] = 1;
            for (int n = 1; n < 16; n++) m_aComp[n] = 2 * m_aComp[n - 1];
        }

        public void RunTree(Tree tree)
        {
            m_lPort = tree.Set(m_lPort, 1, "Count", "DIO Port Count");
            InitModule();
            RunTreePort(tree.GetTree(m_id)); 
            SetModuleOffset();
        }

        void RunTreePort(Tree tree)
        {
            for (int n = 0; n < m_lPort; n++)
            {
                m_aPort[n] = tree.Set(m_aPort[n], -1, n.ToString("00"), "DIO Port Number"); 
            }
        }

    }
}
