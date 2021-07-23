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
                    public CPoint m_szMap = new CPoint();
                    public void SetResult(CPoint szMap, bool bTop, string sMapResult)
                    {
                        m_szMap = new CPoint(szMap);
                        InitMap(); 
                        int x = bTop ? 0 : szMap.X - 1;
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
                            if (bTop)
                            {
                                x++;
                                if (x >= m_szMap.X)
                                {
                                    x = x = bTop ? 0 : szMap.X - 1;
                                    y++;
                                }
                            }
                            else
                            {
                                x--;
                                if (x < 0)
                                {
                                    x = x = bTop ? 0 : szMap.X - 1;
                                    y++;
                                }
                            }
                        }
                    }

                    public string SetSort(Unit unit)
                    {
                        if (unit.m_szMap.X == 0) return "OK";
                        if (m_szMap.X == 0) m_szMap = new CPoint(unit.m_szMap);
                        if (m_szMap.X != unit.m_szMap.X) return "Map Size not Same";
                        if (m_szMap.Y != unit.m_szMap.Y) return "Map Size not Same";
                        InitMap();
                        for (int y = 0; y < m_szMap.Y; y++)
                        {
                            for (int x = 0; x < m_szMap.X; x++) m_aUnit[y][x] = (eResult)Math.Max((int)m_aUnit[y][x], (int)unit.m_aUnit[y][x]);
                        }
                        return "OK";
                    }

                    public void ClearSort()
                    {
                        for (int y = 0; y < m_szMap.Y; y++)
                        {
                            for (int x = 0; x < m_szMap.X; x++) m_aUnit[y][x] = eResult.Good;
                        }
                    }

                    void InitMap()
                    {
                        while (m_aUnit.Count < m_szMap.Y) m_aUnit.Add(new List<eResult>());
                        for (int yp = 0; yp < m_szMap.Y; yp++)
                        {
                            while (m_aUnit[yp].Count < m_szMap.X) m_aUnit[yp].Add(eResult.Good);
                        }
                    }

                    public Dictionary<eResult, int> m_aCount = new Dictionary<eResult, int>();
                    public void CalcCount()
                    {
                        foreach (eResult eResult in Enum.GetValues(typeof(eResult))) m_aCount[eResult] = 0;
                        if (m_szMap.X == 0) return; 
                        for (int y = 0; y < m_szMap.Y; y++)
                        {
                            for (int x = 0; x < m_szMap.X; x++) m_aCount[m_aUnit[y][x]]++;
                        }
                    }

                    public Unit()
                    {
                        foreach (eResult eResult in Enum.GetValues(typeof(eResult))) m_aCount.Add(eResult, 0); 
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
                
                public void SetResult(InfoStrip.eResult result, CPoint szMap, bool bTop, string sMapResult)
                {
                    switch (result)
                    {
                        case InfoStrip.eResult.GOOD: m_eResult = eResult.Good; break;
                        case InfoStrip.eResult.DEF: m_eResult = eResult.Defect; break;
                        case InfoStrip.eResult.POS: m_eResult = eResult.PosError; break;
                        case InfoStrip.eResult.BCD: m_eResult = eResult.Barcode; break; 
                    }
                    m_unit.SetResult(szMap, bTop, sMapResult);
                }

                public string SetSort(Strip strip)
                {
                    m_eResult = (eResult)Math.Max((int)m_eResult, (int)strip.m_eResult);
                    return m_unit.SetSort(strip.m_unit); 
                }

                public eVision m_eVision; 
                public Strip(eVision eVision)
                {
                    m_eVision = eVision; 
                }

                public Strip()
                {
                }
            }

            public void SetResult(eVision vision, InfoStrip.eResult result, CPoint szMap, string sMapResult)
            {
                eVision eVision = eVision.Top3D; 
                switch (vision)
                {
                    case eVision.Top3D: eVision = eVision.Top3D; break;
                    case eVision.Top2D: eVision = eVision.Top2D; break;
                    case eVision.Bottom: eVision = eVision.Bottom; break;
                }
                m_aStrip[eVision].SetResult(result, szMap, vision != eVision.Bottom, sMapResult); 
            }

            public Strip.eResult GetResult()
            {
                Strip.eResult eResult = Strip.eResult.Good; 
                foreach (eVision eVision in Enum.GetValues(typeof(eVision)))
                {
                    eResult = (Strip.eResult)Math.Max((int)eResult, (int)m_aStrip[eVision].m_eResult); 
                }
                return eResult; 
            }

            public CPoint m_szMap = new CPoint(); 
            public string SetSort(bool b3D)
            {
                m_szMap.X = 0;
                m_szMap.Y = 0; 
                string sRun = "";
                if (b3D)
                {
                    sRun = m_stripTotal.SetSort(m_aStrip[eVision.Top3D]);
                    CalcMapSize(m_aStrip[eVision.Top3D].m_unit.m_szMap); 
                    if (sRun != "OK") return sRun;
                }
                sRun = m_stripTotal.SetSort(m_aStrip[eVision.Top2D]);
                CalcMapSize(m_aStrip[eVision.Top2D].m_unit.m_szMap);
                if (sRun != "OK") return sRun;
                sRun = m_stripTotal.SetSort(m_aStrip[eVision.Bottom]);
                CalcMapSize(m_aStrip[eVision.Bottom].m_unit.m_szMap);
                if (sRun != "OK") return sRun;
                return "OK";
            }

            void CalcMapSize(CPoint szMap)
            {
                if (szMap.X == 0) szMap = new CPoint(0, 0); 
                m_szMap.X = Math.Max(m_szMap.X, szMap.X);
                m_szMap.Y = Math.Max(m_szMap.Y, szMap.Y);
            }

            public string m_sStripID; 
            public Dictionary<eVision, Strip> m_aStrip = new Dictionary<eVision, Strip>();
            public Strip m_stripTotal = new Strip(); 
            public Data()
            {
                foreach (eVision eVision in Enum.GetValues(typeof(eVision))) m_aStrip.Add(eVision, new Strip(eVision)); 
            }
        }
        public Data m_data = new Data();
        #endregion

        #region Counter
        public class CountStrip
        {
            public Dictionary<Data.Strip.eResult, int> m_aCount = new Dictionary<Data.Strip.eResult, int>(); 
            public void Clear()
            {
                foreach (Data.Strip.eResult eResult in Enum.GetValues(typeof(Data.Strip.eResult))) m_aCount[eResult] = 0;
            }

            public void AddResult(Data.Strip.eResult eResult)
            {
                m_aCount[eResult]++; 
            }

            public CountStrip()
            {
                foreach (Data.Strip.eResult eResult in Enum.GetValues(typeof(Data.Strip.eResult))) m_aCount.Add(eResult, 0); 
            }
        }
        public Dictionary<eVision, CountStrip> m_countStrip = new Dictionary<eVision, CountStrip>();
        public CountStrip m_countStripTotal = new CountStrip(); 

        public class CountUnit
        {
            public Dictionary<Data.Strip.Unit.eResult, int> m_aCount = new Dictionary<Data.Strip.Unit.eResult, int>();
            public void Clear()
            {
                foreach (Data.Strip.Unit.eResult eResult in Enum.GetValues(typeof(Data.Strip.Unit.eResult))) m_aCount[eResult] = 0;
            }

            public void AddResult(Data.Strip.Unit unit)
            {
                foreach (Data.Strip.Unit.eResult eResult in Enum.GetValues(typeof(Data.Strip.Unit.eResult)))
                {
                    m_aCount[eResult] += unit.m_aCount[eResult];
                }
            }

            public CountUnit()
            {
                foreach (Data.Strip.Unit.eResult eResult in Enum.GetValues(typeof(Data.Strip.Unit.eResult))) m_aCount.Add(eResult, 0);
            }
        }
        public Dictionary<eVision, CountUnit> m_countUnit = new Dictionary<eVision, CountUnit>();
        public CountUnit m_countUnitTotal = new CountUnit(); 

        public void ClearCount()
        {
            m_countStripTotal.Clear(); 
            foreach (eVision eVision in Enum.GetValues(typeof(eVision)))
            {
                m_countStrip[eVision].Clear();
                m_countUnit[eVision].Clear();
            }
            m_bUpdated = true;
        }
        #endregion

        #region Time
        string m_sLot = "";
        public string m_sLotStart = "";
        public DateTime m_dtLotStart;
        public void LotStart(string sLot)
        {
            if (sLot == m_sLot) return;
            m_sLot = sLot;
            m_dtLotStart = DateTime.Now;
            m_sLotStart = m_dtLotStart.ToString("HH:mm:ss"); 
        }

        public string m_sLotTime = "";
        public string m_sTactTime = "";
        public string m_sTactAve = "";
        public void CheckTime()
        {
            DateTime dt = DateTime.Now;
            TimeSpan ts = (dt - m_dtLotStart); 
            m_sLotTime = ts.Hours.ToString("00") + ":" + ts.Minutes.ToString("00") + ":" + ts.Seconds.ToString("00");
            AddDateTime(dt);
            if (m_aTime.Count < 2) return;
            int nTime = m_aTime.Count - 1;
            m_sTactTime = GetTactString((m_aTime[nTime] - m_aTime[nTime - 1]).Milliseconds); 
            m_sTactAve = GetTactString((m_aTime[nTime] - m_aTime[0]).Milliseconds / nTime);
        }

        List<DateTime> m_aTime = new List<DateTime>();
        void AddDateTime(DateTime dt)
        {
            m_aTime.Add(dt);
            while (m_aTime.Count > 6) m_aTime.RemoveAt(0); 
        }

        string GetTactString(int ms)
        {
            int sec = ms / 1000;
            ms %= 1000;
            return sec.ToString() + "." + (ms / 10).ToString("00"); 
        }
        #endregion

        public bool m_bUpdated = false; 
        public string SetSort(bool b3D, InfoStrip infoStrip)
        {
            //return "OK";
            m_data = infoStrip.m_summary;
            m_data.m_sStripID = infoStrip.p_id;
            m_data.m_stripTotal.m_eResult = Data.Strip.eResult.Good; 
            m_data.m_stripTotal.m_unit.ClearSort(); 
            string sRun = m_data.SetSort(b3D);
            if (sRun != "OK") return sRun;
            m_countStripTotal.AddResult(m_data.m_stripTotal.m_eResult); 
            foreach (eVision eVision in Enum.GetValues(typeof(eVision)))
            {
                m_countStrip[eVision].AddResult(m_data.m_aStrip[eVision].m_eResult);
            }
            foreach (eVision eVision in Enum.GetValues(typeof(eVision))) m_data.m_aStrip[eVision].m_unit.CalcCount();
            switch (m_data.m_stripTotal.m_eResult)
            {
                case Data.Strip.eResult.Good:
                case Data.Strip.eResult.Defect:
                    foreach (eVision eVision in Enum.GetValues(typeof(eVision)))
                    {
                        m_countUnit[eVision].AddResult(m_data.m_aStrip[eVision].m_unit);
                    }
                    break; 
            }
            CheckTime(); 
            m_bUpdated = true; 
            return "OK";
        }

        public Summary()
        {
            foreach (eVision eVision in Enum.GetValues(typeof(eVision)))
            {
                m_countStrip.Add(eVision, new CountStrip());
                m_countUnit.Add(eVision, new CountUnit());
            }
        }
    }
}
