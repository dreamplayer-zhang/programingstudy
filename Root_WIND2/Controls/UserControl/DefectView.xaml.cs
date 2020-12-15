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
using RootTools_Vision;

namespace Root_WIND2
{
    /// <summary>
    /// 동작 구현을 급하게 해서 불필요한 코드가 많습니다. ** 코드 최적화가 필요합니다 **
    /// Check Box의 상태에 따라 Canvas 혹은 Grid를 Visible/Collapsed 추가 구현 필요
    /// </summary>
    public partial class DefectView : UserControl
    {
        Recipe recipe;
        public DefectView()
        {
            InitializeComponent();   
        }

        public void SetRecipe(Recipe _recipe)
        {
            this.recipe = _recipe;
        }
        public void Init(bool useFront, bool useBack, bool useEdge, bool useEBR)
        {
            FrontOption.Visibility = VisibleOption(useFront);
            BackOption.Visibility = VisibleOption(useBack);
            EdgeOption.Visibility = VisibleOption(useEdge);
            EBROption.Visibility = VisibleOption(useEBR);
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

            AddEdgeDefect(gridEdge, theta, Brushes.Yellow);
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
            DrawMap();
        }
        private void DrawMap()
        {
            RecipeType_WaferMap mapdata = recipe.WaferMap;
            int[] mapData = mapdata.Data;

            int mapX = mapdata.MapSizeX;
            int mapY = mapdata.MapSizeY;

            int margin = 1;
            double mapW = (FrontsideCanvas.ActualWidth - (margin * mapX)) / mapX;
            double mapH = (FrontsideCanvas.ActualHeight - (margin * mapY)) / mapY;

            for (int x = 0; x < mapX; x++)
                for(int y = 0; y < mapY; y++)
                {
                    if (mapData[x * mapY + y] == 1)
                    {
                        Rectangle map = new Rectangle();
                        map.Stroke = Brushes.Black;
                        //map.Fill = Brushes.Blue;
                        map.Width = mapW - margin * 2;
                        map.Height = mapH - margin * 2;
                        map.StrokeThickness = 0.5;
                        map.Opacity = 0.6;

                        Canvas.SetLeft(map, x * (mapW + margin));
                        Canvas.SetTop(map, y * (mapH + margin));

                        Canvas.SetZIndex(map, 99);
                        FrontsideCanvas.Children.Add(map);
                    }   
                }        
        }

        private void AddEdgeDefect(Grid gridArea, double theta, Brush brush = null)
        {
            Rectangle defect = new Rectangle();
            defect.Width = 10;
            defect.Height = 10;

            if (brush != null)
                defect.Fill = brush;
            else
                defect.Fill = Brushes.Green;

            defect.Stroke = Brushes.Black;
            defect.StrokeThickness = 0.5;
            defect.VerticalAlignment = VerticalAlignment.Bottom;
            defect.RenderTransformOrigin = new Point(0.5, -49);
            RotateTransform rotate = new RotateTransform(theta);
            TransformGroup transformGroup = new TransformGroup();
            transformGroup.Children.Add(rotate);
            defect.RenderTransform = transformGroup;

            gridArea.Children.Add(defect);             
        }
        private void AddFrontDefect(Canvas canvas, double x, double y, Brush brush = null)
        {
            OriginRecipe originRecipe = recipe.GetRecipe<OriginRecipe>();
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
            
            double mapW = (FrontsideCanvas.ActualWidth - (margin * mapX)) / mapX;
            double mapH = (FrontsideCanvas.ActualHeight - (margin * mapY)) / mapY;

            double realSizeX = originRecipe.DiePitchX;
            double realSizeY = originRecipe.DiePitchY;
            double canvasSizeX = mapW;
            double canvasSizeY = mapH;

            double samplingRatioX = canvasSizeX / realSizeX;
            double samplingRatioY = canvasSizeY / realSizeY;

            double canvasOriginPosX = mapdata.MasterDieX * (mapW + margin);
            double canvasOriginPosY = mapdata.MasterDieY * (mapH + margin) + mapH;
           
            Canvas.SetLeft(defect, canvasOriginPosX + x * samplingRatioX);
            Canvas.SetTop(defect, canvasOriginPosY + y * samplingRatioY);

            canvas.Children.Add(defect);
        }
        private void AddBackDefect(Canvas canvas, double x, double y, Brush brush = null)
        {
            BacksideRecipe backsideRecipe = recipe.GetRecipe<BacksideRecipe>();
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
            float ratio_wafer_to_canvas_x = (float)BacksideCanvas.ActualWidth / nWaferSize;
            double waferWidth = nWaferSize * ratio_wafer_to_canvas_x;

            double dWaferRaius = (float)BacksideCanvas.ActualWidth / (double)2;
            double dSamplingRatio = dWaferRaius / 37410; // backside Recipe에서 radius값 가지고오기 <수정>

            double dCanvasWaferCenterX = (float)BacksideCanvas.ActualWidth / 2;
            double dCanvasWaferCenterY = (float)BacksideCanvas.ActualHeight / 2;

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

        private Visibility VisibleOption(bool use)
        {
            if (use)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            FrontsideCanvas.Children.Clear();
            BacksideCanvas.Children.Clear();
            gridEdge.Children.Clear();
            Random random = new Random(1);
            Random random2 = new Random(3);
            Random random3 = new Random(7);
            for (int i = 0; i < 150; i++)
            {

                int r1 = random.Next(-360, 360);
                int r2 = random2.Next(-360, 360);
                int r3 = random3.Next(-360, 360);
                //AddDefect(gridFront, r1);
                //AddDefect(backsideCanvas, r2);
                //AddDefect(gridEdge, r3);

            }
        }

        public void Clear()
		{
            FrontsideCanvas.Children.Clear();
            BacksideCanvas.Children.Clear();
            gridEdge.Children.Clear();
        }
    }
}
