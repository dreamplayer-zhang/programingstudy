using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace RootTools
{
    public class BasicTool : ObservableObject
    {
        public BasicTool()
        {
            InitCrossLine();
        }
        BasicShape basic;
        Line Vertical;
        Line Horizon;
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

        public bool StartDrawBasicTool(Shape shapeType, CPoint memStartPt, CPoint canvasStartPt)
        {
            try
            {
                basic = new BasicShape(shapeType, memStartPt, canvasStartPt);
                basic.m_CanvasStartPt = canvasStartPt;
                basic.m_CanvasShape.Stroke = Brushes.Yellow;
                basic.m_CanvasShape.StrokeThickness = 2;
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
        public bool Drawing_BasicTool(CPoint memNowPt, CPoint canvasNowPt)
        {
            if (basic == null)
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
        public void InitCrossLine()
        {
            if (p_BasicElement.Contains(Vertical) && p_BasicElement.Contains(Horizon))
            {
                p_BasicElement.Remove(Vertical);
                p_BasicElement.Remove(Horizon);
            }
            
            Vertical = new Line();
            Horizon = new Line();

            Brush LineColor = Brushes.Silver;
            double LineThick = 1;
            DoubleCollection LineDash = new DoubleCollection { 3, 4 };

            Vertical.Stroke = LineColor;
            Vertical.StrokeThickness = LineThick;
            Vertical.StrokeDashArray = LineDash;
            Horizon.Stroke = LineColor;
            Horizon.StrokeThickness = LineThick;
            Horizon.StrokeDashArray = LineDash;

            p_BasicElement.Add(Vertical);
            p_BasicElement.Add(Horizon);
        }
        public bool DrawingCrossLine(CPoint nowPt, int canvasWidth, int canvasHeight)
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
                return false;
            }
            return true;
        }


        public void SetTopLeft(Shape shape, CPoint point)
        {
            if (shape is null)
            {
                throw new ArgumentNullException(nameof(shape));
            }

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
    }

    public class BasicShape
    {
        public BasicShape()
        {
        }
        public BasicShape(Shape shapetype, CPoint memPt, CPoint canvasPt)
        {
            m_CanvasShape = shapetype;
            m_MemoryStartPt = memPt;
            m_CanvasStartPt = canvasPt;
        }
        /// <summary>
        /// Canvas Shape
        /// </summary>
        public Shape m_CanvasShape;
        /// <summary>
        /// Object Size in Memory
        /// </summary>
        public Rect m_MemoryObject;
        /// <summary>
        /// Shape Canvas Start Point
        /// </summary>
        public CPoint m_CanvasStartPt;
        
        private CPoint _canvasEndPt;
        /// <summary>
        /// Shape Canvas End Point
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

    

}
