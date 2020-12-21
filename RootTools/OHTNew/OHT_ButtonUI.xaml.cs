using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RootTools.OHTNew
{
    /// <summary>
    /// OHT_ButtonUI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class OHT_ButtonUI : UserControl
    {
        public OHT_ButtonUI()
        {
            InitializeComponent();
        }

        #region DI
        OHT.DI m_DI = null; 
        public void Init(OHT.DI DI, OHT_UI uiOHT)
        {
            m_DI = DI;
            DataContext = DI;
            m_uiOHT = uiOHT;
        }
        #endregion

        #region DO
        OHT.DO m_DO = null;
        public void Init(OHT.DO DO, OHT_UI uiOHT)
        {
            m_DO = DO;
            DataContext = DO;
            m_uiOHT = uiOHT;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if ((m_DI != null) && EQ.p_bSimulate) m_DI.m_di.m_bitDI.p_bOn = !m_DI.m_di.m_bitDI.p_bOn; 
            if (m_DO != null) m_DO.Toggle(); 
        }
        #endregion

        #region Timer
        OHT_UI m_uiOHT;
        public void OnTimer(bool bBlink)
        {
            bool bOn = p_bOn;
            button.Foreground = bOn ? Brushes.Red : Brushes.Black;
            if (p_bWait) bOn = bBlink ? bOn : !bOn;
            button.Background = bOn ? Brushes.Yellow : Brushes.DimGray;
        }

        bool p_bOn
        {
            get
            {
                if (m_DI != null) return m_DI.p_bOn;
                if (m_DO != null) return m_DO.p_bOn;
                return false; 
            }
        }

        bool p_bWait
        {
            get
            {
                if (m_DI != null) return m_DI.p_bWait;
                if (m_DO != null) return m_DO.p_bWait;
                return false;
            }
        }
        #endregion
    }
}
