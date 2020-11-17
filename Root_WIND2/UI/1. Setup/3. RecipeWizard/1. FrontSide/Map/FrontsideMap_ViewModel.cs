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
    public class FrontsideMap_ViewModel : ObservableObject
    {
        private Canvas canvas;
        int nWaferSize = 300;
        int nMapsizeX = 14;
        int nMapsizeY = 14;
        int nOriginX = 10;
        int nOriginY = 10;
        int nShotX = 1;
        int nShotY = 1;
        int nChipsizeX = 14;
        int nChipsizeY = 14;

        // Shot Info
        float fShotOriginX = 0F;
        float fShotOriginY = 0F;
        int nShotMatrixX = 1;
        int nShotMatrixY = 1;
        int nShotSizeX = 1;
        int nShotSizeY = 1;

        List<ChipData> m_ListWaferMap;
        Recipe m_Recipe;
        RecipeInfo m_RecipeInfo;
        //MapData m_WaferMap;
        #region GET/SET
        public Canvas myCanvas { get => canvas; set => canvas = value; }
        public int pWaferSize { get => nWaferSize; set => nWaferSize = value; }
        public int pMapsizeX 
        { 
            get => nMapsizeX; 
            set
            {
                SetProperty(ref nMapsizeX, value);                
            }
        }
        public int pMapsizeY
        {
            get => nMapsizeY;
            set
            {
                SetProperty(ref nMapsizeY, value);
            }
        }
        public int pOriginX { get => nOriginX; set => nOriginX = value; }
        public int pOriginY { get => nOriginY; set => nOriginY = value; }
        public int pShotX { get => nShotX; set => nShotX = value; }
        public int pShotY { get => nShotY; set => nShotY = value; }
        public int pChipsizeX
        {
            get => nChipsizeX;
            set
            {
                SetProperty(ref nChipsizeX, value);
            }
        }
        public int pChipsizeY
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

        public byte[] defaultWaferMap = new byte[] // 14x14
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

        public void Init(Setup_ViewModel setup, FrontSideMap map, Recipe recipe)
        {
            canvas = map.myCanvas;
            m_ListWaferMap = new List<ChipData>();
            m_Recipe = recipe;
            m_RecipeInfo = recipe.GetRecipeInfo();
            DrawMaps();
        }

        public void CreateWaferMap()
        {

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
            m_ListWaferMap.Clear();
            int nX = 14;
            int nY = 14;
            byte[] wafermap = defaultWaferMap;

            int nOriginX = 0;
            int nOriginY = 0;

            for (int y = 0; y < 14; y++)
            {
                for (int x = 0; x < 14; x++)
                {
                    ChipData chipInfo = new ChipData();
                    chipInfo.MapIndex = new Point(x, y);
                    chipInfo.DiePoint = new Point(x - nOriginX, y - nOriginY);

                    if (wafermap[y + nX * x] == 0)
                    {
                        chipInfo.chipinfo = ChipInfo.No_Chip;
                    }
                    else
                    {
                        chipInfo.chipinfo = ChipInfo.Normal_Chip;
                    }
                    m_ListWaferMap.Add(chipInfo);
                }
            }


            WaferMapInfo waferMapInfo = new WaferMapInfo(nX, nY, wafermap, m_ListWaferMap);
            pMapsizeX = waferMapInfo.nMapSizeX;
            pMapsizeY = waferMapInfo.nMapSizeY;
            m_ListWaferMap = waferMapInfo.ListWaferMap;


            int waferSize = nWaferSize;
            int r = waferSize / 2;

            double dChipX = (double)myCanvas.Width / (double)nMapsizeX;
            double dChipY = (double)myCanvas.Height / (double)nMapsizeY;
            int nChip_Left = 0;
            int nChip_Top = 0;
            int nChip_Right = nMapsizeX;
            int nChip_Bottom = nMapsizeY;

            Size chipSize = new Size(dChipX, dChipY);
            Point originPt = new Point(0, 0); // ???

            foreach (ChipData chipData in m_ListWaferMap)
            {
                int i = (int)chipData.MapIndex.X;
                int j = (int)chipData.MapIndex.Y;
                Rectangle crect = new Rectangle();
                crect.Width = chipSize.Width;
                crect.Height = chipSize.Height;
                Canvas.SetLeft(crect, originPt.X - (nChip_Left * chipSize.Width) + (chipSize.Width * i));
                Canvas.SetRight(crect, originPt.X - (nChip_Left * chipSize.Width) + (chipSize.Width * i) + chipSize.Width);
                Canvas.SetTop(crect, originPt.Y - (nChip_Top * chipSize.Height) + (chipSize.Height * j));
                Canvas.SetBottom(crect, originPt.Y - (nChip_Top * chipSize.Height) + (chipSize.Height * j) + chipSize.Height);

                if (chipData.chipinfo == ChipInfo.Normal_Chip)
                {
                    crect.Tag = chipData;
                    crect.ToolTip = chipData.DiePoint.X.ToString() + ", " + chipData.DiePoint.Y.ToString(); // chip index
                    crect.Stroke = Brushes.Transparent;
                    crect.Fill = Brushes.Green;
                    chipData.chipinfo = ChipInfo.Normal_Chip;
                    crect.Opacity = 0.7;
                    crect.StrokeThickness = 2;
                    Canvas.SetZIndex(crect, 99);
                    //m_ListWaferMap.Add(chipData);
                }
                else
                {
                    //chipInfo.chipinfo = ChipInfo.No_Chip;
                    crect.Tag = chipData;
                    crect.ToolTip = chipData.DiePoint.X.ToString() + ", " + chipData.DiePoint.Y.ToString(); // chip index
                    crect.Stroke = Brushes.Transparent;
                    crect.Fill = Brushes.DimGray;
                    crect.Opacity = 0.7;
                    crect.StrokeThickness = 2;
                    Canvas.SetZIndex(crect, 99);

                }
                myCanvas.Children.Add(crect);
                crect.MouseLeftButtonDown += crect_MouseLeftButtonDown;
            }

            SetMapData();
        }



        public void SetMapData()
        {
            int nSize = m_ListWaferMap.Count;
            int nX = pMapsizeX;
            int nY = pMapsizeY;
            byte[] wafermap = new byte[nSize];

            int nIndex = 0;
            foreach(ChipData chipData in m_ListWaferMap)
            {
                switch (chipData.chipinfo)
                {
                    case ChipInfo.Normal_Chip:
                        wafermap[nIndex++] = 1;
                        break;

                    case ChipInfo.No_Chip:
                        wafermap[nIndex++] = 0;
                        break;                  
                }
            }
            RecipeInfo_MapData mapdata = m_Recipe.GetRecipeInfo(typeof(RecipeInfo_MapData)) as RecipeInfo_MapData;
            WaferMapInfo waferMapInfo = new WaferMapInfo(nX, nY, wafermap, m_ListWaferMap);
            mapdata.SetWaferMapData(waferMapInfo);
        }

        //public void LoadMapData(WaferMapInfo LoadwaferMapInfo)
        public void LoadMapData()
        {     
            RecipeInfo_MapData mapdata = m_Recipe.GetRecipeInfo(typeof(RecipeInfo_MapData)) as RecipeInfo_MapData;
            WaferMapInfo LoadwaferMapInfo = mapdata.m_WaferMap;

            if(LoadwaferMapInfo != null)
            {
                m_ListWaferMap.Clear();
                pMapsizeX = LoadwaferMapInfo.nMapSizeX;
                pMapsizeY = LoadwaferMapInfo.nMapSizeY;
                m_ListWaferMap = LoadwaferMapInfo.ListWaferMap;

                /////////
                myCanvas.Children.Clear(); // 초기화
                int waferSize = nWaferSize;
                int r = waferSize / 2;

                double dChipX = (double)myCanvas.Width / (double)nMapsizeX;
                double dChipY = (double)myCanvas.Height / (double)nMapsizeY;

                int x = 0;
                int y = 0;
                int nChip_Left = 0;
                int nChip_Top = 0;
                int nChip_Right = nMapsizeX;
                int nChip_Bottom = nMapsizeY;

                Size chipSize = new Size(dChipX, dChipY);
                Point originPt = new Point(0, 0); // ???

                foreach (ChipData chipData in m_ListWaferMap)
                {
                    int i = (int)chipData.MapIndex.X;
                    int j = (int)chipData.MapIndex.Y;
                    Rectangle crect = new Rectangle();
                    crect.Width = chipSize.Width;
                    crect.Height = chipSize.Height;
                    Canvas.SetLeft(crect, originPt.X - (nChip_Left * chipSize.Width) + (chipSize.Width * i));
                    Canvas.SetRight(crect, originPt.X - (nChip_Left * chipSize.Width) + (chipSize.Width * i) + chipSize.Width);
                    Canvas.SetTop(crect, originPt.Y - (nChip_Top * chipSize.Height) + (chipSize.Height * j));
                    Canvas.SetBottom(crect, originPt.Y - (nChip_Top * chipSize.Height) + (chipSize.Height * j) + chipSize.Height);

                    if (chipData.chipinfo == ChipInfo.Normal_Chip)
                    {
                        crect.Tag = chipData;
                        crect.ToolTip = chipData.DiePoint.X.ToString() + ", " + chipData.DiePoint.Y.ToString(); // chip index
                        crect.Stroke = Brushes.Transparent;
                        crect.Fill = Brushes.Green;
                        chipData.chipinfo = ChipInfo.Normal_Chip;
                        crect.Opacity = 0.7;
                        crect.StrokeThickness = 2;
                        Canvas.SetZIndex(crect, 99);
                        //m_ListWaferMap.Add(chipData);
                    }
                    else
                    {
                        //chipInfo.chipinfo = ChipInfo.No_Chip;
                        crect.Tag = chipData;
                        crect.ToolTip = chipData.DiePoint.X.ToString() + ", " + chipData.DiePoint.Y.ToString(); // chip index
                        crect.Stroke = Brushes.Transparent;
                        crect.Fill = Brushes.DimGray;
                        crect.Opacity = 0.7;
                        crect.StrokeThickness = 2;
                        Canvas.SetZIndex(crect, 99);

                    }
                    myCanvas.Children.Add(crect);
                    crect.MouseLeftButtonDown += crect_MouseLeftButtonDown;
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
            m_ListWaferMap.Clear();
            int waferSize = nWaferSize;
            int r = waferSize / 2;

            double dChipX = (double)myCanvas.Width / (double)nMapsizeX;
            double dChipY = (double)myCanvas.Height / (double)nMapsizeY;

            int x = 0;
            int y = 0;
            int nChip_Left = 0;
            int nChip_Top = 0;
            int nChip_Right = nMapsizeX;
            int nChip_Bottom = nMapsizeY;

            Size chipSize = new Size(dChipX, dChipY);
            //Size chipSize = new Size(nChipsizeX, nChipsizeY);
            Point originPt = new Point(0, 0); // ???

            for (int j = 0; j < (nChip_Top + nChip_Bottom); j++)
                for (int i = 0; i < (nChip_Left + nChip_Right); i++)
                {
                    ChipData chipInfo = new ChipData();
                    chipInfo.MapIndex = new Point(i, j);
                    chipInfo.DiePoint = new Point(i - x, j - y);

                    Rectangle crect = new Rectangle();
                    crect.Width = chipSize.Width;
                    crect.Height = chipSize.Height;

                    Canvas.SetLeft(crect, originPt.X - (nChip_Left * chipSize.Width) + (chipSize.Width * i));
                    Canvas.SetRight(crect, originPt.X - (nChip_Left * chipSize.Width) + (chipSize.Width * i) + chipSize.Width);
                    Canvas.SetTop(crect, originPt.Y - (nChip_Top * chipSize.Height) + (chipSize.Height * j));
                    Canvas.SetBottom(crect, originPt.Y - (nChip_Top * chipSize.Height) + (chipSize.Height * j) + chipSize.Height);
                    double left = Canvas.GetLeft(crect);
                    double top = Canvas.GetTop(crect);
                    double right = Canvas.GetRight(crect);
                    double bottom = Canvas.GetBottom(crect);
                    double leftPow = Math.Pow(left - r, 2);
                    double topPow = Math.Pow(top - r, 2);
                    double RightPow = Math.Pow(right - r, 2);
                    double BottomPow = Math.Pow(bottom - r, 2);
                    double rPow = Math.Pow(r, 2);
                    bool lt = leftPow + topPow <= rPow;
                    bool lb = leftPow + BottomPow <= rPow;
                    bool rt = RightPow + topPow <= rPow;
                    bool rb = RightPow + BottomPow <= rPow;

                    List<bool> condition = new List<bool>();
                    if (lt)
                        condition.Add(lt);
                    if (lb)
                        condition.Add(lb);
                    if (rt)
                        condition.Add(rt);
                    if (rb)
                        condition.Add(rb);

                    bool bAutoDraw = false;
                    if (bAutoDraw)
                    {

                        if (condition.Count == 4) // inner
                        {
                            crect.Tag = chipInfo;
                            crect.ToolTip = chipInfo.DiePoint.X.ToString() + ", " + chipInfo.DiePoint.Y.ToString(); // chip index
                            crect.Stroke = Brushes.Transparent;
                            crect.Fill = Brushes.Green;
                            chipInfo.chipinfo = ChipInfo.Normal_Chip;
                            crect.Opacity = 0.7;
                            crect.StrokeThickness = 2;
                            Canvas.SetZIndex(crect, 99);
                            m_ListWaferMap.Add(chipInfo);
                        }
                        else if (condition.Count >= 1) // Partial
                        {
                            chipInfo.chipinfo = ChipInfo.Partial_Chip;
                            crect.Tag = chipInfo;
                            crect.ToolTip = chipInfo.DiePoint.X.ToString() + ", " + chipInfo.DiePoint.Y.ToString(); // chip index
                            crect.Stroke = Brushes.Transparent;
                            crect.Fill = Brushes.Blue;
                            crect.Opacity = 0.7;
                            crect.StrokeThickness = 2;
                            Canvas.SetZIndex(crect, 99);
                            m_ListWaferMap.Add(chipInfo);
                        }
                        else
                        {
                            chipInfo.chipinfo = ChipInfo.No_Chip;
                            crect.Tag = chipInfo;
                            crect.ToolTip = chipInfo.DiePoint.X.ToString() + ", " + chipInfo.DiePoint.Y.ToString(); // chip index
                            crect.Stroke = Brushes.Transparent;
                            crect.Fill = Brushes.DimGray;
                            crect.Opacity = 0.7;
                            crect.StrokeThickness = 2;
                            Canvas.SetZIndex(crect, 99);
                            m_ListWaferMap.Add(chipInfo);
                        }
                    }
                    else
                    {
                        
                        crect.Tag = chipInfo;
                        crect.ToolTip = chipInfo.DiePoint.X.ToString() + ", " + chipInfo.DiePoint.Y.ToString(); // chip index
                        crect.Stroke = Brushes.Transparent;
                        crect.Fill = Brushes.Green;
                        chipInfo.chipinfo = ChipInfo.Normal_Chip;
                        crect.Opacity = 0.7;
                        crect.StrokeThickness = 2;
                        Canvas.SetZIndex(crect, 99);
                        m_ListWaferMap.Add(chipInfo);
                    }


                    //var cl = Math.Round(Canvas.GetLeft(crect) * 1000) / 1000;
                    //var ct = Math.Round(Canvas.GetTop(crect) * 1000) / 1000;

                    //if (cl == Math.Round(originPt.X * 1000) / 1000 && ct == Math.Round((originPt.Y - chipSize.Height) * 1000) / 1000)
                    //{
                    //    crect.Fill = Brushes.Red;
                    //    ChipData chip = (ChipData)crect.Tag;
                    //    //m_MapData.CenterIndex = new System.Drawing.Point((int)chip.DiePoint.X, (int)chip.DiePoint.Y);
                    //}

                    // Edit을 위한 Event 추가
                    myCanvas.Children.Add(crect);
                    crect.MouseLeftButtonDown += crect_MouseLeftButtonDown;
                }
        }

        public void DrawMaps_Shot()
        {
            myCanvas.Children.Clear(); // 초기화
            m_ListWaferMap.Clear();
            float wafer_size = (float)nWaferSize * 1000; // [mm] -> [um]

            float wafer_center_x = wafer_size / 2;
            float wafer_center_y = wafer_size / 2;

            float radius = wafer_size / 2;

            float shot_origin_x = wafer_center_x - fShotOriginX;   // Shot Origin 좌하단 기준
            float shot_origin_y = wafer_center_y + fShotOriginY;

            //float shot_count_x = (float)wafer_size / (float)nShotSizeX;
            //float shot_count_y = (float)wafer_size / (float)nShotSizeY;

            float die_size_x = (float)nShotSizeX / (float)nShotMatrixX;
            float die_size_y = (float)nShotSizeX / (float)nShotMatrixY;

            //float chip_count_x = shot_count_x * nShotMatrixX;
            //float chip_count_y = shot_count_y * nShotMatrixY;

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
                    ChipData chip_data = new ChipData();

                    chip_data.MapIndex = new Point(x, y);
                    chip_data.DiePoint = new Point(x, y);

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
                        chip_data.chipinfo = ChipInfo.No_Chip;
                        m_ListWaferMap.Add(chip_data);
                        continue;
                    }


                    Canvas.SetLeft(canvas_rect, (left + wafer_center_x) * ratio_wafer_to_canvas_x);
                    Canvas.SetRight(canvas_rect, (right+ wafer_center_x) * ratio_wafer_to_canvas_x);
                    Canvas.SetTop(canvas_rect, (top + wafer_center_y) * ratio_wafer_to_canvas_y);
                    Canvas.SetBottom(canvas_rect, (bottom + wafer_center_y) * ratio_wafer_to_canvas_y);

                    chip_data.chipinfo = ChipInfo.Normal_Chip;

                    canvas_rect.Tag = chip_data;
                    canvas_rect.ToolTip = chip_data.DiePoint.X.ToString() + ", " + chip_data.DiePoint.Y.ToString(); // chip index
                    canvas_rect.Stroke = Brushes.Transparent;
                    canvas_rect.Fill = Brushes.Green;
                    canvas_rect.Opacity = 0.7;
                    canvas_rect.StrokeThickness = 2;
                    Canvas.SetZIndex(canvas_rect, 99);
                    m_ListWaferMap.Add(chip_data);


                    myCanvas.Children.Add(canvas_rect);
                    canvas_rect.MouseLeftButtonDown += crect_MouseLeftButtonDown;
                }
            }

            // Set MapInfo

            pMapsizeX = die_count_x;
            pMapsizeY = die_count_y;
            pChipsizeX = (int)die_size_x;
            pChipsizeY = (int)die_size_y;

            SetMapData();
        }

        private void crect_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Rectangle selected = (Rectangle)sender;
            ChipData chipData = (ChipData)selected.Tag;
            //int stride = (int)m_MapData.PartialMapSize.Height;

            if(chipData.chipinfo == ChipInfo.Normal_Chip)
            {
                selected.Fill = Brushes.DimGray;
                chipData.chipinfo = ChipInfo.No_Chip;
            }
            else if(chipData.chipinfo == ChipInfo.No_Chip)
            {
                selected.Fill = Brushes.Green;
                chipData.chipinfo = ChipInfo.Normal_Chip;
            }

            SetMapData();
        }
    }
}



