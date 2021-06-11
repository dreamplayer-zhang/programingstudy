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
        public Shape UIElement;
        public Grid ModifyTool;
        public object Tag;
        private bool _isSelected = false;
        public bool isSelected
        {
            get
            {
                if(ModifyTool != null)
                    if (!_isSelected)
                        ModifyTool.Visibility = Visibility.Hidden;

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

        public PointLine() 
        {
            this.StartPt = new CPoint();
        }

        public PointLine(CPoint startPt, int width)
        {
            this.StartPt = new CPoint();
            
            StartPt.X = startPt.X;
            StartPt.Y = startPt.Y;
            Width = width;
        }

    }

    public class ScanLine : TShape
    {
        public CPoint StartPt;
        public CPoint EndPt;
        public int MaxY;
        public int MinY;
        public int XofMinY;
        public int dx;
        public int dy;
        public double slope; // dx/dy (inverse of dy/dx)

        public ScanLine()
        {
            this.StartPt = new CPoint();
            this.EndPt = new CPoint();
        }

        public ScanLine(CPoint startPt, CPoint endPt)
        {
            this.StartPt = new CPoint();
            this.EndPt = new CPoint();

            StartPt.X = startPt.X;
            StartPt.Y = startPt.Y;
            EndPt.X = endPt.X;
            EndPt.Y = endPt.Y;

            MaxY = Math.Max(StartPt.Y, EndPt.Y);
            MinY = Math.Min(StartPt.Y, EndPt.Y);

            if (MinY == StartPt.Y)
            {
                XofMinY = StartPt.X;
            }
            else
            {
                XofMinY = EndPt.X;
            }

            dy = EndPt.Y - StartPt.Y;
            dx = EndPt.X - StartPt.X;
            
            if (dy == 0)
            {
                slope = 1.0;
            }
            if (dx == 0)
            {
                slope = 0.0;
            }
            if (dy != 0 && dx != 0)
            {
                slope = (double)dx / (double)dy;
            }
        }
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
            Pixel = new Rectangle();
            CanvasPoint = canvastPt;
            MemoryPoint = memPt;
        }
        public Rectangle Pixel;
        public CPoint CanvasPoint;
        public CPoint MemoryPoint;
    }
    public class TLine : TShape
    {
        public Line _CanvasLine;
        public Line CanvasLine
        {
            get
            {
                return _CanvasLine;
            }
            set
            {
                base.UIElement = value;
                base.UIElement.Tag = this;
                _CanvasLine = value;
            }
        }
        public TLine()
        {
            CanvasLine = new Line();
            Data = new List<CPoint>();
        }
        public TLine(Brush brush, double thickness, double opacity)
        {
            CanvasLine = new Line();
            CanvasLine.Stroke = brush;
            CanvasLine.StrokeThickness = thickness;
            CanvasLine.Opacity = opacity;

            Data = new List<CPoint>();
        }

        public CPoint MemoryStartPoint;
        public CPoint MemoryEndPoint;
        public List<CPoint> Data;
        public void SetData()
        {
            Data.Clear();
            CPoint startPt = new CPoint(MemoryStartPoint.X, MemoryStartPoint.Y);
            CPoint endPt = new CPoint(MemoryEndPoint.X, MemoryEndPoint.Y);
            bool steep = Math.Abs(endPt.Y - startPt.Y) > Math.Abs(endPt.X - startPt.X);
            if (steep)
            {
                int temp;
                temp = startPt.X;
                startPt.X = startPt.Y;
                startPt.Y = temp;

                temp = endPt.X;
                endPt.X = endPt.Y;
                endPt.Y = temp;
            }
            if (startPt.X > endPt.X)
            {
                int temp;
                temp = startPt.X;
                startPt.X = endPt.X;
                endPt.X = temp;

                temp = startPt.Y;
                startPt.Y = MemoryEndPoint.Y;
                endPt.Y = temp;
            }
            int dx = endPt.X - startPt.X;
            int dy = Math.Abs(endPt.Y - startPt.Y);
            int error = dx / 2;
            int ystep = (startPt.Y < endPt.Y) ? 1 : -1;
            int y = startPt.Y;
            for (int x = startPt.X; x <= endPt.X; x++)
            {
                CPoint point = new CPoint((steep ? y : x), (steep ? x : y));
                Data.Add(point);
                error = error - dy;
                if (error < 0)
                {
                    y += ystep;
                    error += dx;
                }
            }           
        }
    }
    public class TCropTool : TShape
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
                base.UIElement.Tag = this;
                _CanvasRect = value;
            }
        }
        public Image CropImage;
        public CRect MemoryRect;
        public CPoint MemPointBuffer;
        public ImageData CropImageData;
        public TCropTool()
        {
            MemoryRect = new CRect();
            CanvasRect = new Rectangle();
        }
        public TCropTool(Brush brush, double thickness, double opacity)
        {
            MemoryRect = new CRect();
            CanvasRect = new Rectangle();
            CanvasRect.Stroke = brush;
            CanvasRect.StrokeThickness = thickness;
            CanvasRect.Opacity = opacity;

        }
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
                base.UIElement.Tag = this;
                _CanvasRect = value;
            }
        }

        public CRect MemoryRect;
        public CPoint MemPointBuffer;
        public Brush FillBrush;
        public List<PointLine> Data;
        public TRect()
        {
            MemoryRect = new CRect();
            CanvasRect = new Rectangle();
            MemPointBuffer = new CPoint();
            Data = new List<PointLine>();
        }
        public TRect(Brush brush, double thickness, double opacity)
        {
            MemoryRect = new CRect();
            CanvasRect = new Rectangle();
            Data = new List<PointLine>();

            CanvasRect.Stroke = brush;
            FillBrush = brush;
            CanvasRect.StrokeThickness = thickness;
            CanvasRect.Opacity = opacity;

        }

		public TRect(TRect rect)
		{
            this.CanvasRect = rect.CanvasRect;
            this.Data = new List<PointLine>(rect.Data);
            this.FillBrush = rect.FillBrush;
            this.isSelected = rect.isSelected;
            this.MemoryRect = new CRect(rect.MemoryRect);
            this.MemPointBuffer = new CPoint(rect.MemPointBuffer);
            this.ModifyTool = rect.ModifyTool;
            this.Tag = rect.Tag;
            this.UIElement = rect.UIElement;
        }

		public void SetData()
        {
            Data = new List<PointLine>();
            for (int y = MemoryRect.Top; y < MemoryRect.Bottom+1; y++)
            {
                PointLine dataline = new PointLine();
                dataline.StartPt = new CPoint(MemoryRect.Left, y);
                dataline.Width = MemoryRect.Width+1;
                Data.Add(dataline);
            }
        }


    }

    public class TEllipse : TShape
    {
        public Grid _grid;

        public Ellipse _CanvasEllipse;
        public Ellipse CanvasEllipse
        {
            get
            {
                return _CanvasEllipse;
            }
            set
            {
                base.UIElement = value;
                base.UIElement.Tag = this;
                _CanvasEllipse = value;
            }
        }

        public CRect MemoryRect;
        public CPoint MemPointBuffer;
        public List<PointLine> Data;

        public TEllipse()
        {
            _grid = new Grid();
            CanvasEllipse = new Ellipse();
            MemoryRect = new CRect();
            MemPointBuffer = new CPoint();
            Data = new List<PointLine>();
        }
        public TEllipse(Brush brush, double thickness, double opacity)
        {
            _grid = new Grid();
            CanvasEllipse = new Ellipse();
            MemoryRect = new CRect();
            MemPointBuffer = new CPoint();
            Data = new List<PointLine>();

            _grid.Children.Add(CanvasEllipse);
            CanvasEllipse.Stroke = Brushes.Red;
            CanvasEllipse.Stroke = brush;
            CanvasEllipse.Fill = brush;
            CanvasEllipse.StrokeThickness = thickness;
            CanvasEllipse.Opacity = opacity;
        }
    }

    public class TPolygon : TShape
    {
        public Polygon _CanvasPolygon;
        public Polygon CanvasPolygon
        {
            get
            {
                return _CanvasPolygon;
            }
            set
            {
                base.UIElement = value;
                base.UIElement.Tag = this;
                _CanvasPolygon = value;
            }
        }

        public Polyline CanvasPolyLine;
        public List<CPoint> ListMemoryPoint;
        public List<PointLine> Data;
        public TPolygon(Brush brush, double thickness, double opacity)
        {
            CanvasPolygon = new Polygon();
            CanvasPolygon.Stroke = brush;
            CanvasPolygon.Fill = brush;
            CanvasPolygon.StrokeThickness = thickness;
            CanvasPolygon.Opacity = opacity;

            CanvasPolyLine = new Polyline();
            CanvasPolyLine.Stroke = brush;
            CanvasPolyLine.StrokeThickness = thickness;
            CanvasPolyLine.StrokeDashArray = new DoubleCollection();
            CanvasPolyLine.StrokeDashArray.Add(5);
            CanvasPolyLine.StrokeDashArray.Add(5);

            ListMemoryPoint = new List<CPoint>();
            Data = new List<PointLine>();
        }
    }
}
