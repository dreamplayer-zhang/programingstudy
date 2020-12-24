using RootTools.Trees;
using System;
using System.Collections.Generic;

namespace RootTools.Control.ACS
{
    public class ACSListDIO : ListDIO
    {
        #region Byte
        public string m_sName = ""; 
        public int m_lByte = 1;
        List<uint> m_aRead = new List<uint>();
        public uint[] m_aBitComp = new uint[8];
        void InitModule()
        {
            while (m_aRead.Count < m_lByte)
            {
                for (int n = 0, nID = 8 * m_aRead.Count; n < 8; n++, nID++) AddBit(NewBit(nID));
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
                    ACSBitDO bitDO = new ACSBitDO(m_acs, this);
                    bitDO.Init(nID, m_log);
                    return bitDO;
            }
            return null;
        }

        void SetModuleOffset()
        {
            if (m_eDIO != eDIO.Output) return;
            int nID = 0;
            for (int nByte = 0; nByte < m_lByte; nByte++)
            {
                for (int nBit = 0; nBit < 8; nBit++, nID++) 
                {
                    ((ACSBitDO)m_aDIO[nID]).m_nByte = nByte;
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
                dynamic d = m_acs.m_channel.ReadVariable(m_sName, -1, 0, m_lByte);
                object[] aRead = d; 
                for (int n = 0; n < m_lByte; n++)
                {
                    m_aRead[n] = (uint)aRead[n];
                    for (int m = 0, nID = 8 * n; m < 8; m++, nID++) m_aDIO[nID].p_bOn = ((m_aRead[n] & m_aBitComp[m]) > 0); 
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
                dynamic d = m_acs.m_channel.ReadVariable(m_sName, -1, 0, m_lByte);
                object[] aRead = d;
                for (int n = 0; n < m_lByte; n++)
                {
                    m_aRead[n] = (uint)aRead[n];
                    for (int m = 0, nID = 8 * n; m < 8; m++, nID++) m_aDIO[nID].p_bOn = ((m_aRead[n] & m_aBitComp[m]) > 0);
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
            m_aBitComp[0] = 1;
            for (int n = 1; n < 8; n++) m_aBitComp[n] = 2 * m_aBitComp[n - 1];
        }

        public void RunTree(Tree tree)
        {
            m_lByte = tree.Set(m_lByte, 1, "Count", "DIO Port Count (Byte)");
            InitModule();
            SetModuleOffset();
        }

    }
}
