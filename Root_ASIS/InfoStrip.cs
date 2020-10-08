using RootTools;

namespace Root_ASIS
{
    public class InfoStrip : NotifyProperty
    {
        #region Property
        public int p_iStrip { get; set; }
        #endregion

        #region Result
        public enum eResult
        {
            Good,
            Xout,
            Error,
            Rework
        }
        eResult _eResult = eResult.Good; 
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

        public int m_nXout = -1;
        #endregion

        #region AOI Position
        RPoint m_rpShift = new RPoint();
        double m_fAngle = 0; 
        public void SetInfoPos(RPoint rpShift, double fAngle)
        {
            m_rpShift = rpShift;
            m_fAngle = fAngle; 
        }
        #endregion

        public InfoStrip(int iStrip)
        {
            p_iStrip = iStrip;
        }
    }
}
