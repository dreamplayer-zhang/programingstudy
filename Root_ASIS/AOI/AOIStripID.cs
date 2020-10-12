using Root_ASIS.Teach;
using RootTools;
using RootTools.Memory;
using RootTools.ToolBoxs;
using RootTools.Trees;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Root_ASIS.AOI
{
    public class AOIStripID : IAOI
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
                m_sz = tree.Set(m_sz, m_sz, "szROI", "szROI", false);
                m_aoiData.RunTree(tree, false);
            }

            public string m_id;
            AOIStripID m_aoi;
            public UnitID(string id, AOIStripID aoi)
            {
                m_id = id;
                m_aoi = aoi;
                m_aoiData = new AOIData(id, m_sz);
            }
        }
        List<UnitID> m_aUnitID = new List<UnitID>();
        public int p_lUnitID
        {
            get { return m_aUnitID.Count; }
            set
            {
                while (m_aUnitID.Count < value)
                {
                    UnitID unitID = new UnitID(m_id + "." + m_aUnitID.Count.ToString("00"), this); 
                    m_aUnitID.Add(unitID); 
                }
                while (m_aUnitID.Count > value) m_aUnitID.RemoveAt(m_aUnitID.Count - 1);
            }
        }

        void RunTreeUnit(Tree tree)
        {
            p_lUnitID = tree.Set(p_lUnitID, p_lUnitID, "lUnitID", "StripID ROI Count"); 
        }
        #endregion

        #region Tree
        public void RunTree(Tree tree)
        {
            RunTreeUnit(tree.GetTree("UnitID", false, false));
            //RunTreeInspect(tree.GetTree("Inspect", false));
        }
        #endregion

        string m_id;
        Log m_log;
        public AOIStripID(string id, Log log)
        {
            m_id = id;
            m_log = log;
//            InitUnit();
//            InitResult();
        }
    }
}
