using RootTools.Trees;
using System.Collections.Generic;

namespace RootTools.Control.Ajin
{
    public class AjinListDIO : ListDIO
    {
        #region Module
        public int m_nModule = 1;
        public int p_nBit
        {
            get { return 16 * m_nModule; }
        }

        public List<int> m_aModule = new List<int>();
        public List<int> m_aOffset = new List<int>();
        List<uint> m_aRead = new List<uint>();
        uint[] m_aComp = new uint[16];
        void InitModule()
        {
            while (m_aModule.Count < m_nModule)
            {
                for (int n = 0, nID = 16 * m_aModule.Count; n < 16; n++, nID++) AddBit(NewBit(nID));
                m_aModule.Add(-1);
                m_aOffset.Add(-1);
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
                    AjinBitDO bitDO = new AjinBitDO();
                    bitDO.Init(nID, m_log);
                    return bitDO; 
            }
            return null; 
        }

        void SetModuleOffset()
        {
            if (m_eDIO != eDIO.Output) return;
            int nID = 0;
            for (int nModule = 0; nModule < m_nModule; nModule++)
            {
                for (int nOffset = 0; nOffset < 16; nOffset++, nID++)
                {
                    ((AjinBitDO)m_aDIO[nID]).m_nModule = m_aModule[nModule];
                    ((AjinBitDO)m_aDIO[nID]).m_nOffset = 16 * m_aOffset[nModule] + nOffset;
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
            for (int n = 0; n < m_nModule; n++)
            {
                if ((m_aModule[n] >= 0) && (m_aOffset[n] >= 0))
                {
                    uint uRead = 0;
                    CAXD.AxdiReadInportWord(m_aModule[n], m_aOffset[n], ref uRead);
                    m_aRead[n] = uRead; 
                }
                for (int m = 0, nID = 16 * n; m < 16; m++, nID++) m_aDIO[nID].p_bOn = ((m_aRead[n] & m_aComp[m]) > 0); 
            }
        }

        void ReadOutput()
        {
            if (EQ.p_bSimulate) return; 
            for (int n = 0; n < m_nModule; n++)
            {
                if ((m_aModule[n] >= 0) && (m_aOffset[n] >= 0))
                {
                    uint uRead = 0;
                    CAXD.AxdoReadOutportWord(m_aModule[n], m_aOffset[n], ref uRead);
                    m_aRead[n] = uRead;
                }
                for (int m = 0, nID = 16 * n; m < 16; m++, nID++) m_aDIO[nID].p_bOn = ((m_aRead[n] & m_aComp[m]) > 0);
            }
        }
        #endregion

        string m_id;
        LogWriter m_log;
        public void Init(eDIO dio, LogWriter log)
        {
            m_id = dio.ToString();
            m_eDIO = dio;
            m_log = log;
            m_aComp[0] = 1;
            for (int n = 1; n < 16; n++) m_aComp[n] = 2 * m_aComp[n - 1];
        }

        public void RunTree(Tree tree)
        {
            m_nModule = tree.Set(m_nModule, 1, "Count", "Module Count");
            InitModule();
            for (int n = 0; n < m_nModule; n++)
            {
                string sTree = m_id + "." + n.ToString("00") + " (" + (16 * n).ToString() + "~" + (16 * n + 15).ToString() + ")"; 
                RunTreeModule(n, tree.GetTree(sTree));
            }
            SetModuleOffset(); 
        }

        void RunTreeModule(int n, Tree tree)
        {
            m_aModule[n] = tree.Set(m_aModule[n], -1, "Module", "Module Number");
            m_aOffset[n] = tree.Set(m_aOffset[n], -1, "Offset", "Offset Number");
        }
    }
}
