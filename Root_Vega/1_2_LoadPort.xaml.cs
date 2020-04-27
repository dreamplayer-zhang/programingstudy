using Root_Vega.Module;
using RootTools.Gem;
using System.Windows.Controls;

namespace Root_Vega
{
    /// <summary>
    /// _1_2_LoadPort.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class _1_2_LoadPort : UserControl
    {
        public _1_2_LoadPort()
        {
            InitializeComponent();
        }

        Loadport m_loadport; 
        public void Init(Loadport loadport)
        {
            m_loadport = loadport;
            this.DataContext = loadport;
            textBoxPodID.DataContext = loadport.m_infoPod;
            toggleButtonAccessLPAuto.DataContext = loadport.m_infoPod;
            toggleButtonAccessLPManual.DataContext = loadport.m_infoPod;
            textBoxLotID.DataContext = loadport.m_infoPod.m_aGemSlot[0];
            textBoxSlotID.DataContext = loadport.m_infoPod.m_aGemSlot[0];
        }

        private void toggleButtonAccessLPAuto_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            m_loadport.m_infoPod.p_eReqAccessLP = GemCarrierBase.eAccessLP.Auto; 
        }

        private void toggleButtonAccessLPManual_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            m_loadport.m_infoPod.p_eReqAccessLP = GemCarrierBase.eAccessLP.Manual; 
        }
    }
}
