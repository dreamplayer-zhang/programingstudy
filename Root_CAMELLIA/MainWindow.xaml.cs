using System;
using System.Windows;
using System.IO;
using System.IO.Ports;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Root_CAMELLIA.UI_Dialog;
using Root_EFEM.Module;
using RootTools;
using RootTools.Memory;
using RootTools.Module;
using System.Runtime.InteropServices;
using SSLNet;

namespace Root_CAMELLIA
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    ///

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
        OHTs_UI dlgOHT;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SplashScreenHelper.ShowText("Handler Initialize");
            m_engineer = App.m_engineer;
            m_handler = m_engineer.m_handler;
            //loadportA.Init(m_handler.m_aLoadport[0], m_handler, m_handler.m_aRFID[0]);
            //loadportB.Init(m_handler.m_aLoadport[1], m_handler, m_handler.m_aRFID[1]);
            //marsLogManager = MarsLogManager.Instance;
            int nLPNum = m_handler.m_aLoadport.Count;
            //for (int i = 0; i < nLPNum; i++) dlgOHT.Init(m_handler.m_aLoadport[i].m_OHTsemi);
            dlgOHT = new OHTs_UI();
            dlgOHT.Init((CAMELLIA_Handler)m_engineer.ClassHandler());

            SplashScreenHelper.ShowText("Handler Initialize Done");

            SplashScreenHelper.ShowText("Log View Initialize");
            LogUI.Init(LogView._logView);
            SplashScreenHelper.ShowText("Log View Initialize Done");

            SplashScreenHelper.ShowText("Log View Initialize Done");
            InitTimer();

            SplashScreenHelper.ShowText("Camellia2 Initialize Done");

            //SSLoggerNet sSLoggerNet = new SSLoggerNet();
            //DataFormatter data = new DataFormatter();
            //sSLoggerNet.WriteFNCLog(1, "1", "1", STATUS.START);
            //data.AddData("test", 1);
            //data.Serialize();
            //MarsLogManager instance = MarsLogManager.Instance;
            //instance.m_useLog = true;
            //instance.m_sSLoggerNet.WriteFNCLog(EQ.p_nRunLP, m_handler.m_loadport[0].p_id, "Test", STATUS.START);
            //instance.m_sSLoggerNet.WriteFNCLog(EQ.p_nRunLP, m_handler.m_loadport[0].p_id, "Test", STATUS.END,  MATERIAL_TYPE.FOUP);

            //StreamWriter sw = new StreamWriter, true); // append

            //sw.WriteLine("asdf");


        }

        DispatcherTimer m_timer = new DispatcherTimer();
        MarsLogManager marsLogManager;
        void InitTimer()
        {
            m_timer.Interval = TimeSpan.FromMilliseconds(20);
            m_timer.Tick += M_timer_Tick;
            m_timer.Start();
        }
        // EQ.eState oldstate = EQ.eState.Init;
        bool isCFGWrite = false;
        private void M_timer_Tick(object sender, EventArgs e)
        {
            tbTime.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            if (DateTime.Now.Minute == 0 && DateTime.Now.Second == 0 && !isCFGWrite)
            {
                MarsLogManager.Instance.WriteCFG("Vision", BaseDefine.Category.EQUIPMENT.ToString(), "Version", BaseDefine.Configuration.Version3);
                isCFGWrite = true;
            }

            if (DateTime.Now.Second == 1)
                isCFGWrite = false;
            //int a = 10;
            TimerUI();
            TimerLamp();

            buttonResume.IsEnabled = IsEnable_Resume();
            buttonPause.IsEnabled = IsEnable_Pause();
            buttonInit.IsEnabled = IsEnable_Initial();
            buttonRecovery.IsEnabled = IsEnable_Recovery();

            if (EQ.p_eState == EQ.eState.Ready && m_handler.m_camellia.p_isClearInfoWafer)
            {
                m_handler.p_process.ClearInfoWafer();
                m_handler.m_camellia.p_isClearInfoWafer = false;
            }
        }

        void TimerUI()
        {
            if (EQ.p_eState != EQ.eState.Run) EQ.p_bRecovery = false;
            //textState.Text = m_bRecovery ? "Recovery" : EQ.p_eState.ToString();
            //textState.Text = EQ.p_bRecovery ? "Recovery" : EQ.p_eState.ToString();
            //textState.Text = "asgasgsdagsdg";
        }

        void TimerLamp()
        {
            if(EQ.p_eState == EQ.eState.Error)
            {
                LampTop.Background = Brushes.Red;
                LampMid.Background = new SolidColorBrush(Color.FromArgb(255, 221, 221, 221));
                LampBot.Background = new SolidColorBrush(Color.FromArgb(255, 221, 221, 221));
            }
            else if(EQ.p_eState == EQ.eState.Run)
            {
                LampTop.Background = new SolidColorBrush(Color.FromArgb(255, 221, 221, 221));
                LampMid.Background = new SolidColorBrush(Color.FromArgb(255, 221, 221, 221));
                LampBot.Background = Brushes.ForestGreen;
            }
            else
            {
                LampTop.Background = new SolidColorBrush(Color.FromArgb(255, 221, 221, 221));
                LampMid.Background = Brushes.Gold;
                LampBot.Background = new SolidColorBrush(Color.FromArgb(255, 221, 221, 221));
            }

            //working
        }

        bool IsEnable_Resume()
        {
            if (EQ.p_eState != EQ.eState.Ready) return false;
            if (m_handler.p_process.p_qSequence.Count <= 0) return false;
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
            EQ.p_eState = EQ.eState.Home;
            
            //isClearInfoWafer = true;
            //m_handler.m_camellia

            //Application.Current.Dispatcher.Invoke(delegate ()
            //{
            //    while(EQ.p_eState == EQ.eState.Home)
            //    {
            //        if (EQ.IsStop())
            //        {
            //            EQ.p_eState = EQ.eState.Error;
            //        }
            //    }
            //    m_handler.m_process.ClearInfoWafer();
            //});

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
            m_handler.m_bIsPossible_Recovery = false;
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

        

        //public void ShowOHT()
        //{
        //    m_timer.Start();
        //}

        //DispatcherTimer m_timer = new DispatcherTimer();
        //void InitTimer()
        //{
        //    m_timer.Interval = TimeSpan.FromMilliseconds(10);
        //    m_timer.Tick += M_timer_Tick;
        //}
        //OHTs_UI m_uiOHT;
        //private void M_timer_Tick(object sender, EventArgs e)
        //{
        //    m_timer.Stop();
        //    m_uiOHT = new OHTs_UI();
        //    m_uiOHT.Init((CAMELLIA_Handler)m_engineer.ClassHandler());
        //    m_uiOHT.Show();
        //}

        //Dlg_OHT dlgOHT = new Dlg_OHT();
        private void buttonOHT_Click(object sender, RoutedEventArgs e)
        {
            dlgOHT.Show();
        }

        #region Lamp Check Parameter

      

        #endregion
    }
}
