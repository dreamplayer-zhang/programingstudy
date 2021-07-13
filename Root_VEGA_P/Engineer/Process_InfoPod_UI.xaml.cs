using Root_VEGA_P.Module;
using Root_VEGA_P_Vision.Module;
using System.Windows.Controls;
using System.Windows.Media;

namespace Root_VEGA_P.Engineer
{
    /// <summary>
    /// Process_InfoPod_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Process_InfoPod_UI : UserControl
    {
        public Process_InfoPod_UI()
        {
            InitializeComponent();
        }

        bool m_bTurn = false;

        public bool m_bInitExist = false;
        bool _bExist = false;
        public bool p_bExist
        {
            get { return _bExist; }
            set
            {
                if (_bExist == value) return;
                _bExist = value;
                textBlock.Foreground = value ? (m_bTurn ? Brushes.Red : Brushes.Black) : Brushes.LightGray;
            }
        }

        IRTRChild m_child = null;
        public void Init(InfoPod.ePod ePod, IRTRChild child)
        {
            textBlock.Text = ePod.ToString();
            textBlock.Foreground = Brushes.LightGray;
            m_child = child;
            m_ePod = ePod;
        }

        InfoPod.ePod m_ePod;
        RTR m_rtr = null;
        public void Init(InfoPod.ePod ePod, RTR rtr)
        {
            textBlock.Text = ePod.ToString();
            textBlock.Foreground = Brushes.LightGray;
            m_rtr = rtr;
            m_ePod = ePod;
        }

        public void OnTimer()
        {
            InfoPod infoPod = (m_child != null) ? m_child.p_infoPod : m_rtr.p_infoPod;
            if (infoPod == null) p_bExist = false; 
            else
            {
                m_bTurn = infoPod.p_bTurn;
                p_bExist = (infoPod.p_ePod==m_ePod); 
            }
        }
    }
}
