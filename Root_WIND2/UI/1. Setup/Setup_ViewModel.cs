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
using System.Windows.Threading;
using RootTools;
using RootTools_Vision;

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

        private ObservableCollection<UIElement> m_NaviButtons = new ObservableCollection<UIElement>();
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

        private Recipe recipe;
        public Recipe pRecipe { get => recipe; set => recipe = value; }

        //public MainWindow m_MainWindow;
        public ProgramManager programManager;

        private Home_ViewModel homeVM;
        private Inspection_ViewModel inspectionVM;
        private RecipeWizard_ViewModel wizardVM;
        private Frontside_ViewModel frontsideVM;
        private Backside_ViewModel backsideVM;
        private EBR_ViewModel ebrVM;
        private Edge_ViewModel edgeVM;
        //private InspTest_ViewModel inspTestVM;  //삭제
        private BacksideInspection_ViewModel backsideInspTestVM;
        private Maintenance_ViewModel maintVM;
        private GEM_ViewModel gemVM;

        private InspectionManagerFrontside inspectionMgrVision;
        private InspectionManagerEdge inspectionMgrEFEM;

        public Setup_ViewModel()
        {
            init();
        }

        public Setup_ViewModel(Recipe _recipe = null , InspectionManagerFrontside _inspectionMgrVision = null, InspectionManagerEdge _inspectionMgrEFEM = null)
        {        
            this.recipe = _recipe;
            inspectionMgrVision = _inspectionMgrVision;
            inspectionMgrEFEM = _inspectionMgrEFEM;
            init();
            
            
        }

        public void init()
        {
            InitAllPanel();
            InitAllNaviBtn();
            InitEvent();
            SetHome();
        }

        public void UI_Redraw()
        {
            frontsideVM.UI_Redraw();
            // Back
            // Edr
            // Edge
        }

        private void InitAllPanel()
        {
            homeVM = new Home_ViewModel(this);
            inspectionVM = new Inspection_ViewModel(this);
            Wizard = new RecipeWizard_ViewModel(this);
            frontsideVM = new Frontside_ViewModel(this);
            backsideVM = new Backside_ViewModel(this);
            ebrVM = new EBR_ViewModel(this);
            edgeVM = new Edge_ViewModel(this);
            maintVM = new Maintenance_ViewModel(this);
            gemVM = new GEM_ViewModel(this);
        }
        private void InitAllNaviBtn()
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

            m_btnNaviAlginOrigin = new NaviBtn("Origin");
            m_btnNaviAlginOrigin.Btn.Click += Navi_AlignOriginClick;

            m_btnNaviAlignDiePosition = new NaviBtn("Die Position");
            m_btnNaviAlignDiePosition.Btn.Click += Navi_AlignPositionClick;

            m_btnNaviAlignMap = new NaviBtn("Map");
            m_btnNaviAlignMap.Btn.Click += Navi_AlignMapClick;


            m_btnNaviGeneralMask = new NaviBtn("Mask");
            m_btnNaviGeneralMask.Btn.Click += NaviGeneralMaksBtn_Click;

            m_btnNaviGeneralSetup = new NaviBtn("General Setup");
            m_btnNaviGeneralSetup.Btn.Click += NaviGeneralSetupBtn_Click;

        }

        private void InitEvent()
        {
            WorkEventManager.UIRedraw += UIRedraw_Callback;
        }

        private void UIRedraw_Callback(object obj, UIRedrawEventArgs args)
        {
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                {
                  UI_Redraw();
                }));
            }
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


        // FrontSide
        void Navi_AlignMapClick(object sender, RoutedEventArgs e)
        {
            //SetFrontAlignMap();
        }
        void Navi_AlignOriginClick(object sender, RoutedEventArgs e)
        {
            //SetFrontAlignOrigin();
        }
        void Navi_AlignPositionClick(object sender, RoutedEventArgs e)
        {
            //SetFrontAlignPosition();
        }
        void NaviGeneralMaksBtn_Click(object sender, RoutedEventArgs e)
        {
            //SetFrontGeneralMask();
        }
        void NaviGeneralSetupBtn_Click(object sender, RoutedEventArgs e)
        {
            //SetFrontGeneralSetup();
        }

        // FrontSide - InspTest
        //void Navi_InspBtn_Click(object sender, RoutedEventArgs e)
        //{
        //    SetFrontInspTest();
        //}
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
                    UIManager.Instance.ChangeUIMode();
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
        public ICommand btnNewRecipe
        {
            get
            {
                return new RelayCommand(ProgramManager.Instance.ShowDialogSaveRecipe);
            }

        }

        public ICommand btnSaveRecipe
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (this.Recipe.RecipePath == "")
                    {
                        ProgramManager.Instance.ShowDialogSaveRecipe();
                    }
                    else
                    {
                        ProgramManager.Instance.SaveRecipe(this.Recipe.RecipePath);
                    }
                });
            }
        }

        public ICommand btnLoadRecipe
        {
            get
            {
                return new RelayCommand(ProgramManager.Instance.ShowDialogLoadRecipe);
            }
        }

        public ICommand btnPopUpSetting
        {
            get
            {
                return new RelayCommand(ShowSettingDialog);
            }
        }

        
        internal RecipeWizard_ViewModel Wizard { get => wizardVM; set => wizardVM = value; }
        public Recipe Recipe { get => recipe; set => recipe = value; }
        public InspectionManagerFrontside InspectionVision { get => inspectionMgrVision; set => inspectionMgrVision = value; }
        public InspectionManagerEdge InspectionManagerEFEM { get => inspectionMgrEFEM; set => inspectionMgrEFEM = value; }

        #endregion

        #region Panel Change Method

        #region Main
        public void SetHome()
        {
            p_NaviButtons.Clear();

            homeVM.SetPage(homeVM.Summary);
            p_CurrentPanel = homeVM.Main;
            p_CurrentPanel.DataContext = homeVM;
        }
        public void SetInspection()
        {
            p_NaviButtons.Clear();
            p_NaviButtons.Add(m_btnNaviInspection);

            p_CurrentPanel = inspectionVM.Main;
            p_CurrentPanel.DataContext = inspectionVM;
        }
        public void SetRecipeWizard()
        {
            p_NaviButtons.Clear();
            p_NaviButtons.Add(m_btnNaviRecipeWizard);

            Wizard.SetPage(Wizard.Summary);

            p_CurrentPanel = Wizard.Main;
            p_CurrentPanel.DataContext = Wizard;
        }
        public void SetMaintenance()
        {
            p_NaviButtons.Clear();
            p_NaviButtons.Add(m_btnNaviMaintenance);

            p_CurrentPanel = maintVM.Main;
            p_CurrentPanel.DataContext = maintVM;
        }
        public void SetGEM()
        {
            p_NaviButtons.Clear();
            p_NaviButtons.Add(m_btnNaviGEM);

            p_CurrentPanel = gemVM.Main;
            p_CurrentPanel.DataContext = gemVM;
        }

        public void ShowSettingDialog()
        {
            
            if(UIManager.Instance.SettingDialog.ShowDialog() == true)
            {

            }
        }

        #endregion

        #region Recipe Wizard
        public void SetWizardFrontSide()
        {
            p_NaviButtons.Clear();
            p_NaviButtons.Add(m_btnNaviRecipeWizard);
            p_NaviButtons.Add(m_btnNaviFrontSide);

            frontsideVM.SetPage(frontsideVM.Summary);

            p_CurrentPanel = frontsideVM.Main;
            p_CurrentPanel.DataContext = frontsideVM;
        }
        public void SetWizardBackSide()
        {
            p_NaviButtons.Clear();
            p_NaviButtons.Add(m_btnNaviRecipeWizard);
            p_NaviButtons.Add(m_btnNaviBackSide);

            p_CurrentPanel = backsideVM.Main;
            p_CurrentPanel.DataContext = backsideVM;
        }
        public void SetWizardEBR()
        {
            p_NaviButtons.Clear();
            p_NaviButtons.Add(m_btnNaviRecipeWizard);
            p_NaviButtons.Add(m_btnNaviEBR);

            p_CurrentPanel = ebrVM.Main;
            p_CurrentPanel.DataContext = ebrVM;
        }
        public void SetWizardEdge()
        {
            p_NaviButtons.Clear();
            p_NaviButtons.Add(m_btnNaviRecipeWizard);
            p_NaviButtons.Add(m_btnNaviEdge);

            p_CurrentPanel = edgeVM.Main;
            p_CurrentPanel.DataContext = edgeVM;
        }

        public void SetBacksideInspTest()
        {
            p_NaviButtons.Clear();
            p_NaviButtons.Add(m_btnNaviRecipeWizard);
            p_NaviButtons.Add(m_btnNaviBackSide);
        }
        #endregion

        #endregion
    }



}
