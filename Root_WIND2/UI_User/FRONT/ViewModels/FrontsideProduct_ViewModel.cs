using Root_WIND2.Module;
using RootTools;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Root_WIND2.UI_User
{
    class FrontsideProduct_ViewModel : ObservableObject, IPage
    {
        #region [Members]
        int nWaferSize = 300;
        int mapSizeX = 14;
        int mapSizeY = 14;
        int nOriginX = 10;
        int nOriginY = 10;
        //int nShotX = 1;
        //int nShotY = 1;
        //int nChipsizeX = 14;
        //int nChipsizeY = 14;

        // Shot Info
        //float fShotOriginX = 0F;
        //float fShotOriginY = 0F;
        //int nShotMatrixX = 1;
        //int nShotMatrixY = 1;
        //int nShotSizeX = 1;
        //int nShotSizeY = 1;
        #endregion

        #region [Get/Set]
        public int WaferSize { get => nWaferSize; set => nWaferSize = value; }
        public int MapSizeX
        {
            get => mapSizeX;
            set
            {
                SetProperty(ref mapSizeX, value);
            }
        }
        public int MapSizeY
        {
            get => mapSizeY;
            set
            {
                SetProperty(ref mapSizeY, value);
            }
        }
        public int OriginX
        {
            get => nOriginX;
            set
            {
                SetProperty(ref nOriginX, value);
            }
        }
        public int OriginY
        {
            get => nOriginY;
            set
            {
                SetProperty(ref nOriginY, value);
            }
        }
        public double CanvasWidth
        {
            get;
            set;
        }

        public double CanvasHeight
        {
            get;
            set;
        }

        private DataListView_ViewModel camInfoDataListVM;

        public DataListView_ViewModel CamInfoDataListVM
        {
            get => this.camInfoDataListVM;
            set
            {
                SetProperty(ref this.camInfoDataListVM, value);
            }
        }
        


        public List<string> GrabModeList
        {
            get
            {
                return ((WIND2_Handler)GlobalObjects.Instance.Get<WIND2_Engineer>().ClassHandler()).p_Vision.p_asGrabMode;
            } 
            set
            {

            }
        }

        private double camResolutionX;
        public double CamResolutionX
        {
            get => camResolutionX;
            set
            {
                SetProperty<double>(ref this.camResolutionX, value);
            }
        }

        private double camResolutionY;
        public double CamResolutionY
        {
            get => camResolutionY;
            set
            {
                SetProperty<double>(ref this.camResolutionY, value);
            }
        }

        private int selectedGrabModeIndex = 0;
        public int SelectedGrabModeIndex
        {
            get => this.selectedGrabModeIndex;
            set
            {
                GrabModeBase mode = ((WIND2_Handler)GlobalObjects.Instance.Get<WIND2_Engineer>().ClassHandler()).p_Vision.m_aGrabMode[value];
                this.CamResolutionX = mode.m_dTargetResX_um;
                this.CamResolutionY = mode.m_dTargetResY_um;
                this.WaferSize = mode.m_nWaferSize_mm;

                WIND2_Engineer engineer = GlobalObjects.Instance.Get<WIND2_Engineer>();
                RecipeFront recipeFront = GlobalObjects.Instance.Get<RecipeFront>();

                recipeFront.CameraInfoIndex = value;

                CameraInfo camInfo = DataConverter.GrabModeToCameraInfo(engineer.m_handler.p_Vision.GetGrabMode(recipeFront.CameraInfoIndex));

                this.CamInfoDataListVM.Init(camInfo);


                SetProperty<int>(ref this.selectedGrabModeIndex, value);
            }
        }

        #endregion

        #region Command Btn

        public ICommand LoadedCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    LoadRecipe();
                    DrawMap();
                });
            }
        }

        public ICommand CreateMapCommand
        {
            get
            {
                return new RelayCommand(()=>
                {
                    CreateWaferMap();
                });
            }
        }

        public ICommand ClearMapCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    ClearMap();
                });
            }
        }
        

        public ICommand SizeChangedCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    DrawMap();
                });
            }
        }


        public ICommand CreateDefaultMapCommand
        {
            get
            {
                return new RelayCommand(CreateDefaultMap);
            }
        }
        #endregion

        private int defaultMapSizeX = 14;
        private int defaultMapSizeY = 14;
        public int[] defaultWaferMap = new int[] // 14x14
        {
                0,0,0,0,0,1,1,1,1,0,0,0,0,0,//1 x+(y * width)
                0,0,0,1,1,1,1,1,1,1,1,0,0,0,//2
                0,0,1,1,1,1,1,1,1,1,1,1,0,0,//3
                0,1,1,1,1,1,1,1,1,1,1,1,1,0,//4
                0,1,1,1,1,1,1,1,1,1,1,1,1,0,//5
                1,1,1,1,1,1,1,1,1,1,1,1,1,1,//6
                1,1,1,1,1,1,1,1,1,1,1,1,1,1,//7
                1,1,1,1,1,1,1,1,1,1,1,1,1,1,//8
                1,1,1,1,1,1,1,1,1,1,1,1,1,1,//9
                0,1,1,1,1,1,1,1,1,1,1,1,1,0,//10
                0,1,1,1,1,1,1,1,1,1,1,1,1,0,//11
                0,0,1,1,1,1,1,1,1,1,1,1,0,0,//12
                0,0,0,1,1,1,1,1,1,1,1,0,0,0,//13
                0,0,0,0,0,1,1,1,1,0,0,0,0,0,//14
        };

        public FrontsideProduct_ViewModel()
        {
            chipItems = new ObservableCollection<Rectangle>();

            this.camInfoDataListVM = new DataListView_ViewModel();
        }

        public void LoadRecipe()
        {
            RecipeType_WaferMap wafermap = GlobalObjects.Instance.Get<RecipeFront>().WaferMap;

            CreateRecipeWaferMap(wafermap.MapSizeX, wafermap.MapSizeY, wafermap.Data);

            WIND2_Engineer engineer = GlobalObjects.Instance.Get<WIND2_Engineer>();
            RecipeFront recipeFront = GlobalObjects.Instance.Get<RecipeFront>();

            CameraInfo camInfo = DataConverter.GrabModeToCameraInfo(engineer.m_handler.p_Vision.GetGrabMode(recipeFront.CameraInfoIndex));

            this.CamInfoDataListVM.Init(camInfo);
        }

        #region [Properties]
        private ObservableCollection<Rectangle> chipItems;

        public ObservableCollection<Rectangle> ChipItems
        {
            get => this.chipItems;
            set
            {
                SetProperty<ObservableCollection<Rectangle>>(ref this.chipItems, value);
            }
        }

        public class Chip
        {
            public Chip(double x, double y, double width, double height)
            {
                this.X = x;
                this.Y = y;
                this.Width = width;
                this.Height = height;
            }

            public double X { get; set; }
            public double Y { get; set; }
            public double Width { get; set; }
            public double Height { get; set; }
        }

        #endregion


        #region [Method]
        public void CreateWaferMap()
        {
            CreateRecipeWaferMap(this.mapSizeX, this.mapSizeY, CHIP_TYPE.NORMAL);
            DrawMap();
        }

        public void CreateDefaultMap()
        {
            CreateRecipeWaferMap(defaultMapSizeX, defaultMapSizeY, defaultWaferMap);
            DrawMap();
        }

        public void ClearMap()
        {
            ClearRecipeWaferMap();
            DrawMap();
        }

        public void DrawMap()
        {
            ChipItems.Clear();

            RecipeType_WaferMap waferMap = GlobalObjects.Instance.Get<RecipeFront>().WaferMap;

            int sizeX = waferMap.MapSizeX;
            int sizeY = waferMap.MapSizeY;

            double chipWidth = (double)CanvasWidth / (double)sizeX;
            double chipHeight = (double)CanvasHeight/ (double)sizeY;

            Point originPt = new Point(0, 0);

            for (int y = 0; y < sizeY; y++)
            {
                for (int x = 0; x < sizeX; x++)
                {
                    Rectangle rect = new Rectangle();
                    rect.Width = chipWidth;
                    rect.Height = chipHeight;
                    Canvas.SetLeft(rect, originPt.X + (chipWidth * x));
                    Canvas.SetTop(rect, originPt.Y + (chipHeight * y));
 
                    rect.Tag = new CPoint(x, y);
                    rect.ToolTip = string.Format("({0}, {1})", x, y); // chip index
                    rect.Stroke = Brushes.Transparent;
                    rect.Fill = Brushes.Green;
                    rect.Opacity = 0.7;
                    rect.StrokeThickness = 2;

                    if (waferMap.Data[y * sizeX + x] == 0) // No Chip
                        rect.Fill = Brushes.DimGray;
                    else
                        rect.Fill = Brushes.Green;

                    Canvas.SetZIndex(rect, 99);
                    rect.MouseLeftButtonDown += ChipMouseLeftButtonDown;

                    ChipItems.Add(rect);
                }
            }
        }


        private void ChipMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Rectangle selected = (Rectangle)sender;
            CPoint pos = (CPoint)selected.Tag;
            //int stride = (int)m_MapData.PartialMapSize.Height;

            CHIP_TYPE type = GetRecipeChipType(pos.X, pos.Y);
            switch (type)
            {
                case CHIP_TYPE.NO_CHIP:
                    UpdateRecipeWaferMap(pos.X, pos.Y, CHIP_TYPE.NORMAL);
                    break;
                case CHIP_TYPE.NORMAL:
                    UpdateRecipeWaferMap(pos.X, pos.Y, CHIP_TYPE.NO_CHIP);
                    break;
            }

            selected.Fill = ChipTypeToBrush(GetRecipeChipType(pos.X, pos.Y));
        }

        private void CreateRecipeWaferMap(int mapSizeX, int mapSizeY, CHIP_TYPE type)
        {
            RecipeType_WaferMap waferMap = GlobalObjects.Instance.Get<RecipeFront>().WaferMap;
            waferMap.CreateWaferMap(mapSizeX, mapSizeY, type);
        }

        private void CreateRecipeWaferMap(int mapSizeX, int mapSizeY, int[] mapdata)
        {
            RecipeType_WaferMap waferMap = GlobalObjects.Instance.Get<RecipeFront>().WaferMap;
            waferMap.CreateWaferMap(mapSizeX, mapSizeY, mapdata);
        }

        private void ClearRecipeWaferMap()
        {
            RecipeType_WaferMap waferMap = GlobalObjects.Instance.Get<RecipeFront>().WaferMap;
            waferMap.Clear();
        }

        private void UpdateRecipeWaferMap(int x, int y, CHIP_TYPE type)
        {
            RecipeType_WaferMap waferMap = GlobalObjects.Instance.Get<RecipeFront>().WaferMap;

            waferMap.Data[y * waferMap.MapSizeX + x] = (int)type;
        }

        private CHIP_TYPE GetRecipeChipType(int x, int y)
        {
            RecipeType_WaferMap waferMap = GlobalObjects.Instance.Get<RecipeFront>().WaferMap;

            return (CHIP_TYPE)waferMap.Data[y * waferMap.MapSizeX + x];
        }

        private SolidColorBrush ChipTypeToBrush(CHIP_TYPE type)
        {
            switch (type)
            {
                case CHIP_TYPE.NO_CHIP:
                    return Brushes.DimGray;
                case CHIP_TYPE.NORMAL:
                    return Brushes.Green;
            }
            return Brushes.DimGray;
            #endregion
        }
    }
}