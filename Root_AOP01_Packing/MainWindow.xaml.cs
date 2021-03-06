using RootTools;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Root_AOP01_Packing
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
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        #endregion

        #region Window Event
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(@"C:\Recipe\AOP01")) Directory.CreateDirectory(@"C:\Recipe\AOP01");

            Init();

            if (this.WindowState == WindowState.Maximized)
            {
                MaximizeButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                NormalizeButton.Visibility = Visibility.Collapsed;
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            ThreadStop();
        }
        #endregion

        #region Other Event
        private void ModeSelect_Click(object sender, RoutedEventArgs e)
        {
            MainPanel.Children.Clear();
            MainPanel.Children.Add(ModeSelect);
        }
        #endregion

        #region Mode UI
        public SelectMode ModeSelect;
        public Setup_Panel Setup;
        public Review_Panel Review;
        public Run_Panel Run;
        public Dlg_RunStep Dlg_RunStep_Panel;
        #endregion

        #region ViewModel
        private Setup_ViewModel m_Setup;
        private Run_ViewModel m_Run;
        public Dlg_RunStepViewModel m_Dlg_RunStepViewModel;
        #endregion

        public IDialogService dialogService;
        AOP01_Engineer m_engineer = new AOP01_Engineer();

        public MainWindow()
        {
            InitializeComponent();
        }

        void Init()
        {
            dialogService = new DialogService(this);
            dialogService.Register<Dialog_ImageOpenViewModel, Dialog_ImageOpen>();
            dialogService.Register<TK4S, TK4SModuleUI>();
            dialogService.Register<FFUModule, FFUModuleUI>();
            dialogService.Register<Dlg_RunStepViewModel, Dlg_RunStep>();

            m_engineer.Init("AOP01",dialogService);
            Init_ViewModel();
            Init_UI();

            logViewUI.Init(LogView._logView);
            m_Setup.m_Home.Engineer.Engineer_UI.Init(m_engineer);

        }
        void Init_ViewModel()
        {
            m_Setup = new Setup_ViewModel(this, m_engineer);
            m_Run = new Run_ViewModel(this, m_engineer);
            m_Dlg_RunStepViewModel = new Dlg_RunStepViewModel(this, m_engineer);
        }
        void Init_UI()
        {
            ModeSelect = new SelectMode();
            ModeSelect.Init(this);

            Setup = new Setup_Panel();
            Setup.DataContext = m_Setup;

            Review = new Review_Panel();
            //Review.DataContext =;;

            Run = new Run_Panel();
            Run.DataContext = m_Run;

            Dlg_RunStep_Panel = new Dlg_RunStep();
            Dlg_RunStep_Panel.DataContext = m_Dlg_RunStepViewModel;

            MainPanel.Children.Clear();
            MainPanel.Children.Add(ModeSelect);
        }
        void ThreadStop()
        {
            m_engineer.ThreadStop();
        }


        private void NaviReview_Click(object sender, RoutedEventArgs e)
        {
            MainPanel.Children.Clear();
            MainPanel.Children.Add(Review);
        }

        private void NaviRun_click(object sender, RoutedEventArgs e)
        {
            MainPanel.Children.Clear();
            MainPanel.Children.Add(Run);
        }


        private void ViewMenuItem_Click(object sender, RoutedEventArgs e)
        {
            bool check = false;
            MenuItem item = sender as MenuItem;
            for (int i = 0; i < ViewMenu.Items.Count; i++)
            {
                check = ((MenuItem)ViewMenu.Items[i]).IsChecked || check;

                if (item == (MenuItem)ViewMenu.Items[i])
                {
                    if (item.IsChecked)
                    {
                        viewTab.SelectedIndex = i;
                        foreach (TabItem tab in viewTab.Items)
                        {
                            if (tab.Visibility == Visibility.Visible)
                                viewTab.SelectedIndex = tab.TabIndex;
                        }
                    }
                }

            }
            if (check == false)
            {
                splitter.IsEnabled = false;
                ViewArea.Height = new GridLength(0);
            }
            else
            {
                splitter.IsEnabled = true;
                ViewArea.Height = new GridLength(200);
            }

        }

        private void NaviEngineer_Click(object sender, RoutedEventArgs e)
        {
            m_Setup.Set_EngineerPage();
        }

        private void NaviGEM_Click(object sender, RoutedEventArgs e)
        {
            m_Setup.Set_GEMPage();
        }
    }
}
