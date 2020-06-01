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
                if (IsUsed(nDO)) return "Can't Assign Exist DIO_O Output : " + m_id;
                m_bitDO.SetID(m_log, "Output");
                if (nDO < 0) m_bitDO = new BitDO();
                else
                {
                    m_bitDO = (BitDO)m_listDO.m_aDIO[nDO];
                    m_bitDO.SetID(m_log, m_id);
                }
            }
            m_eRun = (eRun)tree.Set(m_eRun, eRun.Nothing, "Run", "DIO Run Mode", p_bEnableRun);
            m_msRepeat = tree.Set(m_msRepeat, 1000, "Repeat", "Repeat Toggle Period (ms)", m_eRun == eRun.Repeat); 
            return "OK";
        }

        bool IsUsed(int nDO)
        {
            if (nDO < 0) return false;
            if (nDO >= m_listDO.m_aDIO.Count) return true;
            return (m_listDO.m_aDIO[nDO].p_sID != "Output");
        }

        public void Write(bool bOn)
        {
            m_bitDO.Write(bOn); 
        }

        int m_msDelay = 100; 
        public void DelayOff(int msDelay)
        {
            m_msDelay = msDelay;
            m_swRun.Restart();
            m_eRun = eRun.DelayOff;
            m_bitDO.Write(true);
        }

        public bool p_bEnableRun { get; set; }

        enum eRun
        {
            Nothing,
            Repeat,
            DelayOff,
        };
        eRun m_eRun = eRun.Nothing;
        public void RunDIO()
        {
            switch (m_eRun)
            {
                case eRun.Repeat: Repeat(); return;
                case eRun.DelayOff:
                    if (m_swRun.ElapsedMilliseconds < m_msDelay) return;
                    m_eRun = eRun.Nothing;
                    Write(false);
                    break; 
            }
        }

        StopWatch m_swRun = new StopWatch();
        int m_msRepeat = 1000;
        void Repeat()
        {
            if (m_swRun.ElapsedMilliseconds < m_msRepeat) return;
            Write(!p_bOut);
            m_swRun.Restart();
        }
    }
}
