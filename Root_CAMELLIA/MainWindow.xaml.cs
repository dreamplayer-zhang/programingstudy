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
       
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SplashScreenHelper.ShowText("Handler Initialize");
            m_engineer = App.m_engineer;
            m_handler = m_engineer.m_handler;
            //loadportA.Init(m_handler.m_aLoadport[0], m_handler, m_handler.m_aRFID[0]);
            //loadportB.Init(m_handler.m_aLoadport[1], m_handler, m_handler.m_aRFID[1]);

            int nLPNum = m_handler.m_aLoadport.Count;
            for (int i = 0; i < nLPNum; i++) dlgOHT.Init(m_handler.m_aLoadport[i].m_OHTNew);
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
        
        }

        DispatcherTimer m_timer = new DispatcherTimer();
        void InitTimer()
        {
            m_timer.Interval = TimeSpan.FromMilliseconds(20);
            m_timer.Tick += M_timer_Tick;
            m_timer.Start();
        }
        // EQ.eState oldstate = EQ.eState.Init;
        private void M_timer_Tick(object sender, EventArgs e)
        {
            tbTime.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            TimerUI();
            TimerLamp();

            buttonResume.IsEnabled = IsEnable_Resume();
            buttonPause.IsEnabled = IsEnable_Pause();
            buttonInit.IsEnabled = IsEnable_Initial();
            buttonRecovery.IsEnabled = IsEnable_Recovery();

            if (EQ.p_eState == EQ.eState.Ready && m_handler.m_camellia.p_isClearInfoWafer)
            {
                m_handler.m_process.ClearInfoWafer();
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

        #region Lamp Check Parameter

        private void LampPMCheck(double dLeftLampTime)
        {
            //if (dLeftLampTime >= 0)
            //{
            //    string sLeftLampTime = dLeftLampTime.ToString();
            //    p_LampTimeError = sLeftLampTime + " Hours Left until PM !";

            //}
            //else
            //{
            //    p_LampTimeError = "Please, Do Lamp PM";
            //}
        }

        //private void UpdateLampTime(bool Initialize)
        //{
        //    double LampUseTime = UpdateLampData(Initialize, "t");

        //    //p_LampTimeCount = LampUseTime;

        //    // App.m_nanoView.m_PMDatas.
        //    if (LampUseTime > 9500)
        //    {
        //        LampPMCheck(10000 - LampUseTime);
        //    }
        //}

        //public SerialPort sp = new SerialPort();

        //public double UpdateLampData(bool initialize, string CheckWord)
        //{

        //    if (initialize)
        //    {
        //        sp.PortName = "COM2";
        //        sp.BaudRate = 9600;
        //        sp.DataBits = 8;
        //        sp.StopBits = StopBits.One;
        //        sp.Parity = Parity.None;
        //        //sp.DataReceived += new SerialDataReceivedEventHandler(serialPort1_DataReceived);
        //        sp.Open();
        //        sp.Write("c");

        //    }

        //    sp.Write(CheckWord);
        //    string OutputData = sp.ReadExisting();
        //    string[] strtext = new string[7] { "H0:", "T0:", "L0:", "H1:", "T1:", "L1:", "Time:" };
        //    string[] arr = OutputData.Split(':');
        //    string[] strarr = arr[1].Split(',');
        //    double LampUseTime = 0.0;
        //    double value = 0.0;
        //    int Hr, Min, Sec;
        //    int m_Hr_Org, m_Min_Org, m_Sec_Org;
        //    int m_Hr, m_Min, m_Sec;
        //    string filepath;
        //    foreach (string str in strtext)
        //    {
        //        if (OutputData.Contains(str))
        //        {
        //            if (OutputData.Contains("Time:"))
        //            {
        //                filepath = Application.StartupPath + "\\Timedata.txt";

        //                FileInfo fi = new FileInfo(filepath);

        //                m_Hr_Org = m_Min_Org = m_Sec_Org = 0;
        //                m_Hr = m_Min = m_Sec = 0;

        //                if (fi.Exists)
        //                {
        //                    timedata = File.ReadAllText(@filepath);
        //                    char sp = ':';
        //                    string[] spstring = timedata.Split(sp);

        //                    m_Hr_Org = Convert.ToInt32(spstring[0]);
        //                    m_Min_Org = Convert.ToInt32(spstring[1]);
        //                    m_Sec_Org = Convert.ToInt32(spstring[2]);


        //                    Sec = Convert.ToInt32(arr[1]);

        //                    m_Sec = Sec + m_Sec_Org;

        //                    if (m_Sec >= 60)
        //                    {
        //                        m_Min = (m_Sec / 60) + m_Min_Org;

        //                        m_Sec = m_Sec % 60;
        //                    }
        //                    else
        //                    {
        //                        m_Min = m_Min_Org;
        //                    }

        //                    if (m_Min >= 60)
        //                    {
        //                        m_Hr = (m_Min / 60) + m_Hr_Org;

        //                        m_Min = m_Min % 60;
        //                    }
        //                    else
        //                    {
        //                        m_Hr = m_Hr_Org;
        //                    }

        //                    // strtime = string.Format("{0}:{1}:{2}", m_Hr, m_Min, m_Sec);


        //                    value = m_Hr;

        //                }
        //                else
        //                {
        //                    decimal number1 = 0;
        //                    bool canConvert = decimal.TryParse(arr[1], out number1);
        //                    if (canConvert == true)
        //                        value = Convert.ToDouble(number1);
        //                }
        //            }
        //        }
        //        return value;

        //    }

        //}

        #endregion
    }
}
