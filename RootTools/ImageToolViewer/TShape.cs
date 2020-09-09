using RootTools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public UIElement UIElement;
        public Grid ModifyTool;
        private bool _isSelected = false;
        public bool isSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                if (ModifyTool != null)
                    if (value)
                        ModifyTool.Visibility = Visibility.Visible;
                    else
                        ModifyTool.Visibility = Visibility.Hidden;

                _isSelected = value;
            }
        }
    }

    public class PointLine : TShape
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
        public Rectangle _CanvasRect;
        public Rectangle CanvasRect
        {
            get
            {
                return _CanvasRect;
            }
            set
            {
                base.UIElement = value;
                _CanvasRect = value;
            }
        }

        public TRect(Brush brush, double thickness)
        {
            MemoryRect = new CRect();

            CanvasRect = new Rectangle();
            CanvasRect.Opacity = 0.5;
            FillBrush = brush;
            CanvasRect.Stroke = brush;
            CanvasRect.StrokeThickness = thickness;

        }
        public Rectangle RectConvert(CRect memRect)
        {
            Rectangle CanvasRect = new Rectangle();

            return CanvasRect;
        }
        public CPoint MemPointBuffer;
        public Brush FillBrush;

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
