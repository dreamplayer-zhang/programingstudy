using RootTools;
using System;
using System.Collections.Generic;
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

    public delegate void BacksideSetupROIDoneEvent(CPoint centerPt, int radius);

    public class BacksideSetup_ImageViewer_ViewModel : RootViewer_ViewModel
    {
        #region [ColorDefines]
        public class ColorDefines
        {
            public static SolidColorBrush Circle  = Brushes.Blue;
            public static SolidColorBrush SearchCenterPoint = Brushes.Yellow;
            public static SolidColorBrush SearchCircle = Brushes.Yellow;
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
                SetProperty<bool>(ref this.isROIChecked, value);
            }
        }

        public int CircleCenterMemX
        {
            get
            {
                int width = this.circleEndMemoryPoint.X - this.circleStartMemoryPoint.X;
                int height = this.circleEndMemoryPoint.Y - this.circleStartMemoryPoint.Y;
                int radius = width > height ? width : height;
                radius = (int)((double)radius * 0.5);

                return this.circleStartMemoryPoint.X + radius;
            }
        }

        public int CircleCenterMemY
        {
            get
            {
                int width = this.circleEndMemoryPoint.X - this.circleStartMemoryPoint.X;
                int height = this.circleEndMemoryPoint.Y - this.circleStartMemoryPoint.Y;
                int radius = width > height ? width : height;
                radius = (int)((double)radius * 0.5);

                return this.circleStartMemoryPoint.Y + radius;
            }
        }


        public bool IsROI
        {
            get => this.p_UIElement.Contains(this.CircleUI);
        }
        #endregion

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

        private CIRCLE_DRAW_STATE circleState = CIRCLE_DRAW_STATE.None;
        private CIRCLE_EDIT_STATE circleEditState = CIRCLE_EDIT_STATE.None;

        private Ellipse CircleUI;
        private Grid CircleEditUI;
        private const int EditPointSize = 8;

        private CPoint circleStartMemoryPoint = new CPoint();
        private CPoint circleEndMemoryPoint = new CPoint();

        private bool isDrawing = false;
        //

        private Grid SearchedCenterPointUI;
        private Polygon SearchedCircleUI;

        private CPoint searchedCenterMemoryPoint = new CPoint();
        private List<CPoint> searchedCirclePoints = new List<CPoint>();


        #region [Initilize UIElements]
        private void InitializeUIElements()
        {
            #region [ROI]
            CircleUI = new Ellipse();
            CircleUI.Fill = ColorDefines.Circle;
            CircleUI.Stroke = ColorDefines.Circle;
            CircleUI.StrokeThickness = 1;
            CircleUI.Opacity = 0.5;
            
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

            CircleEditUI.Children.Add(CenterPoint);
            CircleEditUI.Children.Add(LeftPoint);
            CircleEditUI.Children.Add(TopPoint);
            CircleEditUI.Children.Add(RightPoint);
            CircleEditUI.Children.Add(BottomPoint);

            #endregion

            #region [SearchedCircle]
            SearchedCenterPointUI = new Grid();
            SearchedCenterPointUI.Width = 15;
            SearchedCenterPointUI.Height = 15;

            Line line1 = new Line(); // H
            line1.X1 = -15;
            line1.Y1 = 0;
            line1.X2 = 15;
            line1.Y2 = 0;
            line1.Stroke = ColorDefines.SearchCenterPoint;
            line1.StrokeThickness = 2;

            Line line2 = new Line(); // H
            line2.X1 = 0;
            line2.Y1 = -15;
            line2.X2 = 0;
            line2.Y2 = 15;
            line2.Stroke = ColorDefines.SearchCenterPoint;
            line2.StrokeThickness = 2;

            SearchedCenterPointUI.Children.Add(line1);
            SearchedCenterPointUI.Children.Add(line2);

            SearchedCircleUI = new Polygon();
            SearchedCircleUI.Stroke = ColorDefines.SearchCircle;
            SearchedCircleUI.StrokeThickness = 2;
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
        #endregion

        private void DrawCircle()
        {
            if(this.p_UIElement.Contains(CircleUI))
            {
                CPoint canvasStartPt = GetCanvasPoint(this.circleStartMemoryPoint);
                CPoint canvasEndPt = GetCanvasPoint(this.circleEndMemoryPoint);

                int width = Math.Abs(canvasStartPt.X - canvasEndPt.X);
                int height = Math.Abs(canvasEndPt.Y - canvasStartPt.Y);
                int radius = width > height ? width : height;

                CircleUI.Width = radius;
                CircleUI.Height = radius;

                Canvas.SetLeft(CircleUI, canvasStartPt.X);
                Canvas.SetTop(CircleUI, canvasStartPt.Y);

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

                if(ROIDone != null)
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
            if(this.p_UIElement.Contains(SearchedCircleUI))
            {
                CPoint canvasPt = GetCanvasPoint(this.searchedCenterMemoryPoint);

                Canvas.SetLeft(SearchedCenterPointUI, canvasPt.X);
                Canvas.SetTop(SearchedCenterPointUI, canvasPt.Y);
            }

            if(this.p_UIElement.Contains(SearchedCircleUI))
            {
                this.SearchedCircleUI.Points.Clear();
                foreach (CPoint pt in this.searchedCirclePoints)
                {
                    CPoint canvasPt = GetCanvasPoint(pt);
                    this.SearchedCircleUI.Points.Add(new Point(canvasPt.X, canvasPt.Y));
                }
            }
        }

        public void SetSearchedCenter(CPoint memPt)
        {
            this.searchedCenterMemoryPoint = memPt;
            if (this.p_UIElement.Contains(SearchedCenterPointUI))
                this.p_UIElement.Remove(SearchedCenterPointUI);

            this.p_UIElement.Add(SearchedCenterPointUI);

            DrawSearchedCircle();
        }

        public void SetSearchedCirclePoints(List<CPoint> points)
        {
            this.searchedCirclePoints = points;
            if (this.p_UIElement.Contains(SearchedCircleUI))
                this.p_UIElement.Remove(SearchedCircleUI);

            this.p_UIElement.Add(SearchedCircleUI);

            DrawSearchedCircle();
        }

        private void RedrawShapes()
        {
            DrawCircle();
            DrawCircleEdit();

            DrawSearchedCircle();            
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
        #endregion
    }
}
