using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RootTools.Control
{
    public class DIO_I4O2 : IDIO
    {
        List<string> m_asID = new List<string>();
        public List<BitDI> m_aBitDI = new List<BitDI>();
        public List<BitDO> m_aBitDO = new List<BitDO>();

        public bool p_bDone
        {
            get
            {
                if (p_bOut) return (IsOff() == false) && IsOn();
                else return IsOff() && (IsOn() == false);
            }
        }

        bool IsOff()
        {
            for (int n = 0; n < 2; n++)
            {
                if (m_aBitDI[n].p_bOn == false) return false;
            }
            return true;
        }

        bool IsOn()
        {
            for (int n = 2; n < 4; n++)
            {
                if (m_aBitDI[n].p_bOn == false) return false;
            }
            return true;
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
        public DIO_I4O2(IToolDIO tool, string id, Log log, bool bEnableRun, string sFalse, string sTrue)
        {
            m_listDI = tool.p_listDI;
            m_listDO = tool.p_listDO;
            m_id = id;
            m_log = log;
            p_bEnableRun = bEnableRun;
            m_asID.Add(sFalse);
            m_asID.Add(sTrue);
            for (int n = 0; n < 4; n++) m_aBitDI.Add(new BitDI());
            m_aBitDO.Add(new BitDO());
            m_aBitDO.Add(new BitDO());
            tool.p_listIDIO.Add(this);

            InitBackgroundWorker();
        }

        public string RunTree(Tree tree)
        {
            string[] sDI = new string[8];
            sDI[0] = RunTreeDI(tree, m_aBitDI[0], m_asID[0] + "0");
            sDI[1] = RunTreeDI(tree, m_aBitDI[1], m_asID[0] + "1");
            sDI[2] = RunTreeDI(tree, m_aBitDI[2], m_asID[1] + "0");
            sDI[3] = RunTreeDI(tree, m_aBitDI[3], m_asID[1] + "1");
            string sDO0 = RunTreeDO(tree, 0);
            string sDO1 = RunTreeDO(tree, 1);
            for (int n = 0; n < 4; n++)
            {
                if (sDI[n] != "OK") return sDI[n];
            }
            if (sDO0 != "OK") return sDO0;
            if (sDO1 != "OK") return sDO1;
            m_secTimeout = tree.Set(m_secTimeout, m_secTimeout, "Timeout", "DIO WaitDone Timeout (sec)");
            m_eRun = (eRun)tree.Set(m_eRun, eRun.Nothing, "Run", "DIO Run Mode", p_bEnableRun);
            return "OK";
        }

        string RunTreeDI(Tree tree, BitDI bitDI, string sID)
        {
            int nDI = tree.Set(bitDI.m_nID, -1, "Input." + sID, "DIO Input Number");
            if (nDI != bitDI.m_nID)
            {
                if (IsUsedDI(nDI)) return "Can't Assign Exist DIO_I Input : " + m_id;
                bitDI.SetID(m_log, "Input");
                if (nDI < 0) bitDI = new BitDI();
                else
                {
                    bitDI = m_listDI.m_aDIO[nDI];
                    bitDI.SetID(m_log, m_id + "." + sID);
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

        public StopWatch m_swWrite = new StopWatch();
        public void Write(bool bOn)
        {
            p_bOut = bOn;
            m_aBitDO[0].Write(!bOn);
            m_aBitDO[1].Write(bOn);
            m_swWrite.Start();
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
                    if (m_bDO[1, 1] && m_aBitDI[1].p_bOn) m_aBitDO[1].Write(false);
                    break;
            }
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
