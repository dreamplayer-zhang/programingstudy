using RootTools;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Root_EFEM.Module;
using Root_AOP01_Inspection.Module;
using RootTools.GAFs;
using static Root_AOP01_Inspection.AOP01_Handler;
using System.Windows.Threading;
using System;
using System.Windows.Media;

namespace Root_AOP01_Inspection
{
    public class Dummy
    {
        public string a
        {
            get;set;
        }
        public string b
        {
            get; set;
        }
        public string c
        {
            get; set;
        }
        public string d
        {
            get; set;
        }
        public string e
        {
            get; set;
        }
    }

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
        //public Dlg_Start Dlg;
        #endregion

        #region ViewModel
        private Setup_ViewModel m_Setup;
        private Run_ViewModel m_Run;
        //private Dlg_ViewModel m_Dlg;
        #endregion

        public AOP01_Engineer m_engineer = new AOP01_Engineer();
        public IDialogService dialogService;

        public MainWindow()
        {
            InitializeComponent();
        }

        void Init()
        {
            dialogService = new DialogService(this);
            dialogService.Register<Dialog_ImageOpenViewModel, Dialog_ImageOpen>();
            if (ProgramManager.Instance.Initialize())
            {
                ProgramManager.Instance.DialogService = this.dialogService;
                m_engineer = ProgramManager.Instance.Engineer;//이중이므로 나중에 m_engineer 제거 필요
            }
            else
            {
                MessageBox.Show("Program Initialization fail");
                return;
            }
            Init_ViewModel();
            Init_UI();
            logViewUI.Init(LogView.m_logView);
            if (m_engineer.m_handler.m_aLoadportType[0] == eLoadport.Cymechs && m_engineer.m_handler.m_aLoadportType[1] == eLoadport.Cymechs)
            {
                Run.Init(m_engineer.m_handler.m_mainVision, m_engineer.m_handler.m_backsideVision, (RTRCleanUnit)m_engineer.m_handler.m_wtr, (Loadport_AOP01)m_engineer.m_handler.m_aLoadport[0], (Loadport_AOP01)m_engineer.m_handler.m_aLoadport[1], m_engineer, (RFID_Brooks)m_engineer.m_handler.m_aRFID[0], (RFID_Brooks)m_engineer.m_handler.m_aRFID[1]);
            }
            if (m_engineer.m_handler.m_FDC.m_aData.Count > 0)
            {
                try
                {
                    FDCName1.DataContext = m_engineer.m_handler.m_FDC.m_aData[0];
                    FDCValue1.DataContext = m_engineer.m_handler.m_FDC.m_aData[0];
                    FDCName2.DataContext = m_engineer.m_handler.m_FDC.m_aData[1];
                    FDCValue2.DataContext = m_engineer.m_handler.m_FDC.m_aData[1];
                    FDCName3.DataContext = m_engineer.m_handler.m_FDC.m_aData[2];
                    FDCValue3.DataContext = m_engineer.m_handler.m_FDC.m_aData[2];
                    FDCName4.DataContext = m_engineer.m_handler.m_FDC.m_aData[3];
                    FDCValue4.DataContext = m_engineer.m_handler.m_FDC.m_aData[3];
                }
                catch { }
            }
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
            if (m_engineer.m_handler.m_FDC.m_aData.Count > 0)
            {
                try
                {
                    FDC1.Background = m_engineer.m_handler.m_FDC.m_aData[0].p_bAlarm == true ? Brushes.Red : Brushes.AliceBlue;
                    FDC2.Background = m_engineer.m_handler.m_FDC.m_aData[1].p_bAlarm == true ? Brushes.Red : Brushes.AliceBlue;
                    FDC3.Background = m_engineer.m_handler.m_FDC.m_aData[2].p_bAlarm == true ? Brushes.Red : Brushes.AliceBlue;
                    FDC4.Background = m_engineer.m_handler.m_FDC.m_aData[3].p_bAlarm == true ? Brushes.Red : Brushes.AliceBlue;
                }
                catch { }
            }
        }
        void Init_ViewModel()
        {
            m_Setup = new Setup_ViewModel(this);
            m_Run = new Run_ViewModel(this);
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

            //Dlg = new Dlg_Start();
            //Dlg.DataContext = m_Dlg;
            MainPanel.Children.Clear();
            MainPanel.Children.Add(ModeSelect);
        }
        void ThreadStop()
        {
            m_engineer.ThreadStop();
        }


        private void NaviRecipeSummary_Click(object sender, RoutedEventArgs e)
        {
            MainPanel.Children.Clear();
            MainPanel.Children.Add(Setup);
            m_Setup.Set_RecipeSummary();
        }

        private void NaviRecipeSpec_Click(object sender, RoutedEventArgs e)
        {
            MainPanel.Children.Clear();
            MainPanel.Children.Add(Setup);
            m_Setup.Set_RecipeSpec();
        }

        private void NaviRecipe45D_Click(object sender, RoutedEventArgs e)
        {
            MainPanel.Children.Clear();
            MainPanel.Children.Add(Setup);
            m_Setup.Set_Recipe45DPanel();
        }

        private void NaviRecipeFrontside_Click(object sender, RoutedEventArgs e)
        {
            MainPanel.Children.Clear();
            MainPanel.Children.Add(Setup);
            m_Setup.Set_RecipeFrontsidePanel();
        }

        private void NaviRecipeEdge_Click(object sender, RoutedEventArgs e)
        {
            MainPanel.Children.Clear();
            MainPanel.Children.Add(Setup);
            m_Setup.Set_RecipeEdgePanel();
        }

        private void NaviRecipeLADS_Click(object sender, RoutedEventArgs e)
        {
            MainPanel.Children.Clear();
            MainPanel.Children.Add(Setup);
            m_Setup.Set_RecipeLADSPanel();
        }

        private void NaviMaintenance_Click(object sender, RoutedEventArgs e)
        {
            MainPanel.Children.Clear();
            MainPanel.Children.Add(Setup);
            m_Setup.Set_MaintenancePanel();
        }

        private void NaviGEM_Click(object sender, RoutedEventArgs e)
        {
            MainPanel.Children.Clear();
            MainPanel.Children.Add(Setup);
            m_Setup.Set_GEMPanel();
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
    }
}
