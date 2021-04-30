using Root_WindII.Engineer;
using RootTools;
using RootTools.Memory;
using RootTools.ToolBoxs;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Root_WindII
{
    public class MainWindow_ViewModel : ObservableObject
    {
        #region [Event]

        #endregion


        #region [ViewModel]

        private int selectedModeIndex = 0;
        public int SelectedModeIndex
        {
            get => this.selectedModeIndex;
            set
            {
                SetProperty(ref this.selectedModeIndex, value);
            }
        }

        private SelectMode_ViewModel selectModeVM;
        public SelectMode_ViewModel SelectModeVM
        {
            get => this.selectModeVM;
            set
            {
                SetProperty(ref this.selectModeVM, value);
            }
        }

        private Setup_ViewModel setupVM;
        public Setup_ViewModel SetupVM
        {
            get => this.setupVM;
            set
            {
                SetProperty(ref this.setupVM, value);
            }
        }

        private MaintenancePanel_ViewModel maintVM;
        public MaintenancePanel_ViewModel MaintVM
		{
            get => this.maintVM;
            set => SetProperty(ref this.maintVM, value);
        }

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
        #endregion

        public MainWindow_ViewModel()
        {
            this.SelectModeVM = new SelectMode_ViewModel();
            this.selectModeVM.SetupSelected += SetupSelected_Callback;
            this.selectModeVM.ReviewSelected += ReviewSelected_Callback;
            this.selectModeVM.OperationSelected += OperationSelected_Callback;
            this.selectModeVM.EngineerSelected += EngineerSelected_Callback;

            Init();

            this.SetupVM = new Setup_ViewModel();
			this.MaintVM = new MaintenancePanel_ViewModel();
		}

        private void ThreadStop()
        {
            GlobalObjects.Instance.Get<WindII_Engineer>().ThreadStop();
        }

        private void Init()
        {
            CreateGlobalPaths();

            if (RegisterGlobalObjects() == false)
            {
                MessageBox.Show("Program Initialization fail");
                return;
            }

            //if (UIManager.Instance.Initialize() == false)
            //{
            //    MessageBox.Show("UI Initialization fail");
            //    return;
            //}

            //// WPF 파라매터 연결
            //UIManager.Instance.MainPanel = this.MainPanel;
            //UIManager.Instance.ChangeUIMode();

            //SettingItem_Database frontSettings = GlobalObjects.Instance.Get<Settings>().GetItem<SettingItem_Database>();
            //logView.Init(LogView._logView);
            //WarningUI.Init(GlobalObjects.Instance.Get<WIND2_Warning>());
            //InitTimer();
        }

        private void CreateGlobalPaths()
        {
            Type t = typeof(Constants.RootPath);
            FieldInfo[] fields = t.GetFields(BindingFlags.Static | BindingFlags.Public);
            foreach (FieldInfo field in fields)
                Directory.CreateDirectory(field.GetValue(null).ToString());
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

        private bool RegisterGlobalObjects()
        {
            try
            {
                // Settings
                Settings settings = GlobalObjects.Instance.Register<Settings>();

                // Engineer
                WindII_Engineer engineer = GlobalObjects.Instance.Register<WindII_Engineer>();
                //DialogService dialogService = GlobalObjects.Instance.Register<DialogService>(this);
                //WIND2_Warning warning = GlobalObjects.Instance.Register<WIND2_Warning>();
                engineer.Init("WIND2F");

                MemoryTool memoryTool = engineer.ClassMemoryTool();
                ImageData frontImage;
                ImageData maskLayer;

                
                // ImageData
                //if (engineer.m_eMode == WIND2_Engineer.eMode.EFEM)
                {
                    MemoryData memoryData = memoryTool.GetMemory(memoryFrontPool, memoryFrontGroup, memoryFront);
                    frontImage = GlobalObjects.Instance.RegisterNamed<ImageData>("FrontImage", memoryFrontPool, memoryFrontGroup, memoryFront, memoryTool, memoryData.p_nCount, memoryData.p_nByte);
                    maskLayer = GlobalObjects.Instance.RegisterNamed<ImageData>("MaskImage", memoryTool.GetMemory(memoryFrontPool, memoryFrontGroup, memoryMask));
                }
                //else
                //{
                //    frontImage = GlobalObjects.Instance.RegisterNamed<ImageData>("FrontImage", memoryTool.GetMemory(memoryFrontPool, memoryFrontGroup, memoryFront));
                //    maskLayer = GlobalObjects.Instance.RegisterNamed<ImageData>("MaskImage", memoryTool.GetMemory(memoryFrontPool, memoryFrontGroup, memoryMask));
                //}

                //ImageData backImage = GlobalObjects.Instance.RegisterNamed<ImageData>("BackImage", memoryTool.GetMemory(memoryBackPool, memoryBackGroup, memoryBack));
                //ImageData edgeTopImage = GlobalObjects.Instance.RegisterNamed<ImageData>("EdgeTopImage", memoryTool.GetMemory(memoryEdgePool, memoryEdgeGroup, memoryEdgeTop));
                //ImageData edgeSideImage = GlobalObjects.Instance.RegisterNamed<ImageData>("EdgeSideImage", memoryTool.GetMemory(memoryEdgePool, memoryEdgeGroup, memoryEdgeSide));
                //ImageData edgeBottomImage = GlobalObjects.Instance.RegisterNamed<ImageData>("EdgeBottomImage", memoryTool.GetMemory(memoryEdgePool, memoryEdgeGroup, memoryEdgeBottom));
                //ImageData ebrImage = GlobalObjects.Instance.RegisterNamed<ImageData>("EBRImage", memoryTool.GetMemory(memoryEdgePool, memoryEdgeGroup, memoryEdgeEBR));

                if (frontImage.m_MemData != null)
                {
                    frontImage.p_nByte = engineer.ClassMemoryTool().GetMemory(memoryFrontPool, memoryFrontGroup, memoryFront).p_nByte;
                    frontImage.p_nPlane = engineer.ClassMemoryTool().GetMemory(memoryFrontPool, memoryFrontGroup, memoryFront).p_nCount;
                }

                //if (edgeTopImage.m_MemData != null)
                //{
                //    edgeTopImage.p_nByte = engineer.ClassMemoryTool().GetMemory(memoryEdgePool, memoryEdgeGroup, memoryEdgeTop).p_nByte;
                //    edgeTopImage.p_nPlane = engineer.ClassMemoryTool().GetMemory(memoryEdgePool, memoryEdgeGroup, memoryEdgeTop).p_nCount;
                //}

                //if (edgeSideImage.m_MemData != null)
                //{
                //    edgeSideImage.p_nByte = engineer.ClassMemoryTool().GetMemory(memoryEdgePool, memoryEdgeGroup, memoryEdgeSide).p_nByte;
                //    edgeSideImage.p_nPlane = engineer.ClassMemoryTool().GetMemory(memoryEdgePool, memoryEdgeGroup, memoryEdgeSide).p_nCount;
                //}

                //if (edgeBottomImage.m_MemData != null)
                //{
                //    edgeBottomImage.p_nByte = engineer.ClassMemoryTool().GetMemory(memoryEdgePool, memoryEdgeGroup, memoryEdgeBottom).p_nByte;
                //    edgeBottomImage.p_nPlane = engineer.ClassMemoryTool().GetMemory(memoryEdgePool, memoryEdgeGroup, memoryEdgeBottom).p_nCount;
                //}

                //if (ebrImage.m_MemData != null)
                //{
                //    ebrImage.p_nByte = engineer.ClassMemoryTool().GetMemory(memoryEdgePool, memoryEdgeGroup, memoryEdgeEBR).p_nByte;
                //    ebrImage.p_nPlane = engineer.ClassMemoryTool().GetMemory(memoryEdgePool, memoryEdgeGroup, memoryEdgeEBR).p_nCount;
                //}
                
                /*
                // Recipe
                RecipeFront recipeFront = GlobalObjects.Instance.Register<RecipeFront>();
                RecipeBack recipeBack = GlobalObjects.Instance.Register<RecipeBack>();
                RecipeEdge recipeEdge = GlobalObjects.Instance.Register<RecipeEdge>();
                RecipeEBR recipeEBR = GlobalObjects.Instance.Register<RecipeEBR>();

                if (frontImage.GetPtr() != IntPtr.Zero)
                {
                    RootTools_Vision.WorkManager3.WorkManager frontInspection = GlobalObjects.Instance.RegisterNamed<RootTools_Vision.WorkManager3.WorkManager>("frontInspection", 4);

                    frontInspection.SetRecipe(recipeFront);
                    frontInspection.SetSharedBuffer(new SharedBufferInfo(
                            frontImage.GetPtr(0),
                            frontImage.p_Size.X,
                            frontImage.p_Size.Y,
                            frontImage.GetBytePerPixel(),
                            frontImage.GetPtr(1),
                            frontImage.GetPtr(2),
                                new MemoryID(memoryFrontPool, memoryFrontGroup, memoryFront)));
                }

                if (backImage.GetPtr() == IntPtr.Zero)
                {
                    //MessageBox.Show("Back Inspection 생성 실패, 메모리 할당 없음");
                }
                else
                {
                    RootTools_Vision.WorkManager3.WorkManager backInspection = GlobalObjects.Instance.RegisterNamed<RootTools_Vision.WorkManager3.WorkManager>("backInspection", 4);

                    backInspection.SetRecipe(recipeBack);
                    backInspection.SetSharedBuffer(new SharedBufferInfo(
                            backImage.GetPtr(0),
                            backImage.p_Size.X,
                            backImage.p_Size.Y,
                            backImage.GetBytePerPixel(),
                            backImage.GetPtr(1),
                            backImage.GetPtr(2),
                            new MemoryID(memoryFrontPool, memoryFrontGroup, memoryFront)));
                }

                if (edgeTopImage.GetPtr() != IntPtr.Zero)
                {
                    RootTools_Vision.WorkManager3.WorkManager edgeTopInspection = GlobalObjects.Instance.RegisterNamed<RootTools_Vision.WorkManager3.WorkManager>("edgeTopInspection", 5);

                    edgeTopInspection.SetRecipe(recipeEdge);
                    edgeTopInspection.SetSharedBuffer(new SharedBufferInfo(
                                edgeTopImage.GetPtr(0),
                                edgeTopImage.p_Size.X,
                                edgeTopImage.p_Size.Y,
                                edgeTopImage.GetBytePerPixel(),
                                edgeTopImage.GetPtr(1),
                                edgeTopImage.GetPtr(2),
                                new MemoryID(memoryFrontPool, memoryFrontGroup, memoryFront)));
                }

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
                //dialogService.Register<Dialog_ImageOpenViewModel, Dialog_ImageOpen>();
                //dialogService.Register<Dialog_Scan_ViewModel, Dialog_Scan>();
                //dialogService.Register<SettingDialog_ViewModel, SettingDialog>();
                //dialogService.Register<TK4S, TK4SModuleUI>();
                //dialogService.Register<FFUModule, FFUModuleUI>();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
            return true;
        }

        private void SetupSelected_Callback()
        {
            this.SelectedModeIndex = 1;
        }

        private void ReviewSelected_Callback()
        {
            this.SelectedModeIndex = 2;
        }

        private void OperationSelected_Callback()
        {
            this.SelectedModeIndex = 3;
        }

        private void EngineerSelected_Callback()
        {
			this.SelectedModeIndex = 4;

            p_CurrentPanel = maintVM.Main;
            p_CurrentPanel.DataContext = maintVM;
            this.MaintVM.SetPage(this.MaintVM.EngineerUI);
        }
    }
}
