using RootTools;
using System;
using System.Collections.Generic;

namespace Root_JEDI_Sorter.Module
{
    public enum eVision
    {
        CCS,
        Top3D,
        Top2D,
        Bottom,
    }

    public enum eResult
    {
        Good,
        Reject,
        Rework,
    }

    public class InfoChip : NotifyProperty
    {
        public eResult m_eResult; 
    }

    public class InfoTray : NotifyProperty
    {
        #region On Inspect
        Dictionary<eVision, bool> m_bInspect = new Dictionary<eVision, bool>();
        void InitInspect()
        {
            foreach (eVision eVision in Enum.GetValues(typeof(eVision))) m_bInspect.Add(eVision, false);
        }

        public void StartInspect(eVision eVision)
        {
            m_bInspect[eVision] = true;
        }

        public bool p_bInspect
        {
            get 
            {
                foreach (eVision eVision in Enum.GetValues(typeof(eVision)))
                {
                    if (m_bInspect[eVision]) return true; 
                }
                return false; 
            }
        }
        #endregion

        #region Chip Count
        public Dictionary<eResult, int> m_aCount = new Dictionary<eResult, int>(); 
        void InitCount()
        {
            foreach (eResult eResult in Enum.GetValues(typeof(eResult))) m_aCount.Add(eResult, 0); 
        }

        public int GetChipCount()
        {
            return m_aCount[eResult.Good] + m_aCount[eResult.Reject] + m_aCount[eResult.Rework]; 
        }
        #endregion

        public string m_sTrayIn = "";
        public string m_sTrayOut = "";
        public string p_id { get; set; }
        public InfoTray(string id)
        {
            p_id = id;
            m_sTrayIn = id; 
            InitInspect();
            InitCount(); 
        }
    }
}
