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
using System.Windows.Input;
using RootTools.Database;
using RootTools.Gem.XGem;

namespace Root_WindII
{
    public class MainWindow_ViewModel : ObservableObject
    {
        #region [ViewModel]
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

        public Operation_UI OperationUI;
        private OperationUI_ViewModel operationVM = new OperationUI_ViewModel();
        public OperationUI_ViewModel OperationVM
        {
            get => this.operationVM;
            set => SetProperty(ref this.operationVM, value);
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

        
        Warning_UI warnui;
        private UserControl m_CurrentSubPanel;
        public UserControl p_CurrentSubPanel
        {
            get
            {
                return m_CurrentSubPanel;
            }
            set
            {
                SetProperty(ref m_CurrentSubPanel, value);
            }
        }
        #endregion

        #region [MenuBar]

        #region [Properties]

        private bool isCheckModeMain = true;
        public bool IsCheckModeMain
        {
            get => this.isCheckModeMain;
            set
            {
                if (value == true)
                {
                    SetProperty(ref this.isCheckModeSetup, false, "IsCheckModeSetup");
                    SetProperty(ref this.isCheckModeReview, false, "IsCheckModeReview");
                    SetProperty(ref this.isCheckModeOperation, false, "IsCheckModeOperation");
                    SetProperty(ref this.isCheckModeEngineer, false, "IsCheckModeEngineer");
                }
                else if (value == false && this.isCheckModeMain == true)
                {
                    return;
                }

                SetProperty(ref this.isCheckModeMain, value);
            }
        }

        private bool isCheckModeSetup = false;
        public bool IsCheckModeSetup
        {
            get => this.isCheckModeSetup;
            set
            {
                if (value == true)
                {
                    SetProperty(ref this.isCheckModeMain, false, "IsCheckModeMain");
                    SetProperty(ref this.isCheckModeReview, false, "IsCheckModeReview");
                    SetProperty(ref this.isCheckModeOperation, false, "IsCheckModeOperation");
                    SetProperty(ref this.isCheckModeEngineer, false, "IsCheckModeEngineer");
                }
                else if (value == false && this.isCheckModeSetup == true)
                {
                    return;
                }

                SetProperty(ref this.isCheckModeSetup, value);
            }
        }

        private bool isCheckModeReview = false;
        public bool IsCheckModeReview
        {
            get => this.isCheckModeReview;
            set
            {
                if (value == true)
                {
                    SetProperty(ref this.isCheckModeMain, false, "IsCheckModeMain");
                    SetProperty(ref this.isCheckModeSetup, false, "IsCheckModeSetup");
                    SetProperty(ref this.isCheckModeOperation, false, "IsCheckModeOperation");
                    SetProperty(ref this.isCheckModeEngineer, false, "IsCheckModeEngineer");
                }
                else if (value == false && this.isCheckModeReview == true)
                {
                    return;
                }
                SetProperty(ref this.isCheckModeReview, value);
            }
        }

        private bool isCheckModeOperation = false;
        public bool IsCheckModeOperation
        {
            get => this.isCheckModeOperation;
            set
            {
                if (value == true)
                {
                    SetProperty(ref this.isCheckModeMain, false, "IsCheckModeMain");
                    SetProperty(ref this.isCheckModeSetup, false, "IsCheckModeSetup");
                    SetProperty(ref this.isCheckModeReview, false, "IsCheckModeReview");
                    SetProperty(ref this.isCheckModeEngineer, false, "IsCheckModeEngineer");

                    p_CurrentPanel = OperationUI;
                    p_CurrentPanel.DataContext = OperationVM;
                }
                else if (value == false && this.isCheckModeOperation == true)
                {
                    return;
                }
                SetProperty(ref this.isCheckModeOperation, value);
            }
        }

        private bool isCheckModeEngineer = false;
        public bool IsCheckModeEngineer
        {
            get => this.isCheckModeEngineer;
            set
            {
                if (value == true)
                {
                    SetProperty(ref this.isCheckModeMain, false, "IsCheckModeMain");
                    SetProperty(ref this.isCheckModeSetup, false, "IsCheckModeSetup");
                    SetProperty(ref this.isCheckModeReview, false, "IsCheckModeReview");
                    SetProperty(ref this.isCheckModeOperation, false, "IsCheckModeOperation");

                    p_CurrentPanel = maintVM.Main;
                    p_CurrentPanel.DataContext = maintVM;

                    this.MaintVM.SetPage(this.MaintVM.EngineerUI);
                }
                else if (value == false && this.isCheckModeEngineer == true)
                {
                    return;
                }
                SetProperty(ref this.isCheckModeEngineer, value);
            }
        }
                
        #endregion
        #region [Command]
        public ICommand GemOnlineClickCommand
        {
            get => new RelayCommand(() =>
            {
                GlobalObjects.Instance.Get<WindII_Engineer>().ClassGem().p_eReqControl = XGem.eControl.ONLINEREMOTE;
            });
        }

        public ICommand GemLocalClickCommand
        {
            get => new RelayCommand(() =>
            {
                GlobalObjects.Instance.Get<WindII_Engineer>().ClassGem().p_eReqControl = XGem.eControl.LOCAL;
            });
        }

        public ICommand GemOfflineClickCommand
        {
            get => new RelayCommand(() =>
            {
                GlobalObjects.Instance.Get<WindII_Engineer>().ClassGem().p_eReqControl = XGem.eControl.OFFLINE;
            });
        }


        public ICommand btnPopUpSetting
        {
            get => new RelayCommand(() =>
            {
                var viewModel = new SettingDialog_ViewModel(GlobalObjects.Instance.Get<Settings>());
                Nullable<bool> result = GlobalObjects.Instance.Get<DialogService>().ShowDialog(viewModel);
                if (result.HasValue)
                {
                    if (result.Value)
                    {

                    }
                    else
                    {

                    }
                }
            });
        }
        #endregion
        #endregion

        #region [Mode Selection Panel]
        #region [Command]
        public ICommand btnSetupCommand
        {
            get => new RelayCommand(() =>
            {
                this.IsCheckModeSetup = true;
            });
        }

        public ICommand btnReviewCommand
        {
            get => new RelayCommand(() =>
            {
                this.IsCheckModeReview = true;
            });
        }

        public ICommand btnOperationCommand
        {
            get => new RelayCommand(() =>
            {
                this.IsCheckModeOperation= true;

            });
        }

        public ICommand btnEngineerCommand
        {
            get => new RelayCommand(() =>
            {
                this.IsCheckModeEngineer = true;
            });
        }
        #endregion
        #endregion

        public MainWindow_ViewModel()
        {
            Init();

            this.SetupVM = new Setup_ViewModel();
			this.MaintVM = new MaintenancePanel_ViewModel();

            warnui = new Warning_UI();
            p_CurrentSubPanel = warnui;
            p_CurrentSubPanel.DataContext = GlobalObjects.Instance.Get<WindII_Warning>();
        }

        private void ThreadStop()
        {
            GlobalObjects.Instance.Get<WindII_Engineer>().ThreadStop();
        }

        public IDialogService dialogService;

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

            SettingItem_Database frontSettings = GlobalObjects.Instance.Get<Settings>().GetItem<SettingItem_Database>();
            DatabaseManager.Instance.SetDatabase(1, frontSettings.SerevrName, frontSettings.DBName, frontSettings.DBUserID, frontSettings.DBPassword);
            DatabaseManager.Instance.ValidateDatabase();
            OperationUI = new Operation_UI();
            OperationVM.Init();

            //logView.Init(LogView._logView);
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

        //private string memoryRACPool = "RAC.Memory";
        //private string memoryRACGroup = "RAC";
        //private string memoryRAC = "RACMain";

        private bool RegisterGlobalObjects()
        {
            try
            {
                // Settings
                Settings settings = GlobalObjects.Instance.Register<Settings>();

                // Engineer
                WindII_Engineer engineer = GlobalObjects.Instance.Register<WindII_Engineer>();
                DialogService dialogService = GlobalObjects.Instance.Get<DialogService>();
                WindII_Warning warning = GlobalObjects.Instance.Register<WindII_Warning>();
                engineer.Init("WIND2F");

                MemoryTool memoryTool = engineer.ClassMemoryTool();
                ImageData frontImage;
                ImageData maskLayer;
                //ImageData racImage;
                
                // ImageData
                frontImage = GlobalObjects.Instance.RegisterNamed<ImageData>("FrontImage", memoryTool.GetMemory(memoryFrontPool, memoryFrontGroup, memoryFront));
                maskLayer = GlobalObjects.Instance.RegisterNamed<ImageData>("MaskImage", memoryTool.GetMemory(memoryFrontPool, memoryFrontGroup, memoryMask));
                //racImage = GlobalObjects.Instance.RegisterNamed<ImageData>("RACImage", memoryTool.GetMemory(memoryRACPool, memoryRACGroup, memoryRAC));
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

                //if(racImage.m_MemData != null)
                //{
                //    racImage.p_nByte = engineer.ClassMemoryTool().GetMemory(memoryRACPool, memoryRACGroup, memoryRAC).p_nByte;
                //    racImage.p_nPlane = engineer.ClassMemoryTool().GetMemory(memoryRACPool, memoryRACGroup, memoryRAC).p_nCount;
                //}

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
                
                // Recipe
                RecipeFront recipeFront = GlobalObjects.Instance.Register<RecipeFront>();

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

                    CameraInfo camInfo = DataConverter.GrabModeToCameraInfo(engineer.m_handler.p_VisionFront.GetGrabMode(recipeFront.CameraInfoIndex));
                    frontInspection.SetCameraInfo(camInfo);
                }

                Root_EFEM.Module.RecipeAlign recipeAlign = GlobalObjects.Instance.Register<Root_EFEM.Module.RecipeAlign>();

                /*if (backImage.GetPtr() == IntPtr.Zero)
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
                dialogService.Register<Dialog_ImageOpenViewModel, Dialog_ImageOpen>();
                dialogService.Register<Dialog_Scan_ViewModel, Dialog_Scan>();
                dialogService.Register<ManualAlignViewer_ViewModel, ManualAlignViewer>();
                dialogService.Register<SettingDialog_ViewModel, SettingDialog>();
                dialogService.Register<TK4S, TK4SModuleUI>();
                dialogService.Register<FFUModule, FFUModuleUI>();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
            return true;
        }
    }
}
