using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Media;
using System;
using RootTools;
using Root_CAMELLIA.Data;
using RootTools.Memory;

namespace Root_CAMELLIA
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {

        //public CAMELLIA_Engineer m_engineer = new CAMELLIA_Engineer();
        MemoryTool m_memoryTool;


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

            InitTimer();

            Init();
        }

        private void Init()
        {
            DataManager = new DataManager(this);
            dialogService = new DialogService(this);
            DialogInit(dialogService);
            App.m_engineer.Init("CAMELLIA2");
           
        }

        void DialogInit(IDialogService dialogService)
        {
            dialogService.Register<Dlg_RecipeManager_ViewModel, Dlg_RecipeManger>();
            dialogService.Register<Dlg_Engineer_ViewModel, Dlg_Engineer>();
        }


        #region Window
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            ThreadStop();
        }
        #endregion


        void ThreadStop()
        {
            App.m_engineer.ThreadStop();
        }

        #region ViewModel
        //public Dlg_RecipeManager_ViewModel m_RecipeManagerViewModel;
        public MainWindow_ViewModel m_MainWindowViewModel;
        #endregion

        #region Dlg
        IDialogService dialogService;
        #endregion

        #region Getter Setter
        public DataManager DataManager { get; set; }
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

        #region Timer
        DispatcherTimer m_timer = new DispatcherTimer();
        void InitTimer()
        {
            m_timer.Interval = TimeSpan.FromMilliseconds(100);
            m_timer.Tick += M_timer_Tick;
            m_timer.Start();
        }

        private void M_timer_Tick(object sender, EventArgs e)
        {
            tbTime.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
        #endregion

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

            var viewModel = new Dlg_RecipeManager_ViewModel(this);
            Nullable<bool> result = dialogService.ShowDialog(viewModel);
            

        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var viewModel = new Dlg_Engineer_ViewModel(this);
            Nullable<bool> result = dialogService.ShowDialog(viewModel);
        }
    }
}
