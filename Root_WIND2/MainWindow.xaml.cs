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
using System.Diagnostics;
using System.IO;
using System.Reflection;

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

            GlobalObjects.Instance.Get<InspectionManagerFrontside>().Exit();
            GlobalObjects.Instance.Get<InspectionManagerBackside>().Exit();

            GlobalObjects.Instance.Clear();
            //GlobalObjects.Instance.Get<InspectionManagerEdge>().Exit();
            //GlobalObjects.Instance.Get<InspectionManagerEBR>().Exit();

        }
        #endregion

        #region Timer
        DispatcherTimer m_timer = new DispatcherTimer();
        void InitTimer()
        {
            //m_timer.Interval = TimeSpan.FromMilliseconds(100);
            //m_timer.Tick += M_timer_Tick;
            //m_timer.Start();
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
            Application.Current.Shutdown();
        }
        #endregion

        public IDialogService dialogService;

        void Init()
        {
            CreateGlobalPaths();


            if (RegisterGlobalObjects() == false)
            {
                MessageBox.Show("Program Initialization fail");
                return;
            }

            if (UIManager.Instance.Initialize() == false)
            {
                MessageBox.Show("UI Initialization fail");
                return;
            }

            //// WPF 파라매터 연결
            UIManager.Instance.MainPanel = this.MainPanel;

            UIManager.Instance.ChangeUIMode();

            ///////시연용 임시코드
            DatabaseManager.Instance.SetDatabase(1);
            //////
            logView.Init(LogView.m_logView);
            InitTimer();

        }

        void ThreadStop()
        {
            GlobalObjects.Instance.Get<WIND2_Engineer>().ThreadStop();
        }

        private string memoryFrontPool = "Vision.Memory";
        private string memoryFrontGroup = "Vision";
        private string memoryFront = "Main";

        // Backside 메모리 이름 필요


        private string memoryMaskPool = "pool";
        private string memoryMaskGroup = "group";
        private string memoryMask = "ROI";

        private string memoryEdgePool = "EdgeSide Vision.Memory";
        private string memoryEdgeGroup = "EdgeSide Vision";
        private string memoryEdgeTop = "EdgeTop";
        private string memoryEdgeSide = "EdgeSide";
        private string memoryEdgeBottom = "EdgeBottom";
        private string memoryEdgeEBR = "EdgeEBR";


        public bool RegisterGlobalObjects()
        {
            try
            {
                // Engineer
                WIND2_Engineer engineer = GlobalObjects.Instance.Register<WIND2_Engineer>();
                engineer.Init("WIND2");

                MemoryTool memoryTool = engineer.ClassMemoryTool();

                ImageData frontImage = GlobalObjects.Instance.RegisterNamed<ImageData>("FrontImage", memoryTool.GetMemory(memoryFrontPool, memoryFrontGroup, memoryFront));
                ImageData maskLayer = GlobalObjects.Instance.RegisterNamed<ImageData>("MaskImage", memoryTool.GetMemory(memoryMaskPool, memoryMaskGroup, memoryMask));
                ImageData edgeTopImage = GlobalObjects.Instance.RegisterNamed<ImageData>("EdgeTopImage", memoryTool.GetMemory(memoryEdgePool, memoryEdgeGroup, memoryEdgeTop));
                ImageData edgeSideImage = GlobalObjects.Instance.RegisterNamed<ImageData>("EdgeSideImage", memoryTool.GetMemory(memoryEdgePool, memoryEdgeGroup, memoryEdgeSide));
                ImageData edgeBottomImage = GlobalObjects.Instance.RegisterNamed<ImageData>("EdgeBottomImage", memoryTool.GetMemory(memoryEdgePool, memoryEdgeGroup, memoryEdgeBottom));
                ImageData ebrImage = GlobalObjects.Instance.RegisterNamed<ImageData>("EBRImage", memoryTool.GetMemory(memoryEdgePool, memoryEdgeGroup, memoryEdgeEBR));

                frontImage.p_nByte = engineer.ClassMemoryTool().GetMemory(memoryFrontPool, memoryFrontGroup, memoryFront).p_nCount;
                maskLayer.p_nByte = engineer.ClassMemoryTool().GetMemory(memoryMaskPool, memoryMaskGroup, memoryMask).p_nByte;
                edgeTopImage.p_nByte = engineer.ClassMemoryTool().GetMemory(memoryMaskPool, memoryMaskGroup, memoryMask).p_nCount;
                edgeSideImage.p_nByte = engineer.ClassMemoryTool().GetMemory(memoryMaskPool, memoryMaskGroup, memoryMask).p_nCount;
                edgeBottomImage.p_nByte = engineer.ClassMemoryTool().GetMemory(memoryMaskPool, memoryMaskGroup, memoryMask).p_nCount;
                ebrImage.p_nByte = engineer.ClassMemoryTool().GetMemory(memoryMaskPool, memoryMaskGroup, memoryMask).p_nCount;


                // Recipe
                RecipeFront recipeFront = GlobalObjects.Instance.Register<RecipeFront>();
                RecipeBack recipeBack = GlobalObjects.Instance.Register<RecipeBack>();
                RecipeEdge recipeEdge = GlobalObjects.Instance.Register<RecipeEdge>();
                RecipeEBR recipeEBR = GlobalObjects.Instance.Register<RecipeEBR>();


                if(frontImage.GetPtr() == IntPtr.Zero)
                {
                    MessageBox.Show("Front Inspection 생성 실패, 메모리 할당 없음");
                }
                else
                {
                    // Inspection Manager
                    InspectionManagerFrontside inspectionFront = GlobalObjects.Instance.Register<InspectionManagerFrontside>
                        (
                        recipeFront,
                        new SharedBufferInfo(frontImage.GetPtr(0), frontImage.p_Size.X, frontImage.p_Size.Y, frontImage.p_nByte, frontImage.GetPtr(1), frontImage.GetPtr(2))
                        );
                }

                if (frontImage.GetPtr() == IntPtr.Zero)
                {
                    MessageBox.Show("Back Inspection 생성 실패, 메모리 할당 없음");
                }
                else
                {
                    InspectionManagerBackside inspectionBack = GlobalObjects.Instance.Register<InspectionManagerBackside>
                    (
                    recipeBack,
                    new SharedBufferInfo(frontImage.GetPtr(0), frontImage.p_Size.X, frontImage.p_Size.Y, frontImage.p_nByte, frontImage.GetPtr(1), frontImage.GetPtr(2))
                    );
                }

                if (edgeTopImage.GetPtr() == IntPtr.Zero || edgeSideImage.GetPtr() == IntPtr.Zero || edgeBottomImage.GetPtr() == IntPtr.Zero)
                {
                    MessageBox.Show("Edge Inspection 생성 실패, 메모리 할당 없음");
                }
                else
                {
                    InspectionManagerEdge inspectionEdge = GlobalObjects.Instance.Register<InspectionManagerEdge>
                    (
                    recipeEdge,
                    new SharedBufferInfo[]{
                        // [0] Top
                        new SharedBufferInfo(edgeTopImage.GetPtr(0), edgeTopImage.p_Size.X, edgeTopImage.p_Size.Y, edgeTopImage.p_nByte, edgeTopImage.GetPtr(1), edgeTopImage.GetPtr(2)),
                        // [1] Side
                        new SharedBufferInfo(edgeSideImage.GetPtr(0), edgeSideImage.p_Size.X, edgeSideImage.p_Size.Y, edgeSideImage.p_nByte, edgeSideImage.GetPtr(1), edgeSideImage.GetPtr(2)),
                        // [2]
                        new SharedBufferInfo(edgeBottomImage.GetPtr(0), edgeBottomImage.p_Size.X, edgeBottomImage.p_Size.Y, edgeBottomImage.p_nByte, edgeBottomImage.GetPtr(1), edgeBottomImage.GetPtr(2)),
                    });
                }


                if (ebrImage.GetPtr() == IntPtr.Zero)
                {
                    MessageBox.Show("EBR Inspection 생성 실패, 메모리 할당 없음");
                }
                else
                {
                    InspectionManagerEBR inspectionEBR = GlobalObjects.Instance.Register<InspectionManagerEBR>
                    (
                    recipeEBR,
                    new SharedBufferInfo(ebrImage.GetPtr(0), ebrImage.p_Size.X, ebrImage.p_Size.Y, ebrImage.p_nByte, ebrImage.GetPtr(1), ebrImage.GetPtr(2))
                    );
                }



                // DialogService
                DialogService dialogService = GlobalObjects.Instance.Register<DialogService>(this);
                dialogService.Register<Dialog_ImageOpenViewModel, Dialog_ImageOpen>();
                dialogService.Register<Dialog_Scan_ViewModel, Dialog_Scan>();
                dialogService.Register<SettingDialog_ViewModel, SettingDialog>();



            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }

            return true;
        }


        public void CreateGlobalPaths()
        {
            Type t = typeof(Constants.Path);
            FieldInfo[] fields = t.GetFields(BindingFlags.Static | BindingFlags.Public);
            foreach (FieldInfo field in fields)
                Directory.CreateDirectory(field.GetValue(null).ToString());
        }
    }
}