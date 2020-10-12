using RootTools;

namespace Root_ASIS
{
    public static class Strip
    {
        public enum eUnitOrder
        {
            X_Y,
            Y_X,
            InvX_Y,
            InvY_X
        };

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

        public static CPoint p_szBlock
        {
            get { return m_strip.p_szBlock; }
            set { m_strip.p_szBlock = value; }
        }

        public static CPoint p_szUnit
        {
            get { return m_strip.p_szUnit; }
            set { m_strip.p_szUnit = value; }
        }

        public static int p_lUnit { get { return p_szUnit.X * p_szUnit.Y * p_szBlock.X * p_szBlock.Y; } }

        public static eUnitOrder p_eUnitOrder
        { 
            get { return m_strip.p_eUnitOrder; }
            set { m_strip.p_eUnitOrder = value; }
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

        CPoint _szBlock = new CPoint(1, 1); 
        public CPoint p_szBlock
        {
            get { return _szBlock; }
            set
            {
                if (_szBlock == value) return;
                _szBlock = value;
                OnPropertyChanged(); 
            }
        }

        CPoint _szUnit = new CPoint(1, 1);
        public CPoint p_szUnit
        {
            get { return _szUnit; }
            set
            {
                if (_szUnit == value) return;
                _szUnit = value;
                OnPropertyChanged();
            }
        }

        Strip.eUnitOrder _eUnitOrder = Strip.eUnitOrder.X_Y; 
        public Strip.eUnitOrder p_eUnitOrder
        {
            get { return _eUnitOrder; }
            set
            {
                if (_eUnitOrder == value) return;
                _eUnitOrder = value;
                OnPropertyChanged(); 
            }
        }


    }
}
