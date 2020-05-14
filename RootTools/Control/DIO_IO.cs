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
        string m_id;
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

        string m_sTreeInfo = "OK";
        public string RunTree(Tree tree)
        {
            int nDI = tree.Set(m_bitDI.m_nID, -1, "Input", "DIO Input Number");
            int nDO = tree.Set(m_bitDO.m_nID, -1, "Output", "DIO Output Number");
            m_sTreeInfo = "OK";
            m_bitDI = GetBit(m_listDI, m_bitDI, nDI);
            m_bitDO = (BitDO)GetBit(m_listDO, m_bitDO, nDO);
            m_eRun = (eRun)tree.Set(m_eRun, eRun.Nothing, "Run", "DIO Run Mode", p_bEnableRun);
            m_msRepeat = tree.Set(m_msRepeat, 1000, "Repeat", "Repeat Toggle Period (ms)", m_eRun == eRun.Repeat);
            return m_sTreeInfo;
        }

        BitDI GetBit(ListDIO listDIO, BitDI bitDIO, int nDIO)
        {
            if (nDIO == bitDIO.m_nID) return bitDIO;
            if (listDIO.m_aDIO[nDIO].p_sID != listDIO.m_eDIO.ToString())
            {
                m_sTreeInfo = "Can't Assign Exist DIO_IO Input : " + m_id;
                return bitDIO;
            }
            bitDIO.SetID(m_log, listDIO.m_eDIO.ToString());
            listDIO.m_aDIO[nDIO].SetID(m_log, m_id);
            return listDIO.m_aDIO[nDIO];
        }

        public void Write(bool bOn)
        {
            m_bitDO.Write(bOn);
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
