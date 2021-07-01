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

namespace Root_WindII_Option.UI
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

        private int mapSizeX = 40;
        public int MapSizeX
        {
            get => this.mapSizeX;
            set
            {
                SetProperty<int>(ref this.mapSizeX, value);
            }
        }

        private int mapSizeY = 40;
        public int MapSizeY
        {
            get => this.mapSizeY;
            set
            {
                SetProperty<int>(ref this.mapSizeY, value);
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
                if (this.ImageViewerVM.IsROI == false)
                {
                    MessageBox.Show("ROI가 그려지지 않았습니다.");
                    return;
                }

                //CreateROI();
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
        #endregion


    }
}
