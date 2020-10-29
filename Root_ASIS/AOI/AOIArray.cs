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

            public void RunTree(Tree tree)
            {
                m_sz = tree.Set(m_sz, m_sz, "szROI", "szROI", false);
                m_aoiData.RunTree(tree, false);
            }

            public string m_id;
            AOIArray m_aoi;
            public Unit(string id, AOIArray aoi)
            {
                m_id = id;
                m_aoi = aoi;
                m_aoiData = new AOIData(id, m_sz);
            }
        }
        List<Unit> m_aUnitROI = new List<Unit>(); 

        void InitUnit()
        {
            m_aUnitROI.Add(new Unit("Unit Origin", this));
            m_aUnitROI.Add(new Unit("Unit Right", this));
            m_aUnitROI.Add(new Unit("Unit Bottom", this));
            m_aUnitROI.Add(new Unit("Block Right", this));
            m_aUnitROI.Add(new Unit("Block Bottom", this));
        }

        void RunTreeUnit(Tree tree)
        {
            foreach (Unit unit in m_aUnitROI) unit.RunTree(tree.GetTree(unit.m_id)); 
        }
        #endregion

        #region IAOI
        public string p_id { get; set; }
        public int p_nID { get; set; }
        public bool p_bEnable { get; set; }

        public IAOI NewAOI() { return null; }

        public void Draw(MemoryDraw draw, AOIData.eDraw eDraw)
        {
            foreach (Unit unit in m_aUnitROI) unit.m_aoiData.Draw(draw, eDraw); 
        }

        public ObservableCollection<AOIData> p_aROI { get; set; }

        public void ClearActive()
        {
            foreach (Unit unit in m_aUnitROI)
            {
                if (unit.m_aoiData.p_eROI == AOIData.eROI.Active) unit.m_aoiData.p_eROI = AOIData.eROI.Ready;
            }
        }

        public void CalcROICount(ref int nReady, ref int nActive)
        {
            foreach (Unit unit in m_aUnitROI)
            {
                switch (unit.m_aoiData.p_eROI)
                {
                    case AOIData.eROI.Ready: nReady++; break;
                    case AOIData.eROI.Active: nActive++; break;
                }
            }
        }

        public AOIData GetAOIData(AOIData.eROI eROI)
        {
            foreach (Unit unit in m_aUnitROI)
            {
                if (unit.m_aoiData.p_eROI == eROI) return unit.m_aoiData;
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
