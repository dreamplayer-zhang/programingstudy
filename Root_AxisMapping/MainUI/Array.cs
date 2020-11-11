using RootTools;
using System.Windows.Media;

namespace Root_AxisMapping.MainUI
{
    public class Array : NotifyProperty
    {
        #region Property
        public enum eState
        {
            Empty,
            Exist,
            Setup,
        };
        eState _eState = eState.Empty;
        public eState p_eState
        {
            get { return _eState; }
            set
            {
                if (_eState == value) return;
                _eState = value;
                OnPropertyChanged();
                ChangeBrush(false); 
            }
        }

        Brush _brush = Brushes.LightGray; 
        public Brush p_brush 
        {
            get { return _brush; }
            set 
            { 
                _brush = value;
                OnPropertyChanged(); 
            }
        }

        public void ChangeBrush(bool bBlink)
        {
            if (bBlink) p_brush = Brushes.AliceBlue;
            else
            {
                switch (_eState)
                {
                    case eState.Empty: p_brush = Brushes.LightGray; break;
                    case eState.Exist: p_brush = Brushes.Green; break;
                    case eState.Setup: p_brush = Brushes.YellowGreen; break;
                }
            }
        }

        public bool m_bEnable = false; 
        public RPoint m_rpCenter = new RPoint(); 
        #endregion

        #region UI
        Array_UI _ui = null; 
        public Array_UI p_ui
        {
            get
            {
                if (_ui == null)
                {
                    _ui = new Array_UI();
                    _ui.Init(this);
                }
                return _ui;
            }
        }

        public void OnActive()
        {
            m_mapping.OnActive(m_x);
        }

        public void OnSetup()
        {
            m_mapping.OnSetup(m_x); 
        }
        #endregion

        int m_x; 
        Mapping m_mapping; 
        public Array(Mapping mapping, int x)
        {
            m_mapping = mapping;
            m_x = x; 
            p_brush = Brushes.LightGray; 
        }
    }
}
