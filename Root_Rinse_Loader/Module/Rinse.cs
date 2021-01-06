using RootTools;

namespace Root_Rinse_Loader.Module
{
    public static class Rinse
    {
        public static _Rinse m_rinse = new _Rinse(); 

        public enum eMode
        {
            Magazine,
            Stack
        }
        
        public static eMode p_eMode
        {
            get { return m_rinse.p_eMode; }
            set { m_rinse.p_eMode = value; }
        }

        public static double p_widthStrip
        {
            get { return m_rinse.p_widthStrip; }
            set { m_rinse.p_widthStrip = value; }
        }
    }

    public class _Rinse : NotifyProperty
    {
        Rinse.eMode _eMode = Rinse.eMode.Magazine; 
        public Rinse.eMode p_eMode
        {
            get { return _eMode; }
            set
            {
                if (_eMode == value) return;
                _eMode = value;
                OnPropertyChanged(); 
            }
        }

        double _widthStrip = 77;
        public double p_widthStrip
        {
            get { return _widthStrip; }
            set
            {
                if (_widthStrip == value) return;
                _widthStrip = value;
                OnPropertyChanged(); 
            }
        }
    }
}
