using RootTools;
using RootTools.Memory;
using RootTools.ToolBoxs;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Root_ASIS.AOI
{
    public class AOI_StripID : IAOI
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
        List<UnitID> m_aUnitID = new List<UnitID>();
        public int p_lUnitID
        {
            get { return m_aUnitID.Count; }
            set
            {
                while (m_aUnitID.Count < value)
                {
                    UnitID unitID = new UnitID(p_id + "." + m_aUnitID.Count.ToString("00"), this); 
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
        public string p_sAOI { get; set; }
        public int p_nID { get; set; }
        public bool p_bEnable { get; set; }

        public IAOI NewAOI() { return null; }
        public void ReAllocate(List<CPoint> aArray) { }

        public void Draw(MemoryDraw draw, AOIData.eDraw eDraw)
        {
            //forget
        }

        public ObservableCollection<AOIData> p_aROI { get; set; }

        public void ClearActive()
        {
            throw new System.NotImplementedException();
        }

        public void CalcROICount(ref int nReady, ref int nActive)
        {
            throw new System.NotImplementedException();
        }

        public AOIData GetAOIData(AOIData.eROI eROI)
        {
            throw new System.NotImplementedException();
        }
        #endregion

        #region Tree
        public void RunTreeAOI(Tree tree)
        {
            RunTreeUnit(tree.GetTree("UnitID", false, false));
            //RunTreeInspect(tree.GetTree("Inspect", false));
        }
        #endregion

        Log m_log;
        public AOI_StripID(string id, Log log)
        {
            p_aROI = new ObservableCollection<AOIData>(); 
            p_id = id;
            p_sAOI = id; 
            m_log = log;
//            InitUnit();
//            InitResult();
        }
    }
}
