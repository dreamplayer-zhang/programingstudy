using Root_ASIS.Teachs;
using RootTools;
using RootTools.Memory;
using RootTools.ToolBoxs;
using RootTools.Trees;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Root_ASIS.AOI
{
    public class AOI_Unit : IAOI
    {
        #region StringTable
        static string[] m_asStringTable =
        {
            "Gray Value Range (0~255)",
        };
        StringTable.Group m_ST = StringTable.Get("AOIStrip", m_asStringTable);
        #endregion

        #region IAOI
        public bool p_bEnable { get; set; }

        public IAOI NewAOI()
        {
            return new AOI_Unit(p_id, m_log); 
        }

        public void Draw(MemoryDraw draw, AOIData.eDraw eDraw)
        {
            //forget
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

        #region Unit
        public CPoint m_sz = new CPoint(100, 50);

        class Unit
        {
            public AOIData m_aoiData;

            public InfoStrip.UnitResult m_result; 

            string _id = "";
            public string p_id
            {
                get { return _id; }
                set
                {
                    if (_id == value) return;
                    _id = value;
                    m_aoiData.p_id = value; 
                }
            }

            int _nSize = 0; 
            public int p_nSize
            {
                get { return _nSize; }
                set
                {
                    _nSize = value;
                    m_aoiData.m_sDisplay = "Size = " + value.ToString(); 
                }
            }

            public void RunTree(Tree tree)
            {
                m_aoiData.RunTree(tree, false);
            }

            AOI_Unit m_aoi;
            public Unit(string id, AOI_Unit aoi, CPoint sz)
            {
                m_aoiData = new AOIData(id, sz);
                p_id = id;
                m_aoi = aoi;
            }
        }
        List<Unit> m_aUnit = new List<Unit>();

        void InitUnit(InfoStrip infoStrip)
        {
            while (m_aUnit.Count < Strip.p_lUnit)
            {
                Unit unit = new Unit(p_id + "." + m_aUnit.Count.ToString("000"), this, m_sz);
                m_aUnit.Add(unit); 
            }
            while (m_aUnit.Count > Strip.p_lUnit) m_aUnit.RemoveAt(m_aUnit.Count - 1);
            for (int n = 0; n < m_aUnit.Count; n++)
            {
                m_aUnit[n].p_id = p_id + "." + n.ToString("000");
                m_aUnit[n].m_result = (infoStrip != null) ? infoStrip.GetUnitResult(n) : null; 
            }
            p_aROI.Clear();
            p_aROI.Add(m_aUnit[0].m_aoiData); 
        }

        void RunTreeUnit(Tree tree)
        {
            m_sz = tree.Set(m_sz, m_sz, "szROI", "szROI", false);
            foreach (Unit unit in m_aUnit) unit.RunTree(tree.GetTree(unit.p_id)); 
        }
        #endregion

        #region Inspect
        InfoStrip.UnitResult.eLogic m_eLogic = InfoStrip.UnitResult.eLogic.Or; 

        const int c_lInspect = 24;
        Blob[] m_aBlob = new Blob[c_lInspect]; 
        void InitInspect()
        {
            for (int n = 0; n < c_lInspect; n++) m_aBlob[n] = new Blob(); 
        }

        InfoStrip m_infoStrip;
        MemoryData m_memory; 
        public string Inspect(InfoStrip infoStrip, MemoryData memory)
        {
            m_infoStrip = infoStrip;
            m_memory = memory;
            if (infoStrip.p_eResult != InfoStrip.eResult.Xout) return "OK";
            InitUnit(infoStrip); 
            Parallel.For(0, c_lInspect, iAOI => { Inspect(iAOI); });
            return "OK"; 
        }

        void Inspect(int iAOI)
        {
            Blob blob = m_aBlob[iAOI]; 
            while (iAOI < m_aUnit.Count)
            {
                Inspect(m_aUnit[iAOI], blob);
                iAOI += c_lInspect; 
            }
        }

        Blob.eSort m_eSort = Blob.eSort.Size;
        public CPoint m_mmGV = new CPoint(100, 0);
        public CPoint m_mmSize = new CPoint(100, 0); 
        string Inspect(Unit unit, Blob blob)
        {
            if (unit.m_aoiData.m_bEnable == false) return "OK";
            AOIData aoiData = m_infoStrip.GetInfoPos(unit.m_aoiData);
            blob.RunBlob(m_memory, 0, aoiData.m_cp0, m_sz, m_mmGV.X, m_mmGV.Y, 3);
            blob.RunSort(m_eSort);
            if (blob.m_aSort.Count == 0) return "Find Fiducial Error";
            Blob.Island island = blob.m_aSort[0];
            unit.p_nSize = island.GetSize(m_eSort);
            unit.m_aoiData.m_rpCenter = island.m_rpCenter;
            unit.m_aoiData.m_bInspect = (unit.p_nSize >= m_mmSize.X) && ((m_mmSize.Y <= 0) || (unit.p_nSize <= m_mmSize.Y));
            if (unit.m_result != null) unit.m_result.CalcResult(m_eLogic, unit.m_aoiData.m_bInspect); 
            return "OK"; 
        }

        void RunTreeInspect(Tree tree)
        {
            m_mmGV = tree.Set(m_mmGV, m_mmGV, "GV", m_ST.Get("Gray Value Range (0~255)"));
            m_eSort = (Blob.eSort)tree.Set(m_eSort, m_eSort, "Sort", "Select Fiducial by");
            m_mmSize = tree.Set(m_mmSize, m_mmSize, "Size", "Size (pixel)");
            m_eLogic = (InfoStrip.UnitResult.eLogic)tree.Set(m_eLogic, m_eLogic, "Logic", "AOI_Unit Inspect Logic"); 
        }
        #endregion

        #region Tree
        public void RunTreeAOI(Tree tree)
        {
            RunTreeUnit(tree.GetTree("Unit", false, false));
            RunTreeInspect(tree);
        }
        #endregion

        public string p_id { get; set; }
        public int p_nID { get; set; }

        Log m_log;
        public AOI_Unit(string id, Log log)
        {
            p_aROI = new ObservableCollection<AOIData>();
            p_id = id;
            m_log = log;
            p_bEnable = true;
            InitUnit(null);
            InitInspect();
        }
    }
}
