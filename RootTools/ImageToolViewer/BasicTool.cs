using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace RootTools
{
    public class BasicTool : ObservableObject
    {

        public BasicTool()
        {
            InitCrossLine();
            InitBasicShape(new Rectangle(), Brushes.Yellow, 2);
        }
        /// <summary>
        /// Selected Basic Tool
        /// </summary>
        /// 
        BasicShape basic;

        /// <summary>
        /// Cross Line
        /// </summary>
        Line Vertical, Horizon;

        public ToolState eToolState = ToolState.None;

        /// <summary>
        /// UI Proerty
        /// </summary>
        private ObservableCollection<UIElement> m_BasicElement = new ObservableCollection<UIElement>();
        public ObservableCollection<UIElement> p_BasicElement
        {
            get
            {
                return m_BasicElement;
            }
            set
            {
                m_BasicElement = value;
            }
        }

        private Cursor m_Cursor;
        public Cursor p_Cursor
        {
            get
            {
                return m_Cursor;
            }
            set
            {
                SetProperty(ref m_Cursor, value);
            }
        }

        public void InitBasicShape(Shape shapeType, Brush brush, double thickness)
        {
            basic = new BasicShape(shapeType, brush, thickness);

        }
        public void InitCrossLine()
        {
            if (p_BasicElement.Contains(Vertical) && p_BasicElement.Contains(Horizon))
            {
                p_BasicElement.Remove(Vertical);
                p_BasicElement.Remove(Horizon);
            }
            
            Vertical = new Line();
            Horizon = new Line();

            Brush LineBrush = Brushes.Silver;
            double LineThick = 1;
            DoubleCollection LineDash = new DoubleCollection { 3, 4 };

            Vertical.Stroke = LineBrush;
            Vertical.StrokeThickness = LineThick;
            Vertical.StrokeDashArray = LineDash;
            Horizon.Stroke = LineBrush;
            Horizon.StrokeThickness = LineThick;
            Horizon.StrokeDashArray = LineDash;

            p_BasicElement.Add(Vertical);
            p_BasicElement.Add(Horizon);
        }

        public void DrawCrossLine(CPoint nowPt, int canvasWidth, int canvasHeight)
        {
            try
            {
                Vertical.X1 = nowPt.X;
                Vertical.X2 = nowPt.X;

                Horizon.Y1 = nowPt.Y;
                Horizon.Y2 = nowPt.Y;

                
                Vertical.Y2 = canvasHeight;
                Horizon.X2 = canvasWidth;
            }
            catch(Exception e)
            {
                return;
            }
            return;
        }

        public virtual bool Draw(CPoint memPt, CPoint canvasPt, ToolState state = ToolState.None)
        {
            switch (eToolState)
            {
                case ToolState.None :
                    {
                        SetState(state);
                        return true;
                    }
                case ToolState.Start:
                    {
                        if (basic == null)
                            return false;

                        if (p_BasicElement.Contains(basic.m_CanvasShape))
                        {
                            p_BasicElement.Remove(basic.m_CanvasShape);
                            InitBasicShape(new Rectangle(), Brushes.Yellow, 2);
                        }
                        basic.SetStartPoint(memPt, canvasPt);
                        SetTopLeft(basic.m_CanvasShape, canvasPt);
                        p_BasicElement.Add(basic.m_CanvasShape);
                        SetState(ToolState.Drawing);
                        return true;
                    }
                case ToolState.Drawing:
                    {
                        if (state == ToolState.Start)
                        {
                            SetState(ToolState.Done);
                            basic.SetEndPoint(memPt, canvasPt);
                            return true;
                        }

                        CPoint startPt = basic.m_CanvasStartPt;
                        MakeShape(basic, startPt, canvasPt);
                        return true;
                    }
                case ToolState.Done:
                    {
                        p_Cursor = SetMouseCursor(SetHitType(basic, canvasPt));
                        return true;
                    }
                case ToolState.Adjust:
                    {
                        return true;
                    }
            }
            return true;
        }


        public bool StartDrawBasicTool(Shape shapeType, CPoint memStartPt, CPoint canvasStartPt)
        {
            if (basic == null)
                return false;

            try
            {
                SetState(ToolState.Drawing);
                basic.SetStartPoint(memStartPt, canvasStartPt);
                SetTopLeft(basic.m_CanvasShape, canvasStartPt);

                p_BasicElement.Add(basic.m_CanvasShape);
                
            }
            catch(Exception e)
            {
                MessageBox.Show(e.ToString());
                return false;
            }

            return true;
        }
        public bool Drawing_BasicTool(CPoint memNowPt, CPoint canvasNowPt)
        {
            if (eToolState != ToolState.Drawing)
                return false;

            try
            {               
                CPoint startPt = basic.m_CanvasStartPt;
                MakeShape(basic, startPt, canvasNowPt);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                return false;
            }

            return true;
        }
        public bool Drawing_Done(CPoint memEndPt, CPoint canvasEndPt)
        {
            return true;
        }


        public void MakeShape(BasicShape basicShape, CPoint canvasStartPt, CPoint canvasNowPt)
        {
            if (basicShape.m_CanvasShape.GetType() == typeof(Rectangle))
            {
                Rectangle rect = basicShape.m_CanvasShape as Rectangle;

                if (canvasStartPt.X > canvasNowPt.X)
                {
                    Canvas.SetLeft(rect, canvasNowPt.X);
                }

                if (canvasStartPt.Y > canvasNowPt.Y)
                {
                    Canvas.SetTop(rect, canvasNowPt.Y);
                }
                rect.Width = Math.Abs(canvasNowPt.X - canvasStartPt.X);
                rect.Height = Math.Abs(canvasNowPt.Y - canvasStartPt.Y);

            }
            
            
        }
        public void SetTopLeft(Shape shape, CPoint point)
        {
            if (shape.GetType() == typeof(Line))
            {
                ((Line)shape).X1 = point.X;
                ((Line)shape).Y1 = point.Y;
            }
            else
            {
                Canvas.SetLeft(shape, point.X);
                Canvas.SetTop(shape, point.Y);
            }
        }
        public ToolState SetState(ToolState state)
        {
            eToolState = state;
            return state;
        }

        public HitType SetHitType(BasicShape shape, CPoint canvasNowPt)
        {
            double left = Canvas.GetLeft(shape.m_CanvasShape);
            double top = Canvas.GetTop(shape.m_CanvasShape);
            double right = left + shape.m_CanvasShape.Width;
            double bottom = top + shape.m_CanvasShape.Height;

            const double GAP = 15;
            if (canvasNowPt.X < left)
                return HitType.None;
            if (canvasNowPt.X > right)
                return HitType.None;
            if (canvasNowPt.Y < top)
                return HitType.None;
            if (canvasNowPt.Y > bottom)
                return HitType.None;
            if (-1 * GAP <= canvasNowPt.X - left && canvasNowPt.X - left <= GAP)
            {
                if (-1 * GAP <= canvasNowPt.Y - top && canvasNowPt.Y - top <= GAP)
                    return HitType.UL;
                if (-1 * GAP <= bottom - canvasNowPt.Y && bottom - canvasNowPt.Y <= GAP)
                    return HitType.LL;
                return HitType.L;
            }
            if (-1 * GAP < right - canvasNowPt.X && right - canvasNowPt.X <= GAP)
            {
                if (-1 * GAP <= canvasNowPt.Y - top && canvasNowPt.Y - top <= GAP)
                    return HitType.UR;
                if (-1 * GAP <= bottom - canvasNowPt.Y && bottom - canvasNowPt.Y <= GAP)
                    return HitType.LR;
                return HitType.R;
            }
            if (-1 * GAP <= canvasNowPt.Y - top && canvasNowPt.Y - top <= GAP)
                return HitType.T;
            if (-1 * GAP <= bottom - canvasNowPt.Y && bottom - canvasNowPt.Y <= GAP)
                return HitType.B;
            if (left == 0)
                return HitType.None;


            return HitType.Body;
        }        
        public Cursor SetMouseCursor(HitType m_MouseHitType)
        {
            // See what cursor we should display.
            Cursor _cursor = Cursors.Arrow;
            switch (m_MouseHitType)
            {
                case HitType.None:
                    _cursor = Cursors.Arrow;
                    break;
                case HitType.Body:
                    _cursor = Cursors.ScrollAll;
                    break;
                case HitType.UL:
                case HitType.LR:
                    _cursor = Cursors.SizeNWSE;
                    break;
                case HitType.LL:
                case HitType.UR:
                    _cursor = Cursors.SizeNESW;
                    break;
                case HitType.T:
                case HitType.B:
                    _cursor = Cursors.SizeNS;
                    break;
                case HitType.L:
                case HitType.R:
                    _cursor = Cursors.SizeWE;
                    break;
            }

            return _cursor;
        }
        public void test()
        {
            
            
        }
    }
    


    public class BasicShape
    {
        /// <summary>
        /// Basic Shape Init
        /// </summary>
        /// <param name="shapeType">Rectangle, Line, others..</param>
        /// <param name="brush">Shape Brush</param>
        /// <param name="thickness">Shape Line Thickness</param>
        public BasicShape(Shape shapeType, Brush brush, double thickness)
        {
            m_CanvasShape = shapeType;
            m_CanvasShape.Stroke = brush;
            m_CanvasShape.StrokeThickness = thickness;
        }
        public void SetStartPoint(CPoint memStartPt, CPoint canvasStartPt)
        {
            m_MemoryStartPt = memStartPt;
            m_CanvasStartPt = canvasStartPt;
        }
        public void SetEndPoint(CPoint memEndPt, CPoint canvasEndPt)
        {
            m_MemoryEndPt = memEndPt;
            m_CanvasEndPt = canvasEndPt;
            
        }

        /// <summary>
        /// Shape in Canvas
        /// </summary>
        public Shape m_CanvasShape;
        /// <summary>
        /// Object in Memory
        /// </summary>
        public Rect m_MemoryObject;
        /// <summary>
        /// Shape(Rectangle, Line) Canvas Start Point
        /// </summary>
        public CPoint m_CanvasStartPt;
        
        private CPoint _canvasEndPt;
        /// <summary>
        /// Shape(Rectangle, Line) Canvas End Point
        /// </summary>
        public CPoint m_CanvasEndPt
        {
            get
            {
                return _canvasEndPt;
            }
            set
            {
                m_CanvasShape.Width = Math.Abs(value.X - m_CanvasStartPt.X);
                m_CanvasShape.Height= Math.Abs(value.Y - m_CanvasStartPt.Y);

                _canvasEndPt = value;

            }
        }
        /// <summary>
        /// Memory Start Point
        /// </summary>
        public CPoint m_MemoryStartPt;
        private CPoint _memoryEndPt;
        /// <summary>
        /// Memory End Point
        /// </summary>
        public CPoint m_MemoryEndPt
        {
            get
            {
                return _memoryEndPt;
            }
            set
            {
                m_MemoryObject.Width = Math.Abs(value.X - m_MemoryStartPt.X);
                m_MemoryObject.Height = Math.Abs(value.Y - m_MemoryStartPt.Y);
                _memoryEndPt = value;
            }
        }

    }

    public enum ToolState
    {
        None,
        Start,
        Drawing,
        Adjust,
        Done,
    }
    public enum HitType
    {
        None, Body, UL, UR, LR, LL, L, R, T, B
    };

}
namespace RootTools.ToolShapess
{
    public abstract class ToolShapes
    {

    }

    public class PointLine : ToolShapes
    {
        public CPoint StartPt;
        public int Width;
    }
    public class TPoint
    {
        public TPoint()
        {
            CanvasPoint = new CPoint();
            MemoryPoint = new CPoint();
        }
        public TPoint(CPoint canvastPt, CPoint memPt)
        {
            CanvasPoint = canvastPt;
            MemoryPoint = memPt;
        }
        public CPoint CanvasPoint;
        public CPoint MemoryPoint;
    }
    public class TLine
    {
        public TLine(Brush brush, double thickness)
        {
            CanvasLine = new Line();
            CanvasLine.Stroke = brush;
            CanvasLine.StrokeThickness = thickness;
            MemoryLine = new List<PointLine>();
        }
        public Line CanvasLine;
        public CPoint CanvasStartPoint
        {
            get
            {
                return new CPoint((int)CanvasLine.X1, (int)CanvasLine.Y1);
            }
            set
            {
                CanvasLine.X1 = value.X;
                CanvasLine.Y1 = value.Y;
            }
        }
        public CPoint CanvasEndPoint
        {
            get
            {
                return new CPoint((int)CanvasLine.X2, (int)CanvasLine.Y2);
            }
            set
            {
                CanvasLine.X2 = value.X;
                CanvasLine.Y2 = value.Y;
            }

        }

        public List<PointLine> MemoryLine;
    }
    public class TRect
    {
        public TRect(Brush brush, double thickness)
        {
            CanvasRect = new Rectangle();
            CanvasRect.Stroke = brush;
            CanvasRect.StrokeThickness = thickness;
        }
        public void SetLeftTop(CPoint canvasPt, CPoint memPt)
        {
            CanvasLeft = canvasPt.X;
            CanvasTop = canvasPt.Y;
            MemoryRect.Left = memPt.X;
            MemoryRect.Top = memPt.Y;
        }
        public void SetRightBottom(CPoint canvasPt, CPoint memPt)
        {
            CanvasRight = canvasPt.X;
            CanvasBottom = canvasPt.Y;
            MemoryRect.Right = memPt.X;
            MemoryRect.Bottom = memPt.Y;
        }
        public Rectangle CanvasRect;
        public int CanvasLeft;
        public int CanvasTop;
        public int CanvasRight;
        public int CanvasBottom;
        public int CanvasWidth
        {
            get
            {
                return CanvasRight - CanvasLeft;
            }
            set
            {
                CanvasRight = value + CanvasLeft;
                CanvasRect.Width = CanvasRight - CanvasLeft;
            }
        }
        public int CanvasHeight
        {
            get
            {
                return CanvasBottom - CanvasTop;
            }
            set
            {
                CanvasBottom = value + CanvasTop;
                CanvasRect.Height = CanvasBottom - CanvasTop;
            }
        }

        public CRect MemoryRect;

    }
    public class TEllipse
    {
        public TEllipse(Brush brush, double thickness)
        {
            CanvasEllipse = new Ellipse();
            CanvasEllipse.Stroke = brush;
            CanvasEllipse.StrokeThickness = thickness;

            MemoryEllipse = new List<PointLine>();
        }
        public Ellipse CanvasEllipse;
        public int CanvasLeft;
        public int CanvasTop;
        public List<PointLine> MemoryEllipse;
    }
    public class TPolygon
    {
        public TPolygon(Brush brush, double thickness)
        {
            CanvasPolygon.Points = new PointCollection();
            CanvasPolygon = new Polygon();
            CanvasPolygon.Stroke = brush;
            CanvasPolygon.StrokeThickness = thickness;
        }
        public Polygon CanvasPolygon;
        public Point[] CanvasPointArr;
        public Image ImageROI;
        public List<PointLine> MemoryPolygon;
    }
}