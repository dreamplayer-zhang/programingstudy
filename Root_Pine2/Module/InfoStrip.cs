using Root_Pine2_Vision.Module;
using RootTools;
using RootTools.Trees;
using System;
using System.Collections.Generic;

namespace Root_Pine2.Module
{
    public class InfoStrip : NotifyProperty
    {
        #region Result
        public enum eResult
        {
            Init,
            GOOD,
            DEF,
            POS,
            BCD,
            Paper,
        }
        eResult _eResult = eResult.Init;
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

        Dictionary<Vision2D.eVision, bool> m_bInspect = new Dictionary<Vision2D.eVision, bool>();
        void InitInspect()
        {
            m_bInspect.Add(Vision2D.eVision.Top3D, false);
            m_bInspect.Add(Vision2D.eVision.Top2D, false);
            m_bInspect.Add(Vision2D.eVision.Bottom, false);
        }
        public void StartInspect(Vision2D.eVision eVision)
        {
            m_bInspect[eVision] = true; 
        }

        public bool p_bInspect
        {
            get { return m_bInspect[Vision2D.eVision.Top3D] || m_bInspect[Vision2D.eVision.Top2D] || m_bInspect[Vision2D.eVision.Bottom]; }
        }

        public Summary.Data m_summnayData = new Summary.Data(); 
        public string SetResult(Vision2D.eVision eVision, string sStripResult, string sX, string sY, string sMapResult)
        {
            string sResult = "OK";
            try
            {
                eResult eResult = GetResult(sStripResult);
                if (eResult == eResult.Init) return "Invalid Result"; 
                if (p_eResult < eResult) p_eResult = eResult; 
                int szX = Convert.ToInt32(sX);
                int szY = Convert.ToInt32(sY);
                m_summnayData.SetResult(eVision, eResult, new CPoint(szX, szY), sMapResult); 
            }
            catch (Exception e) { sResult = "SetResult Exception : " + e.Message; }
            m_bInspect[eVision] = false; 
            return sResult;
        }

        eResult GetResult(string sStripResult)
        {
            foreach (eResult eResult in Enum.GetValues(typeof(eResult)))
            {
                if (sStripResult == eResult.ToString()) return eResult; 
            }
            return eResult.Init; 
        }
        #endregion

        #region Boat Flow
        public Vision2D.eVision m_eVisionLoad = Vision2D.eVision.Top3D; 
        public Vision2D.eWorks m_eWorks = Vision2D.eWorks.A;
        #endregion

        string _id = ""; 
        public string p_id 
        { 
            get { return _id; }
            set
            {
                _id = value;
                OnPropertyChanged(); 
            }
        }
        public int p_iStrip { get; set; }
        public InfoStrip(int iStrip)
        {
            InitInspect(); 
            p_eMagazine = eMagazine.Magazine0; 
            p_iStrip = iStrip;
            p_id = iStrip.ToString("0000"); 
        }

        public bool m_bPaper = false; 
        public InfoStrip(bool bPaper)
        {
            m_bPaper = bPaper; 
        }

        public enum eMagazine
        {
            Magazine0,
            Magazine1,
            Magazine2,
            Magazine3,
            Magazine4,
            Magazine5,
            Magazine6,
            Magazine7,
        }
        public eMagazine p_eMagazine { get; set; }
        public enum eMagazinePos
        {
            Up,
            Down
        }
        public eMagazinePos p_eMagazinePos { get; set; }
        public int m_iBundle = 0; 
        public string m_sLED; 
        public InfoStrip(eMagazine eMagazine, eMagazinePos eMagazinePos, int iBundle, int iStrip)
        {
            p_eMagazine = eMagazine;
            p_eMagazinePos = eMagazinePos;
            m_iBundle = iBundle; 
            p_iStrip = iStrip;
            p_id = iStrip.ToString("0000"); 
            m_sLED = ((p_eMagazinePos == eMagazinePos.Up) ? "UP" : "DN") + p_iStrip.ToString("00");
        }

        public InfoStrip Clone()
        {
            return new InfoStrip(p_eMagazine, p_eMagazinePos, m_iBundle, p_iStrip); 
        }

        public delegate void dgOnDispose(InfoStrip infoStrip);
        public event dgOnDispose OnDispose;
        public void Dispose()
        {
            if (OnDispose != null) OnDispose(this); 
        }

        public bool IsSame(InfoStrip infoStrip)
        {
            if (p_eMagazine != infoStrip.p_eMagazine) return false;
            if (p_eMagazinePos != infoStrip.p_eMagazinePos) return false;
            return (p_iStrip == infoStrip.p_iStrip); 
        }

        public void RunTreeMagazine(Tree tree, bool bVisible)
        {
            p_eMagazinePos = (eMagazinePos)tree.Set(p_eMagazinePos, p_eMagazinePos, "Magazine", "Magazine Position", bVisible);
            p_iStrip = tree.Set(p_iStrip, p_iStrip, "Strip", "Magazine Strip Slot ID (0 ~ 19)", bVisible);
            if (p_iStrip < 0) p_iStrip = 0;
            if (p_iStrip > 19) p_iStrip = 19;
        }
    }
}
