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
        public ObservableCollection<IAOI> p_aROI { get; set; }
        void ClearAOI()
        {
            p_aAOI.Clear();
            InvalidROI(); 
        }

        public void InvalidROI()
        {
            p_aROI.Clear();
            p_aROI.Add(m_aoiStrip);
            p_aROI.Add(m_aoiArray);
            foreach (IAOI aoi in p_aAOI) p_aROI.Add(aoi); 
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
        public int m_nROIReady = 0;
        public int m_nROIActive = 0; 

        public AOIData p_roiActive
        {
            get { return GetActiveROI(); }
        }

        AOIData GetActiveROI()
        {
            CalcROICount();
            if (m_nROIActive == 1)
            {
                foreach (IAOI aoi in p_aROI)
                {
                    AOIData active = aoi.GetAOIData(AOIData.eROI.Active);
                    if (active != null) return active; 
                }
            }
            if (m_nROIActive > 1) ClearActive();
            foreach (IAOI aoi in p_aROI)
            {
                AOIData active = aoi.GetAOIData(AOIData.eROI.Ready);
                if (active != null)
                {
                    active.p_eROI = AOIData.eROI.Active;
                    return active;
                }
            }
            return null;
        }

        public void CalcROICount()
        {
            m_nROIReady = 0;
            m_nROIActive = 0;
            foreach (IAOI aoi in p_aROI) aoi.CalcROICount(ref m_nROIReady, ref m_nROIActive);
        }

        public void ClearActive()
        {
            foreach (IAOI aoi in p_aROI) aoi.ClearActive(); 
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
            GetActiveROI(); 
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
            foreach (IAOI aoi in p_aROI) aoi.Draw(draw, eDraw); 
            draw.InvalidDraw(); 
        }
        #endregion

        #region Recipe
        public void SaveRecipe(string sFile)
        {
            Job job = new Job(sFile, true, m_log);
            m_treeRootAOI.m_job = job;
            job.Set(m_id, "AOI Count", p_aAOI.Count); 
            for (int n = 0; n < p_aAOI.Count; n++)
            {
                string sKey = m_id + n.ToString("00");
                job.Set(sKey, "AOI", p_aAOI[n].p_id); 
            }
            RunTreeAOI(Tree.eMode.JobSave);
            job.Close();
        }

        public void OpenRecipe(string sFile)
        {
            Job job = new Job(sFile, false, m_log);
            ClearAOI(); 
            m_treeRootAOI.m_job = job;
            int nAOI = job.Set(m_id, "AOI Count", p_aAOI.Count); 
            for (int n = 0; n < nAOI; n++)
            {
                string sKey = m_id + n.ToString("00");
                string sAOI = job.Set(sKey, "AOI", "");
                IAOI aoi = NewAOI(sAOI); 
                if (aoi != null) p_aAOI.Add(aoi); 
            }
            RunTreeAOI(Tree.eMode.JobOpen);
            job.Close();
            Draw(AOIData.eDraw.Inspect);
        }

        IAOI NewAOI(string sAOI)
        {
            foreach (IAOI aoi in m_aEnableAOI)
            {
                if (aoi.p_id == sAOI) return aoi.NewAOI(); 
            }
            return null; 
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
            p_aAOI = new ObservableCollection<IAOI>();
            p_aROI = new ObservableCollection<IAOI>();
            m_id = id;
            m_memoryPool = memoryPool; 
            m_log = LogView.GetLog(id);
            InitListAOI();
            InitTreeSetup();
            ClearAOI();
            InitTreeAOI();
            InitDraw();
            GetActiveROI(); 
        }
    }
}
