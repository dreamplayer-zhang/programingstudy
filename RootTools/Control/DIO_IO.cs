using RootTools.Trees;

namespace RootTools.Control
{
    public class DIO_IO : IDIO
    {
        public BitDI m_bitDI = new BitDI();
        public BitDO m_bitDO = new BitDO();

        public bool p_bIn
        {
            get { return m_bitDI.p_bOn; }
        }

        public bool p_bOut
        {
            get { return m_bitDO.p_bOn; }
        }

        ListDIO m_listDI;
        ListDIO m_listDO;
        public string m_id;
        Log m_log;
        public DIO_IO(IToolDIO tool, string id, Log log, bool bEnableRun)
        {
            m_listDI = tool.p_listDI;
            m_listDO = tool.p_listDO; 
            m_id = id;
            m_log = log;
            p_bEnableRun = bEnableRun;
            tool.p_listIDIO.Add(this);
        }

        public string RunTree(Tree tree)
        {
            string sDI = RunTreeDI(tree);
            string sDO = RunTreeDO(tree);
            if (sDI != "OK") return sDI;
            if (sDO != "OK") return sDO;
            m_eRun = (eRun)tree.Set(m_eRun, eRun.Nothing, "Run", "DIO Run Mode", p_bEnableRun);
            m_msRepeat = tree.Set(m_msRepeat, 1000, "Repeat", "Repeat Toggle Period (ms)", m_eRun == eRun.Repeat);
            return "OK";
        }

        string RunTreeDI(Tree tree)
        {
            int nDI = tree.Set(m_bitDI.m_nID, -1, "Input", "DIO Input Number");
            if (nDI != m_bitDI.m_nID)
            {
                if (IsUsedDI(nDI)) return "Can't Assign Exist DIO_I Input : " + m_id;
                m_bitDI.SetID(m_log, "Input");
                if (nDI < 0) m_bitDI = new BitDI();
                else
                {
                    m_bitDI = m_listDI.m_aDIO[nDI];
                    m_bitDI.SetID(m_log, m_id);
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
            int nDO = tree.Set(m_bitDO.m_nID, -1, "Output", "DIO Output Number");
            if (nDO != m_bitDO.m_nID)
            {
                if (IsUsedDO(nDO)) return "Can't Assign Exist DIO_O Output : " + m_id;
                m_bitDO.SetID(m_log, "Output");
                if (nDO < 0) m_bitDO = new BitDO();
                else
                {
                    m_bitDO = (BitDO)m_listDO.m_aDIO[nDO];
                    m_bitDO.SetID(m_log, m_id);
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
            m_bitDO.Write(bOn);
            m_swWrite.Start(); 
        }

        public bool p_bEnableRun { get; set; }

        enum eRun
        {
            Nothing,
            SyncIO,
            Reverse,
            Repeat
        };
        eRun m_eRun = eRun.Nothing; 
        public void RunDIO()
        {
            switch (m_eRun)
            {
                case eRun.SyncIO: if (p_bIn != p_bOut) Write(p_bIn); break;
                case eRun.Reverse: if (p_bIn == p_bOut) Write(!p_bIn); break;
                case eRun.Repeat: Repeat(); break; 
            }
        }

        StopWatch m_swRepeat = new StopWatch();
        int m_msRepeat = 1000;
        void Repeat()
        {
            if (m_swRepeat.ElapsedMilliseconds < m_msRepeat) return;
            Write(!p_bOut);
            m_swRepeat.Restart();
        }

    }
}
