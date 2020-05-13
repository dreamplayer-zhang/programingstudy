using RootTools.Trees;
using System.Diagnostics;

namespace RootTools.Control
{
    public class DIO_O : IDIO
    {
        public BitDO m_bitDO = new BitDO();

        public bool p_bOut
        {
            get { return m_bitDO.p_bOn; }
        }

        ListDIO m_listDO;
        public string m_id;
        Log m_log;
        public DIO_O(IToolDIO tool, string id, Log log, bool bEnableRun)
        {
            m_listDO = tool.p_listDO;
            m_id = id;
            m_log = log;
            p_bEnableRun = bEnableRun;
            tool.p_listIDIO.Add(this);
        }

        public string RunTree(Tree tree)
        {
            int nDO = tree.Set(m_bitDO.m_nID, -1, "Output", "DIO Output Number");
            if (nDO != m_bitDO.m_nID)
            {
                if (m_listDO.m_aDIO[nDO].p_sID != "Output") return "Can't Assign Exist DIO_O Output : " + m_id;
                m_bitDO.SetID(m_log, "Output"); 
                m_bitDO = (BitDO)m_listDO.m_aDIO[nDO];
                m_bitDO.SetID(m_log, m_id);
            }
            m_eRun = (eRun)tree.Set(m_eRun, eRun.Nothing, "Run", "DIO Run Mode", p_bEnableRun);
            m_msRepeat = tree.Set(m_msRepeat, 1000, "Repeat", "Repeat Toggle Period (ms)", m_eRun == eRun.Repeat); 
            return "OK";
        }

        public void Write(bool bOn)
        {
            m_bitDO.Write(bOn); 
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
            Write(!p_bOut);
            m_swRepeat.Restart();
        }
    }
}
