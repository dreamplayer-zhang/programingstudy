using RootTools;
using RootTools.Memory;
using RootTools.ToolBoxs;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Root_ASIS.AOI
{
    public class AOIArray : NotifyProperty, IArray
    {
        #region StringTable
        static string[] m_asStringTable =
        {
            "Gray Value Range (0~255)",
        };
        StringTable.Group m_ST = StringTable.Get("AOIArray", m_asStringTable);
        #endregion

        #region Unit
        class Unit
        {
            public CPoint m_sz = new CPoint(100, 50);
            public AOIData m_aoiData;
            public RPoint m_rpDelta = new RPoint(); 
            public string m_sInspect = "";
            public int m_nID = 0;
            Blob m_blob = new Blob();

            public string FindCenter(MemoryData memory)
            {
                if (p_bEnable == false) return "OK";
                if (m_aoiData.p_eROI != AOIData.eROI.Done) return m_id + " ROI not Done"; 
                m_blob.RunBlob(memory, 0, m_aoiData.m_cp0, m_aoiData.m_sz, m_aoi.m_mmGV.X, m_aoi.m_mmGV.X, 5);
                m_blob.RunSort(Blob.eSort.Size);
                if (m_blob.m_aSort.Count > 0) m_aoiData.m_rpCenter = m_blob.m_aSort[0].m_rpCenter;
                else m_aoiData.m_rpCenter = new RPoint(m_aoiData.m_cp0.X + m_sz.X / 2.0, m_aoiData.m_cp0.Y + m_sz.Y / 2.0);
                return "OK"; 
            }

            public bool p_bEnable
            {
                get
                {
                    switch (m_nID)
                    {
                        case 1: return (Strip.p_szUnit.X > 1);
                        case 2: return (Strip.p_szUnit.Y > 1);
                        case 3: return (Strip.p_szBlock.X > 1);
                        case 4: return (Strip.p_szBlock.Y > 1);
                        default: return true;
                    }
                }
            }

            public void RunTree(Tree tree)
            {
                m_sz.X = tree.Set(m_sz.X, m_sz.X, "szROIX", "szROI", false);
                m_sz.Y = tree.Set(m_sz.Y, m_sz.Y, "szROIY", "szROI", false);
                m_aoiData.RunTree(tree);
            }

            public string m_id;
            AOIArray m_aoi;
            public Unit(int nID, string id, AOIArray aoi)
            {
                m_nID = nID; 
                m_id = id;
                m_aoi = aoi;
                m_aoiData = new AOIData(id, m_sz);
            }
        }
        List<Unit> m_aUnit = new List<Unit>(); 

        void InitUnit()
        {
            m_aUnit.Add(new Unit(0, "Unit Origin", this));
            m_aUnit.Add(new Unit(1, "Unit Right", this));
            m_aUnit.Add(new Unit(2, "Unit Bottom", this));
            m_aUnit.Add(new Unit(3, "Block Right", this));
            m_aUnit.Add(new Unit(4, "Block Bottom", this));
            InvalidROI(); 
        }

        public void InvalidROI()
        {
            p_aROI.Clear();
            foreach (Unit unit in m_aUnit)
            {
                if (unit.p_bEnable) p_aROI.Add(unit.m_aoiData);
                else unit.m_aoiData.p_eROI = AOIData.eROI.Done; 
            }
        }

        void RunTreeUnit(Tree tree)
        {
            foreach (Unit unit in m_aUnit) unit.RunTree(tree.GetTree(unit.m_id)); 
        }
        #endregion

        #region ReAllocate
        public enum eSide
        {
            Top,
            Bottom,
        }
        public eSide m_eSide = eSide.Top; 

        public CPoint m_mmGV = new CPoint(100, 0); 
        MemoryData m_memory;
        public string ReAllocate(MemoryData memory)
        {
            m_memory = memory;
            foreach (Unit unit in m_aUnit) unit.FindCenter(memory);
            for (int n = 1; n < m_aUnit.Count; n++) m_aUnit[n].m_rpDelta = m_aUnit[n].m_aoiData.m_rpCenter - m_aUnit[0].m_aoiData.m_rpCenter;
            CalcArrayPos(); 
            return "OK"; 
        }

        public List<CPoint> p_aArray { get; set; }
        void CalcArrayPos()
        {
            p_aArray.Clear(); 
            for (int iy = 0; iy < Strip.p_szBlock.Y; iy++)
            {
                CPoint dpBlockY = (Strip.p_szBlock.Y > 1) ? new CPoint((m_aUnit[4].m_rpDelta * iy) / (Strip.p_szBlock.Y - 1)) : new CPoint();
                for (int ix = 0; ix < Strip.p_szBlock.X; ix++)
                {
                    CPoint dpBlockX = (Strip.p_szBlock.X > 1) ? new CPoint((m_aUnit[3].m_rpDelta * ix) / (Strip.p_szBlock.X - 1)) : new CPoint();
                    CalcArrayPos(dpBlockX + dpBlockY);
                }
            }
        }

        void CalcArrayPos(CPoint dpBlock)
        {
            switch (Strip.p_eUnitOrder)
            {
                case Strip.eUnitOrder.Left:
                case Strip.eUnitOrder.Right:
                    for (int iy = 0; iy < Strip.p_szUnit.Y; iy++)
                    {
                        int y = IsInvY(Strip.p_eUnitSecondOrder) ? Strip.p_szUnit.Y - iy - 1 : iy; 
                        CPoint dpUnitY = (Strip.p_szUnit.Y > 1) ? new CPoint((m_aUnit[2].m_rpDelta * y) / (Strip.p_szUnit.Y - 1)) : new CPoint();
                        for (int ix = 0; ix < Strip.p_szUnit.X; ix++)
                        {
                            int x = IsInvX(Strip.p_eUnitOrder) ? Strip.p_szUnit.X - ix - 1 : ix; 
                            CPoint dpUnitX = (Strip.p_szUnit.X > 1) ? new CPoint((m_aUnit[1].m_rpDelta * x) / (Strip.p_szUnit.X - 1)) : new CPoint();
                            p_aArray.Add(dpBlock + dpUnitX + dpUnitY); 
                        }
                    }
                    break;
                case Strip.eUnitOrder.Down:
                case Strip.eUnitOrder.Up:
                    for (int ix = 0; ix < Strip.p_szUnit.X; ix++)
                    {
                        int x = IsInvX(Strip.p_eUnitSecondOrder) ? Strip.p_szUnit.X - ix - 1 : ix;
                        CPoint dpUnitX = (Strip.p_szUnit.X > 1) ? new CPoint((m_aUnit[1].m_rpDelta * x) / (Strip.p_szUnit.X - 1)) : new CPoint();
                        for (int iy = 0; iy < Strip.p_szUnit.Y; iy++)
                        {
                            int y = IsInvY(Strip.p_eUnitOrder) ? Strip.p_szUnit.Y - iy - 1 : iy;
                            CPoint dpUnitY = (Strip.p_szUnit.Y > 1) ? new CPoint((m_aUnit[2].m_rpDelta * y) / (Strip.p_szUnit.Y - 1)) : new CPoint();
                            p_aArray.Add(dpBlock + dpUnitX + dpUnitY);
                        }
                    }
                    break; 
            }
        }

        bool IsInvX(Strip.eUnitOrder eUnitOrder)
        {
            switch (eUnitOrder)
            {
                case Strip.eUnitOrder.Right: return (m_eSide == eSide.Bottom);
                case Strip.eUnitOrder.Left: return (m_eSide == eSide.Top);
            }
            return false; 
        }

        bool IsInvY(Strip.eUnitOrder eUnitOrder)
        {
            switch (eUnitOrder)
            {
                case Strip.eUnitOrder.Down: return false;
                case Strip.eUnitOrder.Up: return true; 
            }
            return false; 
        }

        public void RunTreeArray(Tree tree)
        {
            m_eSide = (eSide)tree.Set(m_eSide, m_eSide, "Side", "Strip Side"); 
            m_mmGV = tree.Set(m_mmGV, m_mmGV, "GV", m_ST.Get("Gray Value Range (0~255)"));
        }
        #endregion

        #region Inspect
        public string BeforeInspect(InfoStrip infoStrip, MemoryData memory) { return "OK"; }
        public string AfterInspect(InfoStrip infoStrip, MemoryData memory) { return "OK"; }

        public string Inspect(InfoStrip infoStrip, MemoryData memory)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region IAOI
        public string p_id { get; set; }
        public int p_nID { get; set; }
        public bool p_bEnable { get; set; }

        public IAOI NewAOI() { return null; }
        public void ReAllocate(List<CPoint> aArray) { }

        public void Draw(MemoryDraw draw, AOIData.eDraw eDraw)
        {
            if (eDraw != AOIData.eDraw.ROI) return; 
            foreach (Unit unit in m_aUnit)
            {
                if (unit.p_bEnable) unit.m_aoiData.Draw(draw, eDraw);
            }
        }

        public ObservableCollection<AOIData> p_aROI { get; set; }

        public void ClearActive()
        {
            foreach (AOIData aoiData in p_aROI)
            {
                if (aoiData.p_eROI == AOIData.eROI.Active) aoiData.p_eROI = AOIData.eROI.Ready;
            }
        }

        public void CalcROICount(ref int nReady, ref int nActive)
        {
            foreach (AOIData aoiData in p_aROI)
            {
                switch (aoiData.p_eROI)
                {
                    case AOIData.eROI.Ready: nReady++; break;
                    case AOIData.eROI.Active: nActive++; break;
                }
            }
        }

        public AOIData GetAOIData(AOIData.eROI eROI)
        {
            foreach (AOIData aoiData in p_aROI)
            {
                if (aoiData.p_eROI == eROI) return aoiData;
            }
            return null;
        }
        #endregion

        #region Tree
        public void RunTreeAOI(Tree tree)
        {
            RunTreeUnit(tree.GetTree("Unit", false, false));
        }
        #endregion

        Log m_log;
        public AOIArray(string id, int nID, Log log)
        {
            p_aROI = new ObservableCollection<AOIData>();
            p_aArray = new List<CPoint>(); 
            p_id = id;
            m_eSide = (eSide)nID; 
            m_log = log;
            InitUnit();
        }
    }
}
