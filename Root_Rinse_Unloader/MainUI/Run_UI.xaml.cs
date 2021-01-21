using Root_Rinse_Unloader.Engineer;
using RootTools;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Root_Rinse_Unloader.MainUI
{
    /// <summary>
    /// Run_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Run_UI : UserControl
    {
        public Run_UI()
        {
            InitializeComponent();
        }
        
        RinseU_Engineer m_engineer;
        RinseU_Handler m_handler;
        public void Init(RinseU_Engineer engineer)
        {
            m_engineer = engineer;
            m_handler = (RinseU_Handler)engineer.ClassHandler();
            rinseUI.Init(m_handler.m_rinse);
            InitTimer();
        }

        #region Timer
        DispatcherTimer m_timer = new DispatcherTimer();
        void InitTimer()
        {
            m_timer.Interval = TimeSpan.FromSeconds(0.01);
            m_timer.Tick += M_timer_Tick;
            m_timer.Start();
        }

        private void M_timer_Tick(object sender, EventArgs e)
        {
            buttonHome.IsEnabled = EQ.p_eState != EQ.eState.Run;
            buttonStart.IsEnabled = EQ.p_eState == EQ.eState.Ready;
            buttonPause.IsEnabled = EQ.p_eState == EQ.eState.Run;
            buttonReset.IsEnabled = EQ.p_eState == EQ.eState.Error;
            buttonPickerSet.IsEnabled = EQ.p_eState == EQ.eState.Ready;
        }
        #endregion

        #region UI Control
        private void buttonHome_Click(object sender, RoutedEventArgs e)
        {
            EQ.p_eState = EQ.eState.Home;
        }

        private void buttonStart_Click(object sender, RoutedEventArgs e)
        {
            EQ.p_eState = EQ.eState.Run;
        }

        private void buttonPause_Click(object sender, RoutedEventArgs e)
        {
            EQ.p_eState = EQ.eState.Ready;
        }

        private void buttonReset_Click(object sender, RoutedEventArgs e)
        {
            EQ.p_eState = EQ.eState.Ready;
        }

        private void buttonPickerSet_Click(object sender, RoutedEventArgs e)
        {
            m_handler.StartPickerSet();
        }
        #endregion
    }
}
