using System.Windows.Controls;
using System.Windows.Input;

namespace RootTools.DMC
{
    /// <summary>
    /// Axis_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class DMCAxis_UI : UserControl
    {
        public DMCAxis_UI()
        {
            InitializeComponent();
        }

        DMCAxis m_dmcAxis;
        public void Init(DMCAxis dmcAxis)
        {
            m_dmcAxis = dmcAxis;
            this.DataContext = dmcAxis;
        }

        private void btnJogM_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            //if (m_dmcAxis.m_bJog) m_dmcAxis.Jog_Minus_Stop(); 
        }

        private void btnJogM_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            m_dmcAxis.Jog_Minus_Stop();
        }

        private void btnJogP_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            m_dmcAxis.Jog_Plus_Move();
        }

        private void btnJogP_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            //if (m_dmcAxis.m_bJog) m_dmcAxis.Jog_Plus_Stop(); 
        }

        private void btnJogP_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            m_dmcAxis.Jog_Plus_Stop();
        }

        private void btnJogM_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            m_dmcAxis.Jog_Plus_Move();
        }
    }
}
