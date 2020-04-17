using RootTools.Trees;
using System.Collections.Generic;

namespace RootTools.Control
{
    public class DIO_I2O2 : IDIO
    {
        List<string> m_asID = new List<string>();
        public List<BitDI> m_aBitDI = new List<BitDI>();
        public List<BitDO> m_aBitDO = new List<BitDO>();

        public bool ReadDIO(ListDIO.eDIO dio, string sID)
        {
            int nIndex = GetIndex(sID); 
            if (nIndex < 0) return false;
            switch (dio)
            {
                case ListDIO.eDIO.Input: return m_aBitDI[nIndex].p_bOn;
                case ListDIO.eDIO.Output: return m_aBitDO[nIndex].p_bOn;
            }
            return false;
        }

        int GetIndex(string sID)
        {
            for (int n = 0; n < m_asID.Count; n++)
            {
                if (sID == m_asID[n]) return n;
            }
            return -1;
        }

        ListDIO m_listDI;
        ListDIO m_listDO;
        string m_id;
        LogWriter m_log;
        public DIO_I2O2(IToolDIO tool, string id, LogWriter log, bool bEnableRun, string sFalse, string sTrue)
        {
            m_listDI = tool.p_listDI;
            m_listDO = tool.p_listDO;
            m_id = id;
            m_log = log;
            p_bEnableRun = bEnableRun; 
            m_asID.Add(sFalse);
            m_asID.Add(sTrue);
            m_aBitDI.Add(new BitDI());
            m_aBitDI.Add(new BitDI());
            m_aBitDO.Add(new BitDO());
            m_aBitDO.Add(new BitDO());
            tool.p_listIDIO.Add(this);
        }

        string m_sTreeInfo = "OK"; 
        public string RunTree(Tree tree)
        {
            int nDI0 = tree.Set(m_aBitDI[0].m_nID, -1, "Input." + m_asID[0], "DIO Input Number");
            int nDI1 = tree.Set(m_aBitDI[1].m_nID, -1, "Input." + m_asID[1], "DIO Input Number");
            int nDO0 = tree.Set(m_aBitDO[0].m_nID, -1, "Output." + m_asID[0], "DIO Output Number");
            int nDO1 = tree.Set(m_aBitDO[1].m_nID, -1, "Output." + m_asID[1], "DIO Output Number");
            m_sTreeInfo = "OK"; 
            m_aBitDI[0] = GetBit(m_listDI, m_aBitDI[0], nDI0, m_asID[0]);
            m_aBitDI[1] = GetBit(m_listDI, m_aBitDI[1], nDI1, m_asID[1]);
            m_aBitDO[0] = (BitDO)GetBit(m_listDO, m_aBitDO[0], nDO0, m_asID[0]);
            m_aBitDO[1] = (BitDO)GetBit(m_listDO, m_aBitDO[1], nDO1, m_asID[1]);
            m_eRun = (eRun)tree.Set(m_eRun, eRun.Nothing, "Run", "DIO Run Mode", p_bEnableRun);
            m_msRepeat = tree.Set(m_msRepeat, 1000, "Repeat", "Repeat Toggle Period (ms)", m_eRun == eRun.Repeat);
            return m_sTreeInfo;
        }

        BitDI GetBit(ListDIO listDIO, BitDI bitDIO, int nDIO, string sID)
        {
            if (nDIO == bitDIO.m_nID) return bitDIO;
            if (listDIO.m_aDIO[nDIO].p_sID != listDIO.m_eDIO.ToString())
            {
                m_sTreeInfo = "Can't Assign Exist DIO_I2O2 Input : " + m_id + "." + sID;
                return bitDIO;
            }
            bitDIO.SetID(m_log, listDIO.m_eDIO.ToString()); 
            listDIO.m_aDIO[nDIO].SetID(m_log, m_id + "." + sID);
            return listDIO.m_aDIO[nDIO];
        }

        public void Write(string sID, bool bOn)
        {
            int nID = GetIndex(sID);
            if (nID < 0) return;
            m_aBitDO[nID].Write(bOn);
            if (bOn) m_aBitDO[1 - nID].Write(false);
        }

        public void Write(bool bOn)
        {
            m_aBitDO[0].Write(!bOn);
            m_aBitDO[1].Write(bOn); 
        }

        public bool p_bEnableRun { get; set; }

        enum eRun
        {
            Nothing,
            OneOutput,
            OffbyInput,
            Repeat
        };
        eRun m_eRun = eRun.Nothing;
        bool[,] m_bDO = new bool[2, 2] { { false, false }, { false, false } };
        public void RunDIO()
        {
            if (m_eRun == eRun.Nothing) return;
            m_bDO[1, 0] = ReadDIO(ListDIO.eDIO.Output, m_asID[0]);
            m_bDO[1, 1] = ReadDIO(ListDIO.eDIO.Output, m_asID[1]);
            switch (m_eRun)
            {
                case eRun.OneOutput:
                    if (m_bDO[1, 0] && (m_bDO[0, 0] == false)) Write(m_asID[1], false);
                    if (m_bDO[1, 1] && (m_bDO[0, 1] == false)) Write(m_asID[0], false);
                    m_bDO[0, 0] = m_bDO[1, 0];
                    m_bDO[0, 1] = m_bDO[1, 1];
                    break;
                case eRun.OffbyInput:
                    if (m_bDO[1, 0] && ReadDIO(ListDIO.eDIO.Input, m_asID[0])) Write(m_asID[0], false);
                    if (m_bDO[1, 1] && ReadDIO(ListDIO.eDIO.Input, m_asID[1])) Write(m_asID[1], false);
                    break;
                case eRun.Repeat: Repeat(); break;
            }
        }

        StopWatch m_swRepeat = new StopWatch();
        int m_msRepeat = 1000;
        void Repeat()
        {
            if (m_swRepeat.ElapsedMilliseconds < m_msRepeat) return;
            m_swRepeat.Restart();
            if (ReadDIO(ListDIO.eDIO.Output, m_asID[0]))
            {
                Write(true); 
                return; 
            }
            if (ReadDIO(ListDIO.eDIO.Output, m_asID[1]))
            {
                Write(false);
                return; 
            }
        }
    }
}
