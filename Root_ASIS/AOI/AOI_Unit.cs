using Root_ASIS.Teach;
using RootTools;
using RootTools.Memory;
using RootTools.ToolBoxs;
using RootTools.Trees;
using System.Collections.Generic;
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
                    m_aoiData.m_id = value; 
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
                Unit unit = new Unit(m_id + "." + m_aUnit.Count.ToString("000"), this, m_sz);
                m_aUnit.Add(unit); 
            }
            while (m_aUnit.Count > Strip.p_lUnit) m_aUnit.RemoveAt(m_aUnit.Count - 1);
            for (int n = 0; n < m_aUnit.Count; n++)
            {
                m_aUnit[n].p_id = m_id + "." + m_aUnit.Count.ToString("000");
                m_aUnit[n].m_result = infoStrip.GetUnitResult(n); 
            }
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
        public int[] m_nGV = new int[2] { 100, 0 };
        public int m_minSize = 100;
        public int m_maxSize = 0; 
        string Inspect(Unit unit, Blob blob)
        {
            if (unit.m_aoiData.m_bEnable == false) return "OK";
            AOIData aoiData = m_infoStrip.GetInfoPos(unit.m_aoiData);
            blob.RunBlob(m_memory, 0, aoiData.m_cp0, m_sz, m_nGV[0], m_nGV[1], 3);
            blob.RunSort(m_eSort);
            if (blob.m_aSort.Count == 0) return "Find Fiducial Error";
            Blob.Island island = blob.m_aSort[0];
            unit.p_nSize = island.GetSize(m_eSort);
            bool bInspect = (unit.p_nSize >= m_minSize) && ((m_maxSize <= 0) || (unit.p_nSize <= m_maxSize));
            unit.m_result.CalcResult(m_eLogic, bInspect); 
            return "OK"; 
        }

        void RunTreeInspect(Tree tree)
        {
            m_nGV[0] = tree.Set(m_nGV[0], m_nGV[0], "Min GV", m_ST.Get("Gray Value Range (0~255)"));
            m_nGV[1] = tree.Set(m_nGV[1], m_nGV[1], "Max GV", m_ST.Get("Gray Value Range (0~255)"));
            m_eSort = (Blob.eSort)tree.Set(m_eSort, m_eSort, "Sort", "Select Fiducial by");

            m_eLogic = (InfoStrip.UnitResult.eLogic)tree.Set(m_eLogic, m_eLogic, "Logic", "AOI_Unit Inspect Logic"); 
//            RunTreeError(tree.GetTree("Error", false));
        }

        #endregion

        #region Tree
        public void RunTree(Tree tree)
        {
            RunTreeUnit(tree.GetTree("Unit", false, false));
            RunTreeInspect(tree.GetTree("Inspect", false));
        }
        #endregion

        string m_id;
        Log m_log;
        public AOI_Unit(string id, Log log)
        {
            m_id = id;
            m_log = log;
            InitInspect(); 
        }
    }
}
