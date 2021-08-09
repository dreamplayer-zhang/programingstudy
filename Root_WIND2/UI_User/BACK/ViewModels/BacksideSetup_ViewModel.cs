using RootTools;
using RootTools_CLR;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Root_WIND2.UI_User
{
    public class BacksideSetup_ViewModel : ObservableObject, IPage
    {

        #region [Properties]
        public enum SEARCH_ROI_OPTIONS
        {
            IncludeWaferEdge = 0,
            ExcludeWaferEdge
        }

        private SEARCH_ROI_OPTIONS searchROIOptions = SEARCH_ROI_OPTIONS.ExcludeWaferEdge;
        public SEARCH_ROI_OPTIONS SearchROIOptions
        {
            get => this.searchROIOptions;
            set
            {
                SetProperty<SEARCH_ROI_OPTIONS>(ref this.searchROIOptions, value);
            }
        }

        private BacksideSetup_ImageViewer_ViewModel imageViewerVM;
        public BacksideSetup_ImageViewer_ViewModel ImageViewerVM
        {
            get => this.imageViewerVM;
            set
            {
                SetProperty<BacksideSetup_ImageViewer_ViewModel>(ref this.imageViewerVM, value);
            }
        }

        private MapViewer_ViewModel mapViewerVM = new MapViewer_ViewModel();
        public MapViewer_ViewModel MapViewerVM
        {
            get => this.mapViewerVM;
            set
            {
                SetProperty<MapViewer_ViewModel>(ref this.mapViewerVM, value);
            }
        }

        private int centerPointX;
        public int CenterPointX
        {
            get => this.centerPointX;
            set
            {
                SetProperty<int>(ref this.centerPointX, value);
            }
        }

        private int centerPointY;
        public int CenterPointY
        {
            get => this.centerPointY;
            set
            {
                SetProperty<int>(ref this.centerPointY, value);
            }
        }

        private int circleRadius;
        public int CircleRadius
        {
            get => this.circleRadius;
            set
            {
                SetProperty<int>(ref this.circleRadius, value);
            }
        }

        private int downSamplingRatio = 20;
        public int DownSamplingRatio
        {
            get => this.downSamplingRatio;
            set
            {
                SetProperty<int>(ref this.downSamplingRatio, value);
            }
        }

        private int mapUnitWidth = 0;
        public int MapUnitWidth
        {
            get => this.mapUnitWidth;
            set
            {
                SetProperty<int>(ref this.mapUnitWidth, value);
            }
        }

        private int mapUnitHeight = 0;
        public int MapUnitHeight
        {
            get => this.mapUnitHeight;
            set
            {
                SetProperty<int>(ref this.mapUnitHeight, value);
            }
        }

        private int originPointX = 0;
        public int OriginPointX
        {
            get => this.originPointX;
            set
            {
                SetProperty<int>(ref this.originPointX, value);
            }
        }

        private int originPointY = 0;
        public int OriginPointY
        {
            get => this.originPointY;
            set
            {
                SetProperty<int>(ref this.originPointY, value);
            }
        }

        private int searchedCenterPointX = 0;
        public int SearchedCenterPointX
        {
            get => this.searchedCenterPointX;
            set
            {
                SetProperty<int>(ref this.searchedCenterPointX, value);
            }
        }

        private int searchedCenterPointY = 0;
        public int SearchedCenterPointY
        {
            get => this.searchedCenterPointY;
            set
            {
                SetProperty<int>(ref this.searchedCenterPointY, value);
            }
        }

        private int searchedCircleRadius = 0;
        public int SearchedCircleRadius
        {
            get => this.searchedCircleRadius;
            set
            {
                SetProperty<int>(ref this.searchedCircleRadius, value);
            }
        }


        private int[] mapData;
        public int[] MapData
        {
            get => this.mapData;
            set
            {
                this.mapData = value;
            }
        }
        #endregion

        public BacksideSetup_ViewModel()
        {
            if (GlobalObjects.Instance.GetNamed<ImageData>("BackImage").GetPtr() == IntPtr.Zero)
                return;

            this.imageViewerVM = new BacksideSetup_ImageViewer_ViewModel();
            this.imageViewerVM.init(GlobalObjects.Instance.GetNamed<ImageData>("BackImage"), GlobalObjects.Instance.Get<DialogService>());

            this.ImageViewerVM.ROIDone += ROIDone_Callback;
        }

        public void LoadRecipe()
        {
            RecipeType_WaferMap waferMap = GlobalObjects.Instance.Get<RecipeBack>().WaferMap;
            if (waferMap.UseExtraMap)
            {
                this.MapViewerVM.CreateMap(waferMap.ExtraMapSizeX, waferMap.ExtraMapSizeY, waferMap.ExtraMapdata);
            }
            else
            {
                this.MapViewerVM.CreateMap(waferMap.MapSizeX, waferMap.MapSizeY, waferMap.Data);
            }
        }

        private void ROIDone_Callback(CPoint centerPt, int radius)
        {
            this.CenterPointX = centerPt.X;
            this.CenterPointY = centerPt.Y;
            this.CircleRadius = radius;
        }

        #region [Command]
        public ICommand LoadedCommand
        {
            get => new RelayCommand(() =>
            {
                this.LoadRecipe();

                if(this.ImageViewerVM != null)
                {
                    this.ImageViewerVM.ReadExclusivePolygon();
                    this.ImageViewerVM.ReadCenterPoint();
                }
            });
        }

        public ICommand btnCreateROICommand
        {
            get => new RelayCommand(() =>
            {
                List<CRect> rectList = this.CalcDiePosition((int)this.ImageViewerVM.CircleCenterPoint.X, (int)this.ImageViewerVM.CircleCenterPoint.Y);

                List<CRect> rectOuterList = this.CalcPartialDiePosition((int)this.ImageViewerVM.CircleCenterPoint.X, (int)this.ImageViewerVM.CircleCenterPoint.Y);

                this.ImageViewerVM.SetMapRectList(rectList, rectOuterList);

                RecipeType_WaferMap waferMap = GlobalObjects.Instance.Get<RecipeBack>().WaferMap;
                if (waferMap.UseExtraMap)
                {
                    this.MapViewerVM.CreateMap(waferMap.ExtraMapSizeX, waferMap.ExtraMapSizeY, waferMap.ExtraMapdata);
                }
                else
                {
                    this.MapViewerVM.CreateMap(waferMap.MapSizeX, waferMap.MapSizeY, waferMap.Data);
                }

            });
        }

        public ICommand btnClearROICommand
        {
            get => new RelayCommand(() =>
             {
                 this.ImageViewerVM.ClearMapRectList();
             });
        }
        #endregion


        #region [Method]

        private void SetRecipe()
        {
            if (mapData == null || mapData.Length == 0) return;

            BacksideRecipe backsideRecipe = GlobalObjects.Instance.Get<RecipeBack>().GetItem<BacksideRecipe>();
            OriginRecipe originRecipe = GlobalObjects.Instance.Get<RecipeBack>().GetItem<OriginRecipe>();
            // Map Data Recipe 생성
            originRecipe.OriginX = this.originPointX;
            originRecipe.OriginY = this.originPointY;
            originRecipe.OriginWidth = this.mapUnitWidth;
            originRecipe.OriginHeight = this.mapUnitHeight;
            originRecipe.DiePitchX = this.mapUnitWidth;
            originRecipe.DiePitchY = this.mapUnitHeight;

            backsideRecipe.CenterX = this.SearchedCenterPointY;
            backsideRecipe.CenterY = this.SearchedCenterPointY;
            backsideRecipe.Radius = this.SearchedCircleRadius;
            backsideRecipe.IsEdgeIncluded = (this.SearchROIOptions == SEARCH_ROI_OPTIONS.IncludeWaferEdge) ? true : false;


            //GlobalObjects.Instance.Get<RecipeBack>().WaferMap = new RecipeType_WaferMap(this.MapSizeX, this.MapSizeY, this.MapData);
        }

        public List<CRect> CalcDiePosition(int centerX, int centerY)
        {
            CameraInfo camInfo = DataConverter.GrabModeToCameraInfo(GlobalObjects.Instance.Get<WIND2_Engineer>().m_handler.p_BackSideVision.GetGrabMode(GlobalObjects.Instance.Get<RecipeBack>().CameraInfoIndex));

            RecipeType_WaferMap waferMap = GlobalObjects.Instance.Get<RecipeBack>().WaferMap;
            OriginRecipe originRecipe = GlobalObjects.Instance.Get<RecipeBack>().GetItem<OriginRecipe>();
            int[] mapData = waferMap.Data;

            int MasterDieX = waferMap.MasterDieX;
            int MasterDieY = waferMap.MasterDieY;

            int originDieX = waferMap.OriginDieX;
            int originDieY = waferMap.OriginDieY;

            double diePitchX = waferMap.DiePitchX / camInfo.RealResX;
            double diePitchY = waferMap.DiePitchY / camInfo.RealResY;

            originRecipe.DiePitchX = (int)diePitchX;
            originRecipe.DiePitchY = (int)diePitchY;

            double sampleCenterX = waferMap.SampleCenterLocationX;
            double sampleCenterY = waferMap.SampleCenterLocationY;

            int mapSizeX = waferMap.MapSizeX;
            int mapSizeY = waferMap.MapSizeY;


            double diameter = this.ImageViewerVM.InspectionCircleDiameter * 1000 / camInfo.RealResX;
            double radius = diameter / (double)2;

            List<CRect> rectList = new List<CRect>();

            int originX = (int)(centerX + sampleCenterX);
            int originY = (int)(centerY - sampleCenterY);


            // Normal
            for (int x = 0; x < mapSizeX; x++)
            {
                for (int y = 0; y < mapSizeY; y++)
                {
                    if (mapData[y * mapSizeX + x] == 1)
                    {
                        int left = (int)(originX - (originDieX - x) * diePitchX);
                        int right = (int)(originX - (originDieX - x - 1) * diePitchX);
                        int top = (int)(originY - (originDieY - y) * diePitchY);
                        int bottom = (int)(originY - (originDieY - y - 1) * diePitchY);

                        rectList.Add(new CRect(left, top, right, bottom));

                        if (x == MasterDieX && y == MasterDieY)
                        {
                            originRecipe.OriginX = left;
                            originRecipe.OriginY = bottom;
                        }
                    }
                }
            }

            BacksideRecipe backsideRecipe = GlobalObjects.Instance.Get<RecipeBack>().GetItem<BacksideRecipe>();

            backsideRecipe.CenterX = centerX;
            backsideRecipe.CenterY = centerY;
            backsideRecipe.Radius = (int)radius;

            return rectList;
        }


        public List<CRect> CalcPartialDiePosition(int centerX, int centerY)
        {
            CameraInfo camInfo = DataConverter.GrabModeToCameraInfo(GlobalObjects.Instance.Get<WIND2_Engineer>().m_handler.p_BackSideVision.GetGrabMode(GlobalObjects.Instance.Get<RecipeBack>().CameraInfoIndex));

            RecipeType_WaferMap waferMap = GlobalObjects.Instance.Get<RecipeBack>().WaferMap;
            OriginRecipe originRecipe = GlobalObjects.Instance.Get<RecipeBack>().GetItem<OriginRecipe>();
            int[] mapData = waferMap.Data;
            int[] extraMapData;


            int masterDieX = waferMap.MasterDieX;
            int masterDieY = waferMap.MasterDieY;

            int originDieX = waferMap.OriginDieX;
            int originDieY = waferMap.OriginDieY;

            double diePitchX = waferMap.DiePitchX / camInfo.RealResX;
            double diePitchY = waferMap.DiePitchY / camInfo.RealResY;

            originRecipe.DiePitchX = (int)diePitchX;
            originRecipe.DiePitchY = (int)diePitchY;

            double sampleCenterX = waferMap.SampleCenterLocationX;
            double sampleCenterY = waferMap.SampleCenterLocationY;

            int mapSizeX = waferMap.MapSizeX;
            int mapSizeY = waferMap.MapSizeY;

            if (mapSizeX == 0 || mapSizeY == 0)
            {
                MessageBox.Show("Map 정보가 없습니다.");
                return new List<CRect>();
            }

           
            double diameter = this.ImageViewerVM.InspectionCircleDiameter * 1000 / camInfo.RealResX;
            double radius = diameter / 2;
            double radius_2 = radius * radius;

            List<CRect> rectList = new List<CRect>();

            int originX = (int)(centerX + sampleCenterX);
            int originY = (int)(centerY - sampleCenterY);

            double left_remain_X = (radius - sampleCenterX) - (originDieX * diePitchX);
            double top_remain_y = (radius - sampleCenterY) - (originDieY * diePitchY);

            double right_remain_X = (radius - sampleCenterX) - ((mapSizeX - originDieX) * diePitchX);
            double bottom_remain_y = (radius - sampleCenterY) - ((mapSizeY - originDieY) * diePitchY);


            int dieLeftCount = 0, dieRightCount = 0, dieTopCount = 0, dieBottomCount = 0;
            if (left_remain_X > 0)
            {
                dieLeftCount = (int)Math.Ceiling(left_remain_X / diePitchX);
            }

            if (top_remain_y > 0)
            {
                dieTopCount = (int)Math.Ceiling(top_remain_y / diePitchY);
            }

            if (right_remain_X > 0)
            {
                dieRightCount = (int)Math.Ceiling(right_remain_X / diePitchX);
            }

            if (bottom_remain_y > 0)
            {
                dieBottomCount = (int)Math.Ceiling(bottom_remain_y / diePitchY);
            }


            int originMapSizeX = mapSizeX;
            int originMapSizeY = mapSizeY;
            mapSizeX += (dieLeftCount + dieRightCount);
            mapSizeY += (dieTopCount + dieBottomCount);

            originDieX += dieLeftCount;
            originDieY += dieTopCount;


            extraMapData = new int[mapSizeX * mapSizeY];

            int startX = mapSizeX - 1;
            int startY = mapSizeY - 1;
            int endX = 0;
            int endY = 0;

            for (int y = 0; y < mapSizeY; y++)
            {
                for (int x = 0; x < mapSizeX; x++)
                {
                    int rel_x = (x - originDieX); // 원점 중심좌표로 Right/Top방향이 +
                    int rel_y = (originDieY - y);

                    int left = (int)(originX + rel_x * diePitchX);
                    int right = (int)(originX + (rel_x + 1) * diePitchX);
                    int top = (int)(originY - (rel_y) * diePitchY);
                    int bottom = (int)(originY - (rel_y - 1) * diePitchY);


                    int map_x = x - dieLeftCount;
                    int map_y = y - dieTopCount;
                    if (map_x >= 0 && map_y >= 0 &&
                        map_x < originMapSizeX && map_y < originMapSizeY)
                    {
                        if (mapData[map_y * originMapSizeX + map_x] == 1)
                        {
                            extraMapData[y * mapSizeX + x] = (int)CHIP_TYPE.NORMAL;
                            continue;
                        }
                    }

                    if (rel_x >= 0 && rel_y >= 0) // 제 1사분면
                    {
                        // 다이의 좌하단이 포함
                        if (Math.Pow(left - centerX, 2) + Math.Pow(bottom - centerY, 2) < radius_2)
                        {
                            extraMapData[y * mapSizeX + x] = (int)CHIP_TYPE.EXTRA;
                            rectList.Add(new CRect(left, top, right, bottom));
                        }

                    }
                    else if (rel_x < 0 && rel_y >= 0) // 제 2사분면
                    {
                        // 다이의 우하단이 포함
                        if (Math.Pow(right - centerX, 2) + Math.Pow(bottom - centerY, 2) < radius_2)
                        {
                            extraMapData[y * mapSizeX + x] = (int)CHIP_TYPE.EXTRA;
                            rectList.Add(new CRect(left, top, right, bottom));
                        }
                    }
                    else if (rel_x < 0 && rel_y < 0) // 제 3사분면
                    {
                        // 다이의 우상단이 포함
                        if (Math.Pow(right - centerX, 2) + Math.Pow(top - centerY, 2) < radius_2)
                        {
                            extraMapData[y * mapSizeX + x] = (int)CHIP_TYPE.EXTRA;
                            rectList.Add(new CRect(left, top, right, bottom));
                        }
                    }
                    else //제 4사분면
                    {
                        // 다이의 좌상단이 포함
                        if (Math.Pow(left - centerX, 2) + Math.Pow(top - centerY, 2) < radius_2)
                        {
                            extraMapData[y * mapSizeX + x] = (int)CHIP_TYPE.EXTRA;
                            rectList.Add(new CRect(left, top, right, bottom));
                        }
                    }

                    if (extraMapData[y * mapSizeX + x] == (int)CHIP_TYPE.EXTRA ||
                        extraMapData[y * mapSizeX + x] == (int)CHIP_TYPE.NORMAL)
                    {
                        if (startX > x) startX = x;
                        if (startY > y) startY = y;

                        if (endX < x) endX = x;
                        if (endY < y) endY = y;
                    }
                }
            }


            //if(mapSizeY == waferMap.MapSizeY && mapSizeX == waferMap.MapSizeX)
            //{
            //    waferMap.UseExtraMap = true;
            //    waferMap.CreateExtraMap(mapSizeX, mapSizeY, extraMapData, 0, 0);
            //}
            //else
            {
                // 비어 있는 행/열 삭제
                int newSizeX = endX - startX + 1;
                int newSizeY = endY - startY + 1;
                int[] newExtraMapData = new int[newSizeX * newSizeY];
                for (int y = startY; y <= endY; y++)
                {
                    for (int x = startX; x <= endX; x++)
                    {
                        int xx = x - startX;
                        int yy = y - startY;
                        newExtraMapData[xx + yy * newSizeX] = extraMapData[x + y * mapSizeX];
                    }
                }

                waferMap.UseExtraMap = true;
                waferMap.CreateExtraMap(newSizeX, newSizeY, newExtraMapData, dieLeftCount, dieTopCount);
            }

            return rectList;
        }
        #endregion


    }
}
