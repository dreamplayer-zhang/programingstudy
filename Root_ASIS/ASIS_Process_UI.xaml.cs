using RootTools;
using System.Windows;
using System.Windows.Controls;

namespace Root_ASIS
{
    /// <summary>
    /// ASIS_Process_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASIS_Process_UI : UserControl
    {
        public ASIS_Process_UI()
        {
            InitializeComponent();
        }

        ASIS_Process m_process;
        public void Init(ASIS_Process process)
        {
            m_process = process;
            this.DataContext = process;

            labelEQState.DataContext = EQ.m_EQ;
            checkBoxStop.DataContext = EQ.m_EQ;
            checkBoxPause.DataContext = EQ.m_EQ;
            checkBoxSimulate.DataContext = EQ.m_EQ;
        }

        private void buttonClearInfoReticle_Click(object sender, RoutedEventArgs e)
        {

        }

        private void buttonSetRecover_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
