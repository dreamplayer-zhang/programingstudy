using RootTools;
using System.Windows;

namespace Root_Vega.ManualJob
{
    /// <summary>
    /// ManualOCR_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ManualOCR_UI : Window
    {
        public ManualOCR_UI()
        {
            InitializeComponent();
        }

        ManualOCR m_manualOCR; 
        public void Init(ManualOCR manualOCR)
        {
            m_manualOCR = manualOCR;
            DataContext = manualOCR; 
        }

        private void buttonRun_Click(object sender, RoutedEventArgs e)
        {
            EQ.p_bStop = false; 
            EQ.p_eState = EQ.eState.Run;
            Close(); 
        }

        private void buttonStop_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
