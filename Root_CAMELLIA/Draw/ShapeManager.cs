using Root_CAMELLIA.ShapeDraw;
using RootTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
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

        public ShapeEllipse(Brush Fillbrush, double thickness = 1, double opacity =1)
        {
            CanvasEllipse = new Ellipse();
            Data = new List<PointLine>();

            //FillBrush = brush;
            CanvasEllipse.Fill = Fillbrush;
            CanvasEllipse.StrokeThickness = thickness;
            CanvasEllipse.Opacity = opacity;

        }
        public void SetData(Circle circle, int nCenterX = 0, int nCenterY = 0, bool isSelected = false)
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

        //public override bool Equals(object obj)
        //{
        //    if (!(obj is ShapeEllipse)) return false;

        //    ShapeEllipse shapeEllipse = obj as ShapeEllipse;
        //    if(shapeEllipse.CanvasEllipse.Width == CanvasEllipse.Width)

        //    return Equals((ShapeManager)obj);
        //}

        //public override int GetHashCode() => UIElement.GetHashCode();
    }
}
