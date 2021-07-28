using RootTools;
using RootTools.Database;
using RootTools_Vision;
using RootTools_Vision.Utility;
using RootTools_Vision.WorkManager3;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace Root_WIND2.UI_User
{
    public class BacksideInspect_ViewModel : ObservableObject, IPage
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
            public static SolidColorBrush Extra = Brushes.DarkSlateGray;
            public static SolidColorBrush Snap = Brushes.LightSkyBlue;
            public static SolidColorBrush Position = Brushes.SkyBlue;
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

        public BacksideInspect_ViewModel()
        {
            if (GlobalObjects.Instance.GetNamed<ImageData>("BackImage").GetPtr() == IntPtr.Zero)
                return;

            // Initialize ImageViewer
            this.imageViewerVM = new BacksideInspect_ImageViewer_ViewModel();
            this.imageViewerVM.init(GlobalObjects.Instance.GetNamed<ImageData>("BackImage"), GlobalObjects.Instance.Get<DialogService>());
            
            // Initialize MapViewer
            this.mapViewerVM = new MapViewer_ViewModel();

            this.p_DataViewer_VM.SelectedCellsChanged += SelectedCellsChanged_Callback;
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
            if (GlobalObjects.Instance.GetNamed<WorkManager>("backInspection") == null) return;

            if (currentRecipe != GlobalObjects.Instance.Get<RecipeBack>().Name)
            {
                RecipeType_WaferMap waferMap = GlobalObjects.Instance.Get<RecipeBack>().WaferMap;
                currentRecipe = GlobalObjects.Instance.Get<RecipeBack>().Name;
                if(waferMap.UseExtraMap)
                {
                    this.MapViewerVM.CreateMap(waferMap.ExtraMapSizeX, waferMap.ExtraMapSizeY);
                }
                else
                {
                    this.MapViewerVM.CreateMap(waferMap.MapSizeX, waferMap.MapSizeY);
                }
                ResetMapColor();
            }
            else
            {
                RecipeType_WaferMap waferMap = GlobalObjects.Instance.Get<RecipeBack>().WaferMap;
                if (waferMap.UseExtraMap)
                {
                    this.MapViewerVM.CreateMap(waferMap.ExtraMapSizeX, waferMap.ExtraMapSizeY);
                }
                else
                {
                    this.MapViewerVM.CreateMap(waferMap.MapSizeX, waferMap.MapSizeY);
                }
                ResetMapColor();
            }
        }

        #region [Method]
        public void ResetMapColor()
        {
            RecipeType_WaferMap waferMap = GlobalObjects.Instance.Get<RecipeBack>().WaferMap;
            if(waferMap.UseExtraMap)
            {
                for (int i = 0; i < waferMap.ExtraMapSizeY; i++)
                {
                    for (int j = 0; j < waferMap.ExtraMapSizeX; j++)
                    {
                        int index = j + i * waferMap.ExtraMapSizeX;
                        CHIP_TYPE type = (CHIP_TYPE)waferMap.ExtraMapdata[index];
                        switch (type)
                        {
                            case CHIP_TYPE.NO_CHIP:
                                this.mapViewerVM.SetChipColor(j, i, MapViewerColorDefines.NoChip);
                                break;
                            case CHIP_TYPE.EXTRA:
                                this.mapViewerVM.SetChipColor(j, i, MapViewerColorDefines.Extra);
                                break;
                            case CHIP_TYPE.NORMAL:
                                this.mapViewerVM.SetChipColor(j, i, MapViewerColorDefines.Normal);
                                break;
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < waferMap.MapSizeY; i++)
                {
                    for (int j = 0; j < waferMap.MapSizeX; j++)
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
        }
        #endregion

        #region [Properties]
        private BacksideInspect_ImageViewer_ViewModel imageViewerVM;
        public BacksideInspect_ImageViewer_ViewModel ImageViewerVM
        {
            get => this.imageViewerVM;
            set
            {
                SetProperty<BacksideInspect_ImageViewer_ViewModel>(ref this.imageViewerVM, value);
            }
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

        #region [Command]
        public RelayCommand LoadedCommand
        {
            get => new RelayCommand(() =>
            {
                if (GlobalObjects.Instance.GetNamed<WorkManager>("backInspection") != null)
                {
                    LoadRecipe();

                    WorkManager workManager = GlobalObjects.Instance.GetNamed<WorkManager>("backInspection");
                    workManager.InspectionStart += InspectionStart_Callback;
                    workManager.PositionDone += PositionDone_Callback;
                    workManager.InspectionDone += InspectionDone_Callback;
                    workManager.ProcessDefectWaferStart += ProcessDefectWaferStart_Callback;
                    workManager.IntegratedProcessDefectDone += ProcessDefectWaferDone_Callback;
                    workManager.WorkplaceStateChanged += WorkplaceStateChanged_Callback;
                }

                if(this.ImageViewerVM != null)
                {
                    this.ImageViewerVM.ReadCenterPoint();
                    this.ImageViewerVM.ReadExclusivePolygon();
                }
            });
        }
        public RelayCommand UnloadedCommand
        {
            get => new RelayCommand(() =>
            {
                if (GlobalObjects.Instance.GetNamed<WorkManager>("backInspection") != null)
                {
                    LoadRecipe();

                    WorkManager workManager = GlobalObjects.Instance.GetNamed<WorkManager>("backInspection");
                    workManager.InspectionStart -= InspectionStart_Callback;
                    workManager.PositionDone -= PositionDone_Callback;
                    workManager.InspectionDone -= InspectionDone_Callback;
                    workManager.ProcessDefectWaferStart -= ProcessDefectWaferStart_Callback;
                    workManager.IntegratedProcessDefectDone -= ProcessDefectWaferDone_Callback;
                    workManager.WorkplaceStateChanged -= WorkplaceStateChanged_Callback;
                }
            });
        }

        public RelayCommand btnStart
        {
            get => new RelayCommand(() =>
            {
                this.ImageViewerVM.ClearObjects();
                if (GlobalObjects.Instance.GetNamed<WorkManager>("backInspection") != null)
                {

                   RecipeBack recipe = GlobalObjects.Instance.Get<RecipeBack>();
                   GlobalObjects.Instance.GetNamed<WorkManager>("backInspection").Start();

                   this.InspectionID = DatabaseManager.Instance.GetInspectionID();
                }
            });
        }

        public RelayCommand btnSnap
        {
            get => new RelayCommand(() =>
            {

            });
        }

        public RelayCommand btnStop
        {
            get => new RelayCommand(() =>
            {
                if (GlobalObjects.Instance.GetNamed<WorkManager>("backInspection") != null)
                {
                    GlobalObjects.Instance.GetNamed<WorkManager>("backInspection").Stop();
                }
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

                WorkManager workManager = GlobalObjects.Instance.GetNamed<WorkManager>("backInspection");
                RecipeBack recipe = GlobalObjects.Instance.Get<RecipeBack>();

                WIND2_Engineer engineer = GlobalObjects.Instance.Get<WIND2_Engineer>();
                GrabModeBack grabMode = engineer.m_handler.p_BackSideVision.GetGrabMode(recipe.CameraInfoIndex);
                InfoWafer infoWafer = engineer.m_handler.p_BackSideVision.p_infoWafer;
                if (infoWafer == null)
                {
                    infoWafer = new InfoWafer("null", 0, engineer);
                }

                Settings settings = new Settings();
                SettingItem_SetupBackside settings_backside = settings.GetItem<SettingItem_SetupBackside>();

                DataTable table = DatabaseManager.Instance.SelectCurrentInspectionDefect();
                List<Defect> defects = Tools.DataTableToDefectList(table);

                KlarfData_Lot klarfData = new KlarfData_Lot();
                Directory.CreateDirectory(settings_backside.KlarfSavePath);

                klarfData.LotStart(settings_backside.KlarfSavePath, infoWafer, recipe.WaferMap, grabMode);
                klarfData.WaferStart(recipe.WaferMap, infoWafer);
                klarfData.AddSlot(recipe.WaferMap, defects, recipe.GetItem<OriginRecipe>(), settings_backside.UseTDIReview);
                klarfData.SetResultTimeStamp();
                klarfData.SaveKlarf();
                klarfData.SaveTiffImageOnlyTDI(defects, workManager.SharedBuffer, new Size(160, 120));


                //Tools.SaveImageJpg(workManager.SharedBuffer,
                //    new Rect(settings_backside.WholeWaferImageStartX, settings_backside.WholeWaferImageStartY, settings_backside.WholeWaferImageEndX, settings_backside.WholeWaferImageEndY),
                //    settings_backside.KlarfSavePath + "\\" + DateTime.Now.ToString("yyyyMMddhhmmss") + "_backside.jpg",
                //    (long)(settings_backside.WholeWaferImageCompressionRate * 100),
                //    settings_backside.OutputImageSizeX,
                //    settings_backside.OutputImageSizeY);

                if (recipe.UseExclusiveRegion == false)
                    recipe.UseExclusiveRegion = true;

                if (recipe.ExclusiveRegionFilePath == "")
                    recipe.ExclusiveRegionFilePath = Constants.FilePath.BacksideExclusiveRegionFilePath;

                recipe.Save();

                List<List<Point>> polygon = PolygonController.ReadPolygonFile(recipe.ExclusiveRegionFilePath);

                BacksideRecipe backRecipe = recipe.GetItem<BacksideRecipe>();
               
                klarfData.SaveImageJpgInterpolation(workManager.SharedBuffer,
                       new Rect(
                           settings_backside.WholeWaferImageStartX,
                           settings_backside.WholeWaferImageStartY,
                           settings_backside.WholeWaferImageEndX,
                           settings_backside.WholeWaferImageEndY),
                       (long)(settings_backside.WholeWaferImageCompressionRate * 100),
                       settings_backside.OutputImageSizeX,
                       settings_backside.OutputImageSizeY, polygon, (int)(settings_backside.SaveWaferSize * 1000 / grabMode.m_dRealResX_um), (int)(settings_backside.SaveWaferSize * 1000 / grabMode.m_dRealResY_um),
                       backRecipe.CenterX,
                       backRecipe.CenterY);
            });
        }

        public RelayCommand btnInspectionIDSearchCommand
        {
            get => new RelayCommand(() =>
            {
                m_DataViewer_VM.pDataTable = DatabaseManager.Instance.SelectTablewithInspectionID("defect", this.inspectionID);
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
            Workplace workplace = obj as Workplace;
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
                //DatabaseManager.Instance.SelectData();
                m_DataViewer_VM.pDataTable = DatabaseManager.Instance.SelectCurrentInspectionDefect();
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
            ImageViewerVM.AddDrawRect(ptNewStart, ptNewEnd, bSuccess ? ImageViewerColorDefines.MasterPostionMove : ImageViewerColorDefines.PostionFail, "position");
            //ImageViewerVM.DrawText(ptNew)
        }

        public void DrawRectChipFeature(CPoint ptOldStart, CPoint ptOldEnd, CPoint ptNewStart, CPoint ptNewEnd, String text, bool bSuccess)
        {
            ImageViewerVM.AddDrawRect(ptOldStart, ptOldEnd, ImageViewerColorDefines.ChipPosition, "position");
            ImageViewerVM.AddDrawRect(ptNewStart, ptNewEnd, bSuccess ? ImageViewerColorDefines.ChipPositionMove : ImageViewerColorDefines.PostionFail, "position");
        }
        public void DrawRectDefect(List<CRect> rectList, List<String> text)
        {
            ImageViewerVM.AddDrawRectList(rectList, ImageViewerColorDefines.Defect, "defect");
        }
        #endregion
    }
}
