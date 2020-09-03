using System;
using System.Collections.Generic;
using System.ServiceModel.Syndication;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace RootTools.ToolShapes
{
    public class ToolShape
    {
        public Brush Stroke
        {
            get;
            set;
        }
        public double StrokeThickness
        {
            get;
            set;
        }
    }

    public class PointLine
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
    public class TRect : ToolShape
    {
        public TRect()
        {
            CanvasRect.Stroke = Stroke;
        }
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
    public class TEllipse : ToolShape
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

    public class TPolygon : ToolShape
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
        public System.Drawing.Bitmap ImageROI;
        public List<PointLine> MemoryPolygon;
    }
}
