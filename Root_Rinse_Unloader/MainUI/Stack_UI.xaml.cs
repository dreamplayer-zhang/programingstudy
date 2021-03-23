using Root_Rinse_Unloader.Module;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace Root_Rinse_Unloader.MainUI
{
    /// <summary>
    /// Stack_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Stack_UI : UserControl
    {
        public Stack_UI()
        {
            InitializeComponent();
        }

        Storage m_storage;
        Loader m_loader;
        public void Init(Storage storage, Loader loader)
        {
            m_storage = storage;
            m_loader = loader;
            InitTimer();
        }

        DispatcherTimer m_timer = new DispatcherTimer();
        void InitTimer()
        {
            m_timer.Interval = TimeSpan.FromMilliseconds(400);
            m_timer.Tick += M_timer_Tick;
            m_timer.Start();
        }

        bool m_bBlink = false;
        private void M_timer_Tick(object sender, EventArgs e)
        {
            m_bBlink = !m_bBlink;
            buttonPickerSetUp.Foreground = (m_bBlink && m_loader.m_bPickerDown == false) ? Brushes.Red : Brushes.Black;
            buttonPickerSetDown.Foreground = (m_bBlink && m_loader.m_bPickerDown) ? Brushes.Red : Brushes.Black;
            buttonVacuumOn.Foreground = (m_bBlink && m_loader.p_bVacuum) ? Brushes.Red : Brushes.Black;
            buttonVacuumOff.Foreground = (m_bBlink && m_loader.p_bVacuum == false) ? Brushes.Red : Brushes.Black;
        }

        private void buttonPickerSetUp_Click(object sender, RoutedEventArgs e)
        {
            m_loader.RunPickerDown(false);
        }

        private void buttonPickerSetDown_Click(object sender, RoutedEventArgs e)
        {
            m_loader.RunPickerDown(true);
        }

        private void buttonVacuumOn_Click(object sender, RoutedEventArgs e)
        {
            m_loader.RunVacuum(true);
        }

        private void buttonVacuumOff_Click(object sender, RoutedEventArgs e)
        {
            m_loader.RunVacuum(false);
        }

        private void buttonElevetorDown_Click(object sender, RoutedEventArgs e)
        {
            m_storage.MoveStack();
        }

        private void buttonElevatorReady_Click(object sender, RoutedEventArgs e)
        {
            m_storage.MoveStackReady();
        }

        private void buttonLoaderRoller_Click(object sender, RoutedEventArgs e)
        {
            m_loader.MoveLoader(Loader.ePos.Roller);
        }

        private void buttonLoaderStorage_Click(object sender, RoutedEventArgs e)
        {
            m_loader.MoveLoader(Loader.ePos.Stotage); 
        }
    }
}
