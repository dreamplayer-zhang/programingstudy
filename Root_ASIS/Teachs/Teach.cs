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

        #region ROI
        public ObservableCollection<AOIData> m_aROI = new ObservableCollection<AOIData>();
        public AOIData m_roiActive = null;
        public void InvalidROI()
        {
            m_aROI.Clear();
            m_aoiStrip.AddROI(m_aROI);
            m_aoiStripID.AddROI(m_aROI);
            foreach (IAOI aoi in m_aAOI) aoi.AddROI(m_aROI);
            CalcROICount();
            if (m_nROI[AOIData.eROI.Ready] == 0) return;
            if (m_nROI[AOIData.eROI.Active] == 1) return;
            if (m_nROI[AOIData.eROI.Active] > 1) ClearActive();
            SelectActive();
            Draw(AOIData.eDraw.ROI); //forget
        }

        public Dictionary<AOIData.eROI, int> m_nROI = new Dictionary<AOIData.eROI, int>();
        void InitROI()
        {
            m_nROI.Add(AOIData.eROI.Ready, 0);
            m_nROI.Add(AOIData.eROI.Active, 0);
            m_nROI.Add(AOIData.eROI.Done, 0);
            InvalidROI();
        }

        void CalcROICount()
        {
            m_nROI[AOIData.eROI.Ready] = 0;
            m_nROI[AOIData.eROI.Active] = 0;
            m_nROI[AOIData.eROI.Done] = 0;
            foreach (AOIData roi in m_aROI) m_nROI[roi.p_eROI]++;
        }

        public void ClearActive()
        {
            foreach (AOIData roi in m_aROI)
            {
                if (roi.p_eROI == AOIData.eROI.Active) roi.p_eROI = AOIData.eROI.Ready;
            }
            m_roiActive = null;
        }

        public void SelectActive()
        {
            foreach (AOIData roi in m_aROI)
            {
                if (roi.p_eROI == AOIData.eROI.Ready)
                {
                    roi.p_eROI = AOIData.eROI.Active;
                    m_roiActive = roi;
                    return;
                }
            }
        }
        #endregion

        #region Memory & Draw
        public MemoryPool m_memoryPool;
        void InitDraw()
        {
            m_memoryPool.m_viewer.OnLBD += M_viewer_OnLBD;
            m_memoryPool.m_viewer.OnMouseMove += M_viewer_OnMouseMove;
        }

        private void M_viewer_OnLBD(bool bDown, CPoint cpImg)
        {
            if (m_roiActive == null) return;
            m_roiActive.LBD(bDown, cpImg);
            InvalidROI(); 
        }

        private void M_viewer_OnMouseMove(CPoint cpImg)
        {
            if (m_roiActive == null) return;
            m_roiActive.MouseMove(cpImg);
            InvalidROI();
        }

        public void Draw(AOIData.eDraw eDraw)
        {
            MemoryDraw draw = m_memoryPool.m_viewer.p_memoryData.m_aDraw[0];
            draw.Clear();
            m_aoiStrip.Draw(draw, eDraw);
            m_aoiStripID.Draw(draw, eDraw);
            foreach (IAOI aoi in m_aAOI) aoi.Draw(draw, eDraw); 
            draw.InvalidDraw(); 
        }
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
            InitDraw();
            InitROI();
        }
    }
}
