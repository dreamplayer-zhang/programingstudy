using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using RootTools.Database;
using RootTools_Vision;

namespace Root_WIND2
{
    /// <summary>
    /// 동작 구현을 급하게 해서 불필요한 코드가 많습니다. ** 코드 최적화가 필요합니다 **
    /// </summary>
    public partial class DefectView : UserControl
    {
        RecipeBase recipe;

        List<Defect> defectList;

        public DefectView()
        {
            InitializeComponent();
            //cb_Edge.IsChecked = true;
            //cb_Back.IsChecked = true;
            cb_Front.IsChecked = true;
        }

        public void SetRecipe(RecipeBase _recipe)
        {
            this.recipe = _recipe;
        }

        public void SetDefectList(List<Defect> defectList)
        {
            this.defectList = defectList;
        }

        private void DefectCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox chk = (CheckBox)sender;

            switch (chk.Name)
            {
                case "cb_Front" :
                    if (cb_Front.IsChecked == true)
                    {
                        cb_Back.IsChecked = false;
                        cb_Edge.IsChecked = false;
                        cb_EBR.IsChecked = false;

                        FrontsideCanvas.Visibility = Visibility.Visible;
                        BacksideCanvas.Visibility = Visibility.Collapsed;
                        gridEdge.Visibility = Visibility.Collapsed;
                        Wafer.Visibility = Visibility.Collapsed;
                    }
                    break;
                case "cb_Back":
                    if (cb_Back.IsChecked == true)
                    {
                        cb_Front.IsChecked = false;
                        cb_Edge.IsChecked = false;
                        cb_EBR.IsChecked = false;

                        FrontsideCanvas.Visibility = Visibility.Collapsed;
                        BacksideCanvas.Visibility = Visibility.Visible;
                        Wafer.Visibility = Visibility.Visible;
                        gridEdge.Visibility = Visibility.Collapsed;
                    }
                    break;
                case "cb_Edge":
                    if (cb_Edge.IsChecked == true)
                    {
                        cb_Front.IsChecked = false;
                        cb_Back.IsChecked = false;
                        cb_EBR.IsChecked = false;

                        FrontsideCanvas.Visibility = Visibility.Collapsed;
                        BacksideCanvas.Visibility = Visibility.Collapsed;
                        gridEdge.Visibility = Visibility.Visible;
                        Wafer.Visibility = Visibility.Visible;
                    }
                    break;
                case "cb_EBR":
                    if (cb_EBR.IsChecked == true)
                    {
                        cb_Front.IsChecked = false;
                        cb_Back.IsChecked = false;
                        cb_Edge.IsChecked = false;
                    }
                    break;
                default:
                    break;
            }
        }

        public void AddFrontDefect(double relX, double relY)
        {  
            AddFrontDefect(FrontsideCanvas, relX, relY);
        }
        public void AddBackDefect(double relX, double relY)
        {
            AddBackDefect(BacksideCanvas, relX, relY);
        }
        public void AddEdgeDefect(double theta)
        {
            AddEdgeDefect(gridEdge, theta);
        }

        public void DisplaySelectedEdgeDefect(int listCnt, int index, double theta)
        {
            if (gridEdge.Children.Count > listCnt)
                gridEdge.Children.RemoveAt(gridEdge.Children.Count - 1);
            AddEdgeDefect(gridEdge, theta, Brushes.Yellow, true);

        }
        public void DisplaySelectedFrontDefect(int listCnt, double relX, double relY)
        {
            if (FrontsideCanvas.Children.Count > listCnt)
                FrontsideCanvas.Children.RemoveAt(FrontsideCanvas.Children.Count - 1);

            AddFrontDefect(FrontsideCanvas, relX, relY, Brushes.Yellow);
        }
        public void DisplaySelectedBackDefect(int listCnt, double relX, double relY)
        {
            if (BacksideCanvas.Children.Count > listCnt)
                BacksideCanvas.Children.RemoveAt(BacksideCanvas.Children.Count - 1);

            AddBackDefect(BacksideCanvas, relX, relY, Brushes.Yellow);
        }

        public void DrawWaferMap()
        {
            //RecipeType_WaferMap mapdata = recipe.WaferMap; // 원래코드

            // 임시
            int[] data = new int[9*9];
            for (int i = 0; i < data.Length; i++)
            {
                if (i == 0 || i == 1 || i == 2
                    || i == 6 || i == 7 || i == 8
                    || i == 9 || i == 10 || i == 16
                    || i == 17 || i == 18 || i == 26
                    || i == 54 || i == 62 || i == 63
                    || i == 64 || i == 70 || i == 71
                    || i == 72 || i == 73 || i == 74
                    || i == 78 || i == 79 || i == 80)
                    data[i] = 0;
                else
                    data[i] = 1;
            }
            RecipeType_WaferMap mapdata = new RecipeType_WaferMap(9, 9, data);
            // UI자료용
            int[] mapData = mapdata.Data;

            int mapX = mapdata.MapSizeX;
            int mapY = mapdata.MapSizeY;

            int margin = 1;
            double mapW = (FrontsideCanvas.Width - (margin * mapX)) / mapX;
            double mapH = (FrontsideCanvas.Height - (margin * mapY)) / mapY;
            MapCanvas.Children.Clear();
            for (int x = 0; x < mapX; x++)
                for(int y = 0; y < mapY; y++)
                {
                    if (mapData[x * mapY + y] == 1)
                    {
                        Rectangle map = new Rectangle();
                        map.Stroke = Brushes.Black;
                        map.Fill = Brushes.Gainsboro;
                        map.Width = mapW - margin * 2;
                        map.Height = mapH - margin * 2;
                        map.StrokeThickness = 0.5;

                        Canvas.SetLeft(map, x * (mapW + margin));
                        Canvas.SetTop(map, y * (mapH + margin));
                        MapCanvas.Children.Add(map);
                    }   
                }        
        }

        private void AddEdgeDefect(Grid gridArea, double theta, Brush brush = null, bool select = false)
        {
            Rectangle defect = new Rectangle();
            defect.VerticalAlignment = VerticalAlignment.Bottom;

            
            RotateTransform rotate = new RotateTransform(theta);
            TransformGroup transformGroup = new TransformGroup();
            transformGroup.Children.Add(rotate);
            defect.RenderTransform = transformGroup;

            if (brush != null)
                defect.Fill = brush;
            else
                defect.Fill = Brushes.Green;

            if (select)
            {
                //defect.Stroke = Brushes.Black;
                //defect.StrokeThickness = 0.5;
                //defect.Width = 6;
                //defect.Height = 6;
                //defect.RenderTransformOrigin = new Point(0.5, -82.333);
                defect.Stroke = Brushes.Black;
                defect.StrokeThickness = 1;
                defect.Width = 10;
                defect.Height = 10;
                defect.RenderTransformOrigin = new Point(0.5, -49);
            }
            else
            {
                defect.Width = 6;
                defect.Height = 6;
                defect.RenderTransformOrigin = new Point(0.5, -82.333);
                //defect.Stroke = Brushes.Black;
                //defect.StrokeThickness = 0.0;
                //defect.Width = 2;
                //defect.Height = 2;
                //defect.RenderTransformOrigin = new Point(0.5, -249);
            }

            gridArea.Children.Add(defect);             
        }
        private void AddFrontDefect(Canvas canvas, double x, double y, Brush brush = null)
        {
            OriginRecipe originRecipe = recipe.GetItem<OriginRecipe>();
            RecipeType_WaferMap mapdata = recipe.WaferMap;

            Rectangle defect = new Rectangle();
            defect.Width = 10;
            defect.Height = 10;

            if (brush == null)
                defect.Fill = Brushes.Red;
            else
                defect.Fill = brush;

            defect.Stroke = Brushes.Black;
            defect.StrokeThickness = 0.5;

            int mapX = mapdata.MapSizeX;
            int mapY = mapdata.MapSizeY;
            int margin = 1;
            
            double mapW = (FrontsideCanvas.Width - (margin * mapX)) / mapX;
            double mapH = (FrontsideCanvas.Height - (margin * mapY)) / mapY;

            double realSizeX = originRecipe.DiePitchX;
            double realSizeY = originRecipe.DiePitchY;
            double canvasSizeX = mapW;
            double canvasSizeY = mapH;

            double samplingRatioX = canvasSizeX / realSizeX;
            double samplingRatioY = canvasSizeY / realSizeY;

            double canvasOriginPosX = mapdata.MasterDieX * (mapW + margin); //*
            double canvasOriginPosY = mapdata.MasterDieY * (mapH + margin) + mapH;//

            Canvas.SetLeft(defect, canvasOriginPosX + x * samplingRatioX);
            Canvas.SetTop(defect, canvasOriginPosY + y * samplingRatioY);

            canvas.Children.Add(defect);
        }
        private void AddBackDefect(Canvas canvas, double x, double y, Brush brush = null)
        {
            BacksideRecipe backsideRecipe = recipe.GetItem<BacksideRecipe>();
            RecipeType_WaferMap mapdata = recipe.WaferMap;

            Rectangle defect = new Rectangle();
            defect.Width = 10;
            defect.Height = 10;

            if (brush == null)
                defect.Fill = Brushes.Blue;
            else
                defect.Fill = brush;

            defect.Stroke = Brushes.Black;
            defect.StrokeThickness = 0.5;

            int nWaferSize = 300; // 300mm 기준
            float ratio_wafer_to_canvas_x = (float)BacksideCanvas.Width / nWaferSize;
            double waferWidth = nWaferSize * ratio_wafer_to_canvas_x;

            double dWaferRaius = (float)BacksideCanvas.Width / (double)2;
            double dSamplingRatio = dWaferRaius / 30430; // backside Recipe에서 radius값 가지고오기 <수정> // 37410 meomo

            double dCanvasWaferCenterX = (float)BacksideCanvas.Width / 2;
            double dCanvasWaferCenterY = (float)BacksideCanvas.Height / 2;

            Canvas.SetLeft(defect, dCanvasWaferCenterX + x * dSamplingRatio);
            Canvas.SetTop(defect, dCanvasWaferCenterY + y * dSamplingRatio);

            canvas.Children.Add(defect);
        }

        //public void AddDefectList(List<EdgeDefect> listDefect)
        //{
        //    foreach (EdgeDefect defect in listDefect)
        //    {
        //        switch (defect.m_eDirection)
        //        {
        //            case eDirection.Front:
        //                {
        //                    AddDefect(gridFront, defect.m_dTheta);
        //                    break;
        //                }
        //            case eDirection.Back:
        //                {
        //                    AddDefect(backsideCanvas, defect.m_dTheta);
        //                    break;
        //                }
        //            case eDirection.Side:
        //                {
        //                    AddDefect(gridEdge, defect.m_dTheta);
        //                    break;
        //                }
        //        }
        //    }
        //}

        public void Clear()
		{
            FrontsideCanvas.Children.Clear();
            BacksideCanvas.Children.Clear();
            gridEdge.Children.Clear();
        }
    }
}
