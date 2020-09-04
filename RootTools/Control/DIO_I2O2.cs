using RootTools.Trees;
using System.Collections.Generic;

namespace RootTools.Control
{
    public class DIO_I2O2 : IDIO
    {
        List<string> m_asID = new List<string>();
        public List<BitDI> m_aBitDI = new List<BitDI>();
        public List<BitDO> m_aBitDO = new List<BitDO>();

        public bool p_bIn
        {
            get
            {
                if (p_bOut) return (m_aBitDI[0].p_bOn == false) && m_aBitDI[1].p_bOn;
                else return m_aBitDI[0].p_bOn && (m_aBitDI[1].p_bOn == false);
            }
        }

        public bool p_bOut
        {
            get { return m_aBitDO[1].p_bOn && (m_aBitDO[0].p_bOn == false); }
        }

        ListDIO m_listDI;
        ListDIO m_listDO;
        string m_id;
        Log m_log;
        public DIO_I2O2(IToolDIO tool, string id, Log log, bool bEnableRun, string sFalse, string sTrue)
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

        public string RunTree(Tree tree)
        {
            string sDI0 = RunTreeDI(tree, 0);
            string sDI1 = RunTreeDI(tree, 1);
            string sDO0 = RunTreeDO(tree, 0);
            string sDO1 = RunTreeDO(tree, 1);
            if (sDI0 != "OK") return sDI0;
            if (sDI1 != "OK") return sDI1;
            if (sDO0 != "OK") return sDO0;
            if (sDO1 != "OK") return sDO1;
            m_eRun = (eRun)tree.Set(m_eRun, eRun.Nothing, "Run", "DIO Run Mode", p_bEnableRun);
            m_msRepeat = tree.Set(m_msRepeat, 1000, "Repeat", "Repeat Toggle Period (ms)", m_eRun == eRun.Repeat);
            return "OK";
        }

        string RunTreeDI(Tree tree, int nIndex)
        {
            int nDI = tree.Set(m_aBitDI[nIndex].m_nID, -1, "Input." + m_asID[nIndex], "DIO Input Number");
            if (nDI != m_aBitDI[nIndex].m_nID)
            {
                if (IsUsedDI(nDI)) return "Can't Assign Exist DIO_I Input : " + m_id;
                m_aBitDI[nIndex].SetID(m_log, "Input");
                if (nDI < 0) m_aBitDI[nIndex] = new BitDI();
                else
                {
                    m_aBitDI[nIndex] = m_listDI.m_aDIO[nDI];
                    m_aBitDI[nIndex].SetID(m_log, m_id + "." + m_asID[nIndex]);
                }
            }
            return "OK";
        }

        bool IsUsedDI(int nDI)
        {
            if (nDI < 0) return false;
            if (nDI >= m_listDI.m_aDIO.Count) return true;
            return (m_listDI.m_aDIO[nDI].p_sID != "Input");
        }

        string RunTreeDO(Tree tree, int nIndex)
        {
            int nDO = tree.Set(m_aBitDO[nIndex].m_nID, -1, "Output." + m_asID[nIndex], "DIO Output Number");
            if (nDO != m_aBitDO[nIndex].m_nID)
            {
                if (IsUsedDO(nDO)) return "Can't Assign Exist DIO_O Output : " + m_id;
                m_aBitDO[nIndex].SetID(m_log, "Output");
                if (nDO < 0) m_aBitDO[nIndex] = new BitDO();
                else
                {
                    m_aBitDO[nIndex] = (BitDO)m_listDO.m_aDIO[nDO];
                    m_aBitDO[nIndex].SetID(m_log, m_id + "." + m_asID[nIndex]);
                }
            }
            return "OK";
        }

        bool IsUsedDO(int nDO)
        {
            if (nDO < 0) return false;
            if (nDO >= m_listDO.m_aDIO.Count) return true;
            return (m_listDO.m_aDIO[nDO].p_sID != "Output");
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
            m_bDO[1, 0] = m_aBitDO[0].p_bOn; 
            m_bDO[1, 1] = m_aBitDO[1].p_bOn;
            switch (m_eRun)
            {
                case eRun.OneOutput:
                    if (m_bDO[1, 0] && (m_bDO[0, 0] == false)) m_aBitDO[1].Write(false);
                    if (m_bDO[1, 1] && (m_bDO[0, 1] == false)) m_aBitDO[0].Write(false);
                    m_bDO[0, 0] = m_bDO[1, 0];
                    m_bDO[0, 1] = m_bDO[1, 1];
                    break;
                case eRun.OffbyInput:
                    if (m_bDO[1, 0] && m_aBitDI[0].p_bOn) m_aBitDO[0].Write(false);
                    if (m_bDO[1, 1] && m_aBitDI[2].p_bOn) m_aBitDO[1].Write(false);
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
            if (m_aBitDO[0].p_bOn) Write(true);
            else if (m_aBitDO[1].p_bOn) Write(false);
        }
    }
}
