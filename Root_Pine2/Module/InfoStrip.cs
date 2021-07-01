using Root_Pine2_Vision.Module;
using RootTools;
using RootTools.Trees;

namespace Root_Pine2.Module
{
    public class InfoStrip : NotifyProperty
    {
        #region Result
        public enum eResult
        {
            Init,
            Good,
            XOut,
            Rework,
            Error,
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
        public int p_nStrip { get; set; }
        public InfoStrip(int nStrip)
        {
            p_eMagazine = eMagazine.Magazine0; 
            p_nStrip = nStrip;
            p_id = "Strip." + nStrip.ToString("000"); 
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
        public string m_sLED; 
        public InfoStrip(eMagazine eMagazine, eMagazinePos eMagazinePos, int nStrip)
        {
            p_eMagazine = eMagazine;
            p_eMagazinePos = eMagazinePos;
            p_nStrip = nStrip;
            p_id = eMagazine.ToString() + "." + eMagazinePos.ToString() + "." + nStrip.ToString("00"); 
            m_sLED = ((p_eMagazinePos == eMagazinePos.Up) ? "Up" : "Dn") + p_nStrip.ToString("00");
        }

        public InfoStrip Clone()
        {
            return new InfoStrip(p_eMagazine, p_eMagazinePos, p_nStrip); 
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
            return (p_nStrip == infoStrip.p_nStrip); 
        }

        public void RunTreeMagazine(Tree tree, bool bVisible)
        {
            p_eMagazinePos = (eMagazinePos)tree.Set(p_eMagazinePos, p_eMagazinePos, "Magazine", "Magazine Position", bVisible);
            p_nStrip = tree.Set(p_nStrip, p_nStrip, "Strip", "Magazine Strip Slot ID (0 ~ 19)", bVisible);
            if (p_nStrip < 0) p_nStrip = 0;
            if (p_nStrip > 19) p_nStrip = 19;
        }
    }
}
