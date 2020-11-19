using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Media;
using RootTools.Memory;
using RootTools;
using RootTools_CLR;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using RootTools_Vision;
using System.Drawing;
using RootTools.Database;

namespace Root_WIND2
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Window Event
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
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Init();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            ThreadStop();
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
        }
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

        #region UI
        public SelectMode m_ModeUI;
        public Setup m_Setup;
        public Review m_Review;
        public Run m_Run;
        public SelectMode ModeUI;
        public Setup Setup;
        public Review Review;
        public Run Run;
        #endregion

        #region ViewModel
        private Setup_ViewModel m_SetupViewModel;
        private Review_ViewModel m_ReviewViewModel;
        private Run_ViewModel m_RunViewModel;
        #endregion

        public WIND2_Engineer m_engineer = new WIND2_Engineer();
        MemoryTool m_memoryTool;
        public ImageData m_Image;
        public ImageData m_ROILayer;
        public IDialogService dialogService;
        string sPool = "pool";
        string sGroup = "group";
        string sMem = "mem";
        string sMemROI = "ROI";
        public int MemWidth = 40000;
        public int MemHeight = 40000;
        public int ROIWidth = 30000;
        public int ROIHeight = 30000; 

        public RecipeManager m_RecipeMGR;
        Recipe m_Recipe;
        RecipeInfo m_RecipeInfo;
        RecipeEditor m_RecipeEditor;

        InspectionManager_Vision inspMgrVision;
        InspectionManager_EFEM inspMgrEFEM;

        // DelegateLoadRecipe(object e)
        //{
        //    m_setupviewmodel.loadRecie(recipe);
        //}

        void Init()
        {
            dialogService = new DialogService(this);
            dialogService.Register<Dialog_ImageOpenViewModel, Dialog_ImageOpen>();

            m_engineer.Init("WIND2");
            m_memoryTool = m_engineer.ClassMemoryTool();
            m_Image = new ImageData(m_memoryTool.GetMemory(sPool, sGroup, sMem)); // Main ImageData
            m_ROILayer = new ImageData(m_memoryTool.GetMemory(sPool, sGroup, sMemROI)); // 4ch ROI BimtapLayer로 사용할 ImageData

            // Recipe Manager
            m_RecipeMGR = new RecipeManager();
            //m_RecipeMGR.DelegateLoadRecipe +=
            m_Recipe = m_RecipeMGR.GetRecipe();
            m_RecipeEditor = m_Recipe.GetRecipeEditor();
            m_RecipeInfo = m_Recipe.GetRecipeInfo();

            // Inspection Manager
            inspMgrVision = new InspectionManager_Vision(m_Image.GetPtr(), m_Image.p_Size.X, m_Image.p_Size.Y);
            inspMgrVision.Recipe = m_Recipe;
            m_engineer.InspectionMgrVision = inspMgrVision;

            ImageData edgeImage = m_engineer.m_handler.m_edgesideVision.GetMemoryData(Module.EdgeSideVision.eMemData.EdgeTop);
            inspMgrEFEM = new InspectionManager_EFEM(edgeImage.GetPtr(), edgeImage.p_Size.X, edgeImage.p_Size.Y, 3);
            inspMgrEFEM.Recipe = m_Recipe;
            m_engineer.InspectionMgrEFEM = inspMgrEFEM;

            ///////시연용 임시코드
            DatabaseManager.Instance.SetDatabase(1);
            //////

            InitModeSelect();
            InitTimer();
            InitSetupMode();
            InitReviewMode();
            InitRunMode();
        }

        void InitModeSelect()
        {
            ModeUI = new SelectMode();
            ModeUI.Init(this);
            MainPanel.Children.Clear();
            MainPanel.Children.Add(ModeUI);
        }
        void InitSetupMode()
        {
            Setup = new Setup();
            m_SetupViewModel = new Setup_ViewModel(this, m_Recipe, inspMgrVision, inspMgrEFEM);
            Setup.DataContext = m_SetupViewModel;
        }
        void InitReviewMode()
        {
            Review = new Review();
            m_ReviewViewModel = new Review_ViewModel(this, Review);
            Review.DataContext = m_ReviewViewModel;
        }
        void InitRunMode()
        {
            Run = new Run();
            m_RunViewModel = new Run_ViewModel(this);
            Run.DataContext = m_RunViewModel;
        }
        
        void ThreadStop()
        {
            m_engineer.ThreadStop();
        }
    }
}