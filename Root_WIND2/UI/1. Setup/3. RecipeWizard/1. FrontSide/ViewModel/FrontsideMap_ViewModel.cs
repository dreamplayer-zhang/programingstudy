//using System;
//using System.Collections.Generic;
//using System.Drawing;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows.Controls;
//using System.Windows.Shapes;

using Microsoft.Xaml.Behaviors;
using RootTools;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;


namespace Root_WIND2
{
    public class FrontsideMap_ViewModel : ObservableObject, IRecipeUILoadable
    {
        private Canvas canvas;
        int nWaferSize = 300;
        int mapSizeX = 14;
        int mapSizeY = 14;
        int nOriginX = 10;
        int nOriginY = 10;
        int nShotX = 1;
        int nShotY = 1;
        int nChipsizeX = 14;
        int nChipsizeY = 14;

        int[] mapdata;

        // Shot Info
        float fShotOriginX = 0F;
        float fShotOriginY = 0F;
        int nShotMatrixX = 1;
        int nShotMatrixY = 1;
        int nShotSizeX = 1;
        int nShotSizeY = 1;

        Recipe recipe;
        //MapData m_WaferMap;
        #region GET/SET
        public Canvas myCanvas { get => canvas; set => canvas = value; }
        public int WaferSize { get => nWaferSize; set => nWaferSize = value; }
        public int MapSizeX 
        { 
            get => mapSizeX; 
            set
            {
                SetProperty(ref mapSizeX, value);
                recipe.WaferMap.MapSizeX = mapSizeX;
            }
        }
        public int MapSizeY
        {
            get => mapSizeY;
            set
            {
                SetProperty(ref mapSizeY, value);
                recipe.WaferMap.MapSizeY = mapSizeY;
            }
        }
        public int OriginX 
        { 
            get => nOriginX;
            set
            {
                SetProperty(ref nOriginX, value);
                recipe.WaferMap.MasterDieX = nOriginX;
            }
        }
        public int OriginY
        {
            get => nOriginY;
            set
            {
                SetProperty(ref nOriginY, value);
                recipe.WaferMap.MasterDieY = nOriginY;
            }
        }
        public int ShotX { get => nShotX; set => nShotX = value; }
        public int ShotY { get => nShotY; set => nShotY = value; }
        public int ChipsizeX
        {
            get => nChipsizeX;
            set
            {
                SetProperty(ref nChipsizeX, value);
            }
        }
        public int ChipsizeY
        {
            get => nChipsizeY;
            set
            {
                SetProperty(ref nChipsizeY, value);
            }
        }

        public float pShotOriginX { get => fShotOriginX; set => fShotOriginX = value; }
        public float pShotOriginY { get => fShotOriginY; set => fShotOriginY = value; }
        public int pShotMatrixX { get => nShotMatrixX; set => nShotMatrixX = value; }
        public int pShotMatrixY { get => nShotMatrixY; set => nShotMatrixY = value; }
        public int pShotSizeX { get => nShotSizeX; set => nShotSizeX = value; }
        public int pShotSizeY { get => nShotSizeY; set => nShotSizeY = value; }

        #endregion

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

        #region Command Btn
        public ICommand CreateMap
        {
            get
            {
                return new RelayCommand(CreateWaferMap);
            }
        }
        public ICommand MapSave
        {
            get
            {
                return new RelayCommand(SetMapData);
            }
        }

        public ICommand CreateMap_Shot
        {
            get
            {
                return new RelayCommand(CreateWaferMap_Shot);
            }
        }

        public ICommand MapLoad
        {
            get
            {
                return new RelayCommand(LoadMapData);
            }
        }

        public ICommand DefaultMap
        {
            get
            {
                return new RelayCommand(SetDefaultMap);
            }
        }
        #endregion


        public FrontsideMap_ViewModel()
        {
        }

        public void Init(Setup_ViewModel setup, FrontSideMap map, Recipe _recipe)
        {
            this.canvas = map.myCanvas;
            this.recipe = _recipe;

            this.MapSizeX = this.recipe.WaferMap.MapSizeX;
            this.MapSizeY = this.recipe.WaferMap.MapSizeY;
            this.OriginX = this.recipe.WaferMap.MasterDieX;
            this.OriginY = this.recipe.WaferMap.MasterDieY;
            this.mapdata = new int[this.MapSizeX * this.MapSizeY];
            DrawMaps();
        }

        public void CreateWaferMap()
        {
            this.mapdata = new int[this.mapSizeX * this.mapSizeY];
            for (int i = 0; i < this.mapSizeX * this.mapSizeY; i++)
                this.mapdata[i] = (int)CHIP_TYPE.NORMAL;

            DrawMaps();
            //DrawWaferCircle();
        }

        public void CreateWaferMap_Shot()
        {

            DrawMaps_Shot();
            //DrawWaferCircle();
        }

        public void SetDefaultMap()
        {
            myCanvas.Children.Clear(); // 초기화

            this.mapdata = (int[])defaultWaferMap.Clone();

            this.MapSizeX = 14;
            this.MapSizeY = 14;

            this.OriginX = 0;
            this.OriginY = 5;

            int waferSize = nWaferSize;
            int r = waferSize / 2;

            double dChipX = (double)myCanvas.Width / (double)mapSizeX;
            double dChipY = (double)myCanvas.Height / (double)mapSizeY;
            int nChip_Left = 0;
            int nChip_Top = 0;
            int nChip_Right = mapSizeX;
            int nChip_Bottom = mapSizeY;

            Size chipSize = new Size(dChipX, dChipY);
            Point originPt = new Point(0, 0); // ???

            for(int y = 0; y < this.MapSizeY; y++)
            {
                for(int x = 0; x < this.MapSizeX; x++)
                {
                    Rectangle crect = new Rectangle();
                    crect.Width = chipSize.Width;
                    crect.Height = chipSize.Height;
                    Canvas.SetLeft(crect, originPt.X - (nChip_Left * chipSize.Width) + (chipSize.Width * x));
                    Canvas.SetRight(crect, originPt.X - (nChip_Left * chipSize.Width) + (chipSize.Width * x) + chipSize.Width);
                    Canvas.SetTop(crect, originPt.Y - (nChip_Top * chipSize.Height) + (chipSize.Height * y));
                    Canvas.SetBottom(crect, originPt.Y - (nChip_Top * chipSize.Height) + (chipSize.Height * y) + chipSize.Height);

                    crect.Tag = new CPoint(x, y);
                    crect.ToolTip = string.Format("({0}, {1})", x, y); // chip index
                    crect.Stroke = Brushes.Transparent;
                    crect.Fill = Brushes.DimGray;
                    crect.Opacity = 0.7;
                    crect.StrokeThickness = 2;

                    if (this.mapdata[y * this.MapSizeX + x] == 0) // No Chip
                        crect.Fill = Brushes.DimGray;
                    else
                        crect.Fill = Brushes.Green;

                    Canvas.SetZIndex(crect, 99);

                    myCanvas.Children.Add(crect);
                    crect.MouseLeftButtonDown += crect_MouseLeftButtonDown;
                }
            }
            SetMapData();
        }



        public void SetMapData()
        {
            this.recipe.WaferMap.CreateWaferMap(MapSizeX, MapSizeY, mapdata);
        }

        public void LoadMapData()
        {     
            RecipeType_WaferMap waferMap = recipe.WaferMap;

            if(waferMap.Data != null)
            {
                this.MapSizeX = waferMap.MapSizeX;
                this.MapSizeY = waferMap.MapSizeY;

                this.OriginX = waferMap.MasterDieX;
                this.OriginY = waferMap.MasterDieY;
                /////////
                myCanvas.Children.Clear(); // 초기화
                int waferSize = nWaferSize;
                int r = waferSize / 2;

                double dChipX = (double)myCanvas.Width / (double)mapSizeX;
                double dChipY = (double)myCanvas.Height / (double)mapSizeY;

                int nChip_Left = 0;
                int nChip_Top = 0;
                int nChip_Right = mapSizeX;
                int nChip_Bottom = mapSizeY;

                mapdata = new int[mapSizeX * mapSizeY];

                Size chipSize = new Size(dChipX, dChipY);
                Point originPt = new Point(0, 0); // ???

                for (int y = 0; y < this.MapSizeY; y++)
                {
                    for (int x = 0; x < this.MapSizeX; x++)
                    {
                        Rectangle crect = new Rectangle();
                        crect.Width = chipSize.Width;
                        crect.Height = chipSize.Height;
                        Canvas.SetLeft(crect, originPt.X - (nChip_Left * chipSize.Width) + (chipSize.Width * x));
                        Canvas.SetRight(crect, originPt.X - (nChip_Left * chipSize.Width) + (chipSize.Width * x) + chipSize.Width);
                        Canvas.SetTop(crect, originPt.Y - (nChip_Top * chipSize.Height) + (chipSize.Height * y));
                        Canvas.SetBottom(crect, originPt.Y - (nChip_Top * chipSize.Height) + (chipSize.Height * y) + chipSize.Height);

                        crect.Tag = new CPoint(x, y);
                        crect.ToolTip = string.Format("({0}, {1})", x, y); // chip index
                        crect.Stroke = Brushes.Transparent;
                        crect.Fill = Brushes.DimGray;
                        crect.Opacity = 0.7;
                        crect.StrokeThickness = 2;

                        mapdata[y * MapSizeX + x] = (int)waferMap.GetChipType(x, y);
                        if (waferMap.GetChipType(x, y) == CHIP_TYPE.NO_CHIP) // No Chip
                            crect.Fill = Brushes.DimGray;
                        else
                            crect.Fill = Brushes.Green;

                        Canvas.SetZIndex(crect, 99);

                        myCanvas.Children.Add(crect);
                        crect.MouseLeftButtonDown += crect_MouseLeftButtonDown;
                    }
                }
            }
        }

        public void DrawPlain()
        {
            int waferSize = 80;
            Polygon notch = new Polygon();
            PointCollection pc = new PointCollection();
            pc.Add(new Point((waferSize / 2) - 3, waferSize));
            pc.Add(new Point(waferSize / 2, waferSize - 10));
            pc.Add(new Point((waferSize / 2) + 3, waferSize));
            pc.Add(new Point((waferSize / 2) - 3, waferSize));
            notch.Points = pc;
            notch.Fill = Brushes.SteelBlue;

            Line vertical = new Line();
            vertical.Y1 = -50;
            vertical.Y2 = 350;
            vertical.Stroke = Brushes.Tomato;
            vertical.StrokeThickness = 1;
            vertical.StrokeDashArray = new DoubleCollection { 3, 2 };
            Canvas.SetLeft(vertical, 150);
            Canvas.SetZIndex(vertical, 99);

            Line horizon = new Line();
            horizon.X1 = -50;
            horizon.X2 = 350;
            horizon.Stroke = Brushes.Tomato;
            horizon.StrokeThickness = 1;
            horizon.StrokeDashArray = new DoubleCollection { 3, 2 };
            Canvas.SetTop(horizon, 150);
            Canvas.SetZIndex(horizon, 99);

            myCanvas.Children.Add(horizon);
            myCanvas.Children.Add(vertical);
        }

        public void DrawWaferCircle()
        {
            int waferSize = nWaferSize;
            Ellipse wafer = new Ellipse();
            wafer.Width = waferSize;
            wafer.Height = waferSize;
            wafer.Stroke = Brushes.Black;
            wafer.StrokeThickness = 0.5;
            wafer.Opacity = 0.5;
            Canvas.SetLeft(wafer, 0);
            Canvas.SetTop(wafer, 0);
            myCanvas.Children.Add(wafer);
        }

        public void DrawMaps()
        {
            myCanvas.Children.Clear(); // 초기화
            int waferSize = nWaferSize;
            int r = waferSize / 2;

            double dChipX = (double)myCanvas.Width / (double)mapSizeX;
            double dChipY = (double)myCanvas.Height / (double)mapSizeY;

            int nChip_Left = 0;
            int nChip_Top = 0;
            int nChip_Right = mapSizeX;
            int nChip_Bottom = mapSizeY;

            Size chipSize = new Size(dChipX, dChipY);
            Point originPt = new Point(0, 0); 

            for (int y = 0; y < this.MapSizeY; y++)
            {
                for (int x = 0; x < this.MapSizeX; x++)
                {
                    Rectangle crect = new Rectangle();
                    crect.Width = chipSize.Width;
                    crect.Height = chipSize.Height;
                    Canvas.SetLeft(crect, originPt.X - (nChip_Left * chipSize.Width) + (chipSize.Width * x));
                    Canvas.SetRight(crect, originPt.X - (nChip_Left * chipSize.Width) + (chipSize.Width * x) + chipSize.Width);
                    Canvas.SetTop(crect, originPt.Y - (nChip_Top * chipSize.Height) + (chipSize.Height * y));
                    Canvas.SetBottom(crect, originPt.Y - (nChip_Top * chipSize.Height) + (chipSize.Height * y) + chipSize.Height);

                    crect.Tag = new CPoint(x, y);
                    crect.ToolTip = string.Format("({0}, {1})", x, y); // chip index
                    crect.Stroke = Brushes.Transparent;
                    crect.Fill = Brushes.DimGray;
                    crect.Opacity = 0.7;
                    crect.StrokeThickness = 2;

                    if (this.mapdata[y * this.MapSizeX + x] == 0) // No Chip
                        crect.Fill = Brushes.DimGray;
                    else
                        crect.Fill = Brushes.Green;

                    Canvas.SetZIndex(crect, 99);

                    myCanvas.Children.Add(crect);
                    crect.MouseLeftButtonDown += crect_MouseLeftButtonDown;
                }
            }

            SetMapData();
        }

        public void DrawMaps_Shot()
        {
            myCanvas.Children.Clear(); // 초기화

            float wafer_size = (float)nWaferSize * 1000; // [mm] -> [um]

            float wafer_center_x = wafer_size / 2;
            float wafer_center_y = wafer_size / 2;

            float radius = wafer_size / 2;

            float shot_origin_x = wafer_center_x - fShotOriginX;   // Shot Origin 좌하단 기준
            float shot_origin_y = wafer_center_y + fShotOriginY;

            float die_size_x = (float)nShotSizeX / (float)nShotMatrixX;
            float die_size_y = (float)nShotSizeX / (float)nShotMatrixY;

            int left_die_count_x = (int)(shot_origin_x / die_size_x);
            int right_die_count_x = (int)((wafer_size - shot_origin_x) / die_size_x);

            int top_die_count_y = (int)(shot_origin_y / die_size_y);
            int bottom_die_count_y = (int)((wafer_size - shot_origin_y) / die_size_y);


            left_die_count_x = shot_origin_x % die_size_x == 0 ? (left_die_count_x - 1) : left_die_count_x;
            right_die_count_x = (wafer_size - shot_origin_x) % die_size_x == 0 ? (right_die_count_x - 1) : right_die_count_x;

            top_die_count_y = shot_origin_y % die_size_y == 0 ? (top_die_count_y - 1) : top_die_count_y;
            bottom_die_count_y = (wafer_size - shot_origin_y) % die_size_y == 0 ? (bottom_die_count_y - 1) : bottom_die_count_y;

            float start_die_position_x = shot_origin_x - die_size_x * left_die_count_x;
            float start_die_position_y = shot_origin_y - die_size_y * top_die_count_y;

            int die_count_x = left_die_count_x + right_die_count_x;
            int die_count_y = top_die_count_y + bottom_die_count_y;

            float ratio_wafer_to_canvas_x = (float)myCanvas.Width / wafer_size;
            float ratio_wafer_to_canvas_y = (float)myCanvas.Height / wafer_size;

            int canvas_die_size_x = (int)(die_size_x * ratio_wafer_to_canvas_x);
            int canvas_die_size_y = (int)(die_size_y * ratio_wafer_to_canvas_y);


            // Draw Circle
            Ellipse wafer_circle = new Ellipse();
            wafer_circle.Width = wafer_size * ratio_wafer_to_canvas_x;
            wafer_circle.Height = wafer_size * ratio_wafer_to_canvas_y;
            Canvas.SetLeft(wafer_circle, 0);
            Canvas.SetRight(wafer_circle, wafer_size * ratio_wafer_to_canvas_x);
            Canvas.SetTop(wafer_circle, 0);
            Canvas.SetBottom(wafer_circle, wafer_size * ratio_wafer_to_canvas_y);

            wafer_circle.Stroke = Brushes.Black;
            wafer_circle.Fill = Brushes.Transparent;
            wafer_circle.StrokeThickness = 2;

            Line wafer_center_line_x = new Line();
            wafer_center_line_x.X1 = wafer_center_x * ratio_wafer_to_canvas_x;
            wafer_center_line_x.Y1 = 0;
            wafer_center_line_x.X2 = wafer_center_x * ratio_wafer_to_canvas_x;
            wafer_center_line_x.Y2 = wafer_size * ratio_wafer_to_canvas_y;

            wafer_center_line_x.Stroke = Brushes.Orange;
            wafer_center_line_x.StrokeDashArray = new DoubleCollection(2) { 5, 5 };
            wafer_center_line_x.StrokeThickness = 1;

            Line wafer_center_line_y = new Line();
            wafer_center_line_y.X1 = 0;
            wafer_center_line_y.Y1 = wafer_center_y * ratio_wafer_to_canvas_y;
            wafer_center_line_y.X2 = wafer_size * ratio_wafer_to_canvas_x;
            wafer_center_line_y.Y2 = wafer_center_y * ratio_wafer_to_canvas_y;

            wafer_center_line_y.Stroke = Brushes.Orange;
            wafer_center_line_y.StrokeDashArray = new DoubleCollection(2) { 5, 5 };
            wafer_center_line_y.StrokeThickness = 1;
            Canvas.SetZIndex(wafer_center_line_x, 100);
            Canvas.SetZIndex(wafer_center_line_y, 100);
            myCanvas.Children.Add(wafer_circle);

            myCanvas.Children.Add(wafer_center_line_x);
            myCanvas.Children.Add(wafer_center_line_y);

            for (int y = 0; y < die_count_y; y++)
            {
                for(int x = 0; x < die_count_x; x++)
                {
                    Rectangle canvas_rect = new Rectangle();
                    canvas_rect.Width = canvas_die_size_x;
                    canvas_rect.Height = canvas_die_size_y;

                    float left = start_die_position_x + (die_size_x * x) - wafer_center_x;
                    float right = start_die_position_x + (die_size_x * (x + 1)) - wafer_center_x;
                    float top = start_die_position_y + (die_size_y * y) - wafer_center_y;
                    float bottom = start_die_position_y + (die_size_y * (y + 1)) - wafer_center_y;

                    if((left * left) + (top * top) > (radius * radius)  ||                        
                        (right * right) + (top * top) > (radius * radius) ||
                        (left * left) + (bottom * bottom) > (radius * radius)||
                        (right * right) + (bottom * bottom) > (radius * radius))
                    {
                        //this.recipe.WaferMap.SetChipType(x, y, CHIP_TYPE.NO_CHIP);
                        this.mapdata[x + y * mapSizeX] = (int)CHIP_TYPE.NO_CHIP;
                        continue;
                    }


                    Canvas.SetLeft(canvas_rect, (left + wafer_center_x) * ratio_wafer_to_canvas_x);
                    Canvas.SetRight(canvas_rect, (right+ wafer_center_x) * ratio_wafer_to_canvas_x);
                    Canvas.SetTop(canvas_rect, (top + wafer_center_y) * ratio_wafer_to_canvas_y);
                    Canvas.SetBottom(canvas_rect, (bottom + wafer_center_y) * ratio_wafer_to_canvas_y);

                    canvas_rect.Tag = new CPoint(x, y);
                    canvas_rect.ToolTip = string.Format("({0}, {1})", x, y); // chip index
                    canvas_rect.Stroke = Brushes.Transparent;
                    canvas_rect.Fill = Brushes.Green;
                    canvas_rect.Opacity = 0.7;
                    canvas_rect.StrokeThickness = 2;
                    Canvas.SetZIndex(canvas_rect, 99);

                    myCanvas.Children.Add(canvas_rect);
                    canvas_rect.MouseLeftButtonDown += crect_MouseLeftButtonDown;
                }
            }

            // Set MapInfo

            MapSizeX = die_count_x;
            MapSizeY = die_count_y;
            ChipsizeX = (int)die_size_x;
            ChipsizeY = (int)die_size_y;

            SetMapData();
        }

        private void crect_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Rectangle selected = (Rectangle)sender;
            CPoint pos = (CPoint)selected.Tag;
            //int stride = (int)m_MapData.PartialMapSize.Height;
            

            if (this.mapdata[this.mapSizeX * pos.Y + pos.X] == (int)CHIP_TYPE.NO_CHIP)
            {
                selected.Fill = Brushes.Green;
                this.mapdata[this.mapSizeX * pos.Y + pos.X] = (int)CHIP_TYPE.NORMAL;
            }
            else if (this.mapdata[this.mapSizeX * pos.Y + pos.X] == (int)CHIP_TYPE.NORMAL)
            {
                selected.Fill = Brushes.DimGray;
                this.mapdata[this.mapSizeX * pos.Y + pos.X] = (int)CHIP_TYPE.NO_CHIP;
            }

            SetMapData();
        }

        public void Load()
        {
            this.LoadMapData();
        }
    }
}



