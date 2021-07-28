using RootTools;
using RootTools_CLR;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml.Serialization;

namespace Root_WIND2.UI_User
{

    public delegate void BacksideSetupROIDoneEvent(CPoint centerPt, int radius);

    #region [Center Data]
    public class BacksideCircleData
    {
        public CPoint CenterPoint
        {
            get;
            set;
        }

        public int Radius
        {
            get;
            set;
        }

        public BacksideCircleData()
        {
            CenterPoint = new CPoint();
            Radius = 0;
        }
    }
    #endregion

    public class BacksideSetup_ImageViewer_ViewModel : RootViewer_ViewModel
    {
        #region [ColorDefines]
        public class ColorDefines
        {
            public static SolidColorBrush Circle  = Brushes.Blue;
            public static SolidColorBrush SearchCenterPoint = Brushes.Magenta;
            public static SolidColorBrush SearchCircle = Brushes.Yellow;
            public static SolidColorBrush Map = Brushes.YellowGreen;
            public static SolidColorBrush OutterMap = Brushes.Yellow;
            public static SolidColorBrush MapFill = Brushes.Transparent;

            public static SolidColorBrush ExclusivePolyStroke = Brushes.Red;
            public static SolidColorBrush ExclusivePolyFill = Brushes.Red;
        }

        #endregion




        public event BacksideSetupROIDoneEvent ROIDone;

        public BacksideSetup_ImageViewer_ViewModel()
        {
            this.p_VisibleMenu = System.Windows.Visibility.Collapsed;

            InitializeUIElements();
        }


        #region [Properties]
        private bool isROIChecked = false;
        public bool IsROIChecked
        {
            get => this.isROIChecked;
            set
            {
                if(value == true)
                {
                    this.IsPolyChecked = false;
                }
                SetProperty<bool>(ref this.isROIChecked, value);
            }
        }

        private bool isPolyChecked = false;
        public bool IsPolyChecked
        {
            get => this.isPolyChecked;
            set
            {
                if (value == true)
                {
                    this.IsROIChecked = false;

                    AddExclusivePolygon();
                }
                else
                {
                    int lastIndex = this.exclusivePolyMemPointsList.Count - 1;
                    if (lastIndex >= 0)
                    {
                        Polygon lastPoly = this.ExclusivePolyList[lastIndex];
                        if (lastPoly.Points.Count == 0)
                        {
                            this.p_UIElement.Remove(lastPoly);
                            this.ExclusivePolyList.Remove(lastPoly);
                            this.exclusivePolyMemPointsList.Remove(this.exclusivePolyMemPointsList[lastIndex]);
                        }
                    }
                }

                SetProperty<bool>(ref this.isPolyChecked, value);
            }
        }

        public CPoint CircleCenterPoint
        {
            get
            {
                return this.searchedCenterMemoryPoint;
            }
            set
            {
                SetProperty(ref this.searchedCenterMemoryPoint, value);
            }
        }

        


        public bool IsROI
        {
            get => this.p_UIElement.Contains(this.CircleUI);
        }

        public int InspectionCircleDiameter
        {
            get => this.inspectionCircleDiameter;
            set
            {
                SetProperty(ref this.inspectionCircleDiameter, value);
            }
        }
        #endregion


        public bool HitTest(Point hitPt)
        {
            //PathGeometry geometry = new PathGeometry();
            //PathFigure path = new PathFigure();
            //path.Segments.Add(new PolyLineSegment(this.exclusivePolyMemPointsList[0].ToList(), true));
            //geometry.Figures.Add(path);

            PathGeometry pathGeometry = new PathGeometry();

            foreach (List<Point> points in this.exclusivePolyMemPointsList)
            {
                PathFigure figure = new PathFigure();
                figure.StartPoint = points[0];

                PathSegmentCollection segments = new PathSegmentCollection();

                foreach (Point pt in points)
                {
                    if (points[0] == pt) continue;

                    LineSegment lineSegment = new LineSegment();
                    lineSegment.Point = pt;

                    segments.Add(lineSegment);
                }

                figure.Segments = segments;

                pathGeometry.Figures.Add(figure);
            }

            return pathGeometry.FillContains(hitPt);
        }

        #region [Draw Method]
        private enum CIRCLE_DRAW_STATE
        {
            None = 0,
            Start,
            End,
        }

        private enum CIRCLE_EDIT_STATE
        {
            None = 0,
            Center,
            Left,
            Top,
            Right,
            Bottom
        }

        private enum CIRCLE_CENTER_EDIT_STATE
        {
            None = 0,
            Edit
        }

        private CIRCLE_DRAW_STATE circleState = CIRCLE_DRAW_STATE.None;
        private CIRCLE_EDIT_STATE circleEditState = CIRCLE_EDIT_STATE.None;

        private CIRCLE_CENTER_EDIT_STATE circleCenterEditState = CIRCLE_CENTER_EDIT_STATE.None;

        private Ellipse CircleUI;
        private Grid CircleEditUI;
        private const int EditPointSize = 8;

        private CPoint circleStartMemoryPoint = new CPoint();
        private CPoint circleEndMemoryPoint = new CPoint();

        private bool isDrawing = false;
        //

        private Grid SearchedCenterPointUI;
        private Polygon SearchedCirclePointsUI;

        private int inspectionCircleDiameter = 0;
        private Ellipse InspectionCircleUI;


        private CPoint searchedCenterMemoryPoint = new CPoint();
        private List<CPoint> searchedCirclePoints = new List<CPoint>();

        private List<TRect> mapOutterRectList = new List<TRect>();
        private List<TRect> mapRectList = new List<TRect>();

        // Exclusive Region
        private List<Polygon> ExclusivePolyList;
        private List<List<Point>> exclusivePolyMemPointsList = new List<List<Point>>();


        public List<List<Point>> ExclusivePolyMemPointsList
        {
            get => this.exclusivePolyMemPointsList;
        }

        #region [Initilize UIElements]
        private void InitializeUIElements()
        {
            #region [ROI]
            CircleUI = new Ellipse();
            CircleUI.Fill = ColorDefines.Circle;
            CircleUI.Stroke = ColorDefines.Circle;
            CircleUI.StrokeThickness = 1;
            CircleUI.Opacity = 0.5;

            Canvas.SetZIndex(CircleUI, 1);

            CircleEditUI = new Grid();

            Ellipse CenterPoint = new Ellipse();
            CenterPoint.Width = EditPointSize;
            CenterPoint.Height = EditPointSize;
            CenterPoint.Fill = Brushes.White;
            CenterPoint.Stroke = Brushes.Gray;
            CenterPoint.StrokeThickness = 1;
            CenterPoint.Cursor = Cursors.Hand;
            CenterPoint.MouseLeftButtonDown += MouseLeftButtonDown_CircleEditUI_Center;
            CenterPoint.MouseLeftButtonUp += MouseLeftButtonUp_CircleEditUI_Center;
            CenterPoint.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            CenterPoint.VerticalAlignment = System.Windows.VerticalAlignment.Center;

            Ellipse LeftPoint = new Ellipse();
            LeftPoint.Width = EditPointSize;
            LeftPoint.Height = EditPointSize;
            LeftPoint.Fill = Brushes.White;
            LeftPoint.Stroke = Brushes.Gray;
            LeftPoint.StrokeThickness = 1;
            LeftPoint.Cursor = Cursors.SizeWE;
            LeftPoint.MouseLeftButtonDown += MouseLeftButtonDown_CircleEditUI_Left;
            LeftPoint.MouseLeftButtonUp += MouseLeftButtonUp_CircleEditUI_Left;
            LeftPoint.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            LeftPoint.VerticalAlignment = System.Windows.VerticalAlignment.Center;

            Ellipse TopPoint = new Ellipse();
            TopPoint.Width = EditPointSize;
            TopPoint.Height = EditPointSize;
            TopPoint.Fill = Brushes.White;
            TopPoint.Stroke = Brushes.Gray;
            TopPoint.StrokeThickness = 1;
            TopPoint.Cursor = Cursors.SizeNS;
            TopPoint.MouseLeftButtonDown += MouseLeftButtonDown_CircleEditUI_Top;
            TopPoint.MouseLeftButtonUp += MouseLeftButtonUp_CircleEditUI_Top;
            TopPoint.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            TopPoint.VerticalAlignment = System.Windows.VerticalAlignment.Top;

            Ellipse RightPoint = new Ellipse();
            RightPoint.Width = EditPointSize;
            RightPoint.Height = EditPointSize;
            RightPoint.Fill = Brushes.White;
            RightPoint.Stroke = Brushes.Gray;
            RightPoint.StrokeThickness = 1;
            RightPoint.Cursor = Cursors.SizeWE;
            RightPoint.MouseLeftButtonDown += MouseLeftButtonDown_CircleEditUI_Right;
            RightPoint.MouseLeftButtonUp += MouseLeftButtonUp_CircleEditUI_Right;
            RightPoint.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
            RightPoint.VerticalAlignment = System.Windows.VerticalAlignment.Center;

            Ellipse BottomPoint = new Ellipse();
            BottomPoint.Width = EditPointSize;
            BottomPoint.Height = EditPointSize;
            BottomPoint.Fill = Brushes.White;
            BottomPoint.Stroke = Brushes.Gray;
            BottomPoint.StrokeThickness = 1;
            BottomPoint.Cursor = Cursors.SizeNS;
            BottomPoint.MouseLeftButtonDown += MouseLeftButtonDown_CircleEditUI_Bottom;
            BottomPoint.MouseLeftButtonUp += MouseLeftButtonUp_CircleEditUI_Bottom;
            BottomPoint.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            BottomPoint.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;

            //Canvas.SetZIndex(CenterPoint, 97);
            //Canvas.SetZIndex(LeftPoint, 97);
            //Canvas.SetZIndex(TopPoint, 97);
            //Canvas.SetZIndex(RightPoint, 97);
            //Canvas.SetZIndex(BottomPoint, 97);

            

            CircleEditUI.Children.Add(CenterPoint);
            CircleEditUI.Children.Add(LeftPoint);
            CircleEditUI.Children.Add(TopPoint);
            CircleEditUI.Children.Add(RightPoint);
            CircleEditUI.Children.Add(BottomPoint);

            Canvas.SetZIndex(CircleEditUI, 2);

            #endregion

            #region [SearchedCircle]
            SearchedCenterPointUI = new Grid();
            SearchedCenterPointUI.Width = 30;
            SearchedCenterPointUI.Height = 30;

            Line line1 = new Line(); // H
            line1.X1 = 0;
            line1.Y1 = 0;
            line1.X2 = 30;
            line1.Y2 = 30;
            line1.Stroke = ColorDefines.SearchCenterPoint;
            line1.StrokeThickness = 3;

            Line line2 = new Line(); // H
            line2.X1 = 0;
            line2.Y1 = 30;
            line2.X2 = 30;
            line2.Y2 = 0;
            line2.Stroke = ColorDefines.SearchCenterPoint;
            line2.StrokeThickness = 3;

            //Canvas.SetZIndex(line1, 99);
            //Canvas.SetZIndex(line2, 99);

            Ellipse circleCenterEditPoint = new Ellipse();
            circleCenterEditPoint.Width = EditPointSize;
            circleCenterEditPoint.Height = EditPointSize;
            circleCenterEditPoint.Fill = Brushes.White;
            circleCenterEditPoint.Stroke = Brushes.Gray;
            circleCenterEditPoint.StrokeThickness = 1;
            circleCenterEditPoint.Cursor = Cursors.SizeAll;
            circleCenterEditPoint.MouseLeftButtonDown += MouseLeftButtonDown_CircleCenterEditUI_Bottom;
            circleCenterEditPoint.MouseLeftButtonUp += MouseLeftButtonUp_CircleCenterEditUI_Bottom;
            circleCenterEditPoint.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            circleCenterEditPoint.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            //Canvas.SetZIndex(circleCenterEditPoint, 99);

            SearchedCenterPointUI.Children.Add(line1);
            SearchedCenterPointUI.Children.Add(line2);
            SearchedCenterPointUI.Children.Add(circleCenterEditPoint);

            Canvas.SetZIndex(SearchedCenterPointUI, 3);

            SearchedCirclePointsUI = new Polygon();
            SearchedCirclePointsUI.Stroke = ColorDefines.SearchCircle;
            SearchedCirclePointsUI.StrokeThickness = 2;

            Canvas.SetZIndex(SearchedCirclePointsUI, 3);

            InspectionCircleUI = new Ellipse();
            InspectionCircleUI.Stroke = ColorDefines.SearchCenterPoint;
            InspectionCircleUI.StrokeThickness = 2;

            Canvas.SetZIndex(InspectionCircleUI, 3);

            #endregion


            #region [Exclusive Polygon]
            ExclusivePolyList = new List<Polygon>();
            #endregion

        }

        private void MouseLeftButtonDown_CircleEditUI_Center(object sender, MouseEventArgs e)
        {
            this.circleEditState = CIRCLE_EDIT_STATE.Center;
        }

        private void MouseLeftButtonUp_CircleEditUI_Center(object sender, MouseEventArgs e)
        {
            this.circleEditState = CIRCLE_EDIT_STATE.None;
        }

        private void MouseLeftButtonDown_CircleEditUI_Left(object sender, MouseEventArgs e)
        {
            this.circleEditState = CIRCLE_EDIT_STATE.Left;
        }

        private void MouseLeftButtonUp_CircleEditUI_Left(object sender, MouseEventArgs e)
        {
            this.circleEditState = CIRCLE_EDIT_STATE.None;
        }

        private void MouseLeftButtonDown_CircleEditUI_Top(object sender, MouseEventArgs e)
        {
            this.circleEditState = CIRCLE_EDIT_STATE.Top;
        }

        private void MouseLeftButtonUp_CircleEditUI_Top(object sender, MouseEventArgs e)
        {
            this.circleEditState = CIRCLE_EDIT_STATE.None;
        }

        private void MouseLeftButtonDown_CircleEditUI_Right(object sender, MouseEventArgs e)
        {
            this.circleEditState = CIRCLE_EDIT_STATE.Right;
        }

        private void MouseLeftButtonUp_CircleEditUI_Right(object sender, MouseEventArgs e)
        {
            this.circleEditState = CIRCLE_EDIT_STATE.None;
        }

        private void MouseLeftButtonDown_CircleEditUI_Bottom(object sender, MouseEventArgs e)
        {
            this.circleEditState = CIRCLE_EDIT_STATE.Bottom;
        }

        private void MouseLeftButtonUp_CircleEditUI_Bottom(object sender, MouseEventArgs e)
        {
            this.circleEditState = CIRCLE_EDIT_STATE.None;
        }

        private void MouseLeftButtonDown_CircleCenterEditUI_Bottom(object sender, MouseEventArgs e)
        {
            this.circleCenterEditState = CIRCLE_CENTER_EDIT_STATE.Edit;
        }

        private void MouseLeftButtonUp_CircleCenterEditUI_Bottom(object sender, MouseEventArgs e)
        {
            this.circleCenterEditState = CIRCLE_CENTER_EDIT_STATE.None;
        }

        #endregion

        private void DrawCircle()
        {
            if(this.p_UIElement.Contains(CircleUI))
            {
                //this.p_UIElement.Remove(CircleUI);
                //return;

                CPoint canvasStartPt = GetCanvasPoint(this.circleStartMemoryPoint);
                CPoint canvasEndPt = GetCanvasPoint(this.circleEndMemoryPoint);

                int width = Math.Abs(canvasStartPt.X - canvasEndPt.X);
                int height = Math.Abs(canvasEndPt.Y - canvasStartPt.Y);
                int radius = width > height ? width : height;

                CircleUI.Width = radius;
                CircleUI.Height = radius;

                Canvas.SetLeft(CircleUI, canvasStartPt.X);
                Canvas.SetTop(CircleUI, canvasStartPt.Y);
                //Canvas.SetZIndex(CircleUI, 99);

                DrawCircleEdit();
            }
        }

        private void DrawCircleEdit()
        {
            if (this.p_UIElement.Contains(CircleEditUI))
            {
                CPoint canvasStartPt = GetCanvasPoint(this.circleStartMemoryPoint);
                CPoint canvasEndPt = GetCanvasPoint(this.circleEndMemoryPoint);

                int width = Math.Abs(canvasStartPt.X - canvasEndPt.X);
                int height = Math.Abs(canvasEndPt.Y - canvasStartPt.Y);
                int radius = width > height ? width : height;
                radius += EditPointSize;

                CircleEditUI.Width = radius;
                CircleEditUI.Height = radius;
                Canvas.SetLeft(CircleEditUI, canvasStartPt.X - EditPointSize / 2);
                Canvas.SetTop(CircleEditUI, canvasStartPt.Y - EditPointSize / 2);


                if (ROIDone != null)
                {

                    int memW = this.circleEndMemoryPoint.X - this.circleStartMemoryPoint.X;
                    int memH = this.circleEndMemoryPoint.Y - this.circleStartMemoryPoint.Y;
                    int memR = memW > memH ? memW: memH;

                    ROIDone(new CPoint(this.circleStartMemoryPoint.X + memR/2, this.circleStartMemoryPoint.Y + memR/2), memR);
                }
                    
            }
        }

        public void DrawSearchedCircle()
        {
            if(this.p_UIElement.Contains(SearchedCirclePointsUI))
            {
                this.SearchedCirclePointsUI.Points.Clear();
                foreach (CPoint pt in this.searchedCirclePoints)
                {
                    CPoint canvasPt = GetCanvasPoint(pt);
                    this.SearchedCirclePointsUI.Points.Add(new Point(canvasPt.X, canvasPt.Y));
                }
            }
        }

        private void DrawInspectionROI()
        {
            if (this.p_UIElement.Contains(SearchedCenterPointUI))
            {
                CPoint canvasPt = GetCanvasPoint(this.searchedCenterMemoryPoint);

                Canvas.SetLeft(SearchedCenterPointUI, canvasPt.X - 15);
                Canvas.SetTop(SearchedCenterPointUI, canvasPt.Y - 15);
            }

            if (this.p_UIElement.Contains(InspectionCircleUI))
            {
                CameraInfo camInfo = DataConverter.GrabModeToCameraInfo(GlobalObjects.Instance.Get<WIND2_Engineer>().m_handler.p_BackSideVision.GetGrabMode(GlobalObjects.Instance.Get<RecipeBack>().CameraInfoIndex));

                int diameter = (int)(this.inspectionCircleDiameter * 1000 / camInfo.RealResX);
                int radius = diameter / 2;

                CPoint canvasCircleStart = GetCanvasPoint(new CPoint(
                    (int)this.searchedCenterMemoryPoint.X - radius,
                    (int)this.searchedCenterMemoryPoint.Y - radius));

                CPoint canvasCircleEnd = GetCanvasPoint(new CPoint(
                    (int)this.searchedCenterMemoryPoint.X + radius,
                    (int)this.searchedCenterMemoryPoint.Y + radius));

                int canvasDiameter = (int)(canvasCircleEnd.X - canvasCircleStart.X);

                InspectionCircleUI.Width = canvasDiameter;
                InspectionCircleUI.Height = canvasDiameter;

                Canvas.SetLeft(InspectionCircleUI, canvasCircleStart.X);
                Canvas.SetTop(InspectionCircleUI, canvasCircleStart.Y);
            }
        }


        public void AddExclusivePolygonPoint(CPoint memPt)
        {
            CPoint canvasPt = GetCanvasPoint(new CPoint((int)memPt.X, (int)memPt.Y));

            int lastIndex = this.exclusivePolyMemPointsList.Count - 1;

            this.exclusivePolyMemPointsList[lastIndex].Add(new Point(memPt.X, memPt.Y));
            this.ExclusivePolyList[lastIndex].Points.Add(new Point(canvasPt.X, canvasPt.Y));

            if (this.p_UIElement.Contains(this.ExclusivePolyList[lastIndex]) == false)
                this.p_UIElement.Add(this.ExclusivePolyList[lastIndex]);
        }


        public void AddExclusivePolygonPoint(Point memPt)
        {
            CPoint canvasPt = GetCanvasPoint(new CPoint((int)memPt.X, (int)memPt.Y));

            int lastIndex = this.exclusivePolyMemPointsList.Count - 1;

            this.exclusivePolyMemPointsList[lastIndex].Add(memPt);
            this.ExclusivePolyList[lastIndex].Points.Add(new Point(canvasPt.X, canvasPt.Y));

            if(this.p_UIElement.Contains(this.ExclusivePolyList[lastIndex]) == false)
                this.p_UIElement.Add(this.ExclusivePolyList[lastIndex]);
        }

        public void AddExclusivePolygon()
        {
            Polygon polygon = new Polygon();
            polygon.Stroke = ColorDefines.ExclusivePolyStroke;
            polygon.StrokeThickness = 2;
            polygon.Fill = ColorDefines.ExclusivePolyFill;
            polygon.Opacity = 0.3;

            this.ExclusivePolyList.Add(polygon);
            this.exclusivePolyMemPointsList.Add(new List<Point>());
        }

        public void ClearExclusivePolygon()
        {
            if(this.IsPolyChecked == true)
                this.IsPolyChecked = false;

            foreach(Polygon polygon in this.ExclusivePolyList)
            {
                if(this.p_UIElement.Contains(polygon))
                {
                    this.p_UIElement.Remove(polygon);
                }
            }
            this.ExclusivePolyList.Clear();
            this.exclusivePolyMemPointsList.Clear();
        }

        public void DrawExclusivePolygon()
        {
            int i = 0;
            foreach (Polygon polygon in this.ExclusivePolyList)
            {
                polygon.Points.Clear();
                foreach (Point pt in this.exclusivePolyMemPointsList[i])
                {
                    CPoint canvasPt = GetCanvasPoint(pt);
                    polygon.Points.Add(new Point(canvasPt.X, canvasPt.Y));
                }
                i++;
            }
        }

        public void SetMapRectList(List<CRect> rectList, List<CRect> outterRectList)
        {
            this.mapRectList.Clear();
            this.mapOutterRectList.Clear();
            this.p_DrawElement.Clear();

            foreach (CRect rt in rectList)
            {
                CPoint canvasLeftTop = GetCanvasPoint(new CPoint(rt.Left, rt.Top));
                CPoint canvasRightBottom = GetCanvasPoint(new CPoint(rt.Right, rt.Bottom));

                Rectangle rtUI = new Rectangle();
                rtUI.Width = canvasRightBottom.X - canvasLeftTop.X;
                rtUI.Height = canvasRightBottom.Y - canvasLeftTop.Y;

                rtUI.Stroke = ColorDefines.Map;
                rtUI.StrokeThickness = 2;
                rtUI.Opacity = 1;
                rtUI.Tag = "Map";
                rtUI.Fill = ColorDefines.MapFill;

                Canvas.SetLeft(rtUI, canvasLeftTop.X);
                Canvas.SetTop(rtUI, canvasLeftTop.Y);

                TRect tRect = new TRect();
                tRect.UIElement = rtUI;
                tRect.MemoryRect.Left = rt.Left;
                tRect.MemoryRect.Top = rt.Top;
                tRect.MemoryRect.Right = rt.Right;
                tRect.MemoryRect.Bottom = rt.Bottom;

                Canvas.SetZIndex(rtUI, 2);
                mapRectList.Add(tRect);
                p_DrawElement.Add(rtUI);
            }

            foreach (CRect rt in outterRectList)
            {
                CPoint canvasLeftTop = GetCanvasPoint(new CPoint(rt.Left, rt.Top));
                CPoint canvasRightBottom = GetCanvasPoint(new CPoint(rt.Right, rt.Bottom));

                Rectangle rtUI = new Rectangle();
                rtUI.Width = canvasRightBottom.X - canvasLeftTop.X;
                rtUI.Height = canvasRightBottom.Y - canvasLeftTop.Y;

                rtUI.Stroke = ColorDefines.OutterMap;
                rtUI.StrokeThickness = 2;
                rtUI.Opacity = 1;
                rtUI.Tag = "OutterMap";
                rtUI.Fill = ColorDefines.MapFill;

                Canvas.SetLeft(rtUI, canvasLeftTop.X);
                Canvas.SetTop(rtUI, canvasLeftTop.Y);

                TRect tRect = new TRect();
                tRect.UIElement = rtUI;
                tRect.MemoryRect.Left = rt.Left;
                tRect.MemoryRect.Top = rt.Top;
                tRect.MemoryRect.Right = rt.Right;
                tRect.MemoryRect.Bottom = rt.Bottom;

                mapOutterRectList.Add(tRect);
                p_DrawElement.Add(rtUI);
            }
        }

        public void DrawMapRectList()
        {
            if (this.mapRectList.Count > 0)
            {
                foreach (TRect rt in mapRectList)
                {
                    if (p_DrawElement.Contains(rt.UIElement) == true)
                    {
                        Rectangle rectangle = rt.UIElement as Rectangle;
                        CPoint canvasLeftTop = GetCanvasPoint(new CPoint(rt.MemoryRect.Left, rt.MemoryRect.Top));
                        CPoint canvasRightBottom = GetCanvasPoint(new CPoint(rt.MemoryRect.Right, rt.MemoryRect.Bottom));

                        rectangle.Width = canvasRightBottom.X - canvasLeftTop.X;
                        rectangle.Height = canvasRightBottom.Y - canvasLeftTop.Y;

                        Canvas.SetLeft(rectangle, canvasLeftTop.X);
                        Canvas.SetTop(rectangle, canvasLeftTop.Y);
                    }
                }

                foreach (TRect rt in mapOutterRectList)
                {
                    if (p_DrawElement.Contains(rt.UIElement) == true)
                    {
                        Rectangle rectangle = rt.UIElement as Rectangle;
                        CPoint canvasLeftTop = GetCanvasPoint(new CPoint(rt.MemoryRect.Left, rt.MemoryRect.Top));
                        CPoint canvasRightBottom = GetCanvasPoint(new CPoint(rt.MemoryRect.Right, rt.MemoryRect.Bottom));

                        rectangle.Width = canvasRightBottom.X - canvasLeftTop.X;
                        rectangle.Height = canvasRightBottom.Y - canvasLeftTop.Y;

                        Canvas.SetLeft(rectangle, canvasLeftTop.X);
                        Canvas.SetTop(rectangle, canvasLeftTop.Y);
                    }
                }
            }
        }

        public void SetSearchedCenter(CPoint memPt)
        {
            this.searchedCenterMemoryPoint = memPt;
            if (!this.p_UIElement.Contains(SearchedCenterPointUI))
            {
                this.p_UIElement.Add(SearchedCenterPointUI);
            }

            if (!this.p_UIElement.Contains(InspectionCircleUI))
            {
                this.p_UIElement.Add(InspectionCircleUI);

            }

            DrawInspectionROI();
        }

        public void SetSearchedCirclePoints(List<CPoint> points)
        {
            this.searchedCirclePoints = points;
            if (this.p_UIElement.Contains(SearchedCirclePointsUI))
                this.p_UIElement.Remove(SearchedCirclePointsUI);


            this.p_UIElement.Add(SearchedCirclePointsUI);

            DrawSearchedCircle();
        }

        private void RedrawShapes()
        {
            DrawMapRectList();

            DrawCircle();
            DrawCircleEdit();
            DrawSearchedCircle();

            DrawInspectionROI();

            // Polygon
            DrawExclusivePolygon();

            //Canvas.SetZIndex(CircleEditUI, 99);
        }

        #endregion

        #region [Overrides]
        public override void PreviewMouseDown(object sender, MouseEventArgs e)
        {
            base.PreviewMouseDown(sender, e);

            if (m_KeyEvent != null)
                if (m_KeyEvent.Key == Key.LeftShift && m_KeyEvent.IsDown)
                    return;

            CPoint canvasPt = new CPoint(p_MouseX, p_MouseY);
            CPoint memPt = GetMemPoint(canvasPt);


            //if (HitTest(new Point(memPt.X, memPt.Y)) == true)
            //    MessageBox.Show("Hit!!");

            if (this.IsPolyChecked == true)
            {
                AddExclusivePolygonPoint(memPt);
            }


            if(this.circleCenterEditState == CIRCLE_CENTER_EDIT_STATE.Edit)
            {
                searchedCenterMemoryPoint.X = memPt.X;
                searchedCenterMemoryPoint.Y = memPt.Y;

                DrawInspectionROI();
            }

            if(this.circleEditState == CIRCLE_EDIT_STATE.None)
            {
                switch (circleState)
                {
                    case CIRCLE_DRAW_STATE.None:
                        break;
                    case CIRCLE_DRAW_STATE.Start:
                        circleStartMemoryPoint.X = memPt.X;
                        circleStartMemoryPoint.Y = memPt.Y;

                        if (this.p_UIElement.Contains(CircleEditUI))
                            this.p_UIElement.Remove(CircleEditUI);

                        if (!this.p_UIElement.Contains(CircleUI))
                            this.p_UIElement.Add(CircleUI);
                        isDrawing = true;
                        circleState = CIRCLE_DRAW_STATE.End;

                        break;
                    case CIRCLE_DRAW_STATE.End:
                        circleEndMemoryPoint.X = memPt.X;
                        circleEndMemoryPoint.Y = memPt.Y;
                        isDrawing = false;

                        circleState = CIRCLE_DRAW_STATE.None;
                        this.IsROIChecked = false;

                        if (!this.p_UIElement.Contains(CircleEditUI))
                            this.p_UIElement.Add(CircleEditUI);

                        DrawCircleEdit();

                        break;
                }
            }
        }

        public override void MouseMove(object sender, MouseEventArgs e)
        {
            base.MouseMove(sender, e);

            if (m_KeyEvent != null)
                if (m_KeyEvent.Key == Key.LeftShift && m_KeyEvent.IsDown)
                    return;

            CPoint canvasPt = new CPoint(p_MouseX, p_MouseY);
            CPoint memPt = GetMemPoint(canvasPt);


            Line line = new Line();
            
            if (isDrawing == true)
            {
                circleEndMemoryPoint.X = memPt.X;
                circleEndMemoryPoint.Y = memPt.Y;

                if(circleStartMemoryPoint.X > circleEndMemoryPoint.X)
                {
                    int temp = circleStartMemoryPoint.X;
                    circleStartMemoryPoint.X = circleEndMemoryPoint.X;
                    circleEndMemoryPoint.X = temp;
                }

                if (circleStartMemoryPoint.Y > circleEndMemoryPoint.Y)
                {
                    int temp = circleStartMemoryPoint.Y;
                    circleStartMemoryPoint.X = circleEndMemoryPoint.Y;
                    circleEndMemoryPoint.Y = temp;
                }

                DrawCircle();
            }

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if(this.circleCenterEditState == CIRCLE_CENTER_EDIT_STATE.Edit)
                {
                    searchedCenterMemoryPoint.X = memPt.X;
                    searchedCenterMemoryPoint.Y = memPt.Y;

                    DrawInspectionROI();
                }

                switch (circleEditState)
                {
                    case CIRCLE_EDIT_STATE.None:
                        break;
                    case CIRCLE_EDIT_STATE.Center:
                        {
                            int width = this.circleEndMemoryPoint.X - this.circleStartMemoryPoint.X;
                            int height = this.circleEndMemoryPoint.Y - this.circleStartMemoryPoint.Y;
                            int radius = width > height ? width : height;
                            radius = (int)((double)radius * 0.5);

                            this.circleStartMemoryPoint.X = memPt.X - radius;
                            this.circleStartMemoryPoint.Y = memPt.Y - radius;
                            this.circleEndMemoryPoint.X = memPt.X + radius;
                            this.circleEndMemoryPoint.Y = memPt.Y + radius;
                            DrawCircle();
                        }
                        break;
                    case CIRCLE_EDIT_STATE.Left:
                        {
                            this.circleStartMemoryPoint.X = memPt.X;
                            int radius = this.circleEndMemoryPoint.X - this.circleStartMemoryPoint.X;
                            this.circleEndMemoryPoint.Y = this.circleStartMemoryPoint.Y + radius;

                            DrawCircle();
                        }
                        break;
                    case CIRCLE_EDIT_STATE.Top:
                        {
                            this.circleStartMemoryPoint.Y = memPt.Y;
                            int radius = this.circleEndMemoryPoint.Y - this.circleStartMemoryPoint.Y;
                            this.circleEndMemoryPoint.X = this.circleStartMemoryPoint.X + radius;
                            DrawCircle();
                        }
                        break;
                    case CIRCLE_EDIT_STATE.Right:
                        {
                            this.circleEndMemoryPoint.X = memPt.X;
                            int radius = this.circleEndMemoryPoint.X - this.circleStartMemoryPoint.X;
                            this.circleEndMemoryPoint.Y = this.circleStartMemoryPoint.Y + radius;
                            DrawCircle();
                        }
                        break;
                    case CIRCLE_EDIT_STATE.Bottom:
                        {
                            this.circleEndMemoryPoint.Y = memPt.Y;
                            int radius = this.circleEndMemoryPoint.Y - this.circleStartMemoryPoint.Y;
                            this.circleEndMemoryPoint.X = this.circleStartMemoryPoint.X + radius;
                            DrawCircle();
                        }
                        break;
                }
            }
            else
            {
                circleEditState = CIRCLE_EDIT_STATE.None;
            }
        }

        public override void PreviewMouseUp(object sender, MouseEventArgs e)
        {

        }
        public override void SetRoiRect()
        {
            base.SetRoiRect();

            RedrawShapes();
        }
        public override void CanvasMovePoint_Ref(CPoint point, int nX, int nY)
        {
            base.CanvasMovePoint_Ref(point, nX, nY);

            RedrawShapes();
        }

        public override void MouseWheel(object sender, MouseWheelEventArgs e)
        {
            base.MouseWheel(sender, e);

            RedrawShapes();

        }
        #endregion

        #region [Command]
        public RelayCommand btnOpenCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    this._openImage();
                });
            }
        }

        public RelayCommand btnSaveCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    this._saveImage();
                });
            }
        }

        public RelayCommand btnClearCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    this._clearImage();
                });
            }
        }

        public RelayCommand btnROICommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if(this.isROIChecked == true)
                    {
                        this.circleState = CIRCLE_DRAW_STATE.Start;
                    }
                    else
                    {
                        this.circleState = CIRCLE_DRAW_STATE.None;
                    }
                });
            }
        }

        public RelayCommand btnCircleDetectCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if(this.p_UIElement.Contains(this.CircleUI))
                    {
                        try
                        {
                            SearchCircleCenter();
                        }
                        catch(Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                    }
                    else
                    {
                        MessageBox.Show("ROI가 존재하지 않습니다.");
                    }
                });
            }
        }

        public RelayCommand btnCircleClearCommand
        {
            get => new RelayCommand(() =>
            {
                if (this.p_UIElement.Contains(this.CircleUI))
                {
                    this.p_UIElement.Remove(this.CircleUI);
                }

                if(this.p_UIElement.Contains(this.CircleEditUI))
                {
                    this.p_UIElement.Remove(this.CircleEditUI);
                }

                if(this.p_UIElement.Contains(this.SearchedCirclePointsUI))
                {
                    this.p_UIElement.Remove(this.SearchedCirclePointsUI);
                }
            });
        }

        public void ClearMapRectList()
        {
            this.mapRectList.Clear();
            this.mapOutterRectList.Clear();
            this.p_DrawElement.Clear();
        }


        public void SearchCircleCenter()
        {
            int roiW = this.circleEndMemoryPoint.X - this.circleStartMemoryPoint.X;
            int roiH = this.circleEndMemoryPoint.Y - this.circleStartMemoryPoint.Y;
            float roiR = roiW > roiH ? roiW : roiH;

            int roiCenterX = this.circleStartMemoryPoint.X + (int)roiR / 2;
            int roiCenterY = this.circleStartMemoryPoint.Y + (int)roiR / 2;


            ImageData imageData = this.p_ImageData;

            int memH = imageData.p_Size.Y;
            int memW = imageData.p_Size.X;

            float centerX = roiCenterX; // 레시피 티칭 값 가지고오기
            float centerY = roiCenterY;

            IntPtr mainImage = new IntPtr();

            mainImage = imageData.GetPtr(0);

            Cpp_Point[] circlePoints = null;
            unsafe
            {
                int DownSample = 20;
                int samplingW = memW / DownSample;
                int samplingH = memH / DownSample;

                fixed (byte* pDst = new byte[(long)(samplingW) * (long)(samplingH)]) // 원본 이미지 너무 커서 안열림
                {
                    CLR_IP.Cpp_SubSampling((byte*)mainImage, pDst, memW, memH, 0, 0, memW, memH, DownSample);

                    byte[] pThreshold = new byte[(long)(samplingW) * (long)(samplingH)];

                    CLR_IP.Cpp_Threshold(pDst, pThreshold, samplingW, samplingH, 0, 0, samplingW, samplingH, false, 80);

                    // Param Down Scale
                    centerX /= DownSample; centerY /= DownSample;
                    roiR /= DownSample;
                    memW /= DownSample; memH /= DownSample;

                    fixed(byte* ptrThread = pThreshold)
                    {
                        circlePoints = CLR_IP.Cpp_FindWaferEdge(ptrThread,
                        &centerX, &centerY,
                        &roiR,
                        memW, memH,
                        1
                        );
                    }
                }


                // Param Up Scale
                centerX *= DownSample;
                centerY *= DownSample;

                double outRadius = roiR * DownSample;

                PathGeometry geometry = PolygonController.CreatePolygonGeometry(this.ExclusivePolyMemPointsList);

                List<CPoint> points = new List<CPoint>();
                for (int i = 0; i < circlePoints.Length; i++)
                {
                    CPoint pt = new CPoint(circlePoints[i].x * DownSample, circlePoints[i].y * DownSample);
                    if (!PolygonController.HitTest(geometry, new Point(pt.X, pt.Y)))
                    {
                        double radius = Math.Sqrt(Math.Pow(pt.X - centerX, 2) + Math.Pow(pt.Y - centerY, 2));
                        if( radius < outRadius)
                            points.Add(pt);
                    }
                }

                Point centerPt = Tools.FindCircleCenterByPoints(DataConverter.CPointListToPointList(points), (int)centerX, (int)centerY, 100);

                this.SetSearchedCenter(new CPoint((int)centerPt.X, (int)centerPt.Y));
                this.SetSearchedCirclePoints(points);


            }
        }


        public RelayCommand btnSavePolyCommand
        {
            get => new RelayCommand(() =>
             {
                 this.IsPolyChecked = false;

                 SaveExclusivePolygon();
             });
        }

        public RelayCommand btnCancelPolyCommand
        {
            get => new RelayCommand(() =>
            {
                if(this.IsPolyChecked == true)
                {
                    this.IsPolyChecked = false;

                    if (this.exclusivePolyMemPointsList.Count != 0)
                    {
                        int lastIndex = this.exclusivePolyMemPointsList.Count - 1;
                        Polygon lastPoly = this.ExclusivePolyList[lastIndex];

                        if (lastPoly.Points.Count > 0)
                        {
                            this.p_UIElement.Remove(lastPoly);
                            this.ExclusivePolyList.Remove(lastPoly);
                            this.exclusivePolyMemPointsList.Remove(this.exclusivePolyMemPointsList[lastIndex]);
                        }
                    }
                }
            });
        }

        public RelayCommand btnClearPolyCommand
        {
            get => new RelayCommand(() =>
            {
                ClearExclusivePolygon();
            });
        }

        public RelayCommand btnCircleSaveCommand
        {
            get => new RelayCommand(() =>
             {
                 SaveCenterPoint();
             });
        }



        public void SaveCenterPoint()
        {
            try
            {
                lock (lockObj)
                {
                    if (File.Exists(Constants.FilePath.BacksideCenterPointFilePath))
                    {
                        string strTime = DateTime.Now.ToString("yyyyMMddhhmmss");
                        if (!File.Exists(Constants.RootPath.RootSetupPath + strTime + "_" + Constants.FileName.BacksideCenterPointFileName))
                        {
                            File.Move(Constants.FilePath.BacksideCenterPointFilePath, Constants.RootPath.RootSetupPath + strTime + "_" + Constants.FileName.BacksideCenterPointFileName);
                        }
                    }

                    using (StreamWriter wr = new StreamWriter(Constants.FilePath.BacksideCenterPointFilePath))
                    {
                        BacksideCircleData data = new BacksideCircleData();
                        data.CenterPoint = this.searchedCenterMemoryPoint;
                        data.Radius = this.inspectionCircleDiameter;

                        XmlSerializer xs = new XmlSerializer(typeof(BacksideCircleData));
                        xs.Serialize(wr, data);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void ReadCenterPoint()
        {
            try
            {
                if (File.Exists(Constants.FilePath.BacksideCenterPointFilePath))
                {        
                    using (var sr = new StreamReader(Constants.FilePath.BacksideCenterPointFilePath))
                    {
                        
                        XmlSerializer xs = new XmlSerializer(typeof(BacksideCircleData));
                        BacksideCircleData data = (BacksideCircleData)xs.Deserialize(sr);
                        CPoint centerPt = data.CenterPoint;
                        this.InspectionCircleDiameter = data.Radius;

                        SetSearchedCenter(centerPt);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        private readonly string ExclusivePolygonListFilePath = "Backside_ExclusivePolygonList.xml";

        private object lockObj = new object();
        public void SaveExclusivePolygon()
        {
            try
            {
                lock (lockObj)
                {
                    if (File.Exists(Constants.RootPath.RootSetupPath + ExclusivePolygonListFilePath))
                    {
                        string strTime = DateTime.Now.ToString("yyyyMMddhhmmss");
                        if (!File.Exists(Constants.RootPath.RootSetupPath + strTime + "_" + ExclusivePolygonListFilePath))
                        {
                            File.Move(Constants.RootPath.RootSetupPath + ExclusivePolygonListFilePath, Constants.RootPath.RootSetupPath + strTime + "_" + ExclusivePolygonListFilePath);
                        }
                    }

                    using (StreamWriter wr = new StreamWriter(Constants.RootPath.RootSetupPath + ExclusivePolygonListFilePath))
                    {
                        XmlSerializer xs = new XmlSerializer(typeof(List<List<Point>>));
                        xs.Serialize(wr, this.exclusivePolyMemPointsList);
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void ReadExclusivePolygon()
        {
            try
            {
                if (File.Exists(Constants.RootPath.RootSetupPath + ExclusivePolygonListFilePath))
                {
                    ClearExclusivePolygon();

                    using (var sr = new StreamReader(Constants.RootPath.RootSetupPath + ExclusivePolygonListFilePath))
                    {
                        XmlSerializer xs = new XmlSerializer(typeof(List<List<Point>>));
                        this.exclusivePolyMemPointsList = (List<List<Point>>)xs.Deserialize(sr);

                        foreach (List<Point> pointList in this.exclusivePolyMemPointsList)
                        {
                            Polygon polygon = new Polygon();

                            polygon.Stroke = ColorDefines.ExclusivePolyStroke;
                            polygon.StrokeThickness = 2;
                            polygon.Fill = ColorDefines.ExclusivePolyFill;
                            polygon.Opacity = 0.3;
                            this.ExclusivePolyList.Add(polygon);

                            foreach (Point pt in pointList)
                            {
                                CPoint canvasPt = GetCanvasPoint(pt);
                                polygon.Points.Add(new Point(canvasPt.X, canvasPt.Y));
                            }

                            this.p_UIElement.Add(polygon);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        #endregion
    }
}
