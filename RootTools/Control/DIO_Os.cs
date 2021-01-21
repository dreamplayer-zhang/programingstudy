using RootTools.Trees;
using System;
using System.Collections.Generic;

namespace RootTools.Control
{
    public class DIO_Os : IDIO
    {
        List<string> m_asDO = new List<string>();
        public List<BitDO> m_aBitDO = new List<BitDO>(); 

        public bool ReadDO(Enum eDO)
        {
            return ReadDO(eDO.ToString()); 
        }

        public bool ReadDO(string sDO)
        {
            int nIndex = GetIndex(sDO); 
            if (nIndex < 0) return false;
            return m_aBitDO[nIndex].p_bOn;
        }

        int GetIndex(string sID)
        {
            for (int n = 0; n < m_asDO.Count; n++)
            {
                if (sID == m_asDO[n]) return n;
            }
            return -1;
        }

        ListDIO m_listDO;
        string m_id;
        Log m_log;
        public DIO_Os(IToolDIO tool, string id, Log log, bool bEnableRun, string[] asDO)
        {
            m_listDO = tool.p_listDO;
            m_id = id;
            m_log = log;
            p_bEnableRun = bEnableRun;
            for (int n = 0; n < asDO.Length; n++)
            {
                m_asDO.Add(asDO[n]);
                m_aBitDO.Add(new BitDO()); 
            }
            tool.p_listIDIO.Add(this);
        }

        public string RunTree(Tree tree)
        {
            for (int n = 0; n < m_asDO.Count; n++)
            {
                int nDO = tree.Set(m_aBitDO[n].m_nID, -1, "Output." + m_asDO[n], "DIO Output Number");
                if (nDO != m_aBitDO[n].m_nID)
                {
                    if (IsUsedDO(nDO)) return "Can't Assign Exist DIO_O Output : " + m_id;
                    m_aBitDO[n].SetID(m_log, "Output");
                    if (nDO < 0) m_aBitDO[n] = new BitDO();
                    else
                    {
                        m_aBitDO[n] = (BitDO)m_listDO.m_aDIO[nDO];
                        m_aBitDO[n].SetID(m_log, m_id + "." + m_asDO[n]);
                    }
                }
            }
            m_eRun = (eRun)tree.Set(m_eRun, eRun.Nothing, "Run", "DIO Run Mode", p_bEnableRun);
            m_msRepeat = tree.Set(m_msRepeat, 1000, "Repeat", "Repeat Toggle Period (ms)", m_eRun == eRun.Repeat);
            return "OK"; 
        }

        bool IsUsedDO(int nDO)
        {
            if (nDO < 0) return false;
            if (nDO >= m_listDO.m_aDIO.Count) return true;
            return (m_listDO.m_aDIO[nDO].p_sID != "Output");
        }

        public void AllOff()
        {
            foreach (BitDO bitDO in m_aBitDO) bitDO.Write(false); 
        }

        public void Write(Enum eDO, bool bOn)
        {
            Write(eDO.ToString(), bOn);
        }

        public void Write(Enum eDO)
        {
            ClearBut(eDO);
            Write(eDO.ToString(), true);
        }

        public void Write(string sDO, bool bOn)
        {
            int nIndex = GetIndex(sDO); 
            if (nIndex < 0) return;
            m_aBitDO[nIndex].Write(bOn); 
        }

        public bool p_bEnableRun { get; set; }

        enum eRun
        {
            Nothing,
            OneOutput,
            Repeat
        };
        eRun m_eRun = eRun.Nothing;
        List<bool> m_abDO = new List<bool>(); 
        public void RunDIO()
        {
            if (m_eRun == eRun.Nothing) return; 
            while (m_abDO.Count < m_asDO.Count) m_abDO.Add(false);
            switch (m_eRun)
            {
                case eRun.OneOutput:
                    for (int n = 0; n < m_asDO.Count; n++)
                    {
                        if (ReadDO(m_asDO[n]) && (m_abDO[n] == false)) ClearBut(n);
                    }
                    break;
                case eRun.Repeat: Repeat(); break;
            }
        }

        void ClearBut(int nIndex)
        {
            for (int n = 0; n < m_asDO.Count; n++)
            {
                if (n != nIndex) m_aBitDO[n].Write(false); 
            }
        }

        void ClearBut(Enum eDO)
        {
            string sDO = eDO.ToString(); 
            for (int n = 0; n < m_asDO.Count; n++)
            {
                if (sDO != m_asDO[n]) m_aBitDO[n].Write(false);
            }
        }

        StopWatch m_swRepeat = new StopWatch();
        int m_msRepeat = 1000;
        int m_iRepeat = 0; 
        void Repeat()
        {
            if (m_swRepeat.ElapsedMilliseconds < m_msRepeat) return;
            m_swRepeat.Restart();
            for (int n = 0; n < m_aBitDO.Count; n++) m_aBitDO[n].Write(n == m_iRepeat); 
            m_iRepeat = (m_iRepeat + 1) % m_aBitDO.Count; 
        }
    }
}
