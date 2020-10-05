using RootTools;

namespace Root_ASIS
{
    public static class Strip
    {
        public static _Strip m_strip = new _Strip(); 

        public static bool p_bUseMGZ
        {
            get { return m_strip.p_bUseMGZ; }
            set { m_strip.p_bUseMGZ = value; }
        }

        public static RPoint m_szStripTeach = new RPoint();
        public static RPoint p_szStrip
        {
            get { return m_strip.p_szStrip; }
            set { m_strip.p_szStrip = value; }
        }
    }

    public class _Strip : NotifyProperty
    {
        bool _bUseMGZ = false; 
        public bool p_bUseMGZ
        {
            get { return _bUseMGZ; }
            set
            {
                if (_bUseMGZ == value) return;
                _bUseMGZ = value;
                OnPropertyChanged(); 
            }
        }

        RPoint _szStrip = new RPoint(77, 178); 
        public RPoint p_szStrip
        {
            get { return _szStrip; }
            set
            {
                if (_szStrip == value) return;
                _szStrip = value;
                OnPropertyChanged(); 
            }
        }
    }
}
