using Petzold.Media2D;
using Root_CAMELLIA.ShapeDraw;
using RootTools;
using System;
using System.Collections.Generic;
using DrawingPoint = System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using  System.Windows.Media;
using System.Windows.Shapes;

namespace Root_CAMELLIA.Draw
{
    public class ShapeManager
    {
        public bool IsSelected { get; set; } = false;
        public object Tag { get; set; }
        public Shape UIElement { get; set; }
    }

    public class PointLine : ShapeManager
    {
        public CPoint StartPt;
        public int Width;
    }

    public class ShapeEllipse : ShapeManager
    {
        public double CanvasLeft { get; set; }
        public double CanvasTop { get; set; }
        public List<PointLine> Data { get; set; }
        public Brush FillBrush { get; set; }
        public double CenterX { get; set; }
        public double CenterY { get; set; }

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
        public ShapeEllipse()
        {
            CanvasEllipse = new Ellipse();
            Data = new List<PointLine>();
        }
        public ShapeEllipse(Brush Fillbrush, Brush strokeBrush, double thickness = 1, double opacity = 1)
        {
            CanvasEllipse = new Ellipse();
            Data = new List<PointLine>();

            CanvasEllipse.Fill = Fillbrush;
            CanvasEllipse.Stroke = strokeBrush;
            CanvasEllipse.StrokeThickness = thickness;
            CanvasEllipse.Opacity = opacity;

        }

        public ShapeEllipse(Brush Fillbrush, double thickness = 1, double opacity = 1)
        {
            CanvasEllipse = new Ellipse();
            Data = new List<PointLine>();

            //FillBrush = brush;
            CanvasEllipse.Fill = Fillbrush;
            CanvasEllipse.StrokeThickness = thickness;
            CanvasEllipse.Opacity = opacity;

        }
        public void SetData(Circle circle, int nCenterX = 0, int nCenterY = 0, int zIndex = 0, bool isSelected = false)
        {
            CanvasEllipse.Width = circle.Width;
            CanvasEllipse.Height = circle.Height;
            CanvasLeft = nCenterX + circle.X - circle.Width;
            CanvasTop = nCenterY - circle.Y - circle.Height;
            Canvas.SetLeft(this.CanvasEllipse, nCenterX + circle.X - circle.Width);
            Canvas.SetTop(this.CanvasEllipse, nCenterY - circle.Y - circle.Height);
            CenterX = nCenterX + circle.X - (circle.Width / 2);
            CenterY = nCenterY - circle.Y - (circle.Height / 2);
            IsSelected = isSelected;
            Panel.SetZIndex(CanvasEllipse, zIndex);
            //Data.Clear();
            //Ellipse.Width = 
        }

        public void SetBrush(Brush fillBrush, Brush strokBrush)
        {
            CanvasEllipse.Fill = fillBrush;
            CanvasEllipse.Stroke = strokBrush;
        }
        public void SetBrush(Brush fillBrush)
        {
            CanvasEllipse.Fill = fillBrush;
        }
    }
    public class ShapeArrowLine : ShapeManager
    {
        public ArrowLine _CanvasArrowLine;
        public ArrowLine CanvasArrowLine
        {
            get
            {
                return _CanvasArrowLine;
            }
            set
            {
                base.UIElement = value;
                base.UIElement.Tag = this;
                _CanvasArrowLine = value;
            }
        }
        public ShapeArrowLine(Brush strokeBrush, double thickness = 10, double opacity = 1)
        {
            CanvasArrowLine = new ArrowLine();
            CanvasArrowLine.Stroke = strokeBrush;
            CanvasArrowLine.StrokeThickness = thickness;
            CanvasArrowLine.Opacity = opacity;
            CanvasArrowLine.ArrowLength = 20;
        }
        public void SetData(DrawingPoint.PointF[] pt, Brush strokeBrush, int arrowLength, double thickness, int zIndex = 0)
        {
            CanvasArrowLine.StrokeThickness = thickness;
            CanvasArrowLine.Stroke = strokeBrush;
            CanvasArrowLine.ArrowLength = arrowLength;
            CanvasArrowLine.X1 = pt[0].X;
            CanvasArrowLine.Y1 = pt[0].Y;
            CanvasArrowLine.X2 = pt[1].X;
            CanvasArrowLine.Y2 = pt[1].Y;
            Panel.SetZIndex(CanvasArrowLine, zIndex);
        }
    }
}
