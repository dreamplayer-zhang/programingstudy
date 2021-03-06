using RootTools;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Root_WIND2
{
    class FrontsideSummary_ViewModel : RootViewer_ViewModel, IRecipeUILoadable
    {
        public void init(Setup_ViewModel _setup)
        {
            this.setup = _setup;

            MapControl_VM = new MapControl_ViewModel();
            MapControl_VM.SetMap();
            
            //timer.Interval = TimeSpan.FromSeconds(1);    
            //timer.Tick += new EventHandler(DateTimeUpdate);         
            //timer.Start();               
        }

        public void ConnectInspItemDataGrid(FrontsideSpec_ViewModel _spec)
        {
            this.inspItem = _spec.p_cInspItem;
        }

        #region [Member Variables]
        Setup_ViewModel setup;

        DispatcherTimer timer = new DispatcherTimer();
        ImageData masterImageData;
        #endregion

        #region GET/SET
        private string dateTime;
        public string CurDateTime
        {
            get
            {
                return dateTime;
            }
            set
            {
                SetProperty(ref dateTime, value);
            }
        }
        // Map Info
        private int mapSzX = 0;
        public int MapSzX
        {
            get
            {
                return mapSzX;
            }
            set
            {
                SetProperty(ref mapSzX, value);
            }
        }
        private int mapSzY = 0;
        public int MapSzY
        {
            get => mapSzY;
            set
            {
                SetProperty(ref mapSzY, value);
            }
        }

        private int originDieX = 0;
        public int OriginDieX
        {
            get => originDieX;
            set
            {
                SetProperty(ref originDieX, value);
            }
        }
        private int originDieY = 0;
        public int OriginDieY
        {
            get => originDieY;
            set
            {
                SetProperty(ref originDieY, value);
            }
        }

        private double chipSizeX = 0;
        public double ChipSizeX
        {
            get => chipSizeX;
            set
            {
                SetProperty(ref chipSizeX, value);
            }
        }
        private double chipSizeY = 0;
        public double ChipSizeY
        {
            get => chipSizeY;
            set
            {
                SetProperty(ref chipSizeY, value);
            }
        }

        // Shot Info
        private int shotOriginX = 0;
        public int ShotOriginX
        {
            get => shotOriginX;
            set
            {
                SetProperty(ref shotOriginX, value);
            }
        }

        private int shotOriginY = 0;
        public int ShotOriginY
        {
            get => shotOriginY;
            set
            {
                SetProperty(ref shotOriginY, value);
            }
        }

        private int shotMatrixX = 0;
        public int ShotMatrixX
        {
            get => shotMatrixX;
            set
            {
                SetProperty(ref shotMatrixX, value);
            }
        }

        private int shotMatrixY = 0;
        public int ShotMatrixY
        {
            get => shotMatrixY;
            set
            {
                SetProperty(ref shotMatrixY, value);
            }
        }

        private double shotSizeX = 0;
        public double ShotSizeX
        {
            get => shotSizeX;
            set
            {
                SetProperty(ref shotSizeX, value);
            }
        }

        private double shotSizeY = 0;
        public double ShotSizeY
        {
            get => shotSizeY;
            set
            {
                SetProperty(ref shotSizeY, value);
            }
        }

        public bool Check_DisplayNone
        {
            get
            {
                return displayOption == DisplayOption.None;
            }
            set
            {
                displayOption = value ? DisplayOption.None : displayOption;

                if (masterImageData == null) return;

                if (displayOption == DisplayOption.None)
                {
                    
                    OriginRecipe originRecipe = GlobalObjects.Instance.Get<RecipeFront>().GetItem<OriginRecipe>();
                    Dispatcher.CurrentDispatcher.BeginInvoke(new ThreadStart(() =>
                    {
                        MasterImg = masterImageData.GetBitMapSource(originRecipe.MasterImage.ByteCnt);
                    }));
                }
            }

        }
        public bool Check_DisplayPosition
        {
            get
            {
                return displayOption == DisplayOption.Position;
            }
            set
            {
                displayOption = value ? DisplayOption.Position : displayOption;
                if (displayOption == DisplayOption.Position)
                {
                }
            }
        }
        public bool Check_DisplayROI
        {
            get
            {
                return displayOption == DisplayOption.ROI;
            }
            set
            {
                displayOption = value ? DisplayOption.ROI : displayOption;
                if (displayOption == DisplayOption.ROI)
                {
                }
            }
        }

        private MapControl_ViewModel mapControl_VM;
        public MapControl_ViewModel MapControl_VM
        {
            get
            {
                return mapControl_VM;
            }
            set
            {
                SetProperty(ref mapControl_VM, value);
            }
        }
 
        private ObservableCollection<InspectionItem> inspItem;
        public ObservableCollection<InspectionItem> InspItem
        {
            get
            {
                return inspItem;
            }
            set
            {
                SetProperty(ref inspItem, value);
            }
        }

        private BitmapSource masterImg;
        public BitmapSource MasterImg
        {
            get
            {
                return masterImg;
            }
            set
            {
                SetProperty(ref masterImg, value);
            }
        }
        #endregion

        #region DataTypeEnum
        private DisplayOption displayOption = DisplayOption.None;
        private enum DisplayOption
        {
            None,
            Position,
            ROI,
        }
        #endregion

        private void DateTimeUpdate(object sender, EventArgs e)
        {
            CurDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
        private void DrawMapData()
        {

            RecipeType_WaferMap mapdata = GlobalObjects.Instance.Get<RecipeFront>().WaferMap;
            if (mapdata.Data.Length > 0)
            {
                int nMapX = mapdata.MapSizeX;
                int nMapY = mapdata.MapSizeY;

                MapControl_VM.SetMap(false, new CPoint(mapdata.MasterDieX, mapdata.MasterDieY));
            }
            else
            {
                MapControl_VM.SetMap(false, new CPoint(0, 0));
            }
        }
        private void SetMapData()
        {
            RecipeType_WaferMap waferMap = GlobalObjects.Instance.Get<RecipeFront>().WaferMap;
            MapSzX = waferMap.MapSizeX;
            MapSzY = waferMap.MapSizeY;

            OriginDieX = waferMap.MasterDieX;
            OriginDieY = waferMap.MasterDieY;

            //ChipSizeX = recipe.WaferMap.;
            //ChipSizeY = recipe.WaferMap.;

            //ShotOriginX = recipe.WaferMap.;
            //ShotOriginY = recipe.WaferMap.;

            //ShotMatrixX = recipe.WaferMap.;
            //ShotMatrixY = recipe.WaferMap.;

            //ShotSizeX = recipe.WaferMap.;
            //ShotSizeY = recipe.WaferMap.;
        }
        public void SetPage()
        {
            SetMapData();
            DrawMapData();

            //LoadMasterImage();
        }
        public void LoadMasterImage()
        {

            GlobalObjects.Instance.Get<RecipeFront>().LoadMasterImage();

            OriginRecipe originRecipe = GlobalObjects.Instance.Get<RecipeFront>().GetItem<OriginRecipe>();
            if (originRecipe.MasterImage != null)
            {
                masterImageData = new ImageData(originRecipe.MasterImage.Width, originRecipe.MasterImage.Height, originRecipe.MasterImage.ByteCnt);

                if(masterImageData != null)
                {
                    masterImageData.m_eMode = ImageData.eMode.ImageBuffer;
                    masterImageData.SetData(Marshal.UnsafeAddrOfPinnedArrayElement(originRecipe.MasterImage.RawData, 0)
                        , new CRect(0, 0, originRecipe.MasterImage.Width, originRecipe.MasterImage.Height)
                        , originRecipe.MasterImage.Width, originRecipe.MasterImage.ByteCnt);

                    Dispatcher.CurrentDispatcher.BeginInvoke(new ThreadStart(() =>
                    {
                        MasterImg = masterImageData.GetBitMapSource(originRecipe.MasterImage.ByteCnt);
                    }));
                }
            }
        }
        public void LoadSummaryData()
        {
            SetMapData();
            DrawMapData();
            //LoadMasterImage();
        }

        public void Load()
        {
            LoadSummaryData();
        }
    }
}
