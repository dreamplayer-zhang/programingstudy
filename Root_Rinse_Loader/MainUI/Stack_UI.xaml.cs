using Root_Rinse_Loader.Module;
using RootTools.Control;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace Root_Rinse_Loader.MainUI
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
        Storage.Stack m_stack;
        Loader m_loader; 
        public void Init(Storage.Stack stack, Loader loader)
        {
            m_stack = stack;
            m_storage = stack.m_storage; 
            m_loader = loader;
            InitCheckSensor(); 
            InitTimer(); 
        }

        class CheckSensor
        {
            public Button m_button = new Button();
            DIO_I m_diCheck;

            bool _bCheck = false; 
            bool p_bCheck
            {
                get { return _bCheck; }
                set
                {
                    if (_bCheck == value) return;
                    _bCheck = value;
                    m_button.Background = value ? Brushes.YellowGreen : Brushes.DarkGray; 
                }
            }
            
            public void OnTimer(bool bBlink)
            {
                p_bCheck = bBlink && m_diCheck.p_bIn; 
            }

            public CheckSensor(DIO_I diCheck)
            {
                string[] asCheck = diCheck.m_id.Split('.'); 
                m_diCheck = diCheck;
                m_button.Content = asCheck[asCheck.Length - 1];
                m_button.FontSize = 30;
                m_button.Margin = new Thickness(5, 10, 5, 10);
            }
        }
        List<CheckSensor> m_aCheckSensor = new List<CheckSensor>(); 
        void InitCheckSensor()
        {
            gridCheck.Children.Clear();
            gridCheck.ColumnDefinitions.Clear(); 
            for (int n = 0; n < 4; n++)
            {
                CheckSensor check = new CheckSensor(m_stack.m_diCheck[n]);
                gridCheck.ColumnDefinitions.Add(new ColumnDefinition());
                Grid.SetColumn(check.m_button, gridCheck.Children.Count);
                gridCheck.Children.Add(check.m_button);
                m_aCheckSensor.Add(check); 
            }
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
            foreach (CheckSensor check in m_aCheckSensor) check.OnTimer(m_bBlink); 
        }

        private void buttonPickerSetUp_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            m_loader.RunPickerDown(false);
        }

        private void buttonPickerSetDown_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            m_loader.RunPickerDown(true);
        }

        private void buttonVacuumOn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            m_loader.RunVacuum(true);
        }

        private void buttonVacuumOff_Click(object sender, System.Windows.RoutedEventArgs e)
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
    }
}
