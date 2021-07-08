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
                    public CPoint m_szMap;
                    public void SetResult(CPoint szMap, string sMapResult)
                    {
                        m_szMap = szMap;
                        while (m_aUnit.Count < m_szMap.Y) m_aUnit.Add(new List<eResult>());
                        for (int yp = 0; yp < m_szMap.Y; yp++)
                        {
                            while (m_aUnit[yp].Count < m_szMap.X) m_aUnit[yp].Add(eResult.Good);
                        }
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
        }
        Data m_data = new Data(); 
        #endregion

        public void AddInfoStrip(InfoStrip infoStrip)
        {

        }

        public Summary()
        {
            m_data.InitStrip(); 
        }
    }
}
