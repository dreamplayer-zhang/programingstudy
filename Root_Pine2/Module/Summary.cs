using Root_Pine2_Vision.Module;
using RootTools;
using System;
using System.Collections.Generic;

namespace Root_Pine2.Module
{
    public class Summary
    {
        #region Data
        public class Data
        {
            public enum eVision
            {
                Top3D,
                Top2D,
                Bottom,
                Total
            }
            public class Strip
            {
                public class Unit
                {
                    public enum eResult
                    {
                        Good,
                        Bad,
                        PosError,
                        XOut,
                        Unknown
                    }
                    public List<List<eResult>> m_aUnit = new List<List<eResult>>();
                    public CPoint m_szMap = null;
                    public void SetResult(CPoint szMap, string sMapResult)
                    {
                        m_szMap = szMap;
                        InitMap(); 
                        int x = 0;
                        int y = 0;
                        foreach (char c in sMapResult)
                        {
                            switch (c)
                            {
                                case '0': m_aUnit[y][x] = eResult.Bad; break;
                                case '1': m_aUnit[y][x] = eResult.Good; break;
                                case '4': m_aUnit[y][x] = eResult.PosError; break;
                                case '5': m_aUnit[y][x] = eResult.XOut; break;
                                default: m_aUnit[y][x] = eResult.Unknown; break;
                            }
                            x++;
                            if (x >= m_szMap.X)
                            {
                                x = 0;
                                y++;
                            }
                        }
                    }

                    public string SetSort(Unit unit)
                    {
                        if (m_szMap == null) m_szMap = new CPoint(unit.m_szMap); 
                        if (m_szMap.X != unit.m_szMap.X) return "Map Size not Same";
                        if (m_szMap.Y != unit.m_szMap.Y) return "Map Size not Same";
                        InitMap(); 
                        for (int y = 0; y < m_szMap.Y; y++)
                        {
                            for (int x = 0; x < m_szMap.X; x++) m_aUnit[y][x] = (eResult)Math.Max((int)m_aUnit[y][x], (int)unit.m_aUnit[y][x]); 
                        }
                        return "OK"; 
                    }

                    void InitMap()
                    {
                        while (m_aUnit.Count < m_szMap.Y) m_aUnit.Add(new List<eResult>());
                        for (int yp = 0; yp < m_szMap.Y; yp++)
                        {
                            while (m_aUnit[yp].Count < m_szMap.X) m_aUnit[yp].Add(eResult.Good);
                        }
                    }
                }
                public Unit m_unit = new Unit(); 

                public enum eResult
                {
                    Good,
                    Defect,
                    PosError,
                    Barcode
                }
                public eResult m_eResult = eResult.Good;
                
                public void SetResult(InfoStrip.eResult result, CPoint szMap, string sMapResult)
                {
                    switch (result)
                    {
                        case InfoStrip.eResult.GOOD: 
                            m_eResult = eResult.Good;
                            m_unit.SetResult(szMap, sMapResult); 
                            break;
                        case InfoStrip.eResult.DEF: 
                            m_eResult = eResult.Defect;
                            m_unit.SetResult(szMap, sMapResult); 
                            break;
                        case InfoStrip.eResult.POS: m_eResult = eResult.PosError; break;
                        case InfoStrip.eResult.BCD: m_eResult = eResult.Barcode; break; 
                    }
                }

                public string SetSort(Strip strip)
                {
                    m_eResult = (eResult)Math.Max((int)m_eResult, (int)strip.m_eResult);
                    return m_unit.SetSort(strip.m_unit); 
                }
            }
            public Dictionary<eVision, Strip> m_aStrip = new Dictionary<eVision, Strip>();
            public void InitStrip()
            {
                m_aStrip.Add(eVision.Top3D, new Strip());
                m_aStrip.Add(eVision.Top2D, new Strip());
                m_aStrip.Add(eVision.Bottom, new Strip());
                m_aStrip.Add(eVision.Total, new Strip());
            }

            public void SetResult(Vision2D.eVision vision, InfoStrip.eResult result, CPoint szMap, string sMapResult)
            {
                eVision eVision = eVision.Top3D; 
                switch (vision)
                {
                    case Vision2D.eVision.Top3D: eVision = eVision.Top3D; break;
                    case Vision2D.eVision.Top2D: eVision = eVision.Top2D; break;
                    case Vision2D.eVision.Bottom: eVision = eVision.Bottom; break;
                }
                m_aStrip[eVision].SetResult(result, szMap, sMapResult); 
            }

            public string SetSort(bool b3D)
            {
                string sRun = "";
                if (b3D)
                {
                    sRun = m_aStrip[eVision.Total].SetSort(m_aStrip[eVision.Top3D]);
                    if (sRun != "OK") return sRun;
                }
                sRun = m_aStrip[eVision.Total].SetSort(m_aStrip[eVision.Top2D]);
                if (sRun != "OK") return sRun;
                sRun = m_aStrip[eVision.Total].SetSort(m_aStrip[eVision.Bottom]);
                if (sRun != "OK") return sRun;
                return "OK";
            }
        }
        public Data m_data = new Data();
        #endregion

        #region Counter
        public class Count
        {
            public Dictionary<Data.Strip.eResult, int> m_aCount = new Dictionary<Data.Strip.eResult, int>(); 
            public void Clear()
            {
                foreach (Data.Strip.eResult eResult in Enum.GetValues(typeof(Data.Strip.eResult))) m_aCount[eResult] = 0;
            }

            public void Add(Data.Strip.eResult eResult)
            {
                m_aCount[eResult]++; 
            }

            public Count()
            {
                foreach (Data.Strip.eResult eResult in Enum.GetValues(typeof(Data.Strip.eResult))) m_aCount.Add(eResult, 0); 
            }
        }
        public Dictionary<Data.eVision, Count> m_countStrip = new Dictionary<Data.eVision, Count>(); 
        public void ClearCount()
        {
            foreach (Data.eVision eVision in Enum.GetValues(typeof(Data.eVision))) m_countStrip[eVision].Clear(); 
        }
        #endregion

        public string SetSort(bool b3D, InfoStrip infoStrip)
        {
            m_data = infoStrip.m_summnayData;
            string sRun = m_data.SetSort(b3D);
            if (sRun != "OK") return sRun;
            foreach (Data.eVision eVision in Enum.GetValues(typeof(Data.eVision)))
            {
                m_countStrip[eVision].Add(m_data.m_aStrip[eVision].m_eResult);
            }
            switch (m_data.m_aStrip[Data.eVision.Total].m_eResult)
            {
                case Data.Strip.eResult.Good:
                case Data.Strip.eResult.Defect:
                    break; 
            }
            //forget
            return "OK";
        }

        public Summary()
        {
            m_data.InitStrip();
            foreach (Data.eVision eVision in Enum.GetValues(typeof(Data.eVision))) m_countStrip.Add(eVision, new Count());
        }
    }
}
