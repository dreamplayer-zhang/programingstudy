﻿using RootTools;
using RootTools.Database;
using RootTools_Vision;
using System;
using System.Collections.Generic;
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

            WorkEventManager.InspectionStart += InspectionStart_Callback;
            WorkEventManager.PositionDone += PositionDone_Callback;
            WorkEventManager.InspectionDone += InspectionDone_Callback;
            WorkEventManager.ProcessDefectWaferStart += ProcessDefectWaferStart_Callback;
            WorkEventManager.ProcessDefectWaferDone += ProcessDefectWaferDone_Callback;

            // Initialize MapViewer
            this.mapViewerVM = new MapViewer_ViewModel();
        }

        private string currentRecipe = "";
        public void LoadRecipe()
        {
            if (currentRecipe != GlobalObjects.Instance.Get<RecipeBack>().Name)
            {
                RecipeType_WaferMap waferMap = GlobalObjects.Instance.Get<RecipeBack>().WaferMap;
                currentRecipe = GlobalObjects.Instance.Get<RecipeBack>().Name;
                this.MapViewerVM.CreateMap(waferMap.MapSizeX, waferMap.MapSizeY);
                ResetMapColor();
            }
            else
            {
                RecipeType_WaferMap waferMap = GlobalObjects.Instance.Get<RecipeBack>().WaferMap;
                if (waferMap.MapSizeX != this.MapViewerVM.MapSizeX || waferMap.MapSizeY != this.MapViewerVM.MapSizeY)
                {
                    this.MapViewerVM.CreateMap(waferMap.MapSizeX, waferMap.MapSizeY);
                    ResetMapColor();
                }
            }
        }

        #region [Method]
        public void ResetMapColor()
        {
            RecipeType_WaferMap waferMap = GlobalObjects.Instance.Get<RecipeBack>().WaferMap;
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
        #endregion

        #region [Command]
        public RelayCommand LoadedCommand
        {
            get => new RelayCommand(() =>
            {
                LoadRecipe();

                WorkEventManager.WorkplaceStateChanged += WorkplaceStateChanged_Callback;
            });
        }
        public RelayCommand UnloadedCommand
        {
            get => new RelayCommand(() =>
            {
                WorkEventManager.WorkplaceStateChanged -= WorkplaceStateChanged_Callback;
            });
        }

        public RelayCommand btnStart
        {
            get => new RelayCommand(() =>
            {
                this.ImageViewerVM.ClearObjects();
                GlobalObjects.Instance.Get<InspectionManagerBackside>().Start(WORK_TYPE.SNAP);
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
                GlobalObjects.Instance.Get<InspectionManagerBackside>().Stop();
            });
        }

        public RelayCommand btnClear
        {
            get => new RelayCommand(() =>
            {
                this.ImageViewerVM.ClearObjects();
            });
        }

        public RelayCommand btnRemote
        {
            get => new RelayCommand(() =>
            {

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

        object lockObj = new object();
        private void PositionDone_Callback(object obj, PositionDoneEventArgs args)
        {
            Workplace workplace = obj as Workplace;
            lock (this.lockObj)
            {
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

        private void ProcessDefectWaferDone_Callback(object obj, ProcessDefectWaferDoneEventArgs args)
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                DatabaseManager.Instance.SelectData();
                m_DataViewer_VM.pDataTable = DatabaseManager.Instance.pDefectTable;
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
