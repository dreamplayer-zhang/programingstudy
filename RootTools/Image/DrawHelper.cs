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

namespace RootTools
{

    public class ModifyManager
    {
        public enum HitType
        {
            None, Body, UL, UR, LR, LL, L, R, T, B
        };

        public bool p_ModifyState = false;
        public bool p_SetState = false;
        public bool p_SetStateDone = false;

        private DrawToolVM m_ModifyDrawerTool = null;
        private DrawToolVM p_ModifyDrawerTool
        {
            get { return m_ModifyDrawerTool; }
            set { m_ModifyDrawerTool = value; }
        }
        public Shape p_ModifyTarget = null;
        public System.Windows.Shapes.Rectangle p_ModifyRect;
        public Rect p_ModifyData;
        private Rect p_ModifyStartData;


        ImageViewer_ViewModel p_ImageViewer;


        //public bool p_State = false;


        public CPoint Rect_StartPt;
        public CPoint Rect_EndPt;
        public CPoint PreMousePt;
        public HitType m_MouseHitType = HitType.None;



        public ModifyManager(ImageViewer_ViewModel _ImageViewer)
        {
            p_ImageViewer = _ImageViewer;
        }


        public HitType SetHitType(CPoint point)
        {
            double left = Canvas.GetLeft(p_ModifyRect);
            double top = Canvas.GetTop(p_ModifyRect);
            double right = left + p_ModifyRect.Width;
            double bottom = top + p_ModifyRect.Height;

            const double GAP = 10;
            if (point.X < left) return HitType.None;
            if (point.X > right) return HitType.None;
            if (point.Y < top) return HitType.None;
            if (point.Y > bottom) return HitType.None;
            if (-1 * GAP <= point.X - left && point.X - left <= GAP)
            {
                if (-1 * GAP <= point.Y - top && point.Y - top <= GAP)
                    return HitType.UL;
                if (-1 * GAP <= bottom - point.Y && bottom - point.Y <= GAP)
                    return HitType.LL;
                return HitType.L;
            }
            if (-1 * GAP < right - point.X && right - point.X <= GAP)
            {
                if (-1 * GAP <= point.Y - top && point.Y - top <= GAP)
                    return HitType.UR;
                if (-1 * GAP <= bottom - point.Y && bottom - point.Y <= GAP)
                    return HitType.LR;
                return HitType.R;
            }
            if (-1 * GAP <= point.Y - top && point.Y - top <= GAP)
                return HitType.T;
            if (-1 * GAP <= bottom - point.Y && bottom - point.Y <= GAP)
                return HitType.B;
            if (left == 0)
                return HitType.None;


            return HitType.Body;
        }
        public Cursor SetMouseCursor(HitType m_MouseHitType)
        {
            // See what cursor we should display.
            Cursor desired_cursor = Cursors.Arrow;
            switch (m_MouseHitType)
            {
                case HitType.None:
                    desired_cursor = Cursors.Arrow;
                    break;
                case HitType.Body:
                    desired_cursor = Cursors.ScrollAll;
                    break;
                case HitType.UL:
                case HitType.LR:
                    desired_cursor = Cursors.SizeNWSE;
                    break;
                case HitType.LL:
                case HitType.UR:
                    desired_cursor = Cursors.SizeNESW;
                    break;
                case HitType.T:
                case HitType.B:
                    desired_cursor = Cursors.SizeNS;
                    break;
                case HitType.L:
                case HitType.R:
                    desired_cursor = Cursors.SizeWE;
                    break;
            }
            // Display the desired cursor.
            if (p_ImageViewer.p_MouseCursor != desired_cursor) p_ImageViewer.p_MouseCursor = desired_cursor;

            return desired_cursor;
        }
        public void AdjustOrigin(CPoint CurrentPoint)
        {

            int offset_x = CurrentPoint.X - PreMousePt.X;
            int offset_y = CurrentPoint.Y - PreMousePt.Y;
            CPoint Offset = new CPoint(offset_x, offset_y);


            int new_x1 = (int)p_ModifyData.X;
            int new_y1 = (int)p_ModifyData.Y;

            int new_x2 = (int)(p_ModifyData.Width);
            int new_y2 = (int)(p_ModifyData.Height);

            switch (m_MouseHitType)
            {
                case HitType.Body:
                    new_x1 += Offset.X;
                    new_y1 += Offset.Y;
                    new_x2 += Offset.X;
                    new_y2 += Offset.Y;

                    break;
                case HitType.UL:
                    new_x1 += Offset.X;
                    new_y1 += Offset.Y;

                    break;
                case HitType.UR:
                    new_y1 += Offset.Y;
                    new_x2 += Offset.X;
                    break;
                case HitType.LR:
                    new_x2 += Offset.X;
                    new_y2 += Offset.Y;
                    break;
                case HitType.LL:
                    new_x1 += Offset.X;
                    new_y2 += Offset.Y;

                    break;
                case HitType.L:
                    new_x1 += Offset.X;
                    break;
                case HitType.R:
                    new_x2 += Offset.X;
                    break;
                case HitType.B:
                    new_y2 += Offset.Y;
                    break;
                case HitType.T:
                    new_y1 += Offset.Y;
                    break;
            }

            p_ModifyData.X = new_x1;
            p_ModifyData.Y = new_y1;
            p_ModifyData.Width = new_x2;
            p_ModifyData.Height = new_y2;

            CheckMinSize(ref p_ModifyData, 50);



            Point test1 = GetCanvasPoint(p_ModifyData.X, p_ModifyData.Y);
            Point test2 = GetCanvasPoint(p_ModifyData.Width, p_ModifyData.Height);

            PreMousePt = CurrentPoint;
            Redrawing();
            //p_ImageViewer.SetImageSource();
            //CPoint MemLeftTop = GetMemPoint((int)new_x, (int)new_y);
            //CPoint MemRightBot = GetMemPoint((int)(new_x + new_width), (int)(new_y + new_height));
            //m_DD.m_OriginData.m_rt.Left = MemLeftTop.X;
            //m_DD.m_OriginData.m_rt.Top = MemLeftTop.Y;
            //m_DD.m_OriginData.m_rt.Right = MemRightBot.X;
            //m_DD.m_OriginData.m_rt.Bottom = MemRightBot.Y;

        }

        public void ModifyStart()
        {
            PreMousePt = new CPoint(p_ImageViewer.p_MouseMemX, p_ImageViewer.p_MouseMemY);
            p_ModifyStartData = p_ModifyData;

        }

        public void ModifyEnd()
        {
            p_ImageViewer.m_HistoryWorker.AddHistory(p_ModifyDrawerTool, Work.Modify, p_ModifyTarget, p_ModifyStartData, p_ModifyTarget, p_ModifyData);
        }

        private void CheckMinSize(ref Rect _Data, int min)
        {
            int size = (int)(min * p_ImageViewer.p_View_Rect.Width / p_ImageViewer.p_CanvasWidth);
            if (size > _Data.Width - _Data.X)
            {
                _Data.Width = _Data.X + size;
            }
            size = (int)(min * p_ImageViewer.p_View_Rect.Height / p_ImageViewer.p_CanvasHeight);
            if (size > _Data.Height - _Data.Y)
            {
                _Data.Height = _Data.Y + size;
            }


        }

        public void Redrawing()
        {

            Point TopLeft = GetCanvasPoint(p_ModifyData.Location);
            SetTopLeft(p_ModifyRect, TopLeft);
            Point BottomRight = GetCanvasPoint(p_ModifyData.Size.Width, p_ModifyData.Size.Height);
            SetBottomRight(p_ModifyRect, BottomRight);
        }

        public void SetModifyData(DrawToolVM _DrawerToolVM, Shape _Shape)
        {

            if (p_ModifyTarget != null)
            {
                p_ModifyDrawerTool.m_ListShape.Add(p_ModifyTarget);
                p_ModifyDrawerTool.m_ListRect.Add(p_ModifyData);
            }

            p_SetState = true;
            p_ModifyDrawerTool = _DrawerToolVM;
            p_ModifyTarget = _Shape;

            int result = p_ModifyDrawerTool.m_ListShape.IndexOf(p_ModifyTarget);
            p_ModifyData = p_ModifyDrawerTool.m_ListRect[result];
            p_ModifyRect = new System.Windows.Shapes.Rectangle();

            p_ModifyDrawerTool.m_ListShape.RemoveAt(result);
            p_ModifyDrawerTool.m_ListRect.RemoveAt(result);




            Point TopLeft = GetCanvasPoint(p_ModifyData.Location);
            SetTopLeft(p_ModifyRect, TopLeft);
            Point BottomRight = GetCanvasPoint(p_ModifyData.Size.Width, p_ModifyData.Size.Height);
            SetBottomRight(p_ModifyRect, BottomRight);


            m_MouseHitType = SetHitType(new CPoint(p_ImageViewer.p_MouseX, p_ImageViewer.p_MouseY));
            SetMouseCursor(m_MouseHitType);

            p_ModifyRect.Stroke = System.Windows.Media.Brushes.GreenYellow;
            p_ModifyRect.StrokeDashArray = new DoubleCollection { 3, 2 };
            p_ImageViewer.p_Element.Add(p_ModifyRect);
            p_ModifyTarget.StrokeDashArray = p_ModifyDrawerTool.m_StrokeDashArray;

            p_ModifyDrawerTool.m_Element.Clear();
            foreach (UIElement myObj in p_ModifyDrawerTool.m_ListShape)
            {
                p_ModifyDrawerTool.m_Element.Add(myObj);
            }
            //p_ModifyDrawerTool.Redrawing();
            p_ImageViewer.SetImageSource();
        }

        public void DeleteModifyData()
        {
            if (p_ModifyTarget != null)
            {
                p_ModifyDrawerTool.m_ListShape.Add(p_ModifyTarget);
                p_ModifyDrawerTool.m_ListRect.Add(p_ModifyData);
                p_ModifyTarget.StrokeDashArray = new DoubleCollection(1);
            }

            p_ModifyDrawerTool.m_Element.Clear();
            foreach (UIElement myObj in p_ModifyDrawerTool.m_ListShape)
            {
                p_ModifyDrawerTool.m_Element.Add(myObj);
            }

            p_SetState = false;
            p_SetStateDone = false;
            p_ModifyState = false;
            p_ModifyDrawerTool = null;
            p_ModifyTarget = null;
            p_ModifyRect = null;

            p_ImageViewer.SetImageSource();
        }

        void SetTopLeft(Shape _shape, Point _point)
        {
            if (_shape.GetType() == typeof(Line))
            {
                ((Line)_shape).X1 = _point.X;
                ((Line)_shape).Y1 = _point.Y;
            }
            else
            {
                Canvas.SetLeft(_shape, _point.X);
                Canvas.SetTop(_shape, _point.Y);
            }
        }
        void SetBottomRight(Shape _shape, Point _point)
        {
            if (_shape.GetType() == typeof(Line))
            {
                ((Line)_shape).X2 = _point.X;
                ((Line)_shape).Y2 = _point.Y;
            }
            else
            {
                Point StartPoint = new Point(Canvas.GetLeft(_shape), Canvas.GetTop(_shape));
                if (_point.X > StartPoint.X)
                {
                    _shape.Width = _point.X - StartPoint.X;
                }
                else
                {
                    Canvas.SetLeft(_shape, _point.X);
                    _shape.Width = StartPoint.X - _point.X;
                }

                if (_point.Y > StartPoint.Y)
                {
                    _shape.Height = _point.Y - StartPoint.Y;
                }
                else
                {
                    Canvas.SetTop(_shape, _point.Y);
                    _shape.Height = StartPoint.Y - _point.Y;
                }
            }
        }


        Point GetCanvasPoint(Point mem)
        {
            if (p_ImageViewer.p_View_Rect.Width > 0 && p_ImageViewer.p_View_Rect.Height > 0)
            {

                int nX = (int)Math.Round((double)(mem.X - p_ImageViewer.p_View_Rect.X) * p_ImageViewer.p_CanvasWidth / p_ImageViewer.p_View_Rect.Width, MidpointRounding.ToEven);
                //int xx = (memX - p_ROI_Rect.X) * ViewWidth / p_ROI_Rect.Width;
                int nY = (int)Math.Round((double)(mem.Y - p_ImageViewer.p_View_Rect.Y) * p_ImageViewer.p_CanvasHeight / p_ImageViewer.p_View_Rect.Height, MidpointRounding.AwayFromZero);
                return new Point(nX, nY);
            }
            return new Point(0, 0);
        }

        Point GetCanvasPoint<T>(T memX, T memY)
        {
            if (p_ImageViewer.p_View_Rect.Width > 0 && p_ImageViewer.p_View_Rect.Height > 0)
            {

                int nX = (int)Math.Round((double)(Convert.ToDouble(memX) - p_ImageViewer.p_View_Rect.X) * p_ImageViewer.p_CanvasWidth / p_ImageViewer.p_View_Rect.Width, MidpointRounding.ToEven);
                //int xx = (memX - p_ROI_Rect.X) * ViewWidth / p_ROI_Rect.Width;
                int nY = (int)Math.Round((double)(Convert.ToDouble(memY) - p_ImageViewer.p_View_Rect.Y) * p_ImageViewer.p_CanvasHeight / p_ImageViewer.p_View_Rect.Height, MidpointRounding.AwayFromZero);
                return new Point(nX, nY);
            }
            return new Point(0, 0);
        }


        protected Point GetMemPoint(Point canvas)
        {
            double nX = p_ImageViewer.p_View_Rect.X + canvas.X * p_ImageViewer.p_View_Rect.Width / p_ImageViewer.p_CanvasWidth;
            double nY = p_ImageViewer.p_View_Rect.Y + canvas.Y * p_ImageViewer.p_View_Rect.Height / p_ImageViewer.p_CanvasHeight;
            return new Point(nX, nY);
        }
    }



    public class DrawHelper_old
    {
        ImageViewer_ViewModel p_ImageViewer;

        public bool p_State = false;
        public TextBlock DrawnTb = new TextBlock();
        public System.Windows.Shapes.Rectangle DrawnShape = new System.Windows.Shapes.Rectangle();
        public CPoint Shape_StartPt = new CPoint();
        public CPoint Shape_EndPt = new CPoint();
        public CRect preRect = new CRect();


        public DrawHelper_old(ImageViewer_ViewModel _ImageViewer)
        {
            p_ImageViewer = _ImageViewer;
        }

        public void SetShapeStyle(int nDash1, int nDash2)
        {
            DrawnShape.Stroke = System.Windows.Media.Brushes.Yellow;
            DrawnShape.StrokeThickness = 2;
            DrawnShape.StrokeDashArray = new DoubleCollection { nDash1, nDash2 };
            DrawnTb.Foreground = System.Windows.Media.Brushes.Red;
            DrawnTb.FontSize = 18;
        }



        public void StartDrawingShape()
        {
            //if (m_DrawHelper == null)
            //    m_DrawHelper = new DrawHelper();

            p_ImageViewer.p_ViewerUIElement.Clear();
            DrawnShape = new System.Windows.Shapes.Rectangle();
            DrawnTb = new TextBlock();

            Shape_StartPt = new CPoint(p_ImageViewer.p_MouseMemX, p_ImageViewer.p_MouseMemY);
            CPoint CanvasPt = GetCanvasPoint(p_ImageViewer.p_MouseMemX, p_ImageViewer.p_MouseMemY);

            Canvas.SetLeft(DrawnShape, CanvasPt.X);
            Canvas.SetTop(DrawnShape, CanvasPt.Y);
            SetShapeStyle(3, 2);

            p_ImageViewer.p_ViewerUIElement.Add(DrawnTb);
            p_ImageViewer.p_ViewerUIElement.Add(DrawnShape);

        }

        public void SetShapeElement_MemPos(CRect rect)
        {
            p_ImageViewer.p_ViewerUIElement.Clear();
            DrawnShape = new System.Windows.Shapes.Rectangle();
            DrawnTb = new TextBlock();


            Shape_StartPt = new CPoint(rect.Left, rect.Top);
            Shape_EndPt = new CPoint(rect.Right, rect.Bottom);

            int w = Shape_EndPt.X - Shape_StartPt.X;
            int h = Shape_EndPt.Y - Shape_StartPt.Y;
            string msg = "Width :" + w.ToString() + " Height :" + h.ToString();
            DrawnTb.Text = msg;

            CPoint StartPt = GetCanvasPoint(Shape_StartPt.X, Shape_StartPt.Y);
            CPoint EndPt = GetCanvasPoint(Shape_EndPt.X, Shape_EndPt.Y);
            Canvas.SetLeft(DrawnShape, StartPt.X);
            Canvas.SetTop(DrawnShape, StartPt.Y);
            DrawnShape.Width = Math.Abs(StartPt.X - EndPt.X);
            DrawnShape.Height = Math.Abs(StartPt.Y - EndPt.Y);
            SetShapeStyle(3, 2);
            p_ImageViewer.p_ViewerUIElement.Add(DrawnTb);
            DrawnShape.StrokeDashArray = new DoubleCollection(1);
            p_ImageViewer.p_ViewerUIElement.Add(DrawnShape);
        }


        public void RedrawShape()
        {
            //if (m_DrawHelper == null)
            //    return;
            if (DrawnShape == null)
                return;


            p_ImageViewer.p_ViewerUIElement.Remove(DrawnShape);
            DrawnShape = new System.Windows.Shapes.Rectangle();
            CPoint StartPt = GetCanvasPoint(Shape_StartPt.X, Shape_StartPt.Y);
            CPoint EndPt = GetCanvasPoint(Shape_EndPt.X, Shape_EndPt.Y);
            if (EndPt.X < StartPt.X)
                Canvas.SetLeft(DrawnShape, EndPt.X);
            else
                Canvas.SetLeft(DrawnShape, StartPt.X);
            if (EndPt.Y < StartPt.Y)
                Canvas.SetTop(DrawnShape, EndPt.Y);
            else
                Canvas.SetTop(DrawnShape, StartPt.Y);

            SetShapeStyle(1, 0);
            //DrawnRect.Stroke = System.Windows.Media.Brushes.Red;
            //DrawnRect.StrokeThickness = 2;
            DrawnShape.Width = Math.Abs(StartPt.X - EndPt.X);
            DrawnShape.Height = Math.Abs(StartPt.Y - EndPt.Y);
            p_ImageViewer.p_ViewerUIElement.Add(DrawnShape);

            //if (_RedrawDelegate != null)
            //    _RedrawDelegate();
        }

        public void DrawingShapeProgress()
        {
            if (DrawnShape != null)
            {
                Shape_EndPt = new CPoint(p_ImageViewer.p_MouseMemX, p_ImageViewer.p_MouseMemY);
                CPoint StartPt = GetCanvasPoint(Shape_StartPt.X, Shape_StartPt.Y);
                CPoint NowPt = GetCanvasPoint(p_ImageViewer.p_MouseMemX, p_ImageViewer.p_MouseMemY);


                Canvas.SetLeft(DrawnShape, StartPt.X);
                Canvas.SetTop(DrawnShape, StartPt.Y);

                if (Shape_EndPt.X < Shape_StartPt.X)
                {
                    Canvas.SetLeft(DrawnShape, NowPt.X);
                }
                if (Shape_EndPt.Y < Shape_StartPt.Y)
                {
                    Canvas.SetTop(DrawnShape, NowPt.Y);
                }

                DrawnShape.Width = Math.Abs(StartPt.X - NowPt.X);
                DrawnShape.Height = Math.Abs(StartPt.Y - NowPt.Y);

                int w = Shape_EndPt.X - Shape_StartPt.X;
                int h = Shape_EndPt.Y - Shape_StartPt.Y;

                string msg = "Width :" + w.ToString() + " Height :" + h.ToString();
                DrawnTb.Text = msg;
                DrawnShape.StrokeDashArray = new DoubleCollection(1);
            }
        }

        public void DrawShapeDone()
        {
            try
            {
                int w = Shape_EndPt.X - Shape_StartPt.X;
                int h = Shape_EndPt.Y - Shape_StartPt.Y;
                string msg = "Width :" + w.ToString() + " Height :" + h.ToString();
                DrawnTb.Text = msg;
                DrawnShape.StrokeDashArray = new DoubleCollection(1);
            }
            catch
            {
                return;
            }

        }

        public void Clear()
        {
            DrawnTb = new TextBlock();
            DrawnShape = new System.Windows.Shapes.Rectangle();
            Shape_StartPt = new CPoint();
            Shape_EndPt = new CPoint();
            p_ImageViewer.p_ViewerUIElement.Clear();
        }
        CPoint GetCanvasPoint(int memX, int memY)
        {
            if (p_ImageViewer.p_View_Rect.Width > 0 && p_ImageViewer.p_View_Rect.Height > 0)
            {

                int nX = (int)Math.Round((double)(memX - p_ImageViewer.p_View_Rect.X) * p_ImageViewer.p_CanvasWidth / p_ImageViewer.p_View_Rect.Width, MidpointRounding.ToEven);
                //int xx = (memX - p_ROI_Rect.X) * ViewWidth / p_ROI_Rect.Width;
                int nY = (int)Math.Round((double)(memY - p_ImageViewer.p_View_Rect.Y) * p_ImageViewer.p_CanvasHeight / p_ImageViewer.p_View_Rect.Height, MidpointRounding.AwayFromZero);
                return new CPoint(nX, nY);
            }
            return new CPoint(0, 0);
        }
    }


}
