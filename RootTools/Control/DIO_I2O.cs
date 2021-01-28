using RootTools.Trees;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

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

            InitBackgroundWorker(); 
        }

        public string RunTree(Tree tree)
        {
            string sDI0 = RunTreeDI(tree, 0);
            string sDI1 = RunTreeDI(tree, 1);
            string sDO = RunTreeDO(tree);
            if (sDI0 != "OK") return sDI0;
            if (sDI1 != "OK") return sDI1;
            if (sDO != "OK") return sDO;
            m_secTimeout = tree.Set(m_secTimeout, m_secTimeout, "Timeout", "DIO WaitDone Timeout (sec)");
            m_eRun = (eRun)tree.Set(m_eRun, eRun.Nothing, "Run", "DIO Run Mode", p_bEnableRun);
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
