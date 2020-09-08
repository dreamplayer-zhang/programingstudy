using RootTools;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.ServiceModel.Syndication;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace RootTools
{
    public class TShape
    {
        public bool isSelected
        {
            get; set;
        } = false;
    }

    public class PointLine
    {
        public CPoint StartPt;
        public int Width;
    }
    public class TPoint : TShape
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
    public class TLine : TShape
    {
        public TLine()
        {
            CanvasLine = new Line();
            MemoryLine = new List<PointLine>();
        }
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
    public class TRect : TShape
    {
        public TRect()
        {
            CanvasRect = new Rectangle();
            CanvasRect.Tag = this;
            MemoryRect = new CRect();
        }
        public TRect(Brush brush, double thickness)
        {
            CanvasRect = new Rectangle();
            CanvasRect.Fill = brush;
            CanvasRect.Opacity = 0.1;
            CanvasRect.Stroke = brush;
            CanvasRect.StrokeThickness = thickness;

            MemoryRect = new CRect();
        }
        public void SetLeft(int canvasLeft, int memoryLeft)
        {
            CanvasLeft = canvasLeft;
            MemoryRect.Left = memoryLeft;
            Canvas.SetLeft(CanvasRect, CanvasLeft);
        }
        public void SetTop(int canvasTop, int memoryTop)
        {
            CanvasTop = canvasTop;
            MemoryRect.Top = memoryTop;
            Canvas.SetTop(CanvasRect, CanvasTop);
        }
        public void SetRight(int canvasRight, int memoryRight)
        {
            CanvasRight = canvasRight;
            MemoryRect.Right = memoryRight;
            Canvas.SetRight(CanvasRect, CanvasRight);
        }
        public void SetBottom(int canvasBottom, int memoryBottom)
        {
            CanvasBottom = canvasBottom;
            MemoryRect.Bottom = memoryBottom;
            Canvas.SetBottom(CanvasRect, CanvasBottom);
        }

        public void SetLeftTop(CPoint canvasPt, CPoint memPt)
        {
            CanvasLeft = canvasPt.X;
            CanvasTop = canvasPt.Y;
            MemoryRect.Left = memPt.X;
            MemoryRect.Top = memPt.Y;

            Canvas.SetLeft(CanvasRect, CanvasLeft);
            Canvas.SetTop(CanvasRect, CanvasTop);
        }
        public void SetRightBottom(CPoint canvasPt, CPoint memPt)
        {
            CanvasRight = canvasPt.X;
            CanvasBottom = canvasPt.Y;
            MemoryRect.Right = memPt.X;
            MemoryRect.Bottom = memPt.Y;

            Canvas.SetRight(CanvasRect, CanvasRight);
            Canvas.SetBottom(CanvasRect, CanvasBottom);
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
                return Math.Abs(CanvasRight - CanvasLeft);
            }
            set
            {
                CanvasRight = Math.Abs(CanvasLeft + value);
                CanvasRect.Width = Math.Abs(CanvasRight - CanvasLeft);
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
    public class TEllipse : TShape
    {
        public TEllipse()
        {
            CanvasEllipse = new Ellipse();
            MemoryEllipse = new List<PointLine>();
        }
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
    public class TPolygon : TShape
    {
        public TPolygon()
        {
            CanvasPolygon = new Polygon();
            MemoryPolygon = new List<PointLine>();
        }
        public TPolygon(Brush brush, double thickness)
        {
            CanvasPolygon.Points = new PointCollection();
            CanvasPolygon = new Polygon();
            CanvasPolygon.Stroke = brush;
            CanvasPolygon.StrokeThickness = thickness;
        }
        public Polygon CanvasPolygon;
        public List<Point> CanvasPointList;
        public System.Drawing.Bitmap ImageROI;
        public List<PointLine> MemoryPolygon;
    }
}
