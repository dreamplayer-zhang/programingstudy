using Root_ASIS.AOI;
using RootTools;
using RootTools.Memory;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Windows.Threading;

namespace Root_ASIS.Teachs
{
    public class Teach : NotifyProperty
    {
        #region List AOI 
        AOIStrip m_aoiStrip;
        /// <summary> 사용가능한 AOI List </summary>
        List<IAOI> _aAOI = new List<IAOI>();
        void InitListAOI()
        {
            m_aoiStrip = new AOIStrip("AOIStrip", m_log);
            InitLIstAOIArray();

            _aAOI.Add(new AOI_StripID("AOI_StripID", m_log));
            _aAOI.Add(new AOI_Unit("AOI_Unit", m_log));
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

        #region AOI Array
        List<IArray> _aAOIArray = new List<IArray>(); 
        void InitLIstAOIArray()
        {
            _aAOIArray.Add(new AOIArray("AOIArray", m_nID, m_log));
        }


        IArray p_aoiArray
        {
            get 
            {
                foreach (IArray aoi in _aAOIArray) if (aoi.p_id == m_sAOIArray) return aoi;
                return _aAOIArray[0]; 
            }
        }

        string m_sAOIArray = "AOIArray";
        void RunTreeArray(Tree tree)
        {
            List<string> asAOIArray = new List<string>();
            foreach (IAOI aoi in _aAOIArray) asAOIArray.Add(aoi.p_id);
            m_sAOIArray = tree.Set(m_sAOIArray, m_sAOIArray, asAOIArray, "Array", "Select AOI Array");
            p_aoiArray.RunTreeArray(tree.GetTree(p_aoiArray.p_id));
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
            p_roiActive.LBD(bDown, cpImg, m_memoryPool.m_viewer.p_memoryData);
            Draw();
            GetActiveROI();
        }

        private void M_viewer_OnMouseMove(CPoint cpImg)
        {
            if (p_roiActive == null) return;
            p_roiActive.MouseMove(cpImg);
            Draw();
        }

        AOIData.eDraw _eDraw = AOIData.eDraw.ROI;
        public AOIData.eDraw p_eDraw
        {
            get { return _eDraw; }
            set
            {
                if (_eDraw == value) return;
                _eDraw = value;
                OnPropertyChanged();
                Draw();
            }
        }

        public void Draw()
        {
            MemoryDraw draw = m_memoryPool.m_viewer.p_memoryData.m_aDraw[0];
            draw.Clear();
            m_aoiStrip.Draw(draw, p_eDraw); 
            p_aoiArray.Draw(draw, p_eDraw);
            foreach (IAOI aoi in p_aAOI) aoi.Draw(draw, p_eDraw);
            draw.InvalidDraw();
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
            p_aoiArray.InvalidROI(); 
            p_aROI.Add(p_aoiArray);
            foreach (IAOI aoi in p_aAOI) p_aROI.Add(aoi); 
        }

        void RunTreeAOI(Tree tree)
        {
            m_aoiStrip.RunTreeAOI(tree.GetTree("AOIStrip"));
            p_aoiArray.RunTreeAOI(tree.GetTree(p_aoiArray.p_id, true, false)); 
            for (int n = 0; n < p_aAOI.Count; n++)
            {
                p_aAOI[n].p_nID = n;
                p_aAOI[n].p_id = n.ToString("00") + "." + p_aAOI[n].p_sAOI; 
                p_aAOI[n].RunTreeAOI(tree.GetTree(p_aAOI[n].p_id));
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

        public void ReAllocate()
        {
            p_aoiArray.ReAllocate(m_memoryPool.m_viewer.p_memoryData);
            foreach (IAOI aoi in p_aAOI) aoi.ReAllocate(p_aoiArray.p_aArray); 
        }
        #endregion

        #region Inspect
        static InfoStrip m_defaultStrip = new InfoStrip(0);
        enum eTimer
        {
            Before,
            After,
            Stop
        }
        eTimer m_eTimer = eTimer.Stop;
        InfoStrip m_inspectStrip = null;
        MemoryData m_inspectMemory = null; 
        public string StartInspect(InfoStrip infoStrip)
        {
            if (infoStrip == null)
            {
                if (m_nID == 0) m_defaultStrip = new InfoStrip(0); 
                infoStrip = m_defaultStrip;
            }
            m_inspectStrip = infoStrip; 
            m_inspectMemory = m_memoryPool.m_viewer.p_memoryData;
            m_bgwInspect.RunWorkerAsync(); 
            return "OK";
        }

        public bool IsBusy()
        {
            return m_bgwInspect.IsBusy; 
        }

        public string WaitReady()
        {
            while (IsBusy())
            {
                Thread.Sleep(10);
                if (EQ.IsStop()) return "EQ Stop"; 
            }
            return "OK"; 
        }

        BackgroundWorker m_bgwInspect = new BackgroundWorker(); 
        void InitBackgroundWorker()
        {
            m_bgwInspect.DoWork += M_bgwInspect_DoWork;
            m_bgwInspect.RunWorkerCompleted += M_bgwInspect_RunWorkerCompleted;
        }

        private void M_bgwInspect_DoWork(object sender, DoWorkEventArgs e)
        {
            RunTimerInspect(eTimer.Before);
            m_aoiStrip.Inspect(m_inspectStrip, m_inspectMemory);
            foreach (IAOI aoi in p_aAOI) aoi.Inspect(m_inspectStrip, m_inspectMemory);
            RunTimerInspect(eTimer.After);
        }

        private void M_bgwInspect_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Draw(); 
        }

        DispatcherTimer m_timer = new DispatcherTimer(); 
        void InitTimer()
        {
            m_timer.Interval = TimeSpan.FromMilliseconds(1);
            m_timer.Tick += M_timer_Tick;
        }

        void RunTimerInspect(eTimer eTimer)
        {
            m_eTimer = eTimer;
            m_timer.Start();
            while ((m_eTimer != eTimer.Stop) && (EQ.IsStop() == false)) Thread.Sleep(1); 
        }

        private void M_timer_Tick(object sender, EventArgs e)
        {
            m_timer.Stop(); 
            switch (m_eTimer)
            {
                case eTimer.Before:
                    foreach (IAOI aoi in p_aAOI) aoi.BeforeInspect(m_inspectStrip, m_inspectMemory);
                    break;
                case eTimer.After:
                    foreach (IAOI aoi in p_aAOI) aoi.AfterInspect(m_inspectStrip, m_inspectMemory);
                    break;
            }
            m_eTimer = eTimer.Stop; 
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
                string[] asAOI = sAOI.Split('.'); 
                IAOI aoi = NewAOI(asAOI[1]);
                if (aoi != null) p_aAOI.Add(aoi);
            }
            RunTreeAOI(Tree.eMode.JobOpen);
            job.Close();
            Draw();
            InvalidROI();
            m_aoiStrip.Setup(m_memoryPool.m_viewer.p_memoryData); 
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
            RunTreeROI(Tree.eMode.Update);
            RunTreeROI(Tree.eMode.RegWrite);
        }

        public void RunTreeROI(Tree.eMode eMode)
        {
            m_treeRootROI.p_eMode = eMode;
            RunTreeArray(m_treeRootROI.GetTree("Array"));
        }
        #endregion

        public string m_id;
        int m_nID; 
        Log m_log; 
        public Teach(string id, int nID, MemoryPool memoryPool)
        {
            p_aAOI = new ObservableCollection<IAOI>();
            p_aROI = new ObservableCollection<IAOI>();
            m_id = id;
            m_nID = nID; 
            m_memoryPool = memoryPool; 
            m_log = LogView.GetLog(id);
            InitTreeSetup();
            InitListAOI();
            RunTreeSetup(Tree.eMode.RegRead);
            InvalidateListAOI();
            InitTreeROI();
            ClearAOI();
            InitTreeAOI();
            InitDraw();
            GetActiveROI();
            InitBackgroundWorker(); 
            InitTimer(); 
        }
    }
}
