using Root_ASIS.AOI;
using Root_AxisMapping.Module;
using RootTools;
using RootTools.Memory;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace Root_AxisMapping.MainUI
{
    public class Mapping : NotifyProperty
    {
        #region Array
        int _xArray = 0; 
        public int p_xArray
        {
            get { return _xArray; }
            set
            {
                if (_xArray == value) return;
                _xArray = value;
                OnPropertyChanged(); 
            }
        }

        int _yArray = 0;
        public int p_yArray
        {
            get { return _yArray; }
            set
            {
                if (_yArray == value) return;
                _yArray = value;
                OnPropertyChanged();
            }
        }

        CPoint m_szArray = new CPoint(); 
        public Array[,] m_aArray = null;
        public void InitArray()
        {
            m_aArray = new Array[p_xArray, p_yArray];
            double xc = (p_xArray - 1.0) / 2;
            double yc = (p_yArray - 1.0) / 2;
            for (int x = 0; x < p_xArray; x++)
            {
                for (int y = 0; y < p_yArray; y++)
                {
                    m_aArray[x, y] = new Array(this, x);
                    double dx = (x - xc) / (xc + 0.3);
                    double dy = (y - yc) / (yc + 0.3);
                    double r = Math.Sqrt(dx * dx + dy * dy);
                    m_aArray[x, y].p_eState = (r < 1) ? Array.eState.Exist : Array.eState.Empty;
                }
            }
            m_aArray[m_xSetup, 0].OnSetup();
        }

        public int m_xActive = 0;
        public void OnActive(int ix)
        {
            m_xActive = ix;
            for (int x = 0; x < p_xArray; x++)
            {
                for (int y = 0; y < p_yArray; y++) m_aArray[x, y].ChangeBrush(false); 
            }
        }

        public int m_xSetup = 0; 
        public void OnSetup(int ix)
        {
            m_xSetup = ix; 
            for (int x = 0; x < p_xArray; x++)
            {
                for (int y = 0; y < p_yArray; y++)
                {
                    Array.eState eState = m_aArray[x, y].p_eState;
                    if (eState == Array.eState.Setup) m_aArray[x, y].p_eState = Array.eState.Exist;
                }
            }
            for (int y = 0; y < p_yArray; y++)
            {
                if (m_aArray[ix, y].p_eState == Array.eState.Exist) m_aArray[ix, y].p_eState = Array.eState.Setup; 
            }
        }
        #endregion

        #region Grab
        AxisMapping.Run_Grab m_runGrab;
        void InitRunGrab()
        {
            m_runGrab = (AxisMapping.Run_Grab)m_axisMapping.m_runGrab.Clone();
        }

        public string RunGrab()
        {
            AxisMapping.Run_Grab runGrab = (AxisMapping.Run_Grab)m_runGrab.Clone();
            runGrab.m_xStart += m_dx * (m_xActive - m_xSetup); 
            m_axisMapping.StartRun(runGrab);
            return "OK";
        }
        void RunTreeGrab(Tree tree)
        {
            m_runGrab.RunTree(tree, true);
        }
        #endregion

        #region Unit
        public class Unit
        {
            public AOIData m_aoiData;
            public string m_sInspect = "";

            public void RunTree(Tree tree)
            {
                m_sz = tree.Set(m_sz, m_sz, "szROI", "szROI", false);
                m_aoiData.RunTree(tree);
            }

            public string m_id;
            CPoint m_sz = new CPoint(100, 50);
            public Unit(string id)
            {
                m_id = id;
                m_aoiData = new AOIData(id, m_sz);
            }
        }
        Unit[] m_aUnit = new Unit[2];

        void InitUnit()
        {
            m_aUnit[0] = new Unit("Top");
            m_aUnit[1] = new Unit("Bottom");
        }

        void RunTreeUnit(Tree tree)
        {
            m_aUnit[0].RunTree(tree.GetTree(m_aUnit[0].m_id));
            m_aUnit[1].RunTree(tree.GetTree(m_aUnit[1].m_id));
        }
        #endregion

        #region ROI
        CPoint m_szROI = new CPoint(100, 100);
        public ObservableCollection<AOIData> m_aROI = new ObservableCollection<AOIData>();
        void InitROI()
        {
            m_aROI.Clear();
            m_aROI.Add(m_aUnit[0].m_aoiData);
            m_aROI.Add(m_aUnit[1].m_aoiData);
        }

        public AOIData p_roiActive
        {
            get { return GetActineROI(); }
        }

        AOIData GetActineROI()
        {
            foreach (AOIData roi in m_aROI)
            {
                if (roi.p_eROI == AOIData.eROI.Active) return roi;
            }
            foreach (AOIData roi in m_aROI)
            {
                if (roi.p_eROI == AOIData.eROI.Ready)
                {
                    roi.p_eROI = AOIData.eROI.Active;
                    return roi;
                }
            }
            return null;
        }

        public void InvalidROI()
        {
            if (m_xSetup != m_xActive) return; 
            m_aROI[0].p_eROI = AOIData.eROI.Active;
            m_aROI[1].p_eROI = AOIData.eROI.Ready;
        }

        public void ClearActive()
        {
            foreach (AOIData roi in m_aROI)
            {
                if (roi.p_eROI == AOIData.eROI.Active) roi.p_eROI = AOIData.eROI.Ready;
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
            switch (eDraw)
            {
                case AOIData.eDraw.ROI:
                    m_aUnit[0].m_aoiData.Draw(draw, eDraw);
                    m_aUnit[1].m_aoiData.Draw(draw, eDraw);
                    break;
                case AOIData.eDraw.Inspect:
                    foreach (AOIData aoi in m_aAOI)
                    {
                        if (aoi.m_bEnable) aoi.Draw(draw, eDraw); 
                    }
                    break; 
            }
            draw.InvalidDraw();
        }
        #endregion

        #region Inspect
        CPoint m_szAOI = new CPoint(); 
        List<AOIData> m_aAOI = new List<AOIData>(); 
        MemoryData m_memory;
        void InitInspect()
        {
            m_memory = m_memoryPool.m_viewer.p_memoryData;
            m_aAOI.Clear(); 
            m_szROI.X = Math.Max(m_aUnit[0].m_aoiData.m_sz.X, m_aUnit[1].m_aoiData.m_sz.X); 
            m_szROI.Y = Math.Max(m_aUnit[0].m_aoiData.m_sz.Y, m_aUnit[1].m_aoiData.m_sz.Y);
            int xp = Math.Min(m_aUnit[0].m_aoiData.m_cp0.X, m_aUnit[1].m_aoiData.m_cp0.X);
            int y0 = m_aUnit[0].m_aoiData.m_cp0.Y;
            int y1 = m_aUnit[1].m_aoiData.m_cp0.Y;
            int yMin = p_yArray;
            int yMax = 0; 
            for (int y = 0; y < p_yArray; y++)
            {
                if (m_aArray[m_xSetup, y].p_eState != Array.eState.Empty)
                {
                    if (yMin > y) yMin = y;
                    if (yMax < y) yMax = y;
                }
            }
            for (int y = 0; y < p_yArray; y++)
            {
                if (m_aAOI.Count <= y) m_aAOI.Add(new AOIData("AOI." + m_aAOI.Count.ToString(), m_szROI));
                m_aAOI[y].m_cp0 = new CPoint(xp, ((yMax - y) * y0 - (yMin - y) * y1) / (yMax - yMin));
                m_aAOI[y].m_bEnable = false;
            }
        }

        public string Inspect()
        {
            if (m_xActive == m_xSetup) InitInspect();
            for (int y = 0; y < p_yArray; y++)
            {
                m_aAOI[y].m_bEnable = (m_aArray[m_xActive, y].p_eState != Array.eState.Empty);
                if (m_aAOI[y].m_bEnable) InspectBlob(y);
                m_aAOI[y].m_bInspect = m_aAOI[y].m_bEnable;
                m_aArray[m_xActive, y].m_rpCenter = m_aAOI[y].m_bEnable ? m_aAOI[y].m_rpCenter : new RPoint(); 
            }
            Draw(AOIData.eDraw.Inspect);
            SaveResult(); 
            return "OK";
        }

        Blob m_blob = new Blob();
        Blob.eSort m_eSort = Blob.eSort.Size;
        public CPoint m_mmGV = new CPoint(200, 0);
        string InspectBlob(int iAOI)
        {
            AOIData aoi = m_aAOI[iAOI];
            m_blob.RunBlob(m_memory, 0, aoi.m_cp0, aoi.m_sz, m_mmGV.X, m_mmGV.Y, 10); 
            m_blob.RunSort(m_eSort);
            if (m_blob.m_aSort.Count == 0) return "Find Fiducial Error";
            Blob.Island island = m_blob.m_aSort[0];
            aoi.m_bInspect = true;
            aoi.m_rpCenter = island.m_rpCenter;
            aoi.m_sDisplay = "Size = " + island.m_nSize + ", " + island.m_sz.ToString();
            return "OK";
        }

        void SaveResult()
        {
            FileStream fs = new FileStream("c:\\Log\\Mapping" + m_xActive.ToString("00") + ".txt", FileMode.Create);
            StreamWriter sw = new StreamWriter(fs); 
            for (int y = 0; y < p_yArray; y++)
            {
                Array array = m_aArray[m_xActive, y];
                sw.WriteLine((array.p_eState != Array.eState.Empty).ToString() + ", " + array.m_rpCenter.ToString()); 
            }
            sw.Close();
            fs.Close(); 
        }

        public void InspectAll()
        {
            int xActive = m_xActive; 
            for (m_xActive = 0; m_xActive < p_xArray; m_xActive++)
            {
                RunGrab();
                Inspect(); 
            }
            m_xActive = xActive; 
        }

        double m_dx = 2.3;
        void RunTreeInspect(Tree tree)
        {
            m_dx = tree.Set(m_dx, m_dx, "dX", "dX (unit)"); 
            m_mmGV = tree.Set(m_mmGV, m_mmGV, "GV", "Gray Value Range (0~255)");
        }
        #endregion

        #region Tree Grab
        public TreeRoot m_treeRootGrab;
        void InitTreeGrab()
        {
            m_treeRootGrab = new TreeRoot(m_id, m_log);
            RunTreeGrab(Tree.eMode.RegRead);
            m_treeRootGrab.UpdateTree += M_treeRootGrab_UpdateTree;
        }

        private void M_treeRootGrab_UpdateTree()
        {
            RunTreeGrab(Tree.eMode.Update);
            RunTreeGrab(Tree.eMode.RegWrite);
        }

        public void RunTreeGrab(Tree.eMode eMode)
        {
            m_treeRootGrab.p_eMode = eMode;
            RunTreeGrab(m_treeRootGrab.GetTree("Grab"));
        }
        #endregion

        #region Tree Inspect
        public TreeRoot m_treeRootInspect;
        void InitTreeInspect()
        {
            m_treeRootInspect = new TreeRoot(m_id, m_log);
            RunTreeInspect(Tree.eMode.RegRead);
            m_treeRootInspect.UpdateTree += M_treeRootInspect_UpdateTree;
        }

        private void M_treeRootInspect_UpdateTree()
        {
            RunTreeInspect(Tree.eMode.Update);
            RunTreeInspect(Tree.eMode.RegWrite);
        }

        public void RunTreeInspect(Tree.eMode eMode)
        {
            m_treeRootInspect.p_eMode = eMode;
            RunTreeUnit(m_treeRootInspect.GetTree("Unit", false, false));
            RunTreeInspect(m_treeRootInspect.GetTree("Inspect"));
        }
        #endregion

        string m_id;
        AxisMapping_Engineer m_engineer;
        public AxisMapping m_axisMapping;
        Log m_log;
        public Mapping(string id, AxisMapping_Engineer engineer)
        {
            p_xArray = 15;
            p_yArray = 13;
            m_xActive = p_xArray / 2;
            m_xSetup = p_xArray / 2;
            m_id = id;
            m_engineer = engineer;
            m_axisMapping = ((AxisMapping_Handler)engineer.ClassHandler()).m_axisMapping;
            m_memoryPool = m_axisMapping.m_memoryPool;
            m_log = LogView.GetLog(id);
            InitUnit();
            InitRunGrab();
            InitTreeGrab();
            InitTreeInspect();
            InitDraw();
            InitROI();
            GetActineROI();
        }
    }
}
