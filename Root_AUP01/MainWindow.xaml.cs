using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Root_AOP01
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
        public Run_Panel Run;
        #endregion

        #region ViewModel
        private Setup_ViewModel m_Setup;
        private Run_ViewModel m_Run;
        #endregion

        AOP01_Engineer m_engineer = new AOP01_Engineer();

        public MainWindow()
        {
            InitializeComponent();
        }

        void Init()
        {
            Init_ViewModel();
            Init_UI();

            m_engineer.Init("AOP01");
            m_Setup.m_Maintenance.Maintenance.Engineer_UI.Init(m_engineer);
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

            Run = new Run_Panel();
            Run.DataContext = this;

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

        private void NaviRecipeBackside_Click(object sender, RoutedEventArgs e)
        {
            MainPanel.Children.Clear();
            MainPanel.Children.Add(Setup);
            m_Setup.Set_RecipeBacksidePanel();
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

        }

        private void NaviRun_click(object sender, RoutedEventArgs e)
        {
            MainPanel.Children.Clear();
            MainPanel.Children.Add(Run);
        }

        private void ViewMenuItem_Click(object sender, RoutedEventArgs e)
        {
            bool check = false;

            for (int i = 0; i < ViewMenu.Items.Count; i++)
            {
                check = ((MenuItem)ViewMenu.Items[i]).IsChecked || check;
                if ((sender as MenuItem) == (MenuItem)ViewMenu.Items[i])
                {
                    if ((sender as MenuItem).IsChecked)
                        viewTab.SelectedIndex = i;
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
