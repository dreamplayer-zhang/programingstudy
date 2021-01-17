using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Media;
using RootTools.Memory;
using RootTools;
using RootTools_CLR;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using RootTools_Vision;
using System.Drawing;
using RootTools.Database;

namespace Root_WIND2
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Window Event
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
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Init();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            ThreadStop();

            ProgramManager.Instance.Exit();
            ProgramManager.Instance.Dispose();
            GC.SuppressFinalize(this);
        }
        #endregion

        #region Timer
        DispatcherTimer m_timer = new DispatcherTimer();
        void InitTimer()
        {
            //m_timer.Interval = TimeSpan.FromMilliseconds(100);
            //m_timer.Tick += M_timer_Tick;
            //m_timer.Start();
        }

        private void M_timer_Tick(object sender, EventArgs e)
        {
        }
        #endregion

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
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();            
        }
        #endregion

        public IDialogService dialogService;

        void Init()
        {
            dialogService = new DialogService(this);
            dialogService.Register<Dialog_ImageOpenViewModel, Dialog_ImageOpen>();
            dialogService.Register<Dialog_Scan_ViewModel, Dialog_Scan>();
            dialogService.Register<SettingDialog_ViewModel, SettingDialog>();


            if (ProgramManager.Instance.Initialize() == false)
            {
                MessageBox.Show("Program Initialization fail");
                return;
            }

            ProgramManager.Instance.DialogService = this.dialogService;


            if (UIManager.Instance.Initialize(ProgramManager.Instance) == false)
            {
                MessageBox.Show("UI Initialization fail");
                return;
            }

            // WPF 파라매터 연결
            UIManager.Instance.MainPanel = this.MainPanel;

            UIManager.Instance.ChangeUIMode();

            ///////시연용 임시코드
            DatabaseManager.Instance.SetDatabase(1);
            //////
            logView.Init(LogView.m_logView);
            InitTimer();
        }

        void ThreadStop()
        {
            ProgramManager.Instance.Engineer.ThreadStop();
        }
    }
}