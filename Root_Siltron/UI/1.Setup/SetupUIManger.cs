using Root_Siltron.Button;
using RootTools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Root_Siltron
{
    public class SetupUIManger : ObservableObject
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

        public HomeUI m_Home;
        public InspectionUI m_Insp;
        public RecipeWizardUI m_RecipeWizard;
        public AlignmentUI m_AlignMent;
        public GeneralUI m_General;
        public MaintenanceUI m_Maint;
        public GEMUI m_Gem;

        MainWindow m_MainWindow;

        public SetupUIManger()
        {
            init();          
        }

        public void init(MainWindow main = null)
        {
            p_NaviButtons = new ObservableCollection<UIElement>();
            initPanel();
            initNaviBtn();
            SetHome();

            m_MainWindow = main;
        }
        

        private void initPanel()
        {
            m_Home = new HomeUI(this);
            m_Insp = new InspectionUI(this);
            m_RecipeWizard = new RecipeWizardUI(this);
            m_AlignMent = new AlignmentUI(this);
            m_General = new GeneralUI(this);
            m_Maint = new MaintenanceUI(this);
            m_Gem = new GEMUI(this);
        }
        private void initNaviBtn()
        {
            m_btnNaviInspection = new NaviBtn("Inspection");
            m_btnNaviInspection.Btn.Click += NaviInspectionBtn_Click;

            m_btnNaviRecipeWizard = new NaviBtn("Recipe Wizard");
            m_btnNaviRecipeWizard.Btn.Click += NaviRecipeWizardBtn_Click;

            m_btnNaviMaintenance = new NaviBtn("Maintenance");
            m_btnNaviMaintenance.Btn.Click += NaviMaintenanceBtn_Click;

            m_btnNaviGEM = new NaviBtn("GEM");
            m_btnNaviGEM.Btn.Click += NaviGEMBtn_Click;

            #region Align
            m_btnNaviAlignment = new NaviBtn("Alignment");
            m_btnNaviAlignment.Btn.Click += NaviAlignmentBtn_Click;
            m_btnNaviAlginOrigin = new NaviBtn("Origin");
            m_btnNaviAlginOrigin.Btn.Click += NaviAlignOriginBtn_Click;
            m_btnNaviAlignDiePosition = new NaviBtn("Die Position");
            m_btnNaviAlignDiePosition.Btn.Click+=NaviAlignDiePosBtn_Click;
            m_btnNaviAlignSetup = new NaviBtn("Alignment Setup");
            m_btnNaviAlignSetup.Btn.Click += NaviAlignSetupBtn_Click;
            #endregion

            #region General
            m_btnNaviGeneral = new NaviBtn("General");
            m_btnNaviGeneral.Btn.Click += NaviGeneralBtn_Click;          
            m_btnNaviGeneralAlignRcp = new NaviBtn("Algin Recipe");
            m_btnNaviGeneralAlignRcp.Btn.Click += NaviGeneralAlignRcpBtn_Click;
            m_btnNaviGeneralMask = new NaviBtn("Mask");
            m_btnNaviGeneralMask.Btn.Click += NaviGeneralMaksBtn_Click;
            m_btnNaviGeneralSetup = new NaviBtn("General Setup");
            m_btnNaviGeneralSetup.Btn.Click += NaviGeneralSetupBtn_Click;
            #endregion

        }

        #region NaviButtons
        public NaviBtn m_btnNaviInspection;
        public NaviBtn m_btnNaviMaintenance;
        public NaviBtn m_btnNaviGEM;

        public NaviBtn m_btnNaviRecipeWizard;
        public NaviBtn m_btnNaviRecipeSummary;

        public NaviBtn m_btnNaviAlignment;
        public NaviBtn m_btnNaviAlignSummary;
        public NaviBtn m_btnNaviAlignSetup;
        public NaviBtn m_btnNaviAlginOrigin;
        public NaviBtn m_btnNaviAlignDiePosition;

        public NaviBtn m_btnNaviGeneral;
        public NaviBtn m_btnNaviGeneralSummary;
        public NaviBtn m_btnNaviGeneralAlignRcp;
        public NaviBtn m_btnNaviGeneralMask;
        public NaviBtn m_btnNaviGeneralSetup;
        #endregion

        #region NaviBtn Event

        #region Main
        void NaviInspectionBtn_Click(object sender, RoutedEventArgs e)
        {
            SetInspection();
        }
        void NaviMaintenanceBtn_Click(object sender, RoutedEventArgs e)
        {
            SetMaintenance();
        }
        void NaviGEMBtn_Click(object sender, RoutedEventArgs e)
        {
            SetGEM();
        }
        void NaviRecipeWizardBtn_Click(object sender, RoutedEventArgs e)
        {
            SetRecipeWizard();
        }
        #endregion

        #region Align
        void NaviAlignmentBtn_Click(object sender, RoutedEventArgs e)
        {
            SetAlignMent();
        }     
        void NaviAlignSetupBtn_Click(object sender, RoutedEventArgs e)
        {
            SetAlignSetup();
        }
        void NaviAlignOriginBtn_Click(object sender, RoutedEventArgs e)
        {
            SetAlignOrigin();
        }
        void NaviAlignDiePosBtn_Click(object sender, RoutedEventArgs e)
        {
            return;
        }
        #endregion

        #region General
        void NaviGeneralBtn_Click(object sender, RoutedEventArgs e)
        {
            SetGeneral();
        }
        void NaviGeneralAlignRcpBtn_Click(object sender, RoutedEventArgs e)
        {
            SetGeneralAlignRcp();
        }
        void NaviGeneralMaksBtn_Click(object sender, RoutedEventArgs e)
        {
            SetGeneralMask();
        }
        void NaviGeneralSetupBtn_Click(object sender, RoutedEventArgs e)
        {
            SetGeneralSetup();
        }
        #endregion

        #endregion

        #region Panel Chage Event

        public ICommand btnHome
        {
            get
            {
                return new RelayCommand(()=>
                {
                    m_MainWindow.MainPanel.Children.Clear();
                    m_MainWindow.MainPanel.Children.Add(m_MainWindow.m_ModeUI);
                });
            }
        }

        public ICommand btnSetupHome
        {
            get
            {
                return new RelayCommand(SetHome);
            }
        }

        public void SetHome()
        {
            p_NaviButtons.Clear();

            m_Home.SetHomeSummary();
            p_CurrentPanel = m_Home.Main;
            p_CurrentPanel.DataContext = m_Home;
       
        }
        public void SetInspection()
        {
            p_NaviButtons.Clear();
            p_NaviButtons.Add(m_btnNaviInspection);

            p_CurrentPanel = m_Insp.Main;
            p_CurrentPanel.DataContext = m_Insp;
        }
        public void SetRecipeWizard()
        {
            p_NaviButtons.Clear();
            p_NaviButtons.Add(m_btnNaviRecipeWizard);

            m_RecipeWizard.SetRecipeWizardSummary();

            p_CurrentPanel = m_RecipeWizard.Main;
            p_CurrentPanel.DataContext = m_RecipeWizard;
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

        public void SetAlignMent()
        {
            p_NaviButtons.Clear();
            p_NaviButtons.Add(m_btnNaviRecipeWizard);
            p_NaviButtons.Add(m_btnNaviAlignment);

            //m_AlignMent.SetAlignSummary();
            m_AlignMent.SetPage(m_AlignMent.Summary);

            p_CurrentPanel = m_AlignMent.Main;
            p_CurrentPanel.DataContext = m_AlignMent;
        }
        public void SetAlignOrigin()
        {
            p_NaviButtons.Clear();
            p_NaviButtons.Add(m_btnNaviRecipeWizard);
            p_NaviButtons.Add(m_btnNaviAlignment);
            p_NaviButtons.Add(m_btnNaviAlginOrigin);

            //m_AlignMent.SetAlignOrigin();
            m_AlignMent.SetPage(m_AlignMent.Origin);

            p_CurrentPanel = m_AlignMent.Main;
            p_CurrentPanel.DataContext = m_AlignMent;
        }
        public void SetAlignDiePosition()
        {
            p_NaviButtons.Clear();
            p_NaviButtons.Add(m_btnNaviRecipeWizard);
            p_NaviButtons.Add(m_btnNaviAlignment);
            p_NaviButtons.Add(m_btnNaviAlignDiePosition);

            //m_AlignMent.SetAlignDiePosition();
            m_AlignMent.SetPage(m_AlignMent.DiePosition);

            p_CurrentPanel = m_AlignMent.Main;
            p_CurrentPanel.DataContext = m_AlignMent;
        }
        public void SetAlignSetup()
        {
            p_NaviButtons.Clear();
            p_NaviButtons.Add(m_btnNaviRecipeWizard);
            p_NaviButtons.Add(m_btnNaviAlignment);
            p_NaviButtons.Add(m_btnNaviAlignSetup);

            //m_AlignMent.SetAlignSetup();
            m_AlignMent.SetPage(m_AlignMent.Setup);

            p_CurrentPanel = m_AlignMent.Main;
            p_CurrentPanel.DataContext = m_AlignMent;
        }

        public void SetGeneral()
        {
            p_NaviButtons.Clear();
            p_NaviButtons.Add(m_btnNaviRecipeWizard);
            p_NaviButtons.Add(m_btnNaviGeneral);

            m_General.SetGeneralSummary();

            p_CurrentPanel = m_General.Main;
            p_CurrentPanel.DataContext = m_General;
        }
        public void SetGeneralAlignRcp()
        {
            p_NaviButtons.Clear();
            p_NaviButtons.Add(m_btnNaviRecipeWizard);
            p_NaviButtons.Add(m_btnNaviGeneral);
            p_NaviButtons.Add(m_btnNaviGeneralAlignRcp);

            m_General.SetGeneralAlignRcp();

            p_CurrentPanel = m_General.Main;
            p_CurrentPanel.DataContext = m_General;
        }
        public void SetGeneralMask()
        {
            p_NaviButtons.Clear();
            p_NaviButtons.Add(m_btnNaviRecipeWizard);
            p_NaviButtons.Add(m_btnNaviGeneral);
            p_NaviButtons.Add(m_btnNaviGeneralMask);

            m_General.SetGeneralMask();

            p_CurrentPanel = m_General.Main;
            p_CurrentPanel.DataContext = m_General;
        }
        public void SetGeneralSetup()
        {
            p_NaviButtons.Clear();
            p_NaviButtons.Add(m_btnNaviRecipeWizard);
            p_NaviButtons.Add(m_btnNaviGeneral);
            p_NaviButtons.Add(m_btnNaviGeneralSetup);

            m_General.SetGeneralSetup();

            p_CurrentPanel = m_General.Main;
            p_CurrentPanel.DataContext = m_General;
        }


        #endregion

        #region UI
        public class HomeUI
        {
            SetupUIManger m_Navigation;
            public HomePanel Main;
            public HomeSummaryPage Summary;

            public HomeUI(SetupUIManger navi)
            {
                m_Navigation = navi;
                init();              
            }
            private void init()
            {
                Main = new HomePanel();
                Summary = new HomeSummaryPage();

                SetHomeSummary();
            }

            public void SetHomeSummary()
            {
                Main.SubPanel.Children.Clear();
                Main.SubPanel.Children.Add(Summary);
            }
            public ICommand btnSummary
            {
                get
                {
                    return new RelayCommand(SetHomeSummary);
                }
            }
            public ICommand btnInspection
            {
                get
                {
                    return new RelayCommand(m_Navigation.SetInspection);
                }
            }
            public ICommand btnRecipeWizard
            {
                get
                {
                    return new RelayCommand(m_Navigation.SetRecipeWizard);
                }
            }
            public ICommand btnMaintenance
            {
                get
                {
                    return new RelayCommand(m_Navigation.SetMaintenance);
                }
            }
            public ICommand btnGEM
            {
                get
                {
                    return new RelayCommand(m_Navigation.SetGEM);
                }
            }
        }
        public class InspectionUI
        {
            SetupUIManger m_Navigation;
            public InspectionPanel Main;

            public InspectionUI(SetupUIManger navi)
            {
                m_Navigation = navi;
                init();
            }
            private void init()
            {
                Main = new InspectionPanel();
            }

            public ICommand btnInspStart
            {
                get
                {
                    return new RelayCommand(() =>
                    {
                    });
                }
            }
            public ICommand btnInspLoad
            {
                get
                {
                    return new RelayCommand(() =>
                    {
                    });
                }
            }
            public ICommand btnInspSnap
            {
                get
                {
                    return new RelayCommand(() =>
                    {
                    });
                }
            }
            public ICommand btnInspBack
            {
                get
                {
                    return new RelayCommand(m_Navigation.SetHome);
                }
            }
        }
        public class RecipeWizardUI
        {
            SetupUIManger m_Navigation;

            public RecipeWizardPanel Main;
            public RecipeSummaryPage Summary;

            public RecipeWizardUI(SetupUIManger navi)
            {
                m_Navigation = navi;
                init();
            }
            private void init()
            {
                Main = new RecipeWizardPanel();
                Summary = new RecipeSummaryPage();

                SetRecipeWizardSummary();
            }


            public void SetRecipeWizardSummary()
            {
                Main.SubPanel.Children.Clear();
                Main.SubPanel.Children.Add(Summary);        
            }
             
            public ICommand btnWizardSummary
            {
                get
                {
                    return new RelayCommand(SetRecipeWizardSummary);
                }
            }
            public ICommand btnWizardAlignnment
            {
                get
                {
                    return new RelayCommand(() =>
                    {
                        m_Navigation.SetAlignMent();
                    });
                }
            }
            public ICommand btnWizardGeneral
            {
                get
                {
                    return new RelayCommand(() =>
                    {
                        m_Navigation.SetGeneral();
                    });
                }
            }
            public ICommand btnWizardBack
            {
                get
                {
                    return new RelayCommand(() =>
                    {
                        m_Navigation.SetHome();
                    });
                }
            }
            
        }
        public class AlignmentUI
        {
            SetupUIManger m_Navigation;

            public AlignmentPanel Main;
            public AlignmentSummaryPage Summary;
            public AlginmentOriginPage Origin;
            public AlignmentSetupPage Setup;
            public AlignmentDiePosition DiePosition;
            public AlignmentUI(SetupUIManger navi)
            {
                m_Navigation = navi;
                init();
            }
            public void init()
            {
                Main = new AlignmentPanel();
                Summary = new AlignmentSummaryPage();
                Setup = new AlignmentSetupPage();
                Origin = new AlginmentOriginPage();
                DiePosition = new AlignmentDiePosition();

                SetPage(Summary);
                //SetAlignSummary();
            }

            public void SetPage(UserControl page)
            {
                Main.SubPanel.Children.Clear();
                Main.SubPanel.Children.Add(page);
            }


            public void SetAlignSummary()
            {
                Main.SubPanel.Children.Clear();
                Main.SubPanel.Children.Add(Summary);

            }
            public void SetAlignOrigin()
            {
                Main.SubPanel.Children.Clear();
                Main.SubPanel.Children.Add(Origin);
            }
            public void SetAlignDiePosition()
            {
                Main.SubPanel.Children.Clear();
                Main.SubPanel.Children.Add(DiePosition);
            }
            public void SetAlignSetup()
            {
                Main.SubPanel.Children.Clear();
                Main.SubPanel.Children.Add(Setup);          
            }

            public ICommand btnAlignSummary
            {
                get
                {
                    return new RelayCommand(m_Navigation.SetAlignMent);
                }
            }  
            public ICommand btnAlignOrigin
            {
                get
                {
                    return new RelayCommand(m_Navigation.SetAlignOrigin);
                }
            }                   
            public ICommand btnAlignSetup
            {
                get
                {
                    return new RelayCommand(m_Navigation.SetAlignSetup);
                }
            }
            public ICommand btnAlignLoad
            {
                get
                {
                    return new RelayCommand(() =>
                    {
                        
                    });
                }
            }
            public ICommand btnAlignSaveAs
            {
                get
                {
                    return new RelayCommand(() =>
                    {

                    });
                }
            }
            public ICommand btnAlignBack
            {
                get
                {
                    return new RelayCommand(m_Navigation.SetRecipeWizard);
                }
            }
        }
        public class GeneralUI
        {
            SetupUIManger m_Navigation;

            public GeneralPanel Main;
            public GeneralSummaryPage Summary;
            public GeneralAlignRecipePage AlignRecipe;
            public GeneralMaskPage Mask;
            public GeneralSetupPage Setup;

            public GeneralUI(SetupUIManger navi)
            {
                m_Navigation = navi;
                init();
            }
            public void init()
            {
                Main = new GeneralPanel();
                Summary = new GeneralSummaryPage();
                AlignRecipe = new GeneralAlignRecipePage();
                Mask = new GeneralMaskPage();
                Setup = new GeneralSetupPage();

                SetGeneralSummary();
                
            }

            public void SetGeneralSummary()
            {
                Main.SubPanel.Children.Clear();
                Main.SubPanel.Children.Add(Summary);
            }
            public void SetGeneralAlignRcp()
            {
                Main.SubPanel.Children.Clear();
                Main.SubPanel.Children.Add(AlignRecipe);
            }
            public void SetGeneralMask()
            {
                Main.SubPanel.Children.Clear();
                Main.SubPanel.Children.Add(Mask);
            }
            public void SetGeneralSetup()
            {
                Main.SubPanel.Children.Clear();
                Main.SubPanel.Children.Add(Setup);
            }

            public ICommand btnGeneralSummary
            {
                get
                {
                    return new RelayCommand(m_Navigation.SetGeneral);
                }
            }
            public ICommand btnGeneralAlginRecipe
            {
                get
                {
                    return new RelayCommand(m_Navigation.SetGeneralAlignRcp);
                }
            }
            public ICommand btnGeneralMask
            {
                get
                {
                    return new RelayCommand(m_Navigation.SetGeneralMask);
                }
            }
            public ICommand btnGeneralSetup
            {
                get
                {
                    return new RelayCommand(m_Navigation.SetGeneralSetup);
                }
            }
            public ICommand btnGeneralBack
            {
                get
                {
                    return new RelayCommand(m_Navigation.SetRecipeWizard);
                }
            }

        }
        public class MaintenanceUI
        {
            SetupUIManger m_Navigation;
            public MaintenancePanel Main;

            public MaintenanceUI(SetupUIManger navi)
            {
                m_Navigation = navi;
                init();
            }
            public void init()
            {
                Main = new MaintenancePanel();           
            }
            
            public ICommand btnMaintBack
            {
                get
                {
                    return new RelayCommand(() =>
                    {
                        m_Navigation.SetHome();
                    });
                }
            }
        }
        public class GEMUI
        {
            SetupUIManger m_Navigation;
            public GEMPanel Main;
            public NaviBtn naviGEM;

            public GEMUI(SetupUIManger navi)
            {
                m_Navigation = navi;
                init();

            }
            private void init()
            {
                Main = new GEMPanel();
                naviGEM = new NaviBtn("GEM");
            }
            public ICommand btnGEMBack
            {
                get
                {
                    return new RelayCommand(() =>
                    {
                        m_Navigation.SetHome();
                    });
                }
            }
        }
        #endregion
    }



}
