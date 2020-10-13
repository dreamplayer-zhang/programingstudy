using Root_ASIS.AOI;
using RootTools;
using RootTools.Trees;
using System.Collections.Generic;

namespace Root_ASIS.Teach
{
    public class Teach
    {
        #region List AOI 
        AOIStrip m_aoiStrip;
        AOIStripID m_aoiStripID;
        List<IAOI> _aAOIDefault = new List<IAOI>();
        List<IAOI> _aAOI = new List<IAOI>();
        public List<string> m_asAOI = new List<string>(); 
        void InitListAOI()
        {
            m_aoiStrip = new AOIStrip("AOIStrip", m_log);
            _aAOIDefault.Add(m_aoiStrip);
            m_aoiStripID = new AOIStripID("AOIStripID", m_log);
            _aAOIDefault.Add(m_aoiStripID);

            _aAOI.Add(new AOI_Unit("AOI_Unit", m_log));
            foreach (IAOI aoi in _aAOI) m_asAOI.Add(aoi.p_id); 
        }

        void RunTreeAOIEnable(Tree tree)
        {
            foreach (IAOI aoi in _aAOI)
            {
                aoi.p_bEnable = tree.Set(aoi.p_bEnable, aoi.p_bEnable, aoi.p_id, "Enable AOI"); 
            }
        }
        #endregion

        #region AOI
        List<IAOI> m_aAOI = new List<IAOI>(); 
        void InitAOI()
        {
            m_aAOI.Clear();
            foreach (IAOI aoi in _aAOIDefault) m_aAOI.Add(aoi); 
        }
        #endregion

        public string m_id;
        Log m_log; 
        public Teach(string id)
        {
            m_id = id;
            m_log = LogView.GetLog(id); 
            InitListAOI(); 
        }
    }
}
