using Root_VEGA_P_Vision.Engineer;
using Root_VEGA_P_Vision.Module;
using RootTools;
using RootTools.Database;
using RootTools.Module;
using RootTools_Vision;
using RootTools_Vision.WorkManager3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Root_VEGA_P_Vision
{
    class RecipeStain_ViewModel: ObservableObject
    {

        #region [ColorDefines]
        private class ImageViewerColorDefines
        {
            public static SolidColorBrush MasterPosition = Brushes.Magenta;
            public static SolidColorBrush MasterPostionMove = Brushes.Yellow;
            public static SolidColorBrush ChipPosition = Brushes.Blue;
            public static SolidColorBrush ChipPositionMove = Brushes.Yellow;
            public static SolidColorBrush PostionFail = Brushes.Red;
            public static SolidColorBrush Defect = Brushes.Red;
        }

        private class MapViewerColorDefines
        {
            public static SolidColorBrush NoChip = Brushes.LightGray;
            public static SolidColorBrush Normal = Brushes.DimGray;
            public static SolidColorBrush Snap = Brushes.LightSkyBlue;
            public static SolidColorBrush Position = Brushes.LightYellow;
            public static SolidColorBrush Inspection = Brushes.Gold;
            public static SolidColorBrush ProcessDefect = Brushes.YellowGreen;
            public static SolidColorBrush ProcessDefectWafer = Brushes.Green;

            public static SolidColorBrush GetWorkplaceStateColor(WORK_TYPE state)
            {
                switch (state)
                {
                    case WORK_TYPE.NONE:
                        return Normal;
                    case WORK_TYPE.SNAP:
                        return Snap;
                    case WORK_TYPE.ALIGNMENT:
                        return Position;
                    case WORK_TYPE.INSPECTION:
                        return Inspection;
                    case WORK_TYPE.DEFECTPROCESS:
                        return ProcessDefect;
                    case WORK_TYPE.DEFECTPROCESS_ALL:
                        return ProcessDefectWafer;
                    default:
                        return Normal;
                }
            }
        }
        #endregion

        public RecipeSetting_ViewModel recipeSetting;
        public RecipeStain_Panel Main;

        #region [ImageViewer ViewModel]
        RootViewer_ViewModel EIPcovertop_ImageViewerVM, EIPcoverbottom_ImageViewerVM;
        RootViewer_ViewModel EIPbasetop_ImageViewerVM, EIPbasebottom_ImageViewerVM;

        public RootViewer_ViewModel EIPCoverTop_ImageViewerVM
        {
            get => EIPcovertop_ImageViewerVM;
            set => SetProperty(ref EIPcovertop_ImageViewerVM,value);
        }
        public RootViewer_ViewModel EIPCoverBottom_ImaageViewerVM
        {
            get => EIPcoverbottom_ImageViewerVM;
            set => SetProperty(ref EIPcoverbottom_ImageViewerVM, value);
        }
        public RootViewer_ViewModel EIPBaseTop_ImageViewerVM
        {
            get => EIPbasetop_ImageViewerVM;
            set => SetProperty(ref EIPbasetop_ImageViewerVM, value);
        }
        public RootViewer_ViewModel EIPBaseBottom_ImageViewerVM
        {
            get => EIPbasebottom_ImageViewerVM;
            set => SetProperty(ref EIPbasebottom_ImageViewerVM, value);
        }
        #endregion

        public RecipeStain_ViewModel(RecipeSetting_ViewModel recipeSetting)
        {
            this.recipeSetting = recipeSetting;
            Main = new RecipeStain_Panel();

            EIPcovertop_ImageViewerVM = new MaskRootViewer_ViewModel("EIP_Cover.Stain.Front",recipeSetting);

            EIPcoverbottom_ImageViewerVM = new MaskRootViewer_ViewModel("EIP_Cover.Stain.Back",recipeSetting);

            EIPbasetop_ImageViewerVM = new MaskRootViewer_ViewModel("EIP_Plate.Stain.Front",recipeSetting);

            EIPbasebottom_ImageViewerVM = new MaskRootViewer_ViewModel("EIP_Plate.Stain.Back",recipeSetting);

            InitInspMgr();
        }

        //InitInspectionWorkManager
        void InitInspMgr()
        {
            //Vision.eParts
            foreach(var v in Enum.GetValues(typeof(Vision.eParts)))
            {
                foreach(var v2 in Enum.GetValues(typeof(Vision.eUpDown)))
                {
                    string insp = v.ToString() + ".Stain." + v2.ToString() + ".Inspection";

                    if (GlobalObjects.Instance.GetNamed<WorkManager>(insp)!=null)
                    {
                        GlobalObjects.Instance.GetNamed<WorkManager>(insp).InspectionStart += InspectionStart_Callback;
                        GlobalObjects.Instance.GetNamed<WorkManager>(insp).PositionDone += PositionDone_Callback;
                        GlobalObjects.Instance.GetNamed<WorkManager>(insp).InspectionDone += InspectionDone_Callback;
                        GlobalObjects.Instance.GetNamed<WorkManager>(insp).ProcessDefectWaferStart += ProcessDefectWaferStart_Callback;
                        GlobalObjects.Instance.GetNamed<WorkManager>(insp).IntegratedProcessDefectDone += ProcessDefectWaferDone_Callback;
                    }
                }
            }
        }

        private void ProcessDefectWaferDone_Callback(object sender, IntegratedProcessDefectDoneEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                DatabaseManager.Instance.SelectData();
                m_DataViewer_VM.pDataTable = DatabaseManager.Instance.pDefectTable;
            }));
        }

        private void ProcessDefectWaferStart_Callback(object sender, ProcessDefectWaferStartEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                ((MaskRootViewer_ViewModel)EIPcovertop_ImageViewerVM).RemoveObjectsByTag("defect");
            }));
        }

        private void InspectionDone_Callback(object sender, InspectionDoneEventArgs args)
        {
            Workplace workplace = args.workplace;
            if (workplace == null || workplace.DefectList == null) return;
            List<string> textList = new List<string>();
            List<CRect> rectList = new List<CRect>();


            foreach (Defect defectInfo in workplace.DefectList)
            {
                string text = "";

                rectList.Add(new CRect((int)defectInfo.p_rtDefectBox.Left, (int)defectInfo.p_rtDefectBox.Top, (int)defectInfo.p_rtDefectBox.Right, (int)defectInfo.p_rtDefectBox.Bottom));
                textList.Add(text);
            }

            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                DrawRectDefect(rectList, textList);
            }));
        }

        private void PositionDone_Callback(object obj, PositionDoneEventArgs args)
        {
            Workplace workplace = obj as Workplace;
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                string test = "";

                test += "Trans : {" + workplace.OffsetX.ToString() + ", " + workplace.OffsetX.ToString() + "}" + "\n";
                DrawRectMasterFeature(args.ptOldStart, args.ptOldEnd, args.ptNewStart, args.ptNewEnd, test, args.bSuccess);

            }));
        }

        private void InspectionStart_Callback(object sender, InspectionStartArgs e)
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                ((MaskRootViewer_ViewModel)EIPcovertop_ImageViewerVM).ClearObjects();
            }));

        }

        public void DrawRectMasterFeature(CPoint ptOldStart, CPoint ptOldEnd, CPoint ptNewStart, CPoint ptNewEnd, String text, bool bSuccess)
        {
            ((MaskRootViewer_ViewModel)EIPcovertop_ImageViewerVM).AddDrawRect(ptOldStart, ptOldEnd, ImageViewerColorDefines.MasterPosition, "position");
            ((MaskRootViewer_ViewModel)EIPcovertop_ImageViewerVM).AddDrawRect(ptNewStart, ptNewEnd, bSuccess ? ImageViewerColorDefines.MasterPostionMove : ImageViewerColorDefines.PostionFail, "position");
            //ImageViewerVM.DrawText(ptNew)
        }


        private string currentRecipe = "";
        private MapViewer_ViewModel mapViewerVM;
        public MapViewer_ViewModel MapViewerVM
        {
            get => mapViewerVM;
            set
            {
                SetProperty(ref mapViewerVM, value);
            }
        }
        private Database_DataView_VM m_DataViewer_VM = new Database_DataView_VM();
        public Database_DataView_VM p_DataViewer_VM
        {
            get { return this.m_DataViewer_VM; }
            set { SetProperty(ref m_DataViewer_VM, value); }
        }
        public void LoadRecipe()
        {
            if(currentRecipe!= GlobalObjects.Instance.Get<RecipeVision>().Name)
                currentRecipe = GlobalObjects.Instance.Get<RecipeVision>().Name;            
        }

        public void DrawRectDefect(List<CRect> rectList, List<String> text)
        {
            ((MaskRootViewer_ViewModel)EIPcovertop_ImageViewerVM).AddDrawRectList(rectList, ImageViewerColorDefines.Defect, "defect");
        }

        #region RelayCommand
        public ICommand btnBack
        {
            get => new RelayCommand(()=> {
                //recipeSetting.SetRecipeWizard();
            });
        }
        public ICommand btnSnap
        {
            get => new RelayCommand(()=> Snap());
        }
        public ICommand btnInsp
        {
            get => new RelayCommand(() => Inspection());
        }

        void Snap()
        {
            EQ.p_bStop = false;
            Vision vision = GlobalObjects.Instance.Get<VEGA_P_Vision_Engineer>().m_handler.m_vision;
            if (vision.p_eState != ModuleBase.eState.Ready)
            {
                MessageBox.Show("Vision Home이 완료 되지 않았습니다.");
                return;
            }

            vision.StartRun((Run_StainGrab)vision.CloneModuleRun(App.mStainGrab));
        }
        void Inspection()
        {
            if (GlobalObjects.Instance.GetNamed<WorkManager>("EIP_Cover.Stain.Front.Inspection") != null)
            {
                EIPCoverTop_ImageViewerVM.p_ImageData.GetPtr(0);
                OriginRecipe originRecipe = GlobalObjects.Instance.Get<RecipeVision>().GetItem<OriginRecipe>();

                originRecipe.OriginWidth = 1500;
                originRecipe.OriginHeight = 1300;
                GlobalObjects.Instance.GetNamed<WorkManager>("EIP_Cover.Stain.Front.Inspection").Start();
            }
        }
        #endregion
    }
}
