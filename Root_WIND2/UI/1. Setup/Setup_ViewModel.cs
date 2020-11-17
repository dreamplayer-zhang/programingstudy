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

       public Recipe pRecipe { get => m_Recipe; set => m_Recipe = value; }

        public MainWindow m_MainWindow;

        Home_ViewModel m_Home;
        Inspection_ViewModel m_Inspection;
        RecipeWizard_ViewModel m_Wizard;
        Frontside_ViewModel m_FrontSide;
        Backside_ViewModel m_BackSide;
        EBR_ViewModel m_EBR;
        Edge_ViewModel m_Edge;
        InspTest_ViewModel m_InspTest;
        BacksideInspTest_ViewModel m_BacksideInspTest;
        Maintenance_ViewModel m_Maint;
        GEM_ViewModel m_Gem;

        Recipe m_Recipe;
        WIND2_InspectionManager m_InspectionManager;


        public Setup_ViewModel(MainWindow main)
        {
            init(main);
        }

        public Setup_ViewModel(MainWindow main, Recipe recipe = null , WIND2_InspectionManager _InspectionManager = null)
        {        
            m_Recipe = recipe;
            m_InspectionManager = _InspectionManager;

            init(main);            
        }

        public void init(MainWindow main = null)
        {
            m_MainWindow = main;

            InitAllPanel();
            InitAllNaviBtn();
            InitEvent();
            SetHome();
        }

        public void UI_Redraw()
        {
            DrawInsptestMap();
            m_FrontSide.UI_Redraw();
            // Back
            // Edr
            // Edge
        }

        public void DrawInsptestMap()
        {
            RecipeInfo_MapData mapdata = m_Recipe.GetRecipeInfo(typeof(RecipeInfo_MapData)) as RecipeInfo_MapData;
            if (mapdata.m_WaferMap != null)
            {
                int nMapX = mapdata.m_WaferMap.nMapSizeX;
                int nMapY = mapdata.m_WaferMap.nMapSizeY;

                if (p_CurrentPanel == m_InspTest.Main)
                {
                    m_InspTest.p_MapControl_VM.SetMap(mapdata.m_WaferMap.pWaferMap, new CPoint(nMapX, nMapY));
                    m_InspTest.p_MapControl_VM.CreateMapUI();
                }
                else
                {
                    m_BacksideInspTest.p_MapControl_VM.SetMap(mapdata.m_WaferMap.pWaferMap, new CPoint(nMapX, nMapY));
                    m_BacksideInspTest.p_MapControl_VM.CreateMapUI();
                }
            }     
        }

        private void InitAllPanel()
        {
            m_Home = new Home_ViewModel(this);
            m_Inspection = new Inspection_ViewModel(this);
            Wizard = new RecipeWizard_ViewModel(this);
            m_FrontSide = new Frontside_ViewModel(this);
            m_BackSide = new Backside_ViewModel(this);
            m_EBR = new EBR_ViewModel(this);
            m_Edge = new Edge_ViewModel(this);
            m_InspTest = new InspTest_ViewModel(this);
            m_BacksideInspTest = new BacksideInspTest_ViewModel(this);
            m_Maint = new Maintenance_ViewModel(this);
            m_Gem = new GEM_ViewModel(this);
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

            m_btnNaviInspTest = new NaviBtn("Test");
            m_btnNaviInspTest.Btn.Click += Navi_InspBtn_Click;

            m_btnNaviGeneralMask = new NaviBtn("Mask");
            m_btnNaviGeneralMask.Btn.Click += NaviGeneralMaksBtn_Click;

            m_btnNaviGeneralSetup = new NaviBtn("General Setup");
            m_btnNaviGeneralSetup.Btn.Click += NaviGeneralSetupBtn_Click;

        }

        private void InitEvent()
        {
            //m_InspectionManager.MapStateChanged += MapStateChanged_Callback;
            WorkEventManager.PositionDone += PositionDone_Callback;
            WorkEventManager.InspectionDone += SurfaceInspDone_Callback;
            WorkEventManager.ProcessDefectDone += ProcessDefectDone_Callback;
            WorkEventManager.UIRedraw += UIRedraw_Callback;
        }

        private void UIRedraw_Callback(object obj, UIRedrawEventArgs args)
        {
            //Workplace workplace = obj as Workplace;
            lock (this.lockObj)
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                {
                  UI_Redraw();
                }));
            }
        }


        object lockObj = new object();
        private void PositionDone_Callback(object obj, PositionDoneEventArgs args)
        {
            Workplace workplace = obj as Workplace;
            lock(this.lockObj)
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                {
                    String test = "";
                    if(true) // Display Option : Position Trans
                    {
                        test += "Trans : {" + workplace.TransX.ToString() + ", " + workplace.TransY.ToString() + "}" + "\n";
                    }
                    if (workplace.Index == 0)
                        m_InspTest.DrawRectMasterFeature(args.ptOldStart, args.ptOldEnd, args.ptNewStart, args.ptNewEnd, test);
                    else
                        m_InspTest.DrawRectChipFeature(args.ptOldStart, args.ptOldEnd, args.ptNewStart, args.ptNewEnd, test);
                }));
            }
        }
        private void SurfaceInspDone_Callback(object obj, InspectionDoneEventArgs args)
        {
            Workplace workplace = obj as Workplace;
            List<String> textList = new List<String>();
            List<CRect> rectList = new List<CRect>();
            foreach (RootTools.Database.Defect defectInfo in workplace.DefectList)
            {
                String text = "";

                if (true) // Display Option : Rel Position
                    text += "Pos : {" + defectInfo.fRelX.ToString() + ", " + defectInfo.fRelY.ToString() + "}" + "\n";
                if (true) // Display Option : Defect Size
                    text += "Size : " + defectInfo.fSize.ToString() + "\n";
                if (true) // Display Option : GV Value
                    text += "GV : " + defectInfo.fGV.ToString() + "\n";

                rectList.Add(new CRect((int)defectInfo.p_DefectBox.Left, (int)defectInfo.p_DefectBox.Top, (int)defectInfo.p_DefectBox.Right, (int)defectInfo.p_DefectBox.Bottom));
                textList.Add(text);
            }

            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                if (p_CurrentPanel == m_InspTest.Main)
                    m_InspTest.DrawRectDefect(rectList, textList, args.reDraw);
                else
                    m_BacksideInspTest.DrawRectDefect(rectList, textList, args.reDraw);
            }));
        }

        private void ProcessDefectDone_Callback(object obj, PocessDefectDoneEventArgs args)
        {
            Workplace workplace = obj as Workplace;

            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                if (p_CurrentPanel == m_InspTest.Main)
                    m_InspTest.UpdateDataGrid();
                if (p_CurrentPanel == m_InspTest.Main)
                    m_BacksideInspTest.UpdateDataGrid();
            }));
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

        // FrontSide - InspTest Navi Buttons
        public NaviBtn m_btnNaviInspTest;
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
        void Navi_InspBtn_Click(object sender, RoutedEventArgs e)
        {
            SetFrontInspTest();
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

        public ICommand btnTest
        {
            get
            {
                return new RelayCommand(Test);
            }
        }

        public ICommand btnSaveRecipe
        {
            get
            {
                return new RelayCommand(m_MainWindow.m_RecipeMGR.SaveRecipe);
            }
        }

        public ICommand btnLoadRecipe
        {
            get
            {
                return new RelayCommand(m_MainWindow.m_RecipeMGR.LoadRecipe);
            }
        }


        
        public void Test()
        {
            string sMsg = string.Format("{0}, {1}", ((RecipeData_Origin)Recipe.GetRecipeData().GetRecipeData(typeof(RecipeData_Origin))).OriginX, ((RecipeData_Origin)Recipe.GetRecipeData().GetRecipeData(typeof(RecipeData_Origin))).OriginY);
            MessageBox.Show(sMsg);
        }

        internal RecipeWizard_ViewModel Wizard { get => m_Wizard; set => m_Wizard 
                = value; }
        public Recipe Recipe { get => m_Recipe; set => m_Recipe = value; }
        public WIND2_InspectionManager InspectionManager { get => m_InspectionManager; set => m_InspectionManager = value; }
        

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

            Wizard.SetPage(Wizard.Summary);

            p_CurrentPanel = Wizard.Main;
            p_CurrentPanel.DataContext = Wizard;
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

      
        //public void SetFrontAlignOrigin()
        //{
        //    p_NaviButtons.Clear();
        //    p_NaviButtons.Add(m_btnNaviRecipeWizard);
        //    p_NaviButtons.Add(m_btnNaviFrontSide);
        //    p_NaviButtons.Add(m_btnNaviAlignment);
        //    p_NaviButtons.Add(m_btnNaviAlginOrigin);

        //    //m_AlignMent.SetAlignOrigin();
        //    m_AlignMent.SetPage(m_AlignMent.Origin);
        //    p_CurrentPanel = m_AlignMent.Main;
        //    p_CurrentPanel.DataContext = m_AlignMent;

        //}
        //public void SetFrontAlignPosition()
        //{
        //    p_NaviButtons.Clear();
        //    p_NaviButtons.Add(m_btnNaviRecipeWizard);
        //    p_NaviButtons.Add(m_btnNaviFrontSide);
        //    p_NaviButtons.Add(m_btnNaviAlignment);
        //    p_NaviButtons.Add(m_btnNaviAlignDiePosition);

        //    m_AlignMent.SetPage(m_AlignMent.Position);
        //    m_AlignMent.p_Position_VM.init(this, m_AlignMent.m_Recipe);
        //    p_CurrentPanel = m_AlignMent.Main;
        //    p_CurrentPanel.DataContext = m_AlignMent;
        //}
        //public void SetFrontAlignMap()
        //{
        //    p_NaviButtons.Clear();
        //    p_NaviButtons.Add(m_btnNaviRecipeWizard);
        //    p_NaviButtons.Add(m_btnNaviFrontSide);
        //    p_NaviButtons.Add(m_btnNaviAlignment);
        //    p_NaviButtons.Add(m_btnNaviAlignMap);

        //    m_AlignMent.SetPage(m_AlignMent.Map);

        //    p_CurrentPanel = m_AlignMent.Main;
        //    p_CurrentPanel.DataContext = m_AlignMent;
        //}

        // FrontSide - General
        //public void SetFrontGeneralMask()
        //{
        //    p_NaviButtons.Clear();
        //    p_NaviButtons.Add(m_btnNaviRecipeWizard);
        //    p_NaviButtons.Add(m_btnNaviFrontSide);
        //    p_NaviButtons.Add(m_btnNaviGeneral);
        //    p_NaviButtons.Add(m_btnNaviGeneralMask);

        //    m_General.SetPage(m_General.Mask);

        //    p_CurrentPanel = m_General.Main;
        //    p_CurrentPanel.DataContext = m_General;
        //}
        //public void SetFrontGeneralSetup()
        //{
        //    p_NaviButtons.Clear();
        //    p_NaviButtons.Add(m_btnNaviRecipeWizard);
        //    p_NaviButtons.Add(m_btnNaviFrontSide);
        //    p_NaviButtons.Add(m_btnNaviGeneral);
        //    p_NaviButtons.Add(m_btnNaviGeneralSetup);

        //    m_General.SetPage(m_General.Setup);

        //    p_CurrentPanel = m_General.Main;
        //    p_CurrentPanel.DataContext = m_General;
        //}
        // FrontSide Inspection TEST
        public void SetFrontInspTest()
        {
            p_NaviButtons.Clear();
            p_NaviButtons.Add(m_btnNaviRecipeWizard);
            p_NaviButtons.Add(m_btnNaviFrontSide);
            p_NaviButtons.Add(m_btnNaviInspTest);

            m_InspTest.SetPage(m_InspTest.InspTest);

            p_CurrentPanel = m_InspTest.Main;
            p_CurrentPanel.DataContext = m_InspTest;
        }
        public void SetBacksideInspTest()
        {
            p_NaviButtons.Clear();
            p_NaviButtons.Add(m_btnNaviRecipeWizard);
            p_NaviButtons.Add(m_btnNaviBackSide);
            p_NaviButtons.Add(m_btnNaviInspTest);

            m_BacksideInspTest.SetPage(m_BacksideInspTest.InspTest);
           
            p_CurrentPanel = m_BacksideInspTest.Main;
            p_CurrentPanel.DataContext = m_BacksideInspTest;
        }
        #endregion

        #endregion
    }



}
