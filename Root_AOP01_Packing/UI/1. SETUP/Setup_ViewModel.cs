
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Root_AOP01_Packing
{
    class Setup_ViewModel : ObservableObject
    {
        private UserControl _CurrentPanel;
        public UserControl CurrentPanel
        {
            get
            {
                return _CurrentPanel;
            }
            set
            {
                SetProperty(ref _CurrentPanel, value);
            }
        }

        private ObservableCollection<UIElement> _NaviButtons = new ObservableCollection<UIElement>();
        public ObservableCollection<UIElement> NaviButtons
        {
            get
            {
                return _NaviButtons;
            }
            set
            {
                SetProperty(ref _NaviButtons, value);
            }
        }

        public MainWindow m_MainWindow;

        public SetupHome_ViewModel m_Home;

        public Setup_ViewModel(MainWindow mainwindow, AOP01_Engineer engineer)
        {
            m_MainWindow = mainwindow;

            Init_ViewModel();
            Init_NaviBtn();

            Set_HomePanel();
            Set_EngineerPage();
        }

        void Init_ViewModel()
        {
            m_Home = new SetupHome_ViewModel(this);
        }
        void Init_NaviBtn()
        {
            Navi_Setup = new NaviBtn("Setup");
            Navi_Setup.Arrow.Visibility = Visibility.Collapsed;
            Navi_Setup.Btn.Click += NaviHomeBtn_Click;

            Navi_Engineer = new NaviBtn("Engineer");
            Navi_Engineer.Btn.Click += NaviEngineer_Click;

            Navi_GEM = new NaviBtn("GEM");
            Navi_GEM.Btn.Click += NaviGEM_Click;
        }





        #region Navigation

        #region Navi Btns

        public NaviBtn Navi_Setup;
        public NaviBtn Navi_Engineer;
        public NaviBtn Navi_GEM;

        #endregion

        #region NaviBtn Event
        public ICommand btnNaviSetupHome
        {
            get
            {
                return new RelayCommand(() =>
                {
                    Set_HomePanel();
                });
            }
        }
        private void NaviEngineer_Click(object sender, RoutedEventArgs e)
        {
            Set_EngineerPage();
        }
        private void NaviGEM_Click(object sender, RoutedEventArgs e)
        {
            Set_GEMPage();
        }

        private void NaviHomeBtn_Click(object sender, RoutedEventArgs e)
        {
            Set_HomePanel();
        }

        #endregion

        #region Panel change Method
        public void Set_HomePanel()
        {
            NaviButtons.Clear();
            NaviButtons.Add(Navi_Setup);
            NaviButtons.Add(Navi_Engineer);

            CurrentPanel = m_Home.Home;
            CurrentPanel.DataContext = m_Home;
        }
        #endregion

        #region Page Change Method
        public void Set_EngineerPage()
        {
            NaviButtons.Clear();
            NaviButtons.Add(Navi_Setup);
            NaviButtons.Add(Navi_Engineer);

            m_Home.Home.SubPanel.Children.Clear();
            m_Home.Home.SubPanel.Children.Add(m_Home.Engineer);
            m_Home.Home.EngineerBtn.IsChecked = true;
            m_Home.Home.GEMBtn.IsChecked = false;

            CurrentPanel = m_Home.Home;
            CurrentPanel.DataContext = m_Home;
        }
        public void Set_GEMPage()
        {
            NaviButtons.Clear();
            NaviButtons.Add(Navi_Setup);
            NaviButtons.Add(Navi_Engineer);

            m_Home.Home.SubPanel.Children.Clear();
            m_Home.Home.SubPanel.Children.Add(m_Home.GEM);
            m_Home.Home.GEMBtn.IsChecked = true;
            m_Home.Home.EngineerBtn.IsChecked = false;

            CurrentPanel = m_Home.Home;
            CurrentPanel.DataContext = m_Home;
        }

        #endregion

        #endregion
    }
}
