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
        UnitID m_unit; 
        void InitUnit()
        {
            m_unit = new UnitID(p_id, this); 
        }
        #endregion

        #region Inspect
        public string Setup(MemoryData memory)
        {
            return "OK"; 
        }

        public string BeforeInspect(InfoStrip infoStrip, MemoryData memory) { return "OK"; }

        public string Inspect(InfoStrip infoStrip, MemoryData memory) 
        {
            throw new NotImplementedException();
        }

        public string AfterInspect(InfoStrip infoStrip, MemoryData memory) { return "OK"; }
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
        //public bool p_bEnable { get; set; }

        bool _bEnable = false; 
        public bool p_bEnable 
        { 
            get { return _bEnable; }
            set
            {
                _bEnable = value; 
            }
        }


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
            InitUnit();
//            InitResult();
        }
    }
}
