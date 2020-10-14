using Root_ASIS.AOI;
using RootTools;
using RootTools.Memory;
using RootTools.Trees;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Root_ASIS.Teachs
{
    public class Teach
    {
        #region List AOI 
        AOIStrip m_aoiStrip;
        AOIStripID m_aoiStripID;
        List<IAOI> _aAOI = new List<IAOI>();
        void InitListAOI()
        {
            m_aoiStrip = new AOIStrip("AOIStrip", m_log);
            m_aoiStripID = new AOIStripID("AOIStripID", m_log);

            _aAOI.Add(new AOI_Unit("AOI_Unit", m_log));
            InvalidateListAOI(); 
        }

        public ObservableCollection<IAOI> m_aListAOI = new ObservableCollection<IAOI>();
        void InvalidateListAOI()
        {
            m_aListAOI.Clear(); 
            foreach (IAOI aoi in _aAOI)
            {
                if (aoi.p_bEnable) m_aListAOI.Add(aoi);
            }
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
        public ObservableCollection<IAOI> m_aAOI = new ObservableCollection<IAOI>(); 
        void ClearAOI()
        {
            m_aAOI.Clear();
        }

        void RunTreeAOI(Tree tree)
        {
            m_aoiStrip.RunTree(tree.GetTree("AOIStrip"));
            m_aoiStripID.RunTree(tree.GetTree("AOIStripID"));
            for (int n = 0; n < m_aAOI.Count; n++)
            {
                m_aAOI[n].p_nID = n; 
                m_aAOI[n].RunTree(tree.GetTree(n, m_aAOI[n].p_id));
            }
        }
        #endregion

        #region Memory
        public MemoryPool m_memoryPool; 
        #endregion

        #region Tree Setup
        public TreeRoot m_treeRootSetup; 
        void InitTreeSetup()
        {
            m_treeRootSetup = new TreeRoot(m_id + ".Setup", m_log);
            RunTreeSetup(Tree.eMode.RegRead);
            m_treeRootSetup.UpdateTree += M_treeRootSetup_UpdateTree;
        }

        private void M_treeRootSetup_UpdateTree()
        {
            RunTreeSetup(Tree.eMode.Update);
            RunTreeSetup(Tree.eMode.RegWrite);
            InvalidateListAOI(); 
        }

        public void RunTreeSetup(Tree.eMode eMode)
        {
            m_treeRootSetup.p_eMode = eMode;
            RunTreeAOIEnable(m_treeRootSetup.GetTree("AOI Enable")); 
        }
        #endregion

        #region Tree AOI
        public TreeRoot m_treeRootAOI;
        void InitTreeAOI()
        {
            m_treeRootAOI = new TreeRoot(m_id + ".AOI", m_log);
            m_treeRootAOI.UpdateTree += M_treeRootAOI_UpdateTree;
        }

        private void M_treeRootAOI_UpdateTree()
        {
            RunTreeAOI(Tree.eMode.Update);
            RunTreeAOI(Tree.eMode.RegWrite);
        }

        public void RunTreeAOI(Tree.eMode eMode)
        {
            m_treeRootAOI.p_eMode = eMode;
            RunTreeAOI(m_treeRootAOI);
        }
        #endregion

        public string m_id;
        Log m_log; 
        public Teach(string id, MemoryPool memoryPool)
        {
            m_id = id;
            m_memoryPool = memoryPool; 
            m_log = LogView.GetLog(id);
            InitListAOI();
            InitTreeSetup();
            ClearAOI();
            InitTreeAOI();
        }
    }
}
