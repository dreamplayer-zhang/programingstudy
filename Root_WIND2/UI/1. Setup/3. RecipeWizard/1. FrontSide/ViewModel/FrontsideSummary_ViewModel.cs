using RootTools;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace Root_WIND2
{
    class FrontsideSummary_ViewModel : ObservableObject
    {
        public void init(Setup_ViewModel _setup, Recipe _recipe)
        {
            this.setup = _setup;
            this.recipe = _recipe;

            MapControl_VM = new MapControl_ViewModel(this.setup.InspectionVision);
            MapControl_VM.SetMap(setup.InspectionVision.mapdata, new CPoint(14, 14));
            
            timer.Interval = TimeSpan.FromSeconds(1);    
            timer.Tick += new EventHandler(DateTimeUpdate);         
            timer.Start();               
        }

        public void ConnectInspItemDataGrid(FrontsideSpec_ViewModel _spec)
        {
            this.inspItem = _spec.p_cInspItem;
        }

        #region [Member Variables]
        Setup_ViewModel setup;
        Recipe recipe;
        DispatcherTimer timer = new DispatcherTimer();    //객체생성
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
        #endregion

        private void DateTimeUpdate(object sender, EventArgs e)
        {
            CurDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
        private void DrawMapData()
        {
            RecipeType_WaferMap mapdata = recipe.WaferMap;
            if (mapdata.Data != null)
            {
                int nMapX = mapdata.MapSizeX;
                int nMapY = mapdata.MapSizeY;

                MapControl_VM.SetMap(Brushes.Green, mapdata.Data, new CPoint(nMapX, nMapY));
            }
            else
            {
                MapControl_VM.SetMap(Brushes.Green, setup.InspectionVision.mapdata, new CPoint(14, 14));
            }
        }
        private void SetMapData()
        {
            MapSzX = recipe.WaferMap.MapSizeX;
            MapSzY = recipe.WaferMap.MapSizeY;

            OriginDieX = recipe.WaferMap.MasterDieX;
            OriginDieY = recipe.WaferMap.MasterDieY;

            //ChipSizeX = recipe.WaferMap.;
            //ChipSizeY = recipe.WaferMap.;

            //ShotOriginX = recipe.WaferMap.;
            //ShotOriginY = recipe.WaferMap.;

            //ShotMatrixX = recipe.WaferMap.;
            //ShotMatrixY = recipe.WaferMap.;

            //ShotSizeX = recipe.WaferMap.;
            //ShotSizeY = recipe.WaferMap.;
        }

        public void LoadSummaryData()
        {
            DrawMapData();
            SetMapData();
        }
    }
}
