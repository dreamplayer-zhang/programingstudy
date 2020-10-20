﻿using Root_ASIS.AOI;
using Root_ASIS.Teachs;
using Root_AxisMapping.Module;
using RootTools;
using RootTools.Memory;
using RootTools.Trees;
using System.Collections.ObjectModel;

namespace Root_AxisMapping
{
    public class Mapping
    {
        #region Unit
        public class Unit
        {
            public AOIData m_aoiData;
            public string m_sInspect = "";

            public void RunTree(Tree tree)
            {
                m_sz = tree.Set(m_sz, m_sz, "szROI", "szROI", false);
                m_aoiData.RunTree(tree, false);
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
            m_aUnit[0] = new Unit("Origin 0");
            m_aUnit[1] = new Unit("Origin 1");
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

        public void ClearActive()
        {
            foreach (AOIData roi in m_aROI)
            {
                if (roi.p_eROI == AOIData.eROI.Active) roi.p_eROI = AOIData.eROI.Ready;
            }
        }
        #endregion

        #region Inspect
        MemoryData m_memory;
        public string Inspect()
        {
            m_memory = m_memoryPool.m_viewer.p_memoryData;
            for (int n = 0; n < 2; n++)
            {
                m_aUnit[n].m_sInspect = InspectBlob(n);
                if (m_aUnit[n].m_sInspect != "OK") return m_aUnit[n].m_sInspect; 
            }
            return "OK";
        }

        Blob m_blob = new Blob();
        Blob.eSort m_eSort = Blob.eSort.Size;
        public CPoint m_mmGV = new CPoint(100, 0);
        string InspectBlob(int iAOI)
        {
            Unit data = m_aUnit[iAOI];
            m_blob.RunBlob(m_memory, 0, data.m_aoiData.m_cp0, data.m_aoiData.m_sz, m_mmGV.X, m_mmGV.Y, 3);
            m_blob.RunSort(m_eSort);
            if (m_blob.m_aSort.Count == 0) return "Find Fiducial Error";
            Blob.Island island = m_blob.m_aSort[0];
            data.m_aoiData.m_bInspect = true; 
            data.m_aoiData.m_rpCenter = island.m_rpCenter;
            data.m_aoiData.m_sDisplay = "Size = " + island.m_nSize + ", " + island.m_sz.ToString();
            return "OK";
        }

        void RunTreeInspect(Tree tree)
        {
            m_mmGV = tree.Set(m_mmGV, m_mmGV, "GV", "Gray Value Range (0~255)");
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
            m_aUnit[0].m_aoiData.Draw(draw, eDraw);
            m_aUnit[1].m_aoiData.Draw(draw, eDraw);
            draw.InvalidDraw();
        }
        #endregion

        #region Tree
        public TreeRoot m_treeRoot;
        void InitTree()
        {
            m_treeRoot = new TreeRoot(m_id, m_log);
            RunTree(Tree.eMode.RegRead);
            m_treeRoot.UpdateTree += M_treeRoot_UpdateTree;
        }

        private void M_treeRoot_UpdateTree()
        {
            RunTree(Tree.eMode.Update);
            RunTree(Tree.eMode.RegWrite);
        }

        public void RunTree(Tree.eMode eMode)
        {
            m_treeRoot.p_eMode = eMode;
            RunTreeUnit(m_treeRoot.GetTree("Unit", false, false));
            RunTreeInspect(m_treeRoot);
        }
        #endregion

        string m_id;
        AxisMapping_Engineer m_engineer;
        public AxisMapping m_axisMapping; 
        Log m_log; 
        public Mapping(string id, AxisMapping_Engineer engineer)
        {
            m_id = id;
            m_engineer = engineer;
            m_axisMapping = ((AxisMapping_Handler)engineer.ClassHandler()).m_axisMapping;
            m_memoryPool = m_axisMapping.m_memoryPool; 
            m_log = LogView.GetLog(id);
            InitUnit(); 
            InitTree();
            InitDraw();
            InitROI();
            GetActineROI();
        }
    }
}