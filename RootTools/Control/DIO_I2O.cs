using RootTools.Trees;
using System.Collections.Generic;

namespace RootTools.Control
{
    public class DIO_I2O : IDIO
    {
        List<string> m_asID = new List<string>();
        public List<BitDI> m_aBitDI = new List<BitDI>();
        public BitDO m_bitDO = new BitDO();

        public bool p_bDone
        {
            get
            {
                if (p_bOut) return (m_aBitDI[0].p_bOn == false) && m_aBitDI[1].p_bOn; 
                else return m_aBitDI[0].p_bOn && (m_aBitDI[1].p_bOn == false);
            }
        }

        public bool p_bOut
        {
            get { return m_bitDO.p_bOn ^ m_bReverse; }
        }

        ListDIO m_listDI;
        ListDIO m_listDO;
        public string m_id;
        Log m_log;
        public DIO_I2O(IToolDIO tool, string id, Log log, bool bEnableRun, string sFalse, string sTrue)
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
            tool.p_listIDIO.Add(this);
        }

        public string RunTree(Tree tree)
        {
            string sDI0 = RunTreeDI(tree, 0);
            string sDI1 = RunTreeDI(tree, 1);
            string sDO = RunTreeDO(tree);
            if (sDI0 != "OK") return sDI0;
            if (sDI1 != "OK") return sDI1;
            if (sDO != "OK") return sDO;
            m_bReverse = tree.Set(m_bReverse, m_bReverse, "Reverse", "Reverse DO Output");
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

        string RunTreeDO(Tree tree)
        {
            int nDO = tree.Set(m_bitDO.m_nID, -1, "Output." + m_asID[1], "DIO Output Number");
            if (nDO != m_bitDO.m_nID)
            {
                if (IsUsedDO(nDO)) return "Can't Assign Exist DIO_O Output : " + m_id;
                m_bitDO.SetID(m_log, "Output");
                if (nDO < 0) m_bitDO = new BitDO();
                else
                {
                    m_bitDO = (BitDO)m_listDO.m_aDIO[nDO];
                    m_bitDO.SetID(m_log, m_id + "." + m_asID[1]);
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

        public StopWatch m_swWrite = new StopWatch(); 
        bool m_bReverse = false; 
        public void Write(bool bOn)
        {
            m_bitDO.Write(bOn ^ m_bReverse);
            m_swWrite.Start(); 
        }

        public bool p_bEnableRun { get; set; }

        enum eRun
        {
            Nothing,
            Repeat
        };
        eRun m_eRun = eRun.Nothing;
        public void RunDIO()
        {
            if (m_eRun == eRun.Nothing) return;
            switch (m_eRun)
            {
                case eRun.Repeat: Repeat(); break;
            }
        }

        StopWatch m_swRepeat = new StopWatch();
        int m_msRepeat = 1000;
        void Repeat()
        {
            if (m_swRepeat.ElapsedMilliseconds < m_msRepeat) return;
            m_swRepeat.Restart();
            Write(!p_bOut);
       }
    }
}
