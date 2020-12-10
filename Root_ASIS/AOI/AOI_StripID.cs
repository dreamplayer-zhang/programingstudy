using RootTools;
using RootTools.Memory;
using RootTools.ToolBoxs;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;

namespace Root_ASIS.AOI
{
    public class AOI_StripID : IAOI //forget
    {
        #region StringTable
        static string[] m_asStringTable =
        {
            "Gray Value Range (0~255)",
        };
        StringTable.Group m_ST = StringTable.Get("AOIStripID", m_asStringTable);
        #endregion

        #region UnitID
        class UnitID
        {
            public CPoint m_sz = new CPoint(100, 50);
            public AOIData m_aoiData;

            public void RunTree(Tree tree)
            {
                m_sz.X = tree.Set(m_sz.X, m_sz.X, "szROIX", "szROI", false);
                m_sz.Y = tree.Set(m_sz.Y, m_sz.Y, "szROIY", "szROI", false);
                m_aoiData.RunTree(tree);
            }

            public string m_id;
            AOI_StripID m_aoi;
            public UnitID(string id, AOI_StripID aoi)
            {
                m_id = id;
                m_aoi = aoi;
                m_aoiData = new AOIData(id, m_sz);
            }
        }
        UnitID m_unit; 
        void InitUnit()
        {
            m_unit = new UnitID(p_id, this);
            p_aROI.Clear();
            p_aROI.Add(m_unit.m_aoiData); 
        }

        void RunTreeUnit(Tree tree)
        {
            m_unit.RunTree(tree.GetTree(m_unit.m_id));
        }
        #endregion

        #region ROI
        byte[] m_aReference = null;
        public string Setup(MemoryData memory)
        {
            CPoint cp0 = m_unit.m_aoiData.p_cp0; 
            CPoint sz = m_unit.m_sz; 
            m_aReference = new byte[sz.Y * sz.X]; 
            for (int y = 0; y < sz.Y; y++)
            {
                IntPtr ip = memory.GetPtr(0, cp0.X, cp0.Y + y);
                Marshal.Copy(ip, m_aReference, y * sz.X, sz.X); 
            }
            m_aROI.Clear();
            SetupROI(cp0, 0, m_mmGV.X, (m_mmGV.Y == 0) ? 255 : m_mmGV.Y); 
            return "OK"; 
        }

        List<AOIData> m_aROI = new List<AOIData>(); 
        void SetupROI(CPoint cp0, int x, int nGV0, int nGV1)
        {
            while ((x < m_unit.m_sz.X) && (CalcCount(x, nGV0, nGV1) == 0)) x++;
            int x0 = x;
            while ((x < m_unit.m_sz.X) && (CalcCount(x, nGV0, nGV1) > 0)) x++;
            int x1 = x;
            if (x >= m_unit.m_sz.X) return;
            int y0 = m_unit.m_sz.Y;
            int y1 = 0;
            int nSize = 0; 
            for (int y = 0; y < m_unit.m_sz.Y; y++)
            {
                int nCount = CalcCount(y, x0, x1, nGV0, nGV1);
                nSize += nCount; 
                if (nCount > 0)
                {
                    if (y0 > y) y0 = y;
                    if (y1 < y) y1 = y; 
                }
            }
            if (nSize > m_minSize)
            {
                AOIData aoi = new AOIData(p_id + m_aROI.Count.ToString("00"), new CPoint(x1 - x0 + 1, y1 - y0 + 1));
                aoi.p_cp0 = new CPoint(x0, y0); 
                m_aROI.Add(aoi); 
            }
            SetupROI(cp0, x1, nGV0, nGV1); 
        }

        int CalcCount(int x, int nGV0, int nGV1)
        {
            int nCount = 0; 
            for (int y = 0; y < m_unit.m_sz.Y; y++, x += m_unit.m_sz.X)
            {
                if ((m_aReference[x] > nGV0) && (m_aReference[x] <= nGV1)) nCount++; 
            }
            return nCount; 
        }

        int CalcCount(int y, int x0, int x1, int nGV0, int nGV1)
        {
            int nCount = 0; 
            for (int x = x0, p = (x0 + y * m_unit.m_sz.X); x <= x1; x++, p++)
            {
                if ((m_aReference[p] > nGV0) && (m_aReference[p] <= nGV1)) nCount++;
            }
            return nCount; 
        }
        #endregion

        #region Inspect
        CPoint m_mmGV = new CPoint(160, 0);
        int m_minSize = 20;

        public string BeforeInspect(InfoStrip infoStrip, MemoryData memory) { return "OK"; }

        public string Inspect(InfoStrip infoStrip, MemoryData memory) 
        {
            throw new NotImplementedException();
        }

        public string AfterInspect(InfoStrip infoStrip, MemoryData memory) { return "OK"; }

        void RunTreeInspect(Tree tree)
        {
            m_mmGV = tree.Set(m_mmGV, m_mmGV, "GV", "Gray Value Range (0~255)");
            m_minSize = tree.Set(m_minSize, m_minSize, "Min Size", "Minimum Size (Pixel)"); 
        }
        #endregion

        #region IAOI
        string _id = "";
        public string p_id 
        {
            get { return _id; }
            set
            {
                _id = value;
                if (m_unit != null) m_unit.m_aoiData.p_id = value; 
            }
        }
        public string p_sAOI { get; set; }
        public int p_nID { get; set; }
        public bool p_bEnable { get; set; }

        public IAOI NewAOI() 
        {
            return new AOI_StripID(p_id, m_log);  
        }

        public void ReAllocate(List<CPoint> aArray) { }

        public void Draw(MemoryDraw draw, AOIData.eDraw eDraw)
        {
            m_unit.m_aoiData.Draw(draw, eDraw); 
        }

        public ObservableCollection<AOIData> p_aROI { get; set; }

        public void ClearActive()
        {
            if (m_unit.m_aoiData.p_eROI == AOIData.eROI.Active) m_unit.m_aoiData.p_eROI = AOIData.eROI.Ready; 
        }

        public void CalcROICount(ref int nReady, ref int nActive)
        {
            switch (m_unit.m_aoiData.p_eROI)
            {
                case AOIData.eROI.Ready: nReady++; break;
                case AOIData.eROI.Active: nActive++; break; 
            }
        }

        public AOIData GetAOIData(AOIData.eROI eROI)
        {
            return (m_unit.m_aoiData.p_eROI == eROI) ? m_unit.m_aoiData : null; 
        }
        #endregion

        #region Tree
        public void RunTreeAOI(Tree tree)
        {
            RunTreeUnit(tree.GetTree("Unit", false, false));
            RunTreeInspect(tree);
        }
        #endregion

        Log m_log;
        public AOI_StripID(string id, Log log)
        {
            p_aROI = new ObservableCollection<AOIData>(); 
            p_id = id;
            p_sAOI = id; 
            m_log = log;
            InitUnit();
//            InitResult();
        }
    }
}
