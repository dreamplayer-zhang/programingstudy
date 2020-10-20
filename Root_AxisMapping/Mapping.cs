using Root_ASIS.AOI;
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
        #region AOI
        AOIStrip m_aoiStrip;
        void InitListAOI()
        {
            m_aoiStrip = new AOIStrip("AOIStrip", m_log);
        }
        #endregion

        #region ROI
        CPoint m_szROI = new CPoint(100, 100); 
        public ObservableCollection<AOIData> m_aROI = new ObservableCollection<AOIData>();
        void InitROI()
        {
            m_aROI.Clear();
            m_aoiStrip.AddROI(m_aROI); 
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
            m_aoiStrip.Draw(draw, eDraw);
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
            InitListAOI(); 
            InitTree();
            InitDraw();
            InitROI();
            GetActineROI();
        }
    }
}
