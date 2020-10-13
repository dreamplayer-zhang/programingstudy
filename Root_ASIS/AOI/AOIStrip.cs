using Root_ASIS.Teachs;
using RootTools;
using RootTools.Memory;
using RootTools.ToolBoxs;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Root_ASIS.AOI
{
    public class AOIStrip : IAOI
    {
        #region StringTable
        static string[] m_asStringTable =
        {
            "Gray Value Range (0~255)",
        };
        StringTable.Group m_ST = StringTable.Get("AOIStrip", m_asStringTable); 
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
            AOIStrip m_aoi; 
            public Unit(string id, AOIStrip aoi)
            {
                m_id = id;
                m_aoi = aoi; 
                m_aoiData = new AOIData(id, m_sz); 
            }
        }
        Unit[] m_aUnit = new Unit[2]; 

        void InitUnit()
        {
            m_aUnit[0] = new Unit("Origin 0", this);
            m_aUnit[1] = new Unit("Origin 1", this);
        }

        void RunTreeUnit(Tree tree)
        {
            m_aUnit[0].RunTree(tree.GetTree(m_aUnit[0].m_id));
            m_aUnit[1].RunTree(tree.GetTree(m_aUnit[1].m_id));
        }
        #endregion

        #region AOI Result
        class Result
        {
            public class Unit
            {
                public int m_maxSize = 0;
                public int m_maxLength = 0;
                public RPoint m_rpCenter = new RPoint();
            }
            public Unit[] m_aUnit = new Unit[2] { new Unit(), new Unit() }; 

            public RPoint m_rpCenter = new RPoint();
            public double m_fDistance = 0; 
            public double m_fAngle = 0; 
            public void CalcStripPos()
            {
                m_rpCenter = (m_aUnit[0].m_rpCenter + m_aUnit[1].m_rpCenter) / 2;
                RPoint dp = m_aUnit[1].m_rpCenter - m_aUnit[0].m_rpCenter;
                m_fDistance = dp.GetL(); 
                m_fAngle = Math.Atan2(dp.Y, dp.X); 
            }
        }
        Dictionary<eMode, Result> m_aResult = new Dictionary<eMode, Result>(); 
        void InitResult()
        {
            m_aResult.Add(eMode.Inspect, new Result());
            m_aResult.Add(eMode.Setup, new Result());
        }

        double Get_dSize(int iUnit)
        {
            return GetPercent(m_aResult[eMode.Inspect].m_aUnit[iUnit].m_maxSize, m_aResult[eMode.Setup].m_aUnit[iUnit].m_maxSize);
        }

        double Get_dLength(int iUnit)
        {
            return GetPercent(m_aResult[eMode.Inspect].m_aUnit[iUnit].m_maxLength, m_aResult[eMode.Setup].m_aUnit[iUnit].m_maxLength);
        }

        double Get_dCenter(int iUnit)
        {
            RPoint dp = m_aResult[eMode.Inspect].m_aUnit[iUnit].m_rpCenter - m_aResult[eMode.Setup].m_aUnit[iUnit].m_rpCenter;
            return dp.GetL();
        }

        double Get_dDistance()
        {
            return Math.Abs(m_aResult[eMode.Inspect].m_fDistance - m_aResult[eMode.Setup].m_fDistance); 
        }

        double GetPercent(double f0, double f1)
        {
            double f = f0 + f1;
            if (f == 0) return 0;
            return 200 * Math.Abs(f1 - f0) / (f0 + f1);
        }

        void SetInfoPos()
        {
            RPoint rpShift = m_aResult[eMode.Setup].m_rpCenter - m_aResult[eMode.Inspect].m_rpCenter; 
            double fAngle = m_aResult[eMode.Setup].m_fAngle - m_aResult[eMode.Inspect].m_fAngle;
            m_infoStrip.SetInfoPos(rpShift, fAngle); 
        }
        #endregion

        #region Inspect
        public enum eMode
        {
            Inspect,
            Setup
        };

        InfoStrip m_infoStrip;
        MemoryData m_memory; 
        int m_dDistanceError = 10; 
        public string Inspect(InfoStrip infoStrip, MemoryData memory, eMode eMode)
        {
            m_infoStrip = infoStrip;
            m_memory = memory;
            string sInspect = Inspect(eMode);
            if (sInspect == "OK") return sInspect;
            m_infoStrip.p_eResult = InfoStrip.eResult.Rework;
            m_infoStrip.m_sError = sInspect;
            return sInspect; 
        }

        string Inspect(eMode eMode)
        {
            Parallel.For(0, 1, n => { m_aUnit[n].m_sInspect = InspectBlob(eMode, n); });
            foreach (Unit data in m_aUnit)
            {
                if (data.m_sInspect != "OK") return data.m_sInspect;
            }
            m_aResult[eMode].CalcStripPos();
            if (eMode == eMode.Inspect)
            {
                if (Get_dDistance() >= m_dDistanceError) return "Fiducial Distance Error";
                SetInfoPos();
            }
            return "OK";
        }

        Blob[] m_aBlob = new Blob[2] { new Blob(), new Blob() };
        Blob.eSort m_eSort = Blob.eSort.Size;
        public int[] m_nGV = new int[2] { 100, 0 };
        double m_dSizeError = 20;
        double m_dLendthError = 20;
        double m_dCenterError = 50; 
        string InspectBlob(eMode eMode, int iAOI)
        {
            Unit data = m_aUnit[iAOI]; 
            Blob blob = m_aBlob[iAOI];
            blob.RunBlob(m_memory, 0, data.m_aoiData.m_cp0, data.m_aoiData.m_sz, m_nGV[0], m_nGV[1], 3);
            blob.RunSort(m_eSort);
            if (blob.m_aSort.Count == 0) return "Find Fiducial Error"; 
            Blob.Island island = blob.m_aSort[0];
            Result.Unit result = m_aResult[eMode].m_aUnit[iAOI];
            result.m_maxLength = island.m_nLength;
            result.m_maxSize = island.m_nSize;
            result.m_rpCenter = island.m_rpCenter;
            data.m_aoiData.m_sDisplay = "Size = " + island.m_nSize + ", " + island.m_sz.ToString(); 
            if (eMode == eMode.Inspect)
            {
                if (Get_dSize(iAOI) >= m_dSizeError) return "Fiducial Size Error";
                if (Get_dLength(iAOI) >= m_dLendthError) return "Fiducial Length Error";
                if (Get_dCenter(iAOI) >= m_dCenterError) return "Fiducial Position Error";
            }
            return "OK";
        }

        void RunTreeInspect(Tree tree)
        {
            RunTreeInspectGV(tree.GetTree("GV")); 
            m_eSort = (Blob.eSort)tree.Set(m_eSort, m_eSort, "Sort", "Select Fiducial by");
            RunTreeInspectError(tree.GetTree("Error", false)); 
        }

        void RunTreeInspectGV(Tree tree)
        {
            m_nGV[0] = tree.Set(m_nGV[0], m_nGV[0], "Min", m_ST.Get("Gray Value Range (0~255)"));
            m_nGV[1] = tree.Set(m_nGV[1], m_nGV[1], "Max", m_ST.Get("Gray Value Range (0~255)"));
        }

        void RunTreeInspectError(Tree tree)
        {
            m_dSizeError = tree.Set(m_dSizeError, m_dSizeError, "Size", "Size Error (%)");
            m_dLendthError = tree.Set(m_dLendthError, m_dLendthError, "Length", "Length Error (%)");
            m_dCenterError = tree.Set(m_dCenterError, m_dCenterError, "Position", "Center Position Error (pixel)");
            m_dDistanceError = tree.Set(m_dDistanceError, m_dDistanceError, "Distance", "Distance Error (pixel)");
        }
        #endregion

        #region Tree
        public void RunTree(Tree tree)
        {
            RunTreeUnit(tree.GetTree("Unit", false, false));
            RunTreeInspect(tree.GetTree("Inspect", false));
        }
        #endregion

        #region IAOI
        public string p_id { get; set; }
        public bool p_bEnable { get; set; }
        #endregion

        Log m_log;
        public AOIStrip(string id, Log log)
        {
            p_id = id;
            m_log = log;
            p_bEnable = true;
            InitUnit();
            InitResult(); 
        }
    }
}
