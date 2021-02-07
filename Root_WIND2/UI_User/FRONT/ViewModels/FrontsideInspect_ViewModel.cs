﻿using RootTools;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

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
            public static SolidColorBrush NoChip = Brushes.DimGray;
            public static SolidColorBrush Normal = Brushes.LightGray;
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
        #endregion

        public FrontsideInspect_ViewModel()
        {
            // Initialize ImageViewer
            this.imageViewerVM = new FrontsideInspect_ImageViewer_ViewModel();
            this.imageViewerVM.init(GlobalObjects.Instance.GetNamed<ImageData>("FrontImage"), GlobalObjects.Instance.Get<DialogService>());

            WorkEventManager.PositionDone += PositionDone_Callback;
            WorkEventManager.InspectionDone += InspectionDone_Callback;
            //WorkEventManager.ProcessDefectDone += ProcessDefectDone_Callback;

            // Initialize MapViewer
            this.mapViewerVM = new MapViewer_ViewModel();

            WorkEventManager.WorkplaceStateChanged += WorkplaceStateChanged_Callback;
        }

        public void SetPage()
        {
            LoadRecipe();
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

        public RelayCommand btnStart
        {
            get => new RelayCommand(() =>
            {
                this.ImageViewerVM.ClearObjects();
                GlobalObjects.Instance.Get<InspectionManagerFrontside>().Start(WORK_TYPE.SNAP);
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
                GlobalObjects.Instance.Get<InspectionManagerFrontside>().Stop();
            });
        }

        public RelayCommand btnClear
        {
            get => new RelayCommand(() =>
            {
                this.ImageViewerVM.ClearObjects();
            });
        }
        #endregion

        #region [Callback]
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
                DrawRectDefect(rectList, textList, args.reDraw);
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

        #endregion

        #region [ImageView Draw Method]

        public void DrawRectMasterFeature(CPoint ptOldStart, CPoint ptOldEnd, CPoint ptNewStart, CPoint ptNewEnd, String text, bool bSuccess)
        {
            ImageViewerVM.AddDrawRect(ptOldStart, ptOldEnd, ImageViewerColorDefines.MasterPosition);
            ImageViewerVM.AddDrawRect(ptNewStart, ptNewEnd, bSuccess ? ImageViewerColorDefines.MasterPostionMove: ImageViewerColorDefines.PostionFail);
            //ImageViewerVM.DrawText(ptNew)
        }

        public void DrawRectChipFeature(CPoint ptOldStart, CPoint ptOldEnd, CPoint ptNewStart, CPoint ptNewEnd, String text, bool bSuccess)
        {
            ImageViewerVM.AddDrawRect(ptOldStart, ptOldEnd, ImageViewerColorDefines.ChipPosition);
            ImageViewerVM.AddDrawRect(ptNewStart, ptNewEnd, bSuccess ? ImageViewerColorDefines.ChipPositionMove : ImageViewerColorDefines.PostionFail);
        }
        public void DrawRectDefect(List<CRect> rectList, List<String> text, bool reDraw = false)
        {
            ImageViewerVM.AddDrawRectList(rectList, ImageViewerColorDefines.Defect);
        }
            #endregion
    }
}