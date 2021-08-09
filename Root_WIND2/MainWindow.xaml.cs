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
using RootTools_Vision.Utility;

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

            //MessageBox.Show("DDD");
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {

            ThreadStop();

            //GlobalObjects.Instance.Get<InspectionManagerFrontside>().Exit();
            //GlobalObjects.Instance.Get<InspectionManagerBackside>().Exit();

            GlobalObjects.Instance.Clear();
            Application.Current.Shutdown();
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

            SettingItem_Database frontSettings = GlobalObjects.Instance.Get<Settings>().GetItem<SettingItem_Database>();
            ///////시연용 임시코드
            DatabaseManager.Instance.SetDatabase(1, frontSettings.SerevrName, frontSettings.DBName, frontSettings.DBUserID, frontSettings.DBPassword);
            DatabaseManager.Instance.ValidateDatabase();
            //////

            logView.Init(LogView._logView);
            WarningUI.Init(GlobalObjects.Instance.Get<WIND2_Warning>());
            InitTimer();
        }

        void ThreadStop()
        {
            GlobalObjects.Instance.Get<WIND2_Engineer>().ThreadStop();
        }

        private string memoryFrontPool = "Vision.Memory";
        private string memoryFrontGroup = "Vision";
        private string memoryFront = "Main";
        private string memoryMask = "Layer";

        private string memoryBackPool = "BackSide Vision.BackSide Memory";
        private string memoryBackGroup = "BackSide Vision";
        private string memoryBack = "BackSide";


        private string memoryEdgePool = "EdgeSide Vision.Memory";
        private string memoryEdgeGroup = "EdgeSide Vision";
        private string memoryEdgeTop = "EdgeTop";
        private string memoryEdgeSide = "EdgeSide";
        private string memoryEdgeBottom = "EdgeBottom";
        private string memoryEdgeEBR = "EBR";
        
        public bool RegisterGlobalObjects()
        {
            try
            {
                // Settings
                Settings settings = GlobalObjects.Instance.Register<Settings>();
                SettingItem_Setup setupSetting = settings.GetItem<SettingItem_Setup>();
                KlarfSetting klarfSettings = GlobalObjects.Instance.Register<KlarfSetting>();

                // Engineer
                WIND2_Engineer engineer = GlobalObjects.Instance.Register<WIND2_Engineer>();
                DialogService dialogService = GlobalObjects.Instance.Register<DialogService>(this);
                WIND2_Warning warning = GlobalObjects.Instance.Register<WIND2_Warning>();
                engineer.Init("WIND2");
                
                MemoryTool memoryTool = engineer.ClassMemoryTool();
                ImageData frontImage;
                ImageData maskLayer;

                // ImageData
                
                if (engineer.m_eMode == WIND2_Engineer.eMode.EFEM)
                {
                    MemoryData memoryData = memoryTool.GetMemory(memoryFrontPool, memoryFrontGroup, memoryFront);
                    frontImage = GlobalObjects.Instance.RegisterNamed<ImageData>("FrontImage", memoryFrontPool, memoryFrontGroup, memoryFront, memoryTool, memoryData.p_nCount, memoryData.p_nByte);
                    maskLayer = GlobalObjects.Instance.RegisterNamed<ImageData>("MaskImage", memoryTool.GetMemory(memoryFrontPool, memoryFrontGroup, memoryMask));
                }
                else
                {
                    frontImage = GlobalObjects.Instance.RegisterNamed<ImageData>("FrontImage", memoryTool.GetMemory(memoryFrontPool, memoryFrontGroup, memoryFront));
                    maskLayer = GlobalObjects.Instance.RegisterNamed<ImageData>("MaskImage", memoryTool.GetMemory(memoryFrontPool, memoryFrontGroup, memoryMask));
                }

                ImageData backImage = GlobalObjects.Instance.RegisterNamed<ImageData>("BackImage", memoryTool.GetMemory(memoryBackPool, memoryBackGroup, memoryBack));

                ImageData edgeTopImage = GlobalObjects.Instance.RegisterNamed<ImageData>("EdgeTopImage", memoryTool.GetMemory(memoryEdgePool, memoryEdgeGroup, memoryEdgeTop));
                ImageData edgeSideImage = GlobalObjects.Instance.RegisterNamed<ImageData>("EdgeSideImage", memoryTool.GetMemory(memoryEdgePool, memoryEdgeGroup, memoryEdgeSide));
                ImageData edgeBottomImage = GlobalObjects.Instance.RegisterNamed<ImageData>("EdgeBottomImage", memoryTool.GetMemory(memoryEdgePool, memoryEdgeGroup, memoryEdgeBottom));
                ImageData ebrImage = GlobalObjects.Instance.RegisterNamed<ImageData>("EBRImage", memoryTool.GetMemory(memoryEdgePool, memoryEdgeGroup, memoryEdgeEBR));

                if (frontImage.m_MemData != null)
                {
                    frontImage.p_nByte = engineer.ClassMemoryTool().GetMemory(memoryFrontPool, memoryFrontGroup, memoryFront).p_nByte;
                    frontImage.p_nPlane = engineer.ClassMemoryTool().GetMemory(memoryFrontPool, memoryFrontGroup, memoryFront).p_nCount;
                }
                //if (maskLayer.m_MemData != null)
                //{
                //    maskLayer.p_nByte = engineer.ClassMemoryTool().GetMemory(memoryFrontPool, memoryFrontGroup, memoryMask).p_nByte;
                //    maskLayer.p_nPlane = engineer.ClassMemoryTool().GetMemory(memoryFrontPool, memoryFrontGroup, memoryMask).p_nCount;
                //}
                if (edgeTopImage.m_MemData != null)
                {
                    edgeTopImage.p_nByte = engineer.ClassMemoryTool().GetMemory(memoryEdgePool, memoryEdgeGroup, memoryEdgeTop).p_nByte;
                    edgeTopImage.p_nPlane = engineer.ClassMemoryTool().GetMemory(memoryEdgePool, memoryEdgeGroup, memoryEdgeTop).p_nCount;
                }
                if (edgeSideImage.m_MemData != null)
                {
                    edgeSideImage.p_nByte = engineer.ClassMemoryTool().GetMemory(memoryEdgePool, memoryEdgeGroup, memoryEdgeSide).p_nByte;
                    edgeSideImage.p_nPlane = engineer.ClassMemoryTool().GetMemory(memoryEdgePool, memoryEdgeGroup, memoryEdgeSide).p_nCount;
                }
                if (edgeBottomImage.m_MemData != null)
                {
                    edgeBottomImage.p_nByte = engineer.ClassMemoryTool().GetMemory(memoryEdgePool, memoryEdgeGroup, memoryEdgeBottom).p_nByte;
                    edgeBottomImage.p_nPlane = engineer.ClassMemoryTool().GetMemory(memoryEdgePool, memoryEdgeGroup, memoryEdgeBottom).p_nCount;
                }
                if (ebrImage.m_MemData != null)
                {
                    ebrImage.p_nByte = engineer.ClassMemoryTool().GetMemory(memoryEdgePool, memoryEdgeGroup, memoryEdgeEBR).p_nByte;
                    ebrImage.p_nPlane = engineer.ClassMemoryTool().GetMemory(memoryEdgePool, memoryEdgeGroup, memoryEdgeEBR).p_nCount;
                }


                // Recipe
                RecipeFront recipeFront = GlobalObjects.Instance.Register<RecipeFront>();
                RecipeBack recipeBack = GlobalObjects.Instance.Register<RecipeBack>();
                RecipeEdge recipeEdge = GlobalObjects.Instance.Register<RecipeEdge>();
                RecipeEBR recipeEBR = GlobalObjects.Instance.Register<RecipeEBR>();

                //Align
                RecipeAlign recipeAlign = GlobalObjects.Instance.Register<RecipeAlign>();

                
                if (frontImage.GetPtr() != IntPtr.Zero)
                {
                    RootTools_Vision.WorkManager3.WorkManager frontInspection = GlobalObjects.Instance.RegisterNamed<RootTools_Vision.WorkManager3.WorkManager>("frontInspection", 4, true);

                    frontInspection.SetRecipe(recipeFront);
                    frontInspection.SetSharedBuffer(new SharedBufferInfo(
                            frontImage.GetPtr(0), 
                            frontImage.p_Size.X, 
                            frontImage.p_Size.Y, 
                            frontImage.GetBytePerPixel(), 
                            frontImage.GetPtr(1), 
                            frontImage.GetPtr(2), 
                                new MemoryID(memoryFrontPool, memoryFrontGroup, memoryFront)));


                    CameraInfo camInfo = DataConverter.GrabModeToCameraInfo(engineer.m_handler.p_Vision.GetGrabMode(recipeFront.CameraInfoIndex));
                    frontInspection.SetCameraInfo(camInfo);
                }

                //if (frontImage.GetPtr() == IntPtr.Zero)
                //{
                //    //MessageBox.Show("Front Inspection 생성 실패, 메모리 할당 없음");
                //}
                //else
                //{
                //    // Inspection Manager
                //    InspectionManagerFrontside inspectionFront = GlobalObjects.Instance.Register<InspectionManagerFrontside>
                //        (
                //        ((WIND2_Handler)engineer.ClassHandler()).p_Vision,
                //        recipeFront,
                //        new SharedBufferInfo(
                //            frontImage.GetPtr(0), 
                //            frontImage.p_Size.X, 
                //            frontImage.p_Size.Y, 
                //            frontImage.GetBytePerPixel(), 
                //            frontImage.GetPtr(1), 
                //            frontImage.GetPtr(2), 
                //            new MemoryID(memoryFrontPool, memoryFrontGroup, memoryFront)
                //        ));

                //    inspectionFront.SetRecipe(recipeFront);
                //}

                if (backImage.GetPtr() == IntPtr.Zero)
                {
                    //MessageBox.Show("Back Inspection 생성 실패, 메모리 할당 없음");
                }
                else
                {
                    RootTools_Vision.WorkManager3.WorkManager backInspection = GlobalObjects.Instance.RegisterNamed<RootTools_Vision.WorkManager3.WorkManager>("backInspection", 4, true);

                    backInspection.SetRecipe(recipeBack);
                    backInspection.SetSharedBuffer(new SharedBufferInfo(
                            backImage.GetPtr(0),
                            backImage.p_Size.X,
                            backImage.p_Size.Y,
                            backImage.GetBytePerPixel(),
                            backImage.GetPtr(1),
                            backImage.GetPtr(2),
                                new MemoryID(memoryFrontPool, memoryFrontGroup, memoryFront)));


                    //InspectionManagerBackside inspectionBack = GlobalObjects.Instance.Register<InspectionManagerBackside>
                    //(
                    //recipeBack,
                    //new SharedBufferInfo(backImage.GetPtr(0), backImage.p_Size.X, backImage.p_Size.Y, backImage.GetBytePerPixel(), backImage.GetPtr(1), backImage.GetPtr(2))
                    //);
                }

                if (edgeTopImage.GetPtr() != IntPtr.Zero)
                {
                    //RootTools_Vision.WorkManager3.WorkManager edgeTopInspection = GlobalObjects.Instance.RegisterNamed<RootTools_Vision.WorkManager3.WorkManager>("edgeTopInspection", 5);

                    //edgeTopInspection.SetRecipe(recipeEdge);
                    //edgeTopInspection.SetSharedBuffer(new SharedBufferInfo(
                    //            edgeTopImage.GetPtr(0),
                    //            edgeTopImage.p_Size.X,
                    //            edgeTopImage.p_Size.Y,
                    //            edgeTopImage.GetBytePerPixel(),
                    //            edgeTopImage.GetPtr(1),
                    //            edgeTopImage.GetPtr(2),
                    //            new MemoryID(memoryEdgePool, memoryEdgeGroup, memoryEdgeTop)));

                    // Ex) 다중 이미지 버퍼 할당
                    RootTools_Vision.WorkManager3.WorkManager edgeInspection = GlobalObjects.Instance.RegisterNamed<RootTools_Vision.WorkManager3.WorkManager>("edgeInspection");

                    edgeInspection.SetRecipe(recipeEdge);
                    edgeInspection.SetSharedBuffer(new SharedBufferInfo(
                                edgeTopImage.p_Size.X,
                                edgeTopImage.p_Size.Y,
                                edgeTopImage.GetBytePerPixel(),
                                new List<IntPtr>()
                                {
                                    edgeTopImage.GetPtr(0),
                                    edgeTopImage.GetPtr(1),
                                    edgeTopImage.GetPtr(2),
                                    edgeBottomImage.GetPtr(0),
                                    edgeBottomImage.GetPtr(1),
                                    edgeBottomImage.GetPtr(2),
                                    edgeSideImage.GetPtr(0),
                                    edgeSideImage.GetPtr(1),
                                    edgeSideImage.GetPtr(2),
                                }
                                ));

                    CameraInfo camInfo = DataConverter.GrabModeToCameraInfo(engineer.m_handler.p_EdgeSideVision.GetGrabMode(recipeEdge.CameraInfoIndex));
                    edgeInspection.SetCameraInfo(camInfo);
                }

                /*
                if (edgeTopImage.GetPtr() == IntPtr.Zero || edgeSideImage.GetPtr() == IntPtr.Zero || edgeBottomImage.GetPtr() == IntPtr.Zero)
                {
                    //MessageBox.Show("Edge Inspection 생성 실패, 메모리 할당 없음");
                }
                else
                {
                    InspectionManagerEdge inspectionEdge = GlobalObjects.Instance.Register<InspectionManagerEdge>
                    (
                    recipeEdge,
                    new SharedBufferInfo[]{
                        // [0] Top
                        new SharedBufferInfo(edgeTopImage.GetPtr(0), edgeTopImage.p_Size.X, edgeTopImage.p_Size.Y, edgeTopImage.GetBytePerPixel(), edgeTopImage.GetPtr(1), edgeTopImage.GetPtr(2)),
                        // [1] Side
                        new SharedBufferInfo(edgeSideImage.GetPtr(0), edgeSideImage.p_Size.X, edgeSideImage.p_Size.Y, edgeSideImage.GetBytePerPixel(), edgeSideImage.GetPtr(1), edgeSideImage.GetPtr(2)),
                        // [2]
                        new SharedBufferInfo(edgeBottomImage.GetPtr(0), edgeBottomImage.p_Size.X, edgeBottomImage.p_Size.Y, edgeBottomImage.GetBytePerPixel(), edgeBottomImage.GetPtr(1), edgeBottomImage.GetPtr(2)),
                    });
                }
                */

                if (ebrImage.GetPtr() != IntPtr.Zero)
                {
                    RootTools_Vision.WorkManager3.WorkManager ebrInspection = GlobalObjects.Instance.RegisterNamed<RootTools_Vision.WorkManager3.WorkManager>("ebrInspection");

                    ebrInspection.SetRecipe(recipeEBR);
                    ebrInspection.SetSharedBuffer(new SharedBufferInfo(
                            ebrImage.GetPtr(0),
                            ebrImage.p_Size.X,
                            ebrImage.p_Size.Y,
                            ebrImage.GetBytePerPixel()));

                    CameraInfo camInfo = DataConverter.GrabModeToCameraInfo(engineer.m_handler.p_EdgeSideVision.GetGrabMode(recipeEBR.CameraInfoIndex));
                    ebrInspection.SetCameraInfo(camInfo);
                }

                /*
				if (ebrImage.GetPtr() == IntPtr.Zero)
				{
					//MessageBox.Show("EBR Inspection 생성 실패, 메모리 할당 없음");
				}
				else
				{
					InspectionManagerEBR inspectionEBR = GlobalObjects.Instance.Register<InspectionManagerEBR>
					(
					recipeEBR,
					new SharedBufferInfo(ebrImage.GetPtr(0), ebrImage.p_Size.X, ebrImage.p_Size.Y, ebrImage.GetBytePerPixel(), ebrImage.GetPtr(1), ebrImage.GetPtr(2))
					);
				}
                */


				// DialogService

				dialogService.Register<Dialog_ImageOpenViewModel, Dialog_ImageOpen>();
                dialogService.Register<Dialog_Scan_ViewModel, Dialog_Scan>();
                dialogService.Register<SettingDialog_ViewModel, SettingDialog>();
                dialogService.Register<KlarfSettingDialog_ViewModel, KlarfSettingDialog>();
                dialogService.Register<TK4S, TK4SModuleUI>();
                dialogService.Register<FFUModule, FFUModuleUI>();
                dialogService.Register<ManualAlignViewer_ViewModel, ManualAlignViewer>();
                //dialogService.Register<Dialog_PortSelect_ViewModel, Dialog_PortSelect>();
                dialogService.Register<Dialog_MapCreator_ViewModel, Dialog_MapCreator>();



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
            Type t = typeof(Constants.RootPath);
            FieldInfo[] fields = t.GetFields(BindingFlags.Static | BindingFlags.Public);
            foreach (FieldInfo field in fields)
                Directory.CreateDirectory(field.GetValue(null).ToString());
        }
    }
}