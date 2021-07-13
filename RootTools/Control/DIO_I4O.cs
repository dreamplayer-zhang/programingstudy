using RootTools.Trees;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace RootTools.Control
{
    public class DIO_I4O : IDIO
    {
        List<string> m_asID = new List<string>();
        public List<BitDI> m_aBitDIA = new List<BitDI>();
        public List<BitDI> m_aBitDIB = new List<BitDI>();
        public BitDO m_bitDO = new BitDO();

        public bool p_bDone
        {
            get
            {
                if (p_bOut) return (m_aBitDIA[0].p_bOn == false) && (m_aBitDIB[0].p_bOn == false) && m_aBitDIA[1].p_bOn && m_aBitDIA[1].p_bOn;
                else return m_aBitDIA[0].p_bOn && m_aBitDIB[0].p_bOn && (m_aBitDIA[1].p_bOn == false) && (m_aBitDIB[1].p_bOn == false);
            }
        }

        public int m_secTimeout = 7;
        public string WaitDone()
        {
            int msWait = (int)(1000 * m_secTimeout);
            while (m_swWrite.ElapsedMilliseconds < msWait)
            {
                if (EQ.IsStop()) return m_id + " EQ Stop";
                if (p_bDone) return "OK";
            }
            return "DIO Timeout : " + m_id;
        }

        public string RunSol(bool bOn)
        {
            Write(bOn);
            Thread.Sleep(200);
            return WaitDone();
        }

        public bool p_bOut { get; set; }

        ListDIO m_listDI;
        ListDIO m_listDO;
        public string m_id;
        Log m_log;
        public DIO_I4O(IToolDIO tool, string id, Log log, bool bEnableRun, string sFalse, string sTrue)
        {
            m_listDI = tool.p_listDI;
            m_listDO = tool.p_listDO;
            m_id = id;
            m_log = log;
            p_bEnableRun = bEnableRun;
            m_asID.Add(sFalse);
            m_asID.Add(sTrue);
            m_aBitDIA.Add(new BitDI());
            m_aBitDIA.Add(new BitDI());
            m_aBitDIB.Add(new BitDI());
            m_aBitDIB.Add(new BitDI());
            tool.p_listIDIO.Add(this);

            InitBackgroundWorker();
        }

        public string RunTree(Tree tree)
        {
            string sDIA0 = RunTreeDI(tree, m_aBitDIA, "InputA", 0);
            string sDIB0 = RunTreeDI(tree, m_aBitDIB, "InputB", 0);
            string sDIA1 = RunTreeDI(tree, m_aBitDIA, "InputA", 1);
            string sDIB1 = RunTreeDI(tree, m_aBitDIB, "InputB", 1);
            string sDO = RunTreeDO(tree);
            if (sDIA0 != "OK") return sDIA0;
            if (sDIB0 != "OK") return sDIB0;
            if (sDIA1 != "OK") return sDIA1;
            if (sDIB1 != "OK") return sDIB1;
            if (sDO != "OK") return sDO;
            m_secTimeout = tree.Set(m_secTimeout, m_secTimeout, "Timeout", "DIO WaitDone Timeout (sec)");
            m_eRun = (eRun)tree.Set(m_eRun, eRun.Nothing, "Run", "DIO Run Mode", p_bEnableRun);
            return "OK";
        }

        string RunTreeDI(Tree tree, List<BitDI> aBitDI, string sID, int nIndex)
        {
            int nDI = tree.Set(aBitDI[nIndex].m_nID, -1, sID + m_asID[nIndex], "DIO Input Number");
            if (nDI != aBitDI[nIndex].m_nID)
            {
                if (IsUsedDI(nDI)) return "Can't Assign Exist DIO_I Input : " + m_id;
                aBitDI[nIndex].SetID(m_log, "Input");
                if (nDI < 0) aBitDI[nIndex] = new BitDI();
                else
                {
                    aBitDI[nIndex] = m_listDI.m_aDIO[nDI];
                    aBitDI[nIndex].SetID(m_log, m_id + "." + m_asID[nIndex]);
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
        public void Write(bool bOn)
        {
            p_bOut = bOn;
            m_bitDO.Write(bOn);
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
            p_bRepeat = (m_eRun == eRun.Repeat);
        }

        #region Repeat
        bool _bRepeat = false;
        bool p_bRepeat
        {
            get { return _bRepeat; }
            set
            {
                if (_bRepeat == value) return;
                if (value && !m_bgwRepeat.IsBusy) m_bgwRepeat.RunWorkerAsync();
            }
        }

        BackgroundWorker m_bgwRepeat = new BackgroundWorker();
        void InitBackgroundWorker()
        {
            m_bgwRepeat.DoWork += M_bgwRepeat_DoWork;
        }

        private void M_bgwRepeat_DoWork(object sender, DoWorkEventArgs e)
        {
            RunSol(true);
            RunSol(false);
            if (p_bRepeat == false) return;
        }
        #endregion
    }
}
