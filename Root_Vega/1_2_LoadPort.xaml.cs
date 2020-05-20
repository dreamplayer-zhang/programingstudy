using Root_Vega.ManualJob;
using Root_Vega.Module;
using RootTools.Gem;
using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace Root_Vega
{
    /// <summary>
    /// _1_2_LoadPort.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class _1_2_LoadPort : UserControl
    {
        ManualJobSchedule manualjob;
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
            //toggleButtonAccessLPAuto.DataContext = loadport.m_infoPod;
            //toggleButtonAccessLPManual.DataContext = loadport.m_infoPod;
            textBoxLotID.DataContext = loadport.m_infoPod.m_aGemSlot[0];
            textBoxSlotID.DataContext = loadport.m_infoPod.m_aGemSlot[0];

            m_timer.Interval = TimeSpan.FromMilliseconds(20);
            m_timer.Tick += M_timer_Tick;
            m_timer.Start();

            manualjob = new ManualJobSchedule(m_loadport.p_id, m_loadport.m_log);
        }

        DispatcherTimer m_timer = new DispatcherTimer();
        private void M_timer_Tick(object sender, EventArgs e)
        {
            borderPlaced.Background = m_loadport.m_diPlaced.p_bIn ? Brushes.LightGreen : null;
            borderPresent.Background = m_loadport.m_diPresent.p_bIn ? Brushes.LightGreen : null;
            borderLoad.Background = m_loadport.m_diLoad.p_bIn ? Brushes.LightGreen : null;
            borderUnload.Background = m_loadport.m_diUnload.p_bIn ? Brushes.LightGreen : null;
            borderAlarm.Background = (m_loadport.p_eState == RootTools.Module.ModuleBase.eState.Error) ? Brushes.Red : null;
            bool bAuto = (m_loadport.m_infoPod.p_eReqAccessLP == GemCarrierBase.eAccessLP.Auto); 
            borderAccessAuto.Background = bAuto ? Brushes.LightGreen : null;
            borderAccessManual.Background = bAuto ? null : Brushes.LightGreen; 
        }

        private void ToggleButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            manualjob.ShowPopup();
        }
    }
}
