using Root_WindII_Option.Engineer;
using RootTools;
using RootTools.Gem.XGem;
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

namespace Root_WindII_Option
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
                GlobalObjects.Instance.Get<WindII_Option_Engineer>().ClassGem().p_eReqControl = XGem.eControl.ONLINEREMOTE;
            });
        }

        public ICommand GemLocalClickCommand
        {
            get => new RelayCommand(() =>
            {
                GlobalObjects.Instance.Get<WindII_Option_Engineer>().ClassGem().p_eReqControl = XGem.eControl.LOCAL;
            });
        }

        public ICommand GemOfflineClickCommand
        {
            get => new RelayCommand(() =>
            {
                GlobalObjects.Instance.Get<WindII_Option_Engineer>().ClassGem().p_eReqControl = XGem.eControl.OFFLINE;
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

            //warnui = new Warning_UI();
            //p_CurrentSubPanel = warnui;
            //p_CurrentSubPanel.DataContext = GlobalObjects.Instance.Get<WindII_Warning>();
        }

        private void ThreadStop()
        {
            GlobalObjects.Instance.Get<WindII_Option_Engineer>().ThreadStop();
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

        private string memoryBackPool = "BackVision.Memory";
        private string memoryBackGroup = "BackVision";
        private string memoryBack = "BackSide";

        private string memoryEdgePool = "EdgeVision.Memory";
        private string memoryEdgeGroup = "EdgeVision";
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
                WindII_Option_Engineer engineer = GlobalObjects.Instance.Register<WindII_Option_Engineer>();
				//DialogService dialogService = GlobalObjects.Instance.Register<DialogService>(this);
				//WIND2_Warning warning = GlobalObjects.Instance.Register<WIND2_Warning>();
				engineer.Init("WIND2F_Option");

				#region Register Memory
				MemoryTool memoryTool = engineer.ClassMemoryTool();
				ImageData backImage = GlobalObjects.Instance.RegisterNamed<ImageData>("BackImage", memoryTool.GetMemory(memoryBackPool, memoryBackGroup, memoryBack));
				ImageData edgeTopImage = GlobalObjects.Instance.RegisterNamed<ImageData>("EdgeTopImage", memoryTool.GetMemory(memoryEdgePool, memoryEdgeGroup, memoryEdgeTop));
				ImageData edgeSideImage = GlobalObjects.Instance.RegisterNamed<ImageData>("EdgeSideImage", memoryTool.GetMemory(memoryEdgePool, memoryEdgeGroup, memoryEdgeSide));
				ImageData edgeBottomImage = GlobalObjects.Instance.RegisterNamed<ImageData>("EdgeBottomImage", memoryTool.GetMemory(memoryEdgePool, memoryEdgeGroup, memoryEdgeBottom));
				ImageData ebrImage = GlobalObjects.Instance.RegisterNamed<ImageData>("EBRImage", memoryTool.GetMemory(memoryEdgePool, memoryEdgeGroup, memoryEdgeEBR));

                if (backImage.m_MemData != null)
                {
                    backImage.p_nByte = engineer.ClassMemoryTool().GetMemory(memoryBackPool, memoryBackGroup, memoryBack).p_nByte;
                    backImage.p_nPlane = engineer.ClassMemoryTool().GetMemory(memoryBackPool, memoryBackGroup, memoryBack).p_nCount;
                }

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
				#endregion

				#region Register Recipe / Inspection
				RecipeBack recipeBack = GlobalObjects.Instance.Register<RecipeBack>();
                RecipeEdge recipeEdge = GlobalObjects.Instance.Register<RecipeEdge>();
				RecipeEBR recipeEBR = GlobalObjects.Instance.Register<RecipeEBR>();

                if (backImage.GetPtr() != IntPtr.Zero)
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
                            new MemoryID(memoryBackPool, memoryBackGroup, memoryBack)));
				}

                if (edgeTopImage.GetPtr() != IntPtr.Zero)
                {
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

                    CameraInfo camInfo = DataConverter.GrabModeToCameraInfo(engineer.m_handler.p_VisionEdge.GetGrabMode(recipeEdge.CameraInfoIndex));
                    edgeInspection.SetCameraInfo(camInfo);
                }

                if (ebrImage.GetPtr() != IntPtr.Zero)
                {
                    RootTools_Vision.WorkManager3.WorkManager ebrInspection = GlobalObjects.Instance.RegisterNamed<RootTools_Vision.WorkManager3.WorkManager>("ebrInspection");

                    ebrInspection.SetRecipe(recipeEBR);
                    ebrInspection.SetSharedBuffer(new SharedBufferInfo(
                            ebrImage.GetPtr(0),
                            ebrImage.p_Size.X,
                            ebrImage.p_Size.Y,
                            ebrImage.GetBytePerPixel()));

                    CameraInfo camInfo = DataConverter.GrabModeToCameraInfo(engineer.m_handler.p_VisionEdge.GetGrabMode(recipeEBR.CameraInfoIndex));
                    ebrInspection.SetCameraInfo(camInfo);
                }
				#endregion

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
    }
}
