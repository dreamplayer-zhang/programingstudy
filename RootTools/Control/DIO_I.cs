using RootTools.Trees;

namespace RootTools.Control
{
    public class DIO_I : IDIO
    {
        public BitDI m_bitDI = new BitDI();

        bool m_bReverse = false; 

        public bool p_bIn
        {
            get { return (m_bitDI.p_bOn != m_bReverse); }
        }


        ListDIO m_listDI; 
        public string m_id;
        LogWriter m_log;
        public DIO_I(IToolDIO tool, string id, LogWriter log, bool bEnableRun)
        {
            m_listDI = tool.p_listDI;
            m_id = id;
            m_log = log;
            p_bEnableRun = bEnableRun; 
            tool.p_listIDIO.Add(this); 
        }

        public string RunTree(Tree tree)
        {
            int nDI = tree.Set(m_bitDI.m_nID, -1, "Input", "DIO Input Number");
            if (nDI != m_bitDI.m_nID)
            {
                if (IsUsed(nDI)) return "Can't Assign Exist DIO_I Input : " + m_id;
                m_bitDI.SetID(m_log, "Input");
                if (nDI < 0) m_bitDI = new BitDI(); 
                else
                {
                    m_bitDI = m_listDI.m_aDIO[nDI];
                    m_bitDI.SetID(m_log, m_id);
                }
            }
            m_bReverse = tree.Set(m_bReverse, m_bReverse, "Reverse", "Reverse DI Input"); 
            return "OK";
        }

        bool IsUsed(int nDI)
        {
            if (nDI < 0) return true;
            if (nDI >= m_listDI.m_aDIO.Count) return true;
            return (m_listDI.m_aDIO[nDI].p_sID != "Input"); 
        }

        public bool p_bEnableRun { get; set; }

        public void RunDIO()
        {
        }
    }
}
