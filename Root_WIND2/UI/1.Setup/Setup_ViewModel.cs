using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Root_WIND2
{
    public class Setup_ViewModel : ObservableObject
    {
        private UserControl m_CurrentPanel;
        public UserControl p_CurrentPanel
        {
            get
            {
                return m_CurrentPanel;
            }
            set
            {
                SetProperty(ref m_CurrentPanel, value);
            }
        }

        private ObservableCollection<UIElement> m_NaviButtons;
        public ObservableCollection<UIElement> p_NaviButtons
        {
            get
            {
                return m_NaviButtons;
            }
            set
            {
                SetProperty(ref m_NaviButtons, value);
            }
        }

        MainWindow m_MainWindow;

        Home_ViewModel m_Home;
        Inspection_ViewModel m_Inspection;
        RecipeWizard_ViewModel m_Wizard;
        FrontSide_ViewModel m_FrontSide;
        BackSide_ViewModel m_BackSide;
        EBR_ViewModel m_EBR;
        Edge_ViewModel m_Edge;
        Alignment_ViewModel m_AlignMent;
        General_ViewModel m_General;
        Maintenance_ViewModel m_Maint;
        GEM_ViewModel m_Gem;


        public Setup_ViewModel()
        {
            init();
        }

        public void init(MainWindow main = null)
        {
            p_NaviButtons = new ObservableCollection<UIElement>();
            initPanel();
            InitNaviBtn();
            SetHome();

            m_MainWindow = main;
        }
        private void initPanel()
        {
            m_Home = new Home_ViewModel(this);
            m_Inspection = new Inspection_ViewModel(this);
            m_Wizard = new RecipeWizard_ViewModel(this);
            m_FrontSide = new FrontSide_ViewModel(this);
            m_BackSide = new BackSide_ViewModel(this);
            m_EBR = new EBR_ViewModel(this);
            m_Edge = new Edge_ViewModel(this);
            m_AlignMent = new Alignment_ViewModel(this);
            m_General = new General_ViewModel(this);
            m_Maint = new Maintenance_ViewModel(this);
            m_Gem = new GEM_ViewModel(this);
        }
        private void InitNaviBtn()
        {
            m_btnNaviInspection = new NaviBtn("Inspection");
            m_btnNaviInspection.Btn.Click += Navi_InspectionClick;

            m_btnNaviRecipeWizard = new NaviBtn("Recipe Wizard");
            m_btnNaviRecipeWizard.Btn.Click += Navi_RecipeWizardClick;

            m_btnNaviMaintenance = new NaviBtn("Maintenance");
            m_btnNaviMaintenance.Btn.Click += Navi_MaintClick;

            m_btnNaviGEM = new NaviBtn("GEM");
            m_btnNaviGEM.Btn.Click += Navi_GEMClick;

            m_btnNaviFrontSide = new NaviBtn("FrontSide");
            m_btnNaviFrontSide.Btn.Click += Navi_FrontSideClick;

            m_btnNaviBackSide = new NaviBtn("BackSide");
            m_btnNaviBackSide.Btn.Click += Navi_BackSideClick;

            m_btnNaviEBR = new NaviBtn("EBR");
            m_btnNaviEBR.Btn.Click += Navi_EBRClick;

            m_btnNaviEdge = new NaviBtn("Edge");
            m_btnNaviEdge.Btn.Click += Navi_EdgeClick;

            m_btnNaviAlignment = new NaviBtn("Alignment");
            m_btnNaviAlignment.Btn.Click += Navi_AlignmentClick;

            m_btnNaviAlginOrigin = new NaviBtn("Origin");
            m_btnNaviAlginOrigin.Btn.Click += Navi_AlignOriginClick;

            m_btnNaviAlignDiePosition = new NaviBtn("Die Position");
            m_btnNaviAlignDiePosition.Btn.Click += Navi_AlignPositionClick;

            m_btnNaviAlignSetup = new NaviBtn("Alignment Setup");
            m_btnNaviAlignSetup.Btn.Click += Navi_AlignSetupClick;

            m_btnNaviAlignMap = new NaviBtn("Map");
            m_btnNaviAlignMap.Btn.Click += Navi_AlignMapClick;

            m_btnNaviGeneral = new NaviBtn("General");
            m_btnNaviGeneral.Btn.Click += NaviGeneralBtn_Click;

            m_btnNaviGeneralMask = new NaviBtn("Mask");
            m_btnNaviGeneralMask.Btn.Click += NaviGeneralMaksBtn_Click;

            m_btnNaviGeneralSetup = new NaviBtn("General Setup");
            m_btnNaviGeneralSetup.Btn.Click += NaviGeneralSetupBtn_Click;

        }

        #region Navi Buttons

        // SetupHome Navi Buttons
        public NaviBtn m_btnNaviInspection;
        public NaviBtn m_btnNaviRecipeWizard;
        public NaviBtn m_btnNaviMaintenance;
        public NaviBtn m_btnNaviGEM;

        // RecipeWizard Navi Buttons
        public NaviBtn m_btnNaviFrontSide;
        public NaviBtn m_btnNaviBackSide;
        public NaviBtn m_btnNaviEBR;
        public NaviBtn m_btnNaviEdge;

        // FrontSide - Alignment Navi Buttons
        public NaviBtn m_btnNaviAlignment;
        public NaviBtn m_btnNaviAlignSetup;
        public NaviBtn m_btnNaviAlginOrigin;
        public NaviBtn m_btnNaviAlignDiePosition;
        public NaviBtn m_btnNaviAlignMap;

        // FrontSide - General Navi Buttons
        public NaviBtn m_btnNaviGeneral;
        public NaviBtn m_btnNaviGeneralSummary;
        public NaviBtn m_btnNaviGeneralMask;
        public NaviBtn m_btnNaviGeneralSetup;

        // 
        #endregion

        #region NaviBtn Event

        #region Main
        void Navi_InspectionClick(object sender, RoutedEventArgs e)
        {
            SetInspection();
        }
        void Navi_RecipeWizardClick(object sender, RoutedEventArgs e)
        {
            SetRecipeWizard();
        }
        void Navi_MaintClick(object sender, RoutedEventArgs e)
        {
            SetMaintenance();
        }
        void Navi_GEMClick(object sender, RoutedEventArgs e)
        {
            SetGEM();
        }
        #endregion

        #region Recipe Wizard
        private void Navi_FrontSideClick(object sender, RoutedEventArgs e)
        {
            SetWizardFrontSide();
        }
        private void Navi_BackSideClick(object sender, RoutedEventArgs e)
        {
            SetWizardBackSide();
        }
        private void Navi_EBRClick(object sender, RoutedEventArgs e)
        {
            SetWizardEBR();
        }
        private void Navi_EdgeClick(object sender, RoutedEventArgs e)
        {
            SetWizardEdge();
        }


        // FrontSide - Align
        void Navi_AlignmentClick(object sender, RoutedEventArgs e)
        {
            SetFrontAlignment();
        }
        void Navi_AlignSetupClick(object sender, RoutedEventArgs e)
        {
            SetFrontAlignSetup();
        }
        void Navi_AlignOriginClick(object sender, RoutedEventArgs e)
        {
            SetFrontAlignOrigin();
        }
        void Navi_AlignPositionClick(object sender, RoutedEventArgs e)
        {
            SetFrontAlignPosition();
        }
        void Navi_AlignMapClick(object sender, RoutedEventArgs e)
        {
            SetFrontAlignMap();
        }

        // FrontSide - General
        void NaviGeneralBtn_Click(object sender, RoutedEventArgs e)
        {
            SetFrontGeneral();
        }
        void NaviGeneralMaksBtn_Click(object sender, RoutedEventArgs e)
        {
            SetFrontGeneralMask();
        }
        void NaviGeneralSetupBtn_Click(object sender, RoutedEventArgs e)
        {
            SetFrontGeneralSetup();
        }

        #endregion


        #endregion

        #region Command
        public ICommand btnMode
        {
            get
            {
                return new RelayCommand(() =>
                {
                    SetHome();
                    m_MainWindow.MainPanel.Children.Clear();
                    m_MainWindow.MainPanel.Children.Add(m_MainWindow.m_ModeUI);
                });
            }
        }
        public ICommand btnNaviSetupHome
        {
            get
            {
                return new RelayCommand(SetHome);
            }
        }
        #endregion

        #region Panel Change Method

        #region Main
        public void SetHome()
        {
            p_NaviButtons.Clear();

            m_Home.SetPage(m_Home.Summary);
            p_CurrentPanel = m_Home.Main;
            p_CurrentPanel.DataContext = m_Home;
        }
        public void SetInspection()
        {
            p_NaviButtons.Clear();
            p_NaviButtons.Add(m_btnNaviInspection);

            p_CurrentPanel = m_Inspection.Main;
            p_CurrentPanel.DataContext = m_Inspection;
        }
        public void SetRecipeWizard()
        {
            p_NaviButtons.Clear();
            p_NaviButtons.Add(m_btnNaviRecipeWizard);

            m_Wizard.SetPage(m_Wizard.Summary);

            p_CurrentPanel = m_Wizard.Main;
            p_CurrentPanel.DataContext = m_Wizard;
        }
        public void SetMaintenance()
        {
            p_NaviButtons.Clear();
            p_NaviButtons.Add(m_btnNaviMaintenance);

            p_CurrentPanel = m_Maint.Main;
            p_CurrentPanel.DataContext = m_Maint;
        }
        public void SetGEM()
        {
            p_NaviButtons.Clear();
            p_NaviButtons.Add(m_btnNaviGEM);

            p_CurrentPanel = m_Gem.Main;
            p_CurrentPanel.DataContext = m_Gem;
        }
        #endregion

        #region Recipe Wizard
        public void SetWizardFrontSide()
        {
            p_NaviButtons.Clear();
            p_NaviButtons.Add(m_btnNaviRecipeWizard);
            p_NaviButtons.Add(m_btnNaviFrontSide);

            m_FrontSide.SetPage(m_FrontSide.Summary);

            p_CurrentPanel = m_FrontSide.Main;
            p_CurrentPanel.DataContext = m_FrontSide;
        }
        public void SetWizardBackSide()
        {
            p_NaviButtons.Clear();
            p_NaviButtons.Add(m_btnNaviRecipeWizard);
            p_NaviButtons.Add(m_btnNaviBackSide);

            p_CurrentPanel = m_BackSide.Main;
            p_CurrentPanel.DataContext = m_BackSide;
        }
        public void SetWizardEBR()
        {
            p_NaviButtons.Clear();
            p_NaviButtons.Add(m_btnNaviRecipeWizard);
            p_NaviButtons.Add(m_btnNaviEBR);

            p_CurrentPanel = m_EBR.Main;
            p_CurrentPanel.DataContext = m_EBR;
        }
        public void SetWizardEdge()
        {
            p_NaviButtons.Clear();
            p_NaviButtons.Add(m_btnNaviRecipeWizard);
            p_NaviButtons.Add(m_btnNaviEdge);

            p_CurrentPanel = m_Edge.Main;
            p_CurrentPanel.DataContext = m_Edge;
        }

        // FrontSide - Align
        public void SetFrontAlignment()
        {
            p_NaviButtons.Clear();
            p_NaviButtons.Add(m_btnNaviRecipeWizard);
            p_NaviButtons.Add(m_btnNaviFrontSide);
            p_NaviButtons.Add(m_btnNaviAlignment);

            m_AlignMent.SetPage(m_AlignMent.Summary);

            p_CurrentPanel = m_AlignMent.Main;
            p_CurrentPanel.DataContext = m_AlignMent;
        }
        public void SetFrontAlignSetup()
        {
            p_NaviButtons.Clear();
            p_NaviButtons.Add(m_btnNaviRecipeWizard);
            p_NaviButtons.Add(m_btnNaviFrontSide);
            p_NaviButtons.Add(m_btnNaviAlignment);
            p_NaviButtons.Add(m_btnNaviAlignSetup);

            //m_AlignMent.SetAlignSetup();
            m_AlignMent.SetPage(m_AlignMent.Setup);

            p_CurrentPanel = m_AlignMent.Main;
            p_CurrentPanel.DataContext = m_AlignMent;
        }
        public void SetFrontAlignOrigin()
        {
            p_NaviButtons.Clear();
            p_NaviButtons.Add(m_btnNaviRecipeWizard);
            p_NaviButtons.Add(m_btnNaviFrontSide);
            p_NaviButtons.Add(m_btnNaviAlignment);
            p_NaviButtons.Add(m_btnNaviAlginOrigin);

            //m_AlignMent.SetAlignOrigin();
            m_AlignMent.SetPage(m_AlignMent.Origin);

            p_CurrentPanel = m_AlignMent.Main;
            p_CurrentPanel.DataContext = m_AlignMent;
        }
        public void SetFrontAlignPosition()
        {
            p_NaviButtons.Clear();
            p_NaviButtons.Add(m_btnNaviRecipeWizard);
            p_NaviButtons.Add(m_btnNaviFrontSide);
            p_NaviButtons.Add(m_btnNaviAlignment);
            p_NaviButtons.Add(m_btnNaviAlignDiePosition);

            m_AlignMent.SetPage(m_AlignMent.Position);

            p_CurrentPanel = m_AlignMent.Main;
            p_CurrentPanel.DataContext = m_AlignMent;
        }
        public void SetFrontAlignMap()
        {
            p_NaviButtons.Clear();
            p_NaviButtons.Add(m_btnNaviRecipeWizard);
            p_NaviButtons.Add(m_btnNaviFrontSide);
            p_NaviButtons.Add(m_btnNaviAlignment);
            p_NaviButtons.Add(m_btnNaviAlignMap);

            m_AlignMent.SetPage(m_AlignMent.Map);

            p_CurrentPanel = m_AlignMent.Main;
            p_CurrentPanel.DataContext = m_AlignMent;
        }

        // FrontSide - General
        public void SetFrontGeneral()
        {
            p_NaviButtons.Clear();
            p_NaviButtons.Add(m_btnNaviRecipeWizard);
            p_NaviButtons.Add(m_btnNaviFrontSide);
            p_NaviButtons.Add(m_btnNaviGeneral);

            m_General.SetPage(m_General.Summary);

            p_CurrentPanel = m_General.Main;
            p_CurrentPanel.DataContext = m_General;
        }
        public void SetFrontGeneralMask()
        {
            p_NaviButtons.Clear();
            p_NaviButtons.Add(m_btnNaviRecipeWizard);
            p_NaviButtons.Add(m_btnNaviFrontSide);
            p_NaviButtons.Add(m_btnNaviGeneral);
            p_NaviButtons.Add(m_btnNaviGeneralMask);

            m_General.SetPage(m_General.Mask);

            p_CurrentPanel = m_General.Main;
            p_CurrentPanel.DataContext = m_General;
        }
        public void SetFrontGeneralSetup()
        {
            p_NaviButtons.Clear();
            p_NaviButtons.Add(m_btnNaviRecipeWizard);
            p_NaviButtons.Add(m_btnNaviFrontSide);
            p_NaviButtons.Add(m_btnNaviGeneral);
            p_NaviButtons.Add(m_btnNaviGeneralSetup);

            m_General.SetPage(m_General.Setup);

            p_CurrentPanel = m_General.Main;
            p_CurrentPanel.DataContext = m_General;
        }
        #endregion

        #endregion
    }



}
