using RootTools;
using RootTools_CLR;
using RootTools_Vision;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System;

namespace Root_WIND2
{
    class BacksideROI_ViewModel : RootViewer_ViewModel
    {
        bool UIUpdateLock = false;

        public BacksideROI_ViewModel(ImageData image = null, IDialogService dialogService = null)
        {
            base.init(image, dialogService);
            p_VisibleMenu = Visibility.Visible;
            Shapes.CollectionChanged += Shapes_CollectionChanged;            
        }

        private void Shapes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if(!UIUpdateLock)
            {
                var shapes = sender as ObservableCollection<TShape>;
                foreach (TShape shape in shapes)
                {
                    if (!p_UIElements.Contains(shape.UIElement))
                        p_UIElements.Add(shape.UIElement);
                }
            }
        }
        public ObservableCollection<TShape> Shapes = new ObservableCollection<TShape>();
        TShape rectInfo;

        Path CIRCLE_UI;
        Grid CENTERPOINT_UI;
        Polygon WAFEREDGE_UI;
        List<CPoint> PolygonPt = new List<CPoint>();

        Recipe m_Recipe;
        RecipeData_Origin m_RecipeData_Origin;
        RecipeInfo_MapData m_mapInfo;
        
        CPoint CanvasPoint;
        CPoint MemoryPoint;

        bool isIncludeMode = true; // Map
        public bool Check_MapMode
        {
            get { return this.isIncludeMode; }
            set
            {
                if (this.isIncludeMode == value)
                    return;

                this.isIncludeMode = value;
            }
        }

        private CPoint m_CenterPoint = new CPoint();
        public CPoint p_CenterPoint
        {
            get
            {
                return m_CenterPoint;
            }
            set
            {
                SetProperty(ref m_CenterPoint, value);
            }
        }
        private int m_MapSizeX = 40;
        private int m_MapSizeY = 40;
        public int p_MapSizeX
        {
            get
            {
                return m_MapSizeX;
            }
            set
            {
                SetProperty(ref m_MapSizeX, value);
            }
        }
        public int p_MapSizeY
        {
            get
            {
                return m_MapSizeY;
            }
            set
            {
                SetProperty(ref m_MapSizeY, value);
            }
        }
        private int m_Radius = 20000;
        public int p_Radius 
        {
            get
            {
                return m_Radius;
            }
            set
            {
                SetProperty(ref m_Radius, value);
            }
        }

        private ObservableCollection<UIElement> m_UIElements = new ObservableCollection<UIElement>();
        public ObservableCollection<UIElement> p_UIElements
        {
            get
            {
                return m_UIElements;
            }
            set
            {
                m_UIElements = value;
            }
        }
       
        public void init(Setup_ViewModel setup, Recipe recipe)
        {
            m_Recipe = recipe;
            m_RecipeData_Origin = recipe.GetRecipeData(typeof(RecipeData_Origin)) as RecipeData_Origin;
            m_mapInfo = m_Recipe.GetRecipeInfo(typeof(RecipeInfo_MapData)) as RecipeInfo_MapData;
            base.init(setup.m_MainWindow.m_Image, setup.m_MainWindow.dialogService);
            p_VisibleMenu = System.Windows.Visibility.Visible;
        }

        public override void PreviewMouseDown(object sender, MouseEventArgs e)
        {
            base.PreviewMouseDown(sender, e);
            if (m_KeyEvent != null)
                if (m_KeyEvent.Key == Key.LeftCtrl && m_KeyEvent.IsDown)
                    return;
            if (MemoryPoint.X == 0 && MemoryPoint.Y == 0)
                return;

            p_CenterPoint = MemoryPoint;

            if (p_ViewElement.Contains(WAFEREDGE_UI))
                p_ViewElement.Remove(WAFEREDGE_UI);

            DrawCenterPoint(ColorType.Teaching);
            DrawCircle(ColorType.Teaching);
        }
        private void SetRecipeMapData(byte[] mapData, int mapX, int mapY, int originX, int originY, int chipSzX, int chipSzY)
        {
            List<ChipData> ListWaferMap = new List<ChipData>();
            
            for (int y = 0; y < mapY; y++)
            {
                for (int x = 0; x < mapX; x++)
                {
                    ChipData chipInfo = new ChipData();
                    chipInfo.MapIndex = new Point(x, y);
                    chipInfo.DiePoint = new Point(x, y);

                    if (mapData[y + mapX * x] == 0)
                    {
                        chipInfo.chipinfo = ChipInfo.No_Chip;
                    }
                    else
                    {
                        chipInfo.chipinfo = ChipInfo.Normal_Chip;
                    }
                    ListWaferMap.Add(chipInfo);
                }
            }
            // Map Data Recipe 생성
            m_RecipeData_Origin.OriginX = originX;
            m_RecipeData_Origin.OriginY = originY;
            m_RecipeData_Origin.DiePitchX = chipSzX;
            m_RecipeData_Origin.DiePitchY = chipSzY;

            m_mapInfo.m_WaferMap = new WaferMapInfo(mapX, mapY, mapData, ListWaferMap);

            if (true) // Display Map Data Option화
                DrawMapData(mapData, mapX, mapY, originX, originY, chipSzX, chipSzY);
        }
        private void DrawRect(CPoint LT, CPoint RB, ColorType color, String text = null, int FontSz = 15)
        {
            SetShapeColor(color);

            TRect rect = rectInfo as TRect;
            rect.MemPointBuffer = LT;
            rect.MemoryRect.Left = LT.X;
            rect.MemoryRect.Top = LT.Y;
            rectInfo = Drawing(rectInfo, RB);
            Shapes.Add(rectInfo);

            //if (text != null)
            //{
            //    Grid textGrid = WriteInfoText(text, rect, color, FontSz);
            //    InfoTextBolcks.Add(new InfoTextBolck(textGrid, rect));
            //}
        }
        private void DrawRect(List<CRect> RectList, ColorType color, List<String> textList = null, int FontSz = 15)
        {
            UIUpdateLock = true;
            foreach (CRect rectPoint in RectList)
            {
                SetShapeColor(color);
                TRect rect = rectInfo as TRect;

                rect.MemPointBuffer = new CPoint(rectPoint.Left, rectPoint.Top);
                rect.MemoryRect.Left = rectPoint.Left;
                rect.MemoryRect.Top = rectPoint.Top;
                rectInfo = Drawing(rectInfo, new CPoint(rectPoint.Right, rectPoint.Bottom));

                if (RectList.IndexOf(rectPoint) == RectList.Count - 1)
                    UIUpdateLock = false;

                Shapes.Add(rectInfo);
            }
        }
        private void DrawCenterPoint(ColorType color)
        {
            if (p_ViewElement.Contains(CENTERPOINT_UI))
                p_ViewElement.Remove(CENTERPOINT_UI);

            if (CENTERPOINT_UI == null)
            {
                CENTERPOINT_UI = new Grid();
                CENTERPOINT_UI.Width = 20;
                CENTERPOINT_UI.Height = 20;

                Line line1 = new Line();
                line1.X1 = 0;
                line1.Y1 = 0;
                line1.X2 = 1;
                line1.Y2 = 1;
                //line1.Stroke = GetColorBrushType(color);
                line1.StrokeThickness = 1.5;
                line1.Stretch = Stretch.Fill;
                Line line2 = new Line();
                line2.X1 = 0;
                line2.Y1 = 1;
                line2.X2 = 1;
                line2.Y2 = 0;
                //line2.Stroke = GetColorBrushType(color);
                line2.StrokeThickness = 1.5;
                line2.Stretch = Stretch.Fill;
                CENTERPOINT_UI.Children.Add(line1);
                CENTERPOINT_UI.Children.Add(line2);
            }

            foreach (UIElement ui in CENTERPOINT_UI.Children)
            {
                Line line = ui as Line;
                line.Stroke = GetColorBrushType(color);
            }

            Canvas.SetLeft(CENTERPOINT_UI, CanvasPoint.X - 10);
            Canvas.SetTop(CENTERPOINT_UI, CanvasPoint.Y - 10);
            p_ViewElement.Add(CENTERPOINT_UI);
        }
        private void DrawCircle(ColorType color)
        {
            if (p_ViewElement.Contains(CIRCLE_UI))
                p_ViewElement.Remove(CIRCLE_UI);

            if (CIRCLE_UI == null)
            {
                CIRCLE_UI = new Path();
                CIRCLE_UI.Stroke = GetColorBrushType(color);
                CIRCLE_UI.StrokeThickness = 2;
                CIRCLE_UI.StrokeDashArray = new DoubleCollection { 2, 2 };
                EllipseGeometry circle = new EllipseGeometry();
                CIRCLE_UI.Data = circle;
            }

            EllipseGeometry data = CIRCLE_UI.Data as EllipseGeometry;
            data.Center = new Point(CanvasPoint.X, CanvasPoint.Y);

            int radius = p_CenterPoint.X + p_Radius;
            CPoint temp = GetCanvasPoint(new CPoint(radius, radius));
            int CanvasRadius = temp.X - CanvasPoint.X;

            data.RadiusX = (double)CanvasRadius;
            data.RadiusY = (double)CanvasRadius;
            p_ViewElement.Add(CIRCLE_UI);

        }
        private void DrawPolygon(List<CPoint> memPolyPt)
        {
            if (p_ViewElement.Contains(WAFEREDGE_UI))
                p_ViewElement.Remove(WAFEREDGE_UI);

            if (WAFEREDGE_UI == null)
            {
                WAFEREDGE_UI = new Polygon();
                WAFEREDGE_UI.Stroke = GetColorBrushType(ColorType.WaferEdge);
                WAFEREDGE_UI.StrokeThickness = 3;
                WAFEREDGE_UI.StrokeDashArray = new DoubleCollection { 2, 2 };
            }
            else
                WAFEREDGE_UI.Points.Clear();

            foreach (CPoint pt in memPolyPt)
            {
                CPoint a = new CPoint();
                a = GetCanvasPoint(new CPoint(pt.X, pt.Y));
                WAFEREDGE_UI.Points.Add(new Point(a.X, a.Y));
            }
            p_ViewElement.Add(WAFEREDGE_UI);
        }
        private TShape Drawing(TShape shape, CPoint memPt)
        {
            TRect rect = shape as TRect;
            // memright가 0인상태로 canvas rect width가 정해져서 버그...
            // 0이면 min정해줘야되나
            if (rect.MemPointBuffer.X > memPt.X)
            {
                rect.MemoryRect.Left = memPt.X;
                rect.MemoryRect.Right = rect.MemPointBuffer.X;
            }
            else
            {
                rect.MemoryRect.Left = rect.MemPointBuffer.X;
                rect.MemoryRect.Right = memPt.X;
            }
            if (rect.MemPointBuffer.Y > memPt.Y)
            {
                rect.MemoryRect.Top = memPt.Y;
                rect.MemoryRect.Bottom = rect.MemPointBuffer.Y;
            }
            else
            {
                rect.MemoryRect.Top = rect.MemPointBuffer.Y;
                rect.MemoryRect.Bottom = memPt.Y;
            }

            CPoint LT = new CPoint(rect.MemoryRect.Left, rect.MemoryRect.Top);
            CPoint RB = new CPoint(rect.MemoryRect.Right, rect.MemoryRect.Bottom);
            CPoint canvasLT = new CPoint(GetCanvasPoint(LT));
            CPoint canvasRB = new CPoint(GetCanvasPoint(RB));

            int width = Math.Abs(canvasRB.X - canvasLT.X);
            int height = Math.Abs(canvasRB.Y - canvasLT.Y);
            Canvas.SetLeft(rect.CanvasRect, canvasLT.X);
            Canvas.SetTop(rect.CanvasRect, canvasLT.Y);
            Canvas.SetRight(rect.CanvasRect, canvasRB.X);
            Canvas.SetBottom(rect.CanvasRect, canvasRB.Y);
            rect.CanvasRect.Width = width;
            rect.CanvasRect.Height = height;

            return shape;
        }
        private void DrawMapData(byte[] mapData, int mapX, int mapY, int OriginX, int OriginY, int ChipSzX, int ChipSzY)
        {
            RemoveMapDataRect();
            // Map Display
            List<CRect> rectList = new List<CRect>();
            int offsetY = 0;
            bool isOrigin = true;

            for (int x = 0; x < mapX; x++)
                for (int y = 0; y < mapY; y++)
                    if (mapData[y * mapX + x] == 1)
                    {
                        if (isOrigin)
                        {
                            offsetY = OriginY - (y + 1) * ChipSzY;
                            isOrigin = false;
                        }

                        rectList.Add(new CRect(OriginX + x * ChipSzX, offsetY + y * ChipSzY, OriginX + (x + 1) * ChipSzX, offsetY + (y + 1) * ChipSzY));
                    }


            DrawRect(rectList, ColorType.MapData);
        }
        private void RemoveMapDataRect()
        {
            foreach (TShape shape in Shapes)
            {
                if (p_UIElements.Contains(shape.UIElement))
                    p_UIElements.Remove(shape.UIElement);
            }
            Shapes.Clear();
        }
        public override void MouseMove(object sender, MouseEventArgs e)
        {
            base.MouseMove(sender, e);
            MemoryPoint = new CPoint(p_MouseMemX, p_MouseMemY);
            CanvasPoint = GetCanvasPoint(MemoryPoint);
        }

        private void _StartRecipeTeaching()
        {
            int nMemH = p_ImageData.p_Size.Y;
            int nMemW = p_ImageData.p_Size.X;

            float centX = p_CenterPoint.X; // 레시피 티칭 값 가지고오기
            float centY = p_CenterPoint.Y;

            int outMapX = m_MapSizeX, outMapY = m_MapSizeY;
            float outOriginX, outOriginY;
            float outChipSzX, outChipSzY;            
            float outRadius = p_Radius;

            IntPtr MainImage = new IntPtr();
            if (p_ImageData.p_nByte == 3)
            {
                if (p_eColorViewMode != eColorViewMode.All)
                    MainImage = p_ImageData.GetPtr((int)p_eColorViewMode - 1);
                else
                    MainImage = p_ImageData.GetPtr(0);
            }
            else
            { // All 일때는 R채널로...
                MainImage = p_ImageData.GetPtr(0);
            }

            Cpp_Point[] WaferEdge = null;
            byte[] MapData = null;
            unsafe
            {
                int DownSample = 10;

                fixed (byte* pImg = new byte[(long)(nMemW / DownSample) * (long)(nMemH / DownSample)]) // 원본 이미지 너무 커서 안열림
                {
                    CLR_IP.Cpp_SubSampling((byte*)MainImage, pImg, nMemW, nMemH, 0, 0, nMemW, nMemH, DownSample);

                    // Param Down Scale
                    centX /= DownSample; centY /= DownSample;
                    outRadius /= DownSample;
                    nMemW /= DownSample; nMemH /= DownSample;

                    WaferEdge = CLR_IP.Cpp_FindWaferEdge(pImg,
                        &centX, &centY,
                        &outRadius,
                        nMemW, nMemH,
                        1
                        );

                    MapData = CLR_IP.Cpp_GenerateMapData(
                        WaferEdge,
                        &outOriginX,
                        &outOriginY,
                        &outChipSzX,
                        &outChipSzY,
                        &outMapX,
                        &outMapY,
                        nMemW, nMemH,
                        1,
                        isIncludeMode
                        );
                }

                // Param Up Scale
                centX *= DownSample; centY *= DownSample;
                outRadius *= DownSample;
                outOriginX *= DownSample; outOriginY *= DownSample;
                outChipSzX *= DownSample; outChipSzY *= DownSample;

                PolygonPt.Clear();
                if (WAFEREDGE_UI != null)
                    WAFEREDGE_UI.Points.Clear();

                for(int i = 0; i < WaferEdge.Length; i++)
                    PolygonPt.Add(new CPoint(WaferEdge[i].x * DownSample, WaferEdge[i].y * DownSample));

                // UI Data Update
                p_CenterPoint = new CPoint((int)centX, (int)centY);
                p_Radius = (int)outRadius;
                p_MapSizeX = (int)outMapX;
                p_MapSizeY = (int)outMapY;

                CanvasPoint = GetCanvasPoint(new CPoint(p_CenterPoint.X, p_CenterPoint.Y));
                DrawCenterPoint(ColorType.WaferCenter);

                // Wafer Edge Draw                
                DrawPolygon(PolygonPt);
                ReDrawWFCenter(ColorType.WaferCenter);

                // Save Recipe
                SetRecipeMapData(MapData, (int)outMapX, (int)outMapY, (int)outOriginX, (int)outOriginY, (int)outChipSzX, (int)outChipSzY);

                m_RecipeData_Origin.Backside_CenterX = (int)centX;
                m_RecipeData_Origin.Backside_CenterY = (int)centY;
                m_RecipeData_Origin.Backside_Radius = (int)outRadius;

                //SaveContourMap((int)centX, (int)centY, (int)outRadius);
            }
        }
        
        private void ReDrawPolygon()
        {
            if (WAFEREDGE_UI == null)
                return;

            int idx = 0;

            Point[] arrPt = new Point[WAFEREDGE_UI.Points.Count];
            foreach (CPoint pt in PolygonPt)
            {
                CPoint cPt = new CPoint();
                cPt = GetCanvasPoint(new CPoint((int)pt.X, (int)pt.Y));

                arrPt[idx++] = new Point(cPt.X, cPt.Y);
            }
            idx = 0;

            foreach (Point pt in arrPt)
            {
                WAFEREDGE_UI.Points[idx++] = pt;
            }
        }
        private void ReDrawCircle()
        {
            if (CIRCLE_UI == null)
                return;

            if (p_ViewElement.Contains(CIRCLE_UI))
                p_ViewElement.Remove(CIRCLE_UI);

            CPoint WFCenter = GetCanvasPoint(new CPoint(p_CenterPoint.X, p_CenterPoint.Y));

            EllipseGeometry data = CIRCLE_UI.Data as EllipseGeometry;
            data.Center = new Point(WFCenter.X, WFCenter.Y);

            int radius = p_CenterPoint.X + p_Radius;
            CPoint temp = GetCanvasPoint(new CPoint(radius, radius));
            int CanvasRadius = temp.X - WFCenter.X;

            data.RadiusX = (double)CanvasRadius;
            data.RadiusY = (double)CanvasRadius;
            p_ViewElement.Add(CIRCLE_UI);
        }
        private void ReDrawWFCenter(ColorType color)
        {
            if (CENTERPOINT_UI == null)
                return;

            if (p_ViewElement.Contains(CENTERPOINT_UI))
                p_ViewElement.Remove(CENTERPOINT_UI);
            
            foreach (UIElement ui in CENTERPOINT_UI.Children)
            {
                Line line = ui as Line;
                line.Stroke = GetColorBrushType(color);
            }

            CPoint WFCenter = GetCanvasPoint(new CPoint(p_CenterPoint.X, p_CenterPoint.Y));
            Canvas.SetLeft(CENTERPOINT_UI, WFCenter.X - 10);
            Canvas.SetTop(CENTERPOINT_UI, WFCenter.Y - 10);

            p_ViewElement.Add(CENTERPOINT_UI);
        }
        private void RedrawShapes()
        {
            foreach (TShape shape in Shapes)
            {
                TRect rect = shape as TRect;
                CPoint LT = new CPoint(rect.MemoryRect.Left, rect.MemoryRect.Top);
                CPoint RB = new CPoint(rect.MemoryRect.Right, rect.MemoryRect.Bottom);

                CPoint canvasLT = new CPoint(GetCanvasPoint(LT));
                CPoint canvasRB = new CPoint(GetCanvasPoint(RB));

                rect.CanvasRect.Width = canvasRB.X - canvasLT.X;
                rect.CanvasRect.Height = canvasRB.Y - canvasLT.Y;
                Canvas.SetLeft(rect.CanvasRect, canvasLT.X);
                Canvas.SetTop(rect.CanvasRect, canvasLT.Y);
                Canvas.SetRight(rect.CanvasRect, canvasRB.X);
                Canvas.SetBottom(rect.CanvasRect, canvasRB.Y);
            }
        }
        public override void SetRoiRect()
        {
            base.SetRoiRect();
            ReDrawPolygon();
            ReDrawCircle();
            RedrawShapes();
            if (p_ViewElement.Contains(WAFEREDGE_UI))
                ReDrawWFCenter(ColorType.WaferCenter);
            else
                ReDrawWFCenter(ColorType.Teaching); 
        }
        public override void CanvasMovePoint_Ref(CPoint point, int nX, int nY)
        {
            base.CanvasMovePoint_Ref(point, nX, nY);
            ReDrawPolygon();
            ReDrawCircle();
            RedrawShapes();
            if (p_ViewElement.Contains(WAFEREDGE_UI))
                ReDrawWFCenter(ColorType.WaferCenter);
            else
                ReDrawWFCenter(ColorType.Teaching);
        }

        private System.Windows.Media.SolidColorBrush GetColorBrushType(ColorType color)
        {
            switch (color) // 색상 정리 다시...
            {
                case ColorType.Teaching:
                    return Brushes.Blue;
                case ColorType.WaferEdge:
                    return Brushes.Green;
                case ColorType.WaferCenter:
                    return Brushes.Magenta;
                case ColorType.MapData:
                    return Brushes.Yellow;
                default:
                    return Brushes.Black;
            }
        }

        public enum ColorType
        {
            Teaching,
            WaferEdge,
            WaferCenter,
            MapData,
        }
        private void SetShapeColor(ColorType color)
        {
            switch (color)
            {
                case ColorType.MapData:
                    rectInfo = new TRect(Brushes.LimeGreen, 1, 1);
                    break;
                default:
                    rectInfo = new TRect(Brushes.Black, 1, 1);
                    break;
            }

        }
        public ICommand OK
        {
            get
            {
                return new RelayCommand(_StartRecipeTeaching);
            }
        }
        public void SaveContourMap(int CenterX, int CenterY, int radius)
        {
            int DownSample = 1;

            int nMemH = p_ImageData.p_Size.Y;
            int nMemW = p_ImageData.p_Size.X;
          
            // 시각화(Contour Map)하고자 하는 Image Data
            byte[] pRawImg = new byte[(radius * 2) * (radius * 2)];
            // Contour Map
            byte[] pContourMapImg = new byte[(radius * 2) * 3 * (radius * 2)];

            nMemW /= DownSample; nMemH /= DownSample;
            CenterX /= DownSample; CenterY /= DownSample;
            radius /= DownSample;      

            unsafe
            {
                if (p_eColorViewMode == eColorViewMode.All)
                {
                    // SubSample
                    byte[][] pImg = new byte[p_ImageData.p_nByte][];
                    // Wafer ROI 만큼만 잘라냄
                    byte[][] pROIImg = new byte[p_ImageData.p_nByte][];

                    for (int i = 0; i < p_ImageData.p_nByte; i++)
                    {
                        pImg[i] = new byte[(long)nMemW * (long)nMemH];
                        pROIImg[i] = new byte[(long)nMemW * (long)nMemH];

                        CLR_IP.Cpp_SubSampling((byte*)p_ImageData.GetPtr(i), pImg[i], p_ImageData.p_Size.X, p_ImageData.p_Size.Y, 0, 0, p_ImageData.p_Size.X, p_ImageData.p_Size.Y, DownSample);
   
                        CLR_IP.Cpp_CutOutROI(pImg[i], pROIImg[i], nMemW, nMemH, CenterX - radius, CenterY - radius, CenterX + radius, CenterY + radius);
                    }

                    CLR_IP.Cpp_ConvertRGB2H(pROIImg[0], pROIImg[1], pROIImg[2], pRawImg, radius * 2, radius * 2);
                }
                else 
                {
                    byte[] pImg = new byte[(long)nMemW * (long)nMemH];
                    byte[] pROIImg = new byte[(long)nMemW  * (long)nMemH];

                    CLR_IP.Cpp_SubSampling((byte*)p_ImageData.GetPtr((int)p_eColorViewMode - 1), pImg, p_ImageData.p_Size.X, p_ImageData.p_Size.Y, 0, 0, p_ImageData.p_Size.X, p_ImageData.p_Size.Y, DownSample);

                    CLR_IP.Cpp_CutOutROI(pImg, pRawImg, nMemW, nMemH, CenterX - radius, CenterY - radius, CenterX + radius, CenterY + radius);

                }

                CLR_IP.Cpp_DrawContourMap(pRawImg, pContourMapImg, radius * 2, radius * 2);

                string sImagePath = @"D:\ContourMap.bmp";
                CLR_IP.Cpp_SaveBMP(sImagePath, pContourMapImg, radius * 2 * p_ImageData.p_nByte, radius * 2, p_ImageData.p_nByte);
            }
        }
    }
}
