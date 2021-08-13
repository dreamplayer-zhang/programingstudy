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
using System.IO;

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

        public bool dragAction = false;
        CPoint startPos = new CPoint(-1, -1);
        CPoint prevPos = new CPoint(-1, -1);
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
                //GrabModeBase mode = ((WIND2_Handler)GlobalObjects.Instance.Get<WIND2_Engineer>().ClassHandler()).p_Vision.m_aGrabMode[value];
                //this.CamResolutionX = mode.m_dTargetResX_um;
                //this.CamResolutionY = mode.m_dTargetResY_um;
                //this.WaferSize = mode.m_nWaferSize_mm;

                //WIND2_Engineer engineer = GlobalObjects.Instance.Get<WIND2_Engineer>();
                //RecipeFront recipeFront = GlobalObjects.Instance.Get<RecipeFront>();

                //recipeFront.CameraInfoIndex = value;

                //CameraInfo camInfo = DataConverter.GrabModeToCameraInfo(engineer.m_handler.p_Vision.GetGrabMode(recipeFront.CameraInfoIndex));

                //this.CamInfoDataListVM.Init(camInfo);


                //SetProperty<int>(ref this.selectedGrabModeIndex, value);
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

        public ICommand btnToolClearMapCommand
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


        public ICommand btnToolCreateDefaultMapCommand
        {
            get
            {
                return new RelayCommand(CreateDefaultMap);
            }
        }

        public ICommand btnToolInvertCommand
        {
            get
            {
                return new RelayCommand(InvertMap);
            }
        }

        public ICommand btnToolHorizontalFlipCommand
        {
            get
            {
                return new RelayCommand(HorizontalFlipMap);
            }
        }

        public ICommand btnToolVerticalFlipCommand
        {
            get
            {
                return new RelayCommand(VerticalFlipMap);
            }
        }

        public ICommand btnToolFileImportCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();
                    dlg.InitialDirectory = Constants.RootPath.RecipeFrontRootPath;
                    dlg.Title = "Load File";
                    dlg.Filter = "ASC file (*.ASC)|*.ASC|MCTxt file (*.txt)|*.txt|CTMap (*.FAB)|*.FAB|ALPSMap (*.Alpsdata)|*.Alpsdata|Text file (*.txt)|*.txt|xml file (*.xml)|*.xml|dat file (*.dat)|*.dat|G85 Map (*.*)|*.*|TSK Map (*.*)|*.*|Klarf file (*.001,*.smf)|*.001;*.smf";
                    if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        int[] mapdata = new int[0];

                        string sFolderPath = System.IO.Path.GetDirectoryName(dlg.FileName);
                        string sFileNameNoExt = System.IO.Path.GetFileNameWithoutExtension(dlg.FileName);
                        string sFileName = System.IO.Path.GetFileName(dlg.FileName);
                        string sFullPath = System.IO.Path.Combine(sFolderPath, sFileName);

                        try
                        {
                            RecipeType_WaferMap waferMap = GlobalObjects.Instance.Get<RecipeFront>().WaferMap;
                            StreamReader sr = new StreamReader(sFullPath, Encoding.Default);
                            OpenMapDataMap(sr, dlg.FilterIndex);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                    }
                });
            }
        }

        public ICommand btnModeDrawCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (this.IsDrawChecked == true)
                    {
                        this.IsEraseChecked = false;
                    }
                    else
                    {
                        this.IsEraseChecked = true;

                    }
                });
            }
        }

        public ICommand btnModeEraseCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (this.IsEraseChecked == true)
                    {
                        this.IsDrawChecked = false;
                    }
                    else
                    {
                        this.IsDrawChecked = true;

                    }
                });
            }
        }

        public ICommand OpenMapCreatorCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    var viewModel = new Dialog_MapCreator_ViewModel();
                    Nullable<bool> result = GlobalObjects.Instance.Get<DialogService>().ShowDialog(viewModel);
                    DrawMap();
                });
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
            chipItems = new ObservableCollection<UIElement>();

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

            this.SelectedGrabModeIndex = recipeFront.CameraInfoIndex;
        }

        #region [Properties]
        private ObservableCollection<UIElement> chipItems;

        public ObservableCollection<UIElement> ChipItems
        {
            get => this.chipItems;
            set
            {
                SetProperty<ObservableCollection<UIElement>>(ref this.chipItems, value);
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

        private bool isDrawChecked = true;
        public bool IsDrawChecked
        {
            get => this.isDrawChecked;
            set
            {
                SetProperty<bool>(ref this.isDrawChecked, value);
            }
        }

        private bool isEraseChecked = false;
        public bool IsEraseChecked
        {
            get => this.isEraseChecked;
            set
            {
                SetProperty<bool>(ref this.isEraseChecked, value);
            }
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

        public void InvertMap()
        {
            InvertWaferMap();
            DrawMap();
        }
        public void HorizontalFlipMap()
        {
            HorizontalFlipWaferMap();
            DrawMap();
        }

        public void VerticalFlipMap()
        {
            VerticalFlipWaferMap();
            DrawMap();
        }

        public void OpenMapDataMap(StreamReader stdFile, int filterIndex)
        {
            if (filterIndex == 1) // Read ASC file
            {
                OpenASCMapDataWaferMap(stdFile);
                ConvertACSMapDataToWaferMap();
            }
            else if (filterIndex == 6) // Read XML file
            {
                OpenXmlMapDataWaferMap(stdFile);
            }
            else if (filterIndex == 10) // Read Klarf file
            {
                OpenKlarfMapDataWaferMap(stdFile);
            }
            DrawMap();
        }

        public void DrawMap()
        {
            ChipItems.Clear();

            RecipeType_WaferMap waferMap = GlobalObjects.Instance.Get<RecipeFront>().WaferMap;

            int sizeX = waferMap.MapSizeX;
            int sizeY = waferMap.MapSizeY;

            double chipWidth = (double)CanvasWidth / (double)(sizeX + 1); // add 1 for row index line
            double chipHeight = (double)CanvasHeight/ (double)(sizeY + 1); // add 1 for column index line

            Point originPt = new Point(0, 0);

            if (sizeX > 0 && sizeY > 0)
            {
                for (int y = -1; y < sizeY; y++) // start from -1 for column index line
                {
                    for (int x = -1; x < sizeX; x++) // start from -1 for row index line
                    {
                        Rectangle rect = new Rectangle();
                        rect.Width = chipWidth;
                        rect.Height = chipHeight;

                        TextBlock tb = new TextBlock();
                        tb.FontSize = (int)(0.7 * Math.Min(chipWidth, chipHeight));
                        tb.FontWeight = FontWeights.UltraBold;
                        tb.Width = chipWidth;
                        tb.Height = chipHeight;
                        tb.TextAlignment = TextAlignment.Center;
                        tb.Padding = new Thickness(0, (int)((chipHeight - tb.FontSize) / 2), 0, 0);

                        Canvas.SetLeft(rect, originPt.X + (chipWidth * (x + 1)));
                        Canvas.SetTop(rect, originPt.Y + (chipHeight * (y + 1)));

                        Canvas.SetLeft(tb, originPt.X + (chipWidth * (x + 1)));
                        Canvas.SetTop(tb, originPt.Y + (chipHeight * (y + 1)));

                        rect.Tag = new CPoint(x + 1, y + 1);
                        rect.Stroke = Brushes.Transparent;
                        rect.Opacity = 0.7;
                        rect.StrokeThickness = 2;

                        if (x == -1 || y == -1)
                        {
                            rect.Fill = Brushes.Blue;

                            if (x == -1 && y != -1)
                            {
                                tb.Text = string.Format("{0}", y + 1); // row index
                            }
                            else if (x != -1 && y == -1)
                            {
                                tb.Text = string.Format("{0}", x + 1); // column index
                            }
                        }
                        else
                        {
                            rect.ToolTip = string.Format("({0}, {1})", x, y); // chip index

                            if (waferMap.Data[y * sizeX + x] == (int)CHIP_TYPE.NO_CHIP)
                                rect.Fill = Brushes.DimGray;
                            else if (waferMap.Data[y * sizeX + x] == (int)CHIP_TYPE.NORMAL)
                                rect.Fill = Brushes.Green;
                            else if (waferMap.Data[y * sizeX + x] == (int)CHIP_TYPE.FLAT_ZONE)
                                rect.Fill = Brushes.Maroon;

                            rect.MouseLeftButtonDown += ChipMouseLeftButtonDown;
                            rect.MouseMove += ChipMouseMove;
                        }
                        Canvas.SetZIndex(rect, 98);
                        Canvas.SetZIndex(tb, 99);
                        ChipItems.Add(rect);
                        if (tb.Text != "")
                            ChipItems.Add(tb);
                    }
                }
            }
        }

        private void ChipMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Rectangle selected = (Rectangle)sender;
            startPos = (CPoint)selected.Tag;
            prevPos = startPos;
            //int stride = (int)m_MapData.PartialMapSize.Height;

            CHIP_TYPE type = GetRecipeChipType(startPos.X - 1, startPos.Y - 1);
            if (this.IsDrawChecked && type == CHIP_TYPE.NO_CHIP)
            {
                UpdateRecipeWaferMap(startPos.X - 1, startPos.Y - 1, CHIP_TYPE.NORMAL);
            }
            else if (this.IsEraseChecked && type != CHIP_TYPE.NO_CHIP)
            {
                UpdateRecipeWaferMap(startPos.X - 1, startPos.Y - 1, CHIP_TYPE.NO_CHIP);
            }

            selected.Fill = ChipTypeToBrush(GetRecipeChipType(startPos.X - 1, startPos.Y - 1));
        }

        private void ChipMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Rectangle selected = (Rectangle)sender;
                CPoint movingPos = (CPoint)selected.Tag;
                //int stride = (int)m_MapData.PartialMapSize.Height;

                if ((prevPos.X != -1 && prevPos.Y != -1) && (prevPos.X != movingPos.X || prevPos.Y != movingPos.Y))
                {
                    prevPos.X = movingPos.X;
                    prevPos.Y = movingPos.Y;

                    CHIP_TYPE type = GetRecipeChipType(movingPos.X - 1, movingPos.Y - 1);
                    if (this.IsDrawChecked && type == CHIP_TYPE.NO_CHIP)
                    {
                        UpdateRecipeWaferMap(startPos.X - 1, startPos.Y - 1, CHIP_TYPE.NORMAL);
                    }
                    else if (this.IsEraseChecked && type != CHIP_TYPE.NO_CHIP)
                    {
                        UpdateRecipeWaferMap(startPos.X - 1, startPos.Y - 1, CHIP_TYPE.NO_CHIP);
                    }

                    selected.Fill = ChipTypeToBrush(GetRecipeChipType(movingPos.X - 1, movingPos.Y - 1));
                }
            }
        }

        private void ChipMouseLeftButtonUp(object sender, MouseEventArgs e)
        {
            dragAction = false;
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

        private void InvertWaferMap()
        {
            RecipeType_WaferMap waferMap = GlobalObjects.Instance.Get<RecipeFront>().WaferMap;
            waferMap.Invert();
        }

        private void HorizontalFlipWaferMap()
        {
            RecipeType_WaferMap waferMap = GlobalObjects.Instance.Get<RecipeFront>().WaferMap;
            waferMap.HorizontalFlip();
        }

        private void VerticalFlipWaferMap()
        {
            RecipeType_WaferMap waferMap = GlobalObjects.Instance.Get<RecipeFront>().WaferMap;
            waferMap.VerticalFlip();
        }

        private void OpenASCMapDataWaferMap(StreamReader stdFile)
        {
            RecipeType_WaferMap waferMap = GlobalObjects.Instance.Get<RecipeFront>().WaferMap;
            waferMap.OpenASCMapData(stdFile);
        }

        private void ConvertACSMapDataToWaferMap()
        {
            RecipeType_WaferMap waferMap = GlobalObjects.Instance.Get<RecipeFront>().WaferMap;
            waferMap.ConvertACSMapDataToWaferMap();
        }

        private void OpenXmlMapDataWaferMap(StreamReader stdFile)
        {
            RecipeType_WaferMap waferMap = GlobalObjects.Instance.Get<RecipeFront>().WaferMap;
            waferMap.OpenXmlMapData(stdFile);
        }

        private void OpenKlarfMapDataWaferMap(StreamReader stdFile)
        {
            RecipeType_WaferMap waferMap = GlobalObjects.Instance.Get<RecipeFront>().WaferMap;
            waferMap.OpenKlarfMapData(stdFile);
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
                case CHIP_TYPE.FLAT_ZONE:
                    return Brushes.Maroon;
            }
            return Brushes.DimGray;
            #endregion
        }
    }
}