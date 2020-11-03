using Root_ASIS.Teachs;
using RootTools;
using RootTools.Memory;
using RootTools.ToolBoxs;
using RootTools.Trees;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Root_ASIS.AOI
{
    public class AOIArray : IAOI
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
            public string m_sInspect = "";
            public int m_nID = 0;

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

        #region IAOI
        public string p_id { get; set; }
        public int p_nID { get; set; }
        public bool p_bEnable { get; set; }

        public IAOI NewAOI() { return null; }

        public void Draw(MemoryDraw draw, AOIData.eDraw eDraw)
        {
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
        public AOIArray(string id, Log log)
        {
            p_aROI = new ObservableCollection<AOIData>(); 
            p_id = id;
            m_log = log;
            InitUnit();
        }
    }
}
