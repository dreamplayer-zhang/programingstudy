using Root_WIND2.Module;
using RootTools;
using RootTools.Database;
using RootTools.Module;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using System.Data;
using RootTools_Vision.WorkManager3;
using RootTools_Vision.Utility;
using System.IO;
using System.Windows.Input;

namespace Root_WIND2.UI_User
{
    public class FrontsideInspect_ViewModel : ObservableObject, IPage
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

        #region [Properites]
        private FrontsideInspect_ImageViewer_ViewModel imageViewerVM;
        public FrontsideInspect_ImageViewer_ViewModel ImageViewerVM
        {
            get => this.imageViewerVM;
        }

        private MapViewer_ViewModel mapViewerVM;
        public MapViewer_ViewModel MapViewerVM
        {
            get => this.mapViewerVM;
            set
            {
                SetProperty<MapViewer_ViewModel>(ref this.mapViewerVM, value);
            }
        }

        private Database_DataView_VM m_DataViewer_VM = new Database_DataView_VM();
        public Database_DataView_VM p_DataViewer_VM
        {
            get { return this.m_DataViewer_VM; }
            set { SetProperty(ref m_DataViewer_VM, value); }
        }

        private string inspectionID;
        public string InspectionID
        {
            get => this.inspectionID;
            set
            {
                SetProperty(ref this.inspectionID, value);
            }
        }
        #endregion



        public FrontsideInspect_ViewModel()
        {
            if (GlobalObjects.Instance.GetNamed<ImageData>("FrontImage").GetPtr() == IntPtr.Zero && GlobalObjects.Instance.GetNamed<ImageData>("FrontImage").m_eMode != ImageData.eMode.OtherPCMem)
                return;

            // Initialize ImageViewer
            this.imageViewerVM = new FrontsideInspect_ImageViewer_ViewModel();
            this.imageViewerVM.init(GlobalObjects.Instance.GetNamed<ImageData>("FrontImage"), GlobalObjects.Instance.Get<DialogService>());


            if (GlobalObjects.Instance.GetNamed<WorkManager>("frontInspection") != null)
            {
                GlobalObjects.Instance.GetNamed<WorkManager>("frontInspection").InspectionStart += InspectionStart_Callback;
                GlobalObjects.Instance.GetNamed<WorkManager>("frontInspection").PositionDone += PositionDone_Callback;
                GlobalObjects.Instance.GetNamed<WorkManager>("frontInspection").InspectionDone += InspectionDone_Callback;
                GlobalObjects.Instance.GetNamed<WorkManager>("frontInspection").ProcessDefectWaferStart += ProcessDefectWaferStart_Callback;
                GlobalObjects.Instance.GetNamed<WorkManager>("frontInspection").IntegratedProcessDefectDone += ProcessDefectWaferDone_Callback;
                GlobalObjects.Instance.GetNamed<WorkManager>("frontInspection").ProcessDefectDone += ProcessDefectDone_Callback;
            }


            //if (GlobalObjects.Instance.Get<InspectionManagerFrontside>() != null)
            //{
            //    GlobalObjects.Instance.Get<InspectionManagerFrontside>().InspectionStart += InspectionStart_Callback;
            //    GlobalObjects.Instance.Get<InspectionManagerFrontside>().PositionDone += PositionDone_Callback;
            //    GlobalObjects.Instance.Get<InspectionManagerFrontside>().InspectionDone += InspectionDone_Callback;
            //    GlobalObjects.Instance.Get<InspectionManagerFrontside>().ProcessDefectWaferStart += ProcessDefectWaferStart_Callback;
            //    GlobalObjects.Instance.Get<InspectionManagerFrontside>().IntegratedProcessDefectDone += ProcessDefectWaferDone_Callback;
            //}

            this.p_DataViewer_VM.SelectedCellsChanged += SelectedCellsChanged_Callback;

            // Initialize MapViewer
            this.mapViewerVM = new MapViewer_ViewModel();
        }

        private void SelectedCellsChanged_Callback(object obj)
        {
            DataRowView row = (DataRowView)obj;
            if (row == null) return;

            System.Drawing.Rectangle m_View_Rect = new System.Drawing.Rectangle((int)(double)row["m_fAbsX"] - ImageViewerVM.p_View_Rect.Width / 2, (int)(double)row["m_fAbsY"] - this.imageViewerVM.p_View_Rect.Height / 2, this.imageViewerVM.p_View_Rect.Width, this.imageViewerVM.p_View_Rect.Height);
            ImageViewerVM.p_View_Rect = m_View_Rect;
            ImageViewerVM.SetImageSource();
            ImageViewerVM.UpdateImageViewer(); // replace RedrawShapes()
        }

        private string currentRecipe = "";
        public void LoadRecipe()
        {
            if (currentRecipe != GlobalObjects.Instance.Get<RecipeFront>().Name)
            {
                RecipeType_WaferMap waferMap = GlobalObjects.Instance.Get<RecipeFront>().WaferMap;
                currentRecipe = GlobalObjects.Instance.Get<RecipeFront>().Name;
                this.MapViewerVM.CreateMap(waferMap.MapSizeX, waferMap.MapSizeY);
                ResetMapColor();
            }
            else
            {
                RecipeType_WaferMap waferMap = GlobalObjects.Instance.Get<RecipeFront>().WaferMap;
                if(waferMap.MapSizeX != this.MapViewerVM.MapSizeX || waferMap.MapSizeY != this.MapViewerVM.MapSizeY)
                {
                    this.MapViewerVM.CreateMap(waferMap.MapSizeX, waferMap.MapSizeY);
                    ResetMapColor();
                }
            }
        }

        public void ResetMapColor()
        {
            RecipeType_WaferMap waferMap = GlobalObjects.Instance.Get<RecipeFront>().WaferMap;
            for(int i =0;i < waferMap.MapSizeY; i++)
            {
                for(int j=0; j < waferMap.MapSizeX; j++)
                {
                    int index = j + i * waferMap.MapSizeX;
                    CHIP_TYPE type = (CHIP_TYPE)waferMap.Data[index];
                    switch (type)
                    {
                        case CHIP_TYPE.NO_CHIP:
                            this.mapViewerVM.SetChipColor(j, i, MapViewerColorDefines.NoChip);
                            break;
                        case CHIP_TYPE.NORMAL:
                            this.mapViewerVM.SetChipColor(j, i, MapViewerColorDefines.Normal);
                            break;
                    }
                }
            }
        }

        #region [Command]

        public RelayCommand LoadedCommand
        {
            get => new RelayCommand(() =>
            {
                LoadRecipe();

                if (GlobalObjects.Instance.GetNamed<WorkManager>("frontInspection") != null)
                {
                    GlobalObjects.Instance.GetNamed<WorkManager>("frontInspection").WorkplaceStateChanged += WorkplaceStateChanged_Callback;
                    WIND2EventManager.SnapDone += SnapDone_Callback;
                }
                    
                //if (GlobalObjects.Instance.Get<InspectionManagerFrontside>() != null)
                //{
                //    GlobalObjects.Instance.Get<InspectionManagerFrontside>().WorkplaceStateChanged += WorkplaceStateChanged_Callback;
                //    WIND2EventManager.SnapDone += SnapDone_Callback;
                //}
                    
            });
        }
        public RelayCommand UnloadedCommand
        {
            get => new RelayCommand(() =>
            {
                if (GlobalObjects.Instance.GetNamed<WorkManager>("frontInspection") != null)
                {
                    GlobalObjects.Instance.GetNamed<WorkManager>("frontInspection").WorkplaceStateChanged -= WorkplaceStateChanged_Callback;
                    WIND2EventManager.SnapDone -= SnapDone_Callback;
                }

                //if (GlobalObjects.Instance.Get<InspectionManagerFrontside>() != null)
                //{
                //    GlobalObjects.Instance.Get<InspectionManagerFrontside>().WorkplaceStateChanged -= WorkplaceStateChanged_Callback;
                //    WIND2EventManager.SnapDone -= SnapDone_Callback;
                //}
            });
        }

        public RelayCommand btnStart
        {
            get => new RelayCommand(() =>
            {

                DefectCodeManager defectCodeManager = new DefectCodeManager(Constants.FilePath.SettingFilePath);
                defectCodeManager.ReadDefectCodes();

                if (GlobalObjects.Instance.GetNamed<WorkManager>("frontInspection") != null)
                {
                    GlobalObjects.Instance.GetNamed<WorkManager>("frontInspection").Start();                    

                    this.InspectionID = DatabaseManager.Instance.GetInspectionID();
                }
                return;

                //GlobalObjects.Instance.Get<InspectionManagerFrontside>().RemoteStart();
                //this.ImageViewerVM.ClearObjects();
                //if (GlobalObjects.Instance.Get<InspectionManagerFrontside>() != null)
                //{
                //    GlobalObjects.Instance.Get<InspectionManagerFrontside>().Start(WORK_TYPE.SNAP);
                //}
            });
        }

        public RelayCommand btnSnap
        {
            get => new RelayCommand(() =>
            {
                EQ.p_bStop = false;
                Vision vision = ((WIND2_Handler)GlobalObjects.Instance.Get<WIND2_Engineer>().ClassHandler()).p_Vision;
                if (vision.p_eState != ModuleBase.eState.Ready)
                {
                    MessageBox.Show("Vision Home이 완료 되지 않았습니다.");
                    return;
                }

                Run_GrabLineScan Grab = (Run_GrabLineScan)vision.CloneModuleRun("GrabLineScan");
                var viewModel = new Dialog_Scan_ViewModel(vision, Grab);
                Nullable<bool> result = GlobalObjects.Instance.Get<DialogService>().ShowDialog(viewModel);
                if (result.HasValue)
                {
                    if (result.Value)
                    {
                        vision.StartRun(Grab);
                    }
                    else
                    {

                    }
                }
            });
        }

        public RelayCommand btnManualAlign
        {
            get => new RelayCommand(() =>
            {
                var viewModel = new ManualAlignViewer_ViewModel(this.imageViewerVM.p_ImageData);
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

        public RelayCommand btnStop
        {
            get => new RelayCommand(() =>
            {

                if (GlobalObjects.Instance.GetNamed<WorkManager>("frontInspection") != null)
                {
                    GlobalObjects.Instance.GetNamed<WorkManager>("frontInspection").Stop();
                }

                //if (GlobalObjects.Instance.Get<InspectionManagerFrontside>() != null)
                //{
                //   GlobalObjects.Instance.Get<InspectionManagerFrontside>().Stop();
                //}
            });
        }

        public RelayCommand btnClear
        {
            get => new RelayCommand(() =>
            {
                this.ImageViewerVM.ClearObjects();
            });
        }


        public RelayCommand btnSaveKlarf
        {
            get => new RelayCommand(() =>
            {
                WorkManager workManager = GlobalObjects.Instance.GetNamed<WorkManager>("frontInspection");
                RecipeFront recipe = GlobalObjects.Instance.Get<RecipeFront>();

                WIND2_Engineer engineer = GlobalObjects.Instance.Get<WIND2_Engineer>();
                GrabModeFront grabMode = engineer.m_handler.p_Vision.GetGrabMode(recipe.CameraInfoIndex);
                InfoWafer infoWafer = engineer.m_handler.p_Vision.p_infoWafer;
                if(infoWafer == null)
                {
                    infoWafer = new InfoWafer("null", 0, engineer);
                }

                Settings settings = new Settings();
                SettingItem_SetupFrontside settings_frontside = settings.GetItem<SettingItem_SetupFrontside>();

                DataTable table = DatabaseManager.Instance.SelectCurrentInspectionDefect();
                List<Defect> defects = Tools.DataTableToDefectList(table);

                KlarfData_Lot klarfData = new KlarfData_Lot();
                Directory.CreateDirectory(settings_frontside.KlarfSavePath);

                klarfData.LotStart(settings_frontside.KlarfSavePath, infoWafer, recipe.WaferMap, grabMode);
                klarfData.WaferStart(recipe.WaferMap, infoWafer);
                klarfData.AddSlot(recipe.WaferMap, defects, recipe.GetItem<OriginRecipe>(), settings_frontside.UseTDIReview);
                klarfData.SetResultTimeStamp();
                klarfData.SaveKlarf();
                klarfData.SaveTiffImageOnlyTDI(defects, workManager.SharedBuffer, new Size(160, 120));


                klarfData.SaveImageJpg(workManager.SharedBuffer,
                    new Rect(settings_frontside.WholeWaferImageStartX, settings_frontside.WholeWaferImageStartY, settings_frontside.WholeWaferImageEndX, settings_frontside.WholeWaferImageEndY),
                    (long)(settings_frontside.WholeWaferImageCompressionRate * 100),
                    settings_frontside.OutputImageSizeX,
                    settings_frontside.OutputImageSizeY);


            });
        }

        public RelayCommand btnInspectionIDSearchCommand
        {
            get => new RelayCommand(() =>
            {
                m_DataViewer_VM.pDataTable = DatabaseManager.Instance.SelectTablewithInspectionID("defect", this.inspectionID);                
            });
        }

        public RelayCommand btnRemoteStart
        {
            get => new RelayCommand(() =>
            {
                GlobalObjects.Instance.Get<InspectionManagerFrontside>().RemoteProcessStart();
                GlobalObjects.Instance.Get<InspectionManagerFrontside>().RemoteProcessStartWork();
            });
        }
        #endregion

        #region [Callback]
        private void InspectionStart_Callback(object e, InspectionStartArgs args)
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                this.ImageViewerVM.ClearObjects();
            }));
        }

        private void PositionDone_Callback(object obj, PositionDoneEventArgs args)
        {
            Workplace workplace = obj as Workplace;
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                String test = "";
                if (workplace.Index == 0)
                {
                    test += "Trans : {" + workplace.OffsetX.ToString() + ", " + workplace.OffsetX.ToString() + "}" + "\n";
                    DrawRectMasterFeature(args.ptOldStart, args.ptOldEnd, args.ptNewStart, args.ptNewEnd, test, args.bSuccess);
                }
                else
                {
                    test += "Trans : {" + workplace.TransX.ToString() + ", " + workplace.TransY.ToString() + "}" + "\n";
                    DrawRectChipFeature(args.ptOldStart, args.ptOldEnd, args.ptNewStart, args.ptNewEnd, test, args.bSuccess);
                }
            }));
        }

        private void InspectionDone_Callback(object obj, InspectionDoneEventArgs args)
        {
            return;

            Workplace workplace = args.workplace;
            if (workplace == null || workplace.DefectList == null) return;
            List<String> textList = new List<String>();
            List<CRect> rectList = new List<CRect>();


            foreach (RootTools.Database.Defect defectInfo in workplace.DefectList)
            {
                String text = "";

                rectList.Add(new CRect((int)defectInfo.p_rtDefectBox.Left, (int)defectInfo.p_rtDefectBox.Top, (int)defectInfo.p_rtDefectBox.Right, (int)defectInfo.p_rtDefectBox.Bottom));
                textList.Add(text);
            }

            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                DrawRectDefect(rectList, textList);
            }));
        }

        private void ProcessDefectWaferStart_Callback(object obj, ProcessDefectWaferStartEventArgs args)
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                this.ImageViewerVM.RemoveObjectsByTag("defect");
            }));
        }

        private void ProcessDefectWaferDone_Callback(object obj, IntegratedProcessDefectDoneEventArgs args)
        {
            Workplace workplace = obj as Workplace;

            if (workplace == null || workplace.DefectList == null) return;
            List<String> textList = new List<String>();
            List<CRect> rectList = new List<CRect>();

            foreach (RootTools.Database.Defect defectInfo in workplace.DefectList)
            {
                String text = "";

                rectList.Add(new CRect((int)defectInfo.p_rtDefectBox.Left, (int)defectInfo.p_rtDefectBox.Top, (int)defectInfo.p_rtDefectBox.Right, (int)defectInfo.p_rtDefectBox.Bottom));
                textList.Add(text);
            }

            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                DrawRectDefect(rectList, textList);
            }));

            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                m_DataViewer_VM.pDataTable = DatabaseManager.Instance.SelectCurrentInspectionDefect();

              
            }));

            //List<Defect> defects = Tools.DataTableToDefectList(m_DataViewer_VM.pDataTable);
        }

        private void ProcessDefectDone_Callback(object obj, ProcessDefectDoneEventArgs args)
        {
            Workplace workplace = obj as Workplace;
            if (workplace == null || workplace.DefectList == null) return;
            List<String> textList = new List<String>();
            List<CRect> rectList = new List<CRect>();


            foreach (RootTools.Database.Defect defectInfo in workplace.DefectList)
            {
                String text = "";

                rectList.Add(new CRect((int)defectInfo.p_rtDefectBox.Left, (int)defectInfo.p_rtDefectBox.Top, (int)defectInfo.p_rtDefectBox.Right, (int)defectInfo.p_rtDefectBox.Bottom));
                textList.Add(text);
            }

            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                DrawRectDefect(rectList, textList);
            }));
        }


        public void WorkplaceStateChanged_Callback(object obj, WorkplaceStateChangedEventArgs args)
        {
            Workplace wp = args.workplace;

            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                this.MapViewerVM.SetChipColor(wp.MapIndexX, wp.MapIndexY, MapViewerColorDefines.GetWorkplaceStateColor(wp.WorkState));
            }));
        }


        private void SnapDone_Callback(object obj, SnapDoneArgs args)
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                this.ImageViewerVM.SetRoiRect();
            }));
        }
        #endregion

        #region [ImageView Draw Method]

        public void DrawRectMasterFeature(CPoint ptOldStart, CPoint ptOldEnd, CPoint ptNewStart, CPoint ptNewEnd, String text, bool bSuccess)
        {
            ImageViewerVM.AddDrawRect(ptOldStart, ptOldEnd, ImageViewerColorDefines.MasterPosition, "position");
            ImageViewerVM.AddDrawRect(ptNewStart, ptNewEnd, bSuccess ? ImageViewerColorDefines.MasterPostionMove: ImageViewerColorDefines.PostionFail, "position");
            //ImageViewerVM.DrawText(ptNew)
        }

        public void DrawRectChipFeature(CPoint ptOldStart, CPoint ptOldEnd, CPoint ptNewStart, CPoint ptNewEnd, String text, bool bSuccess)
        {
            ImageViewerVM.AddDrawRect(ptOldStart, ptOldEnd, ImageViewerColorDefines.ChipPosition, "position");
            ImageViewerVM.AddDrawRect(ptNewStart, ptNewEnd, bSuccess ? ImageViewerColorDefines.ChipPositionMove : ImageViewerColorDefines.PostionFail, "position");
        }
        public void DrawRectDefect(List<CRect> rectList, List<String> text)
        {
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                ImageViewerVM.AddDrawRectList(rectList, ImageViewerColorDefines.Defect, "defect");
            });
        }
            #endregion
    }
}