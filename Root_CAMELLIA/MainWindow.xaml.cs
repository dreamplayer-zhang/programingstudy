﻿using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Root_CAMELLIA.UI_Dialog;
using Root_EFEM.Module;
using RootTools;
using RootTools.Memory;
using RootTools.Module;

namespace Root_CAMELLIA
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Title Bar
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Maximized;
            NormalizeButton.Visibility = Visibility.Visible;
            MaximizeButton.Visibility = Visibility.Collapsed;
        }
        private void NormalizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Normal;
            MaximizeButton.Visibility = Visibility.Visible;
            NormalizeButton.Visibility = Visibility.Collapsed;
        }
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (this.WindowState == WindowState.Maximized)
                {
                    this.WindowState = WindowState.Normal;
                    MaximizeButton.Visibility = Visibility.Visible;
                    NormalizeButton.Visibility = Visibility.Collapsed;
                }
                else
                {
                    this.WindowState = WindowState.Maximized;
                    NormalizeButton.Visibility = Visibility.Visible;
                    MaximizeButton.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                this.DragMove();
            }
        }
        #endregion

        public MainWindow()
        {
            InitializeComponent();

            if (this.WindowState == WindowState.Maximized)
            {
                MaximizeButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                NormalizeButton.Visibility = Visibility.Collapsed;
            }
            this.DataContext = new MainWindow_ViewModel(this);
        }

        CAMELLIA_Engineer m_engineer;
        CAMELLIA_Handler m_handler;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //App.m_engineer.Init("Camellia");
            m_engineer = App.m_engineer;
            m_handler = m_engineer.m_handler;
            loadportA.Init(m_handler.m_aLoadport[0], m_handler, m_handler.m_aRFID[0]);
            loadportB.Init(m_handler.m_aLoadport[1], m_handler, m_handler.m_aRFID[1]);

            LogUI.Init(LogView._logView);

            int nLPNum = m_handler.m_aLoadport.Count;
            for (int i = 0; i < nLPNum; i++) dlgOHT.Init(m_handler.m_aLoadport[i].m_OHTNew);

            InitTimer();

        }

        DispatcherTimer m_timer = new DispatcherTimer();
        void InitTimer()
        {
            m_timer.Interval = TimeSpan.FromMilliseconds(20);
            m_timer.Tick += M_timer_Tick;
            m_timer.Start();
        }

        private void M_timer_Tick(object sender, EventArgs e)
        {
            TimerUI();
            TimerLamp();

            buttonResume.IsEnabled = IsEnable_Resume();
            buttonPause.IsEnabled = IsEnable_Pause();
            buttonInit.IsEnabled = IsEnable_Initial();
            buttonRecovery.IsEnabled = IsEnable_Recovery();
        }

        void TimerUI()
        {
            if (EQ.p_eState != EQ.eState.Run) EQ.p_bRecovery = false;
            //textState.Text = m_bRecovery ? "Recovery" : EQ.p_eState.ToString();
            textState.Text = EQ.p_bRecovery ? "Recovery" : EQ.p_eState.ToString();
        }

        void TimerLamp()
        {
            //working
        }

        bool IsEnable_Resume()
        {
            if (EQ.p_eState != EQ.eState.Ready) return false;
            if (m_handler.m_process.m_qSequence.Count <= 0) return false;
            return true;
        }

        private void buttonResume_Click(object sender, RoutedEventArgs e)
        {
            if (IsEnable_Resume() == false) return;
            EQ.p_bStop = false;
            EQ.p_eState = EQ.eState.Run;
        }

        bool IsEnable_Initial()
        {
            if (IsRunModule()) return false;
            switch (EQ.p_eState) 
            {
                case EQ.eState.Run: return false;
                case EQ.eState.Home: return false;
            }
            return true;
        }

        private void buttonInit_Click(object sender, RoutedEventArgs e)
        {
            if (IsEnable_Initial() == false) return;
            EQ.p_bStop = false;
            m_handler.m_process.ClearInfoWafer();
            EQ.p_eState = EQ.eState.Home;
            //Camellia Camera Connect
        }

        bool IsRunModule()
        {
            if (IsRunModule((Loadport_RND)m_handler.m_aLoadport[0])) return true;
            if (IsRunModule((Loadport_RND)m_handler.m_aLoadport[1])) return true;
            if (IsRunModule(m_handler.m_wtr)) return true;
            if (IsRunModule(m_handler.m_Aligner)) return true;
            if (IsRunModule(m_handler.m_camellia)) return true;
            return false;
        }

        bool IsRunModule(ModuleBase module)
        {
            if (module.p_eState == ModuleBase.eState.Run) return true;
            if (module.p_eState == ModuleBase.eState.Home) return true;
            return (module.m_qModuleRun.Count > 0);
        }

        

        bool IsEnable_Recovery()
        {
            if (IsRunModule()) return false;
            if (EQ.p_eState != EQ.eState.Ready) return false;
            if (EQ.p_bStop == true) return false;
            return m_handler.IsEnableRecovery();
        }

        private void buttonRecovery_Click(object sender, RoutedEventArgs e)
        {
            if (IsEnable_Recovery() == false) return;
            m_handler.CalcRecover();
            EQ.p_bStop = false;
            EQ.p_eState = EQ.eState.Run; //? Run으로 되어있었음.
            EQ.p_bRecovery = true;
        }

        bool IsEnable_Pause()
        {
            if (EQ.p_eState != EQ.eState.Run) return false;
            return true;
        }

        private void buttonPause_Click(object sender, RoutedEventArgs e)
        {
            if (IsEnable_Pause() == false) return;
            EQ.p_eState = EQ.eState.Ready;
        }

        private void buttonBuzzerOff_Click(object sender, RoutedEventArgs e)
        {
            m_engineer.BuzzerOff();
        }

        Dlg_OHT dlgOHT = new Dlg_OHT();
        private void buttonOHT_Click(object sender, RoutedEventArgs e)
        {
            dlgOHT.Show();
        }
    }
}
