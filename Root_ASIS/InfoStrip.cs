using Root_ASIS.AOI;
using RootTools;
using System;
using System.Collections.Generic;

namespace Root_ASIS
{
    public class InfoStrip : NotifyProperty
    {
        #region Property
        public int p_iStrip { get; set; }
        #endregion

        #region Strip Result
        public enum eResult
        {
            Xout,
            Error,
            Rework
        }
        eResult _eResult = eResult.Xout; 
        public eResult p_eResult
        {
            get { return _eResult; }
            set
            {
                if (_eResult == value) return;
                _eResult = value;
                OnPropertyChanged(); 
            }
        }

        public int m_nXout = 0; 
        public string m_sError = ""; 
        #endregion

        #region Unit Result
        public class UnitResult
        {
            public enum eLogic
            {
                Or,
                And,
                Match,
                Match_False,
                Match_True,
            }

            public bool m_bInspect = false;
            public string m_sInspect = "";

            public string CalcResult(eLogic eLogic, bool bInspect)
            {
                switch (eLogic)
                {
                    case eLogic.Or: m_bInspect |= bInspect; break;
                    case eLogic.And: m_bInspect &= bInspect; break;
                    case eLogic.Match:
                        if (m_bInspect == bInspect) return "OK";
                        m_infoStrip.p_eResult = eResult.Error;
                        m_infoStrip.m_sError = "Unit : " + m_nUnit.ToString() + ", Logic Error : Miss Match";
                        break;
                    case eLogic.Match_False:
                        if (m_bInspect != false) return "OK";
                        if (m_bInspect == bInspect) return "OK";
                        m_infoStrip.p_eResult = eResult.Error;
                        m_infoStrip.m_sError = "Unit : " + m_nUnit.ToString() + ", Logic Error : Match False";
                        break;
                    case eLogic.Match_True:
                        if (m_bInspect != true) return "OK";
                        if (m_bInspect == bInspect) return "OK";
                        m_infoStrip.p_eResult = eResult.Error;
                        m_infoStrip.m_sError = "Unit : " + m_nUnit.ToString() + ", Logic Error : Match False";
                        break;
                }
                return "OK";
            }

            InfoStrip m_infoStrip;
            int m_nUnit = 0;
            public UnitResult(InfoStrip infoStrip, int nUnit)
            {
                m_infoStrip = infoStrip;
                m_nUnit = nUnit; 
            }
        }
        List<UnitResult> m_aUnitResult = new List<UnitResult>(); 
        public UnitResult GetUnitResult(int iUnit)
        {
            while (m_aUnitResult.Count < iUnit) m_aUnitResult.Add(new UnitResult(this, m_aUnitResult.Count)); 
            return m_aUnitResult[iUnit]; 
        }
        #endregion

        #region AOI Position
        RPoint m_rpShift = new RPoint();
        double m_fAngle = 0;
        double m_fCos = 0;
        double m_fSin = 0; 
        public void SetInfoPos(RPoint rpShift, double fAngle)
        {
            m_rpShift = rpShift;
            m_fAngle = fAngle;
            m_fCos = Math.Cos(m_fAngle);
            m_fSin = Math.Sin(m_fAngle); 
        }

        public AOIData GetInfoPos(AOIData aoiData0)
        {
            AOIData aoiData1 = aoiData0.Clone();
            RPoint rpMid = new RPoint(aoiData1.m_cp0 + aoiData1.m_sz);
            rpMid /= 2;
            RPoint drp = rpMid - m_rpShift;
            double dx = m_fCos * drp.X - m_fSin * drp.Y + m_rpShift.X - rpMid.X; 
            double dy = m_fCos * drp.Y + m_fSin * drp.X + m_rpShift.Y - rpMid.Y;
            aoiData1.m_cp0.X += (int)Math.Round(dx);
            aoiData1.m_cp0.Y += (int)Math.Round(dy);
            return aoiData1; 
        }
        #endregion

        public InfoStrip(int iStrip)
        {
            p_iStrip = iStrip;
        }
    }
}
