
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace Root_AOP01
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

        MainWindow m_MainWindow;

        Home_ViewModel m_Home;
        Maintenance_ViewModel m_Maintenance;
        GEM_ViewModel m_GEM;

        RecipeWizard_ViewModel m_RecipeWizard;
        RecipeOption_ViewModel m_RecipeOption;
        Recipe45D_ViewModel m_Recipe45D;
        RecipeBackside_ViewModel m_RecipeBackSide;
        RecipeEdge_ViewModel m_RecipeEdge;
        RecipeLADS_ViewModel m_RecipeLADS;


        public Setup_ViewModel(MainWindow mainwindow)
        {
            m_MainWindow = mainwindow;

            Init_ViewModel();
            Init_NaviBtn();

            Set_HomePanel();
        }

        void Init_ViewModel()
        {
            m_Home = new Home_ViewModel(this);
            m_RecipeWizard = new RecipeWizard_ViewModel(this);
            m_Maintenance = new Maintenance_ViewModel(this);
            m_GEM = new GEM_ViewModel(this);

            m_Recipe45D = new Recipe45D_ViewModel(this);
            m_RecipeBackSide = new RecipeBackside_ViewModel(this);
            m_RecipeEdge = new RecipeEdge_ViewModel(this);
            m_RecipeLADS = new RecipeLADS_ViewModel(this);
            m_RecipeOption = new RecipeOption_ViewModel(this);
        }
        void Init_NaviBtn()
        {
            Navi_Home = new NaviBtn("Home");
            Navi_Home.Btn.Click += NaviHomeBtn_Click;

            Navi_Maintenance = new NaviBtn("Maintenance");
            Navi_Maintenance.Btn.Click += NaviMaintBtn_Click;

            Navi_GEM = new NaviBtn("GEM");
            Navi_GEM.Btn.Click += NaviGEMBtn_Click;

            Navi_RecipeWizard = new NaviBtn("Recipe Wizard");
            Navi_RecipeWizard.Btn.Click += NaviRecipeWizardBtn_Click;

            Navi_RecipeOption = new NaviBtn("Recipe Option");
            Navi_RecipeOption.Btn.Click += NaviRecipeOptionBtn_Click;
            Navi_45D = new NaviBtn("45D");
            Navi_45D.Btn.Click += Navi45DBtn_Click;
            Navi_Backside = new NaviBtn("Backside");
            Navi_Backside.Btn.Click += NaviBacksideBtn_Click;
            Navi_Edge = new NaviBtn("Edge");
            Navi_Edge.Btn.Click += NaviEdgeBtn_Click;
            Navi_LADS = new NaviBtn("LADS");
            Navi_LADS.Btn.Click += NaviLADSBtn_Click;

        }

        #region Navigation

        #region Navi Btns

        NaviBtn Navi_Home;
        NaviBtn Navi_Maintenance;
        NaviBtn Navi_GEM;

        NaviBtn Navi_RecipeWizard;
        NaviBtn Navi_RecipeOption;
        NaviBtn Navi_45D;
        NaviBtn Navi_Backside;
        NaviBtn Navi_Edge;
        NaviBtn Navi_LADS;

        #endregion

        #region NaviBtn Event
        private void NaviLADSBtn_Click(object sender, RoutedEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private void NaviEdgeBtn_Click(object sender, RoutedEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private void NaviBacksideBtn_Click(object sender, RoutedEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private void Navi45DBtn_Click(object sender, RoutedEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private void NaviRecipeOptionBtn_Click(object sender, RoutedEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private void NaviRecipeWizardBtn_Click(object sender, RoutedEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private void NaviGEMBtn_Click(object sender, RoutedEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private void NaviMaintBtn_Click(object sender, RoutedEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private void NaviHomeBtn_Click(object sender, RoutedEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        #endregion

        #region Panel Change Method
        public void Set_HomePanel()
        {
            NaviButtons.Clear();

            CurrentPanel = m_Home.Home;
            CurrentPanel.DataContext = m_Home;
        }
        public void Set_RecipeWizardPanel()
        {
            NaviButtons.Clear();
            NaviButtons.Add(Navi_RecipeWizard);

            CurrentPanel = m_RecipeWizard.RecipeWizard;
            CurrentPanel.DataContext = m_RecipeWizard;
        }
        public void Set_MaintenancePanel()
        {
            NaviButtons.Clear();
            NaviButtons.Add(Navi_Maintenance);

            CurrentPanel = m_Maintenance.Maintenance;
            CurrentPanel.DataContext = m_Maintenance;
        }
        public void Set_GEMPanel()
        {
            NaviButtons.Clear();
            NaviButtons.Add(Navi_GEM);

            CurrentPanel = m_GEM.GEM;
            CurrentPanel.DataContext = m_GEM;
        }
        public void Set_Recipe45DPanel()
        {
            NaviButtons.Clear();
            NaviButtons.Add(Navi_RecipeWizard);
            NaviButtons.Add(Navi_45D);

            CurrentPanel = m_RecipeWizard.Recipe45D;
            CurrentPanel.DataContext = m_Recipe45D;
        }
        public void Set_RecipeBacksidePanel()
        {
            NaviButtons.Clear();
            NaviButtons.Add(Navi_RecipeWizard);
            NaviButtons.Add(Navi_Backside);

            CurrentPanel = m_RecipeWizard.RecipeBackside;
            CurrentPanel.DataContext = m_RecipeBackSide;
        }
        public void Set_RecipeEdgePanel()
        {
            NaviButtons.Clear();
            NaviButtons.Add(Navi_RecipeWizard);
            NaviButtons.Add(Navi_Edge);

            CurrentPanel = m_RecipeWizard.RecipeEdge;
            CurrentPanel.DataContext = m_RecipeEdge;
        }
        public void Set_RecipeLADSPanel()
        {
            NaviButtons.Clear();
            NaviButtons.Add(Navi_RecipeWizard);
            NaviButtons.Add(Navi_LADS);

            CurrentPanel = m_RecipeWizard.RecipeLADS;
            CurrentPanel.DataContext = m_RecipeLADS;
        }
        public void Set_RecipeOptionPanel()
        {
            NaviButtons.Clear();
            NaviButtons.Add(Navi_RecipeWizard);
            NaviButtons.Add(Navi_RecipeOption);

            CurrentPanel = m_RecipeWizard.RecipeOption;
            CurrentPanel.DataContext = m_RecipeOption;
        }
        #endregion

        #endregion
    }
}
