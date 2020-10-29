﻿using Root_ASIS.AOI;
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
        AOIArray m_aoiArray;
        /// <summary> 사용가능한 AOI List </summary>
        List<IAOI> _aAOI = new List<IAOI>();
        void InitListAOI()
        {
            m_aoiStrip = new AOIStrip("AOIStrip", m_log);
            m_aoiArray = new AOIArray("AOIArray", m_log); 

            _aAOI.Add(new AOI_Unit("AOI_Unit", m_log));
            InvalidateListAOI(); 
        }

        /// <summary> 활성화된 AOI List </summary>
        public ObservableCollection<IAOI> m_aEnableAOI = new ObservableCollection<IAOI>();
        void InvalidateListAOI()
        {
            m_aEnableAOI.Clear();
            foreach (IAOI aoi in _aAOI)
            {
                if (aoi.p_bEnable) m_aEnableAOI.Add(aoi);
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
        /// <summary> Inspect를 위한 AOI List, AOI_Strip 제외 </summary>
        public ObservableCollection<IAOI> p_aAOI { get; set; }
        /// <summary> Inspect를 위한 AOI List, AOI_Strip 포함 </summary>
        public List<IAOI> m_aAOI = new List<IAOI>(); 
        void ClearAOI()
        {
            p_aAOI.Clear();
            m_aAOI.Clear();
            m_aAOI.Add(m_aoiStrip);
            m_aAOI.Add(m_aoiArray); 
        }

        void RunTreeAOI(Tree tree)
        {
            m_aoiStrip.RunTreeAOI(tree.GetTree("AOIStrip"));
            m_aoiArray.RunTreeAOI(tree.GetTree("AOIArray", true, false)); 
            for (int n = 0; n < p_aAOI.Count; n++)
            {
                p_aAOI[n].p_nID = n; 
                p_aAOI[n].RunTreeAOI(tree.GetTree(n, p_aAOI[n].p_id));
            }
        }
        #endregion

        #region ROI
        public Dictionary<AOIData.eROI, int> m_nROI = new Dictionary<AOIData.eROI, int>();
        public int m_nROIReady = 0;
        public int m_nROIActive = 0; 

        public AOIData p_roiActive
        {
            get { return GetActineROI(); }
        }

        AOIData GetActineROI()
        {
            CalcROICount();
            if (m_nROIActive == 1)
            {
                foreach (IAOI aoi in m_aAOI)
                {
                    AOIData active = aoi.GetAOIData(AOIData.eROI.Active);
                    if (active != null) return active; 
                }
            }
            if (m_nROIActive > 1) ClearActive();
            foreach (IAOI aoi in m_aAOI)
            {
                AOIData active = aoi.GetAOIData(AOIData.eROI.Ready);
                if (active != null)
                {
                    active.p_eROI = AOIData.eROI.Active;
                    RunTreeROI(Tree.eMode.Init); 
                    return active;
                }
            }
            return null;
        }

        void CalcROICount()
        {
            m_nROIReady = 0;
            m_nROIActive = 0;
            foreach (IAOI aoi in m_aAOI) aoi.CalcROICount(ref m_nROIReady, ref m_nROIActive);
        }

        public void ClearActive()
        {
            foreach (IAOI aoi in m_aAOI) aoi.ClearActive(); 
        }

        void RunTreeROI(Tree tree)
        {
            m_aoiStrip.RunTreeROI(tree.GetTree("AOIStrip"));
            m_aoiArray.RunTreeROI(tree.GetTree("AOIArray"));
            for (int n = 0; n < p_aAOI.Count; n++)
            {
                p_aAOI[n].p_nID = n;
                p_aAOI[n].RunTreeROI(tree.GetTree(n, p_aAOI[n].p_id));
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
            if (p_roiActive == null) return;
            p_roiActive.LBD(bDown, cpImg);
            Draw(AOIData.eDraw.ROI);
            GetActineROI(); 
        }

        private void M_viewer_OnMouseMove(CPoint cpImg)
        {
            if (p_roiActive == null) return;
            p_roiActive.MouseMove(cpImg);
            Draw(AOIData.eDraw.ROI);
        }

        public void Draw(AOIData.eDraw eDraw)
        {
            MemoryDraw draw = m_memoryPool.m_viewer.p_memoryData.m_aDraw[0];
            draw.Clear();
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

        #region Tree ROI
        public TreeRoot m_treeRootROI;
        void InitTreeROI()
        {
            m_treeRootROI = new TreeRoot(m_id + ".ROI", m_log);
            m_treeRootROI.UpdateTree += M_treeRootROI_UpdateTree;
        }

        private void M_treeRootROI_UpdateTree()
        {
            AOIData aoiActive = p_roiActive;
            RunTreeROI(Tree.eMode.Update);
            CalcROICount();
            if (m_nROIActive > 1) aoiActive.p_eROI = AOIData.eROI.Ready;
            RunTreeROI(Tree.eMode.RegWrite);
            RunTreeROI(Tree.eMode.Init);
        }

        public void RunTreeROI(Tree.eMode eMode)
        {
            m_treeRootROI.p_eMode = eMode;
            RunTreeROI(m_treeRootROI);
        }
        #endregion

        public string m_id;
        Log m_log; 
        public Teach(string id, MemoryPool memoryPool)
        {
            p_aAOI = new ObservableCollection<IAOI>();
            m_id = id;
            m_memoryPool = memoryPool; 
            m_log = LogView.GetLog(id);
            InitListAOI();
            InitTreeSetup();
            ClearAOI();
            InitTreeAOI();
            InitTreeROI();
            InitDraw();
            GetActineROI(); 
        }
    }
}
