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
            m_ModeUI.tbDate.Text = DateTime.Now.ToString("HH:mm:ss");
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
        public int MemWidth = 80000;
        public int MemHeight = 80000;
        public int ROIWidth = 30000;
        public int ROIHeight = 30000; // Chip 크기 최대 30,000 * 30,000  Origin ROI 메모리 할당 20.11.02 JTL

        public RecipeManager m_RecipeMGR;
        Recipe m_Recipe;
        RecipeInfo m_RecipeInfo;
        RecipeEditor m_RecipeEditor;

        // DelegateLoadRecipe(object e)
        //{
        //    m_setupviewmodel.loadRecie(recipe);
        //}
        WIND2_InspectionManager m_InspectionManager;

        void Init()
        {
            dialogService = new DialogService(this);
            dialogService.Register<Dialog_ImageOpenViewModel, Dialog_ImageOpen>();

            m_engineer.Init("WIND2");
            m_memoryTool = m_engineer.ClassMemoryTool();
            m_memoryTool.GetPool(sPool, true).p_gbPool = 50;
            m_memoryTool.GetPool(sPool, true).GetGroup(sGroup).CreateMemory(sMem, 3, 1, new CPoint(MemWidth, MemHeight));
            m_memoryTool.GetPool(sPool, true).GetGroup(sGroup).CreateMemory("ROI", 1, 4, new CPoint(MemWidth, MemHeight));
            m_memoryTool.GetMemory(sPool, sGroup, sMem);

            m_Image = new ImageData(m_memoryTool.GetMemory(sPool, sGroup, sMem)); // Main ImageData
            m_ROILayer = new ImageData(m_memoryTool.GetMemory(sPool, sGroup, sMemROI)); // 4ch ROI BimtapLayer로 사용할 ImageData

            // Recipe Manager
            m_RecipeMGR = new RecipeManager();
            //m_RecipeMGR.DelegateLoadRecipe +=
            m_Recipe = m_RecipeMGR.GetRecipe();
            m_RecipeEditor = m_Recipe.GetRecipeEditor();
            m_RecipeInfo = m_Recipe.GetRecipeInfo();

            // Inspction Manager
            m_InspectionManager = new WIND2_InspectionManager(m_Image.GetPtr(), m_Image.p_Size.X, m_Image.p_Size.Y);
            m_InspectionManager.Recipe = m_Recipe;

            m_engineer.InspectionManager = m_InspectionManager;

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
            m_ModeUI = new SelectMode();
            m_ModeUI.Init(this);
            MainPanel.Children.Clear();
            MainPanel.Children.Add(m_ModeUI);
        }
        void InitSetupMode()
        {
            m_Setup = new Setup();
            m_SetupViewModel = new Setup_ViewModel(this, m_Recipe, m_InspectionManager);
            m_Setup.DataContext = m_SetupViewModel;
        }
        void InitReviewMode()
        {
            m_Review = new Review();
            m_ReviewViewModel = new Review_ViewModel(this, m_Review);
            m_Review.DataContext = m_ReviewViewModel;
        }
        void InitRunMode()
        {
            m_Run = new Run();
            m_RunViewModel = new Run_ViewModel(this);
            m_Run.DataContext = m_RunViewModel;
        }


        void ThreadStop()
        {
            m_engineer.ThreadStop();
        }
    }
}