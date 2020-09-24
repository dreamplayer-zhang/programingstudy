using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RootTools.Control
{
    public class DIO_Is : IDIO
    {
        List<string> m_asDI = new List<string>();
        public List<BitDI> m_aBitDI = new List<BitDI>();

        bool m_bReverse = false;

        public bool ReadDI(Enum eDI)
        {
            return ReadDI(eDI.ToString());
        }

        public bool ReadDI(string sDI)
        {
            int nIndex = GetIndex(sDI);
            if (nIndex < 0) return false;
            return m_aBitDI[nIndex].p_bOn != m_bReverse;
        }

        int GetIndex(string sID)
        {
            for (int n = 0; n < m_asDI.Count; n++)
            {
                if (sID == m_asDI[n]) return n;
            }
            return -1;
        }

        public bool ReadDI(int nDI)
        {
            if (nDI < 0) return false;
            if (nDI >= m_aBitDI.Count) return false; 
            return m_aBitDI[nDI].p_bOn != m_bReverse;
        }

        ListDIO m_listDI;
        string m_id;
        Log m_log;
        int m_nContinuos = 0; 
        public DIO_Is(IToolDIO tool, string id, Log log, bool bEnableRun, string[] asDI)
        {
            m_listDI = tool.p_listDI;
            m_id = id;
            m_log = log;
            for (int n = 0; n < asDI.Length; n++)
            {
                m_asDI.Add(asDI[n]);
                m_aBitDI.Add(new BitDI());
            }
            tool.p_listIDIO.Add(this);
            m_nContinuos = 0;
            p_bEnableRun = bEnableRun;
        }

        string m_sDI = "Input"; 
        public DIO_Is(IToolDIO tool, string id, Log log, bool bEnableRun, string sDI, int nCount)
        {
            m_listDI = tool.p_listDI;
            m_id = id;
            m_log = log;
            m_sDI = sDI; 
            for (int n = 0; n < nCount; n++)
            {
                m_asDI.Add(sDI + n.ToString("000"));
                m_aBitDI.Add(new BitDI());
            }
            tool.p_listIDIO.Add(this);
            m_nContinuos = nCount;
            p_bEnableRun = bEnableRun;
        }

        public string RunTree(Tree tree)
        {
            if (m_nContinuos > 1)
            {
                int nDI = tree.Set(m_aBitDI[0].m_nID, -1, "Input." + m_asDI[0], "DIO Input Number");
                if (nDI != m_aBitDI[0].m_nID)
                {
                    for (int n = 0; n < m_nContinuos; n++)
                    {
                        if (IsUsedDI(nDI + n)) return "Can't Assign Exist DIO_I Input : " + m_id;
                    }
                    for (int n = 0; n < m_nContinuos; n++)
                    {
                        m_aBitDI[n].SetID(m_log, "Input");
                        if (nDI < 0) m_aBitDI[n] = new BitDI();
                        else
                        {
                            m_aBitDI[n] = m_listDI.m_aDIO[nDI + n];
                            m_aBitDI[n].SetID(m_log, m_id + "." + m_sDI + n.ToString("000"));
                        }
                    }
                }
            }
            else
            {
                for (int n = 0; n < m_asDI.Count; n++)
                {
                    int nDI = tree.Set(m_aBitDI[n].m_nID, -1, "Input." + m_asDI[n], "DIO Input Number");
                    if (nDI != m_aBitDI[n].m_nID)
                    {
                        if (IsUsedDI(nDI)) return "Can't Assign Exist DIO_I Input : " + m_id;
                        m_aBitDI[n].SetID(m_log, "Input");
                        if (nDI < 0) m_aBitDI[n] = new BitDI();
                        else
                        {
                            m_aBitDI[n] = m_listDI.m_aDIO[nDI];
                            m_aBitDI[n].SetID(m_log, m_id + "." + m_asDI[n]);
                        }
                    }
                }
            }
            m_bReverse = tree.Set(m_bReverse, m_bReverse, "Reverse", "Reverse DI Input");
            return "OK";

        }

        bool IsUsedDI(int nDI)
        {
            if (nDI < 0) return false;
            if (nDI >= m_listDI.m_aDIO.Count) return true;
            return (m_listDI.m_aDIO[nDI].p_sID != "Input");
        }

        public bool p_bEnableRun { get; set; }

        public void RunDIO() { }
    }
}
