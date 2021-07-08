using Root_CAMELLIA.ShapeDraw;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Root_CAMELLIA.Draw
{
    public class GeometryManager
    {
        //public Path path { get; set; }
       // public Shape UIElement { get; set; }
        public Path path { get; set; }

        public GeometryManager()
        {
            path = new Path();
        }
    }
    public class CustomEllipseGeometry : GeometryManager
    {
        public int CanvasLeft { get; set; }
        public int CanvasTop { get; set; }
        //private Path _path;
        //public Path Path
        //{
        //    get
        //    {
        //        return _path;
        //    }
        //    set
        //    {
        //        base.UIElement = value;
        //        base.UIElement.Tag = this;
        //        _path = value;
        //    }
        //}
        
        private EllipseGeometry _ellipseGeometry;
        public EllipseGeometry EllipseGeometry
        {
            get
            {
                return _ellipseGeometry;
            }
            set
            {
                _ellipseGeometry = value;
            }
        }
        public CustomEllipseGeometry()
        {
            EllipseGeometry = new EllipseGeometry();
            base.path = new Path();
            //base.Path = new Path();
        }
        public CustomEllipseGeometry(Brush fillBrush, Brush strokeBrush, double thickness = 1, double opcity = 1)
        {
            EllipseGeometry = new EllipseGeometry();
           // path = new Path();
            path.Stroke = strokeBrush;
            path.Fill = fillBrush;
            path.Data = EllipseGeometry;
            path.StrokeThickness = thickness;
            path.Opacity = opcity;
        }
        public CustomEllipseGeometry(Brush strokeBrush, double thickness = 1, double opcity = 1)
        {
            EllipseGeometry = new EllipseGeometry();
         //   path = new Path();
            path.Stroke = strokeBrush;
            path.Data = EllipseGeometry;
            path.StrokeThickness = thickness;
            path.Opacity = opcity;
        }
        public CustomEllipseGeometry(Brush strokeBrush, string strokeDash, double thickness = 1, double opcity = 1)
        {
            EllipseGeometry = new EllipseGeometry();
           // path = new Path();
            path.Stroke = strokeBrush;
            path.Data = EllipseGeometry;
            path.StrokeDashArray = DoubleCollection.Parse(strokeDash);
            path.StrokeThickness = thickness;
            path.Opacity = opcity;
        }
        public void SetData(Circle circle, int nCenterX = 0, int nCenterY = 0, int thickness = 1)
        {
            EllipseGeometry.RadiusX = circle.Width / 2;
            EllipseGeometry.RadiusY = circle.Height / 2;
            EllipseGeometry.Center = new Point(nCenterX + circle.X, nCenterY - circle.Y);
            path.StrokeThickness = thickness;
        }
    }
    public class CustomRectangleGeometry : GeometryManager
    {
        //private Path _path;
        //public Path Path
        //{
        //    get
        //    {
        //        return _path;
        //    }
        //    set
        //    {
        //        base.UIElement = value;
        //        base.UIElement.Tag = this;
        //        _path = value;
        //    }
        //}

        private RectangleGeometry _rectangleGeometry;
        public RectangleGeometry RectangleGeometry
        {
            get
            {
                return _rectangleGeometry;
            }
            set
            {
                _rectangleGeometry = value;
            }
        }
        public GeometryGroup geometryGroup;
        public CustomRectangleGeometry()
        {
            RectangleGeometry = new RectangleGeometry();
         //   path = new Path();
        }
        public CustomRectangleGeometry(Brush fillBrush, Brush strokeBrush, double thickness = 1, double opcity = 1)
        {
            RectangleGeometry = new RectangleGeometry();
          //  path = new Path();
            path.Fill = fillBrush;
            path.Stroke = strokeBrush;
            path.Data = RectangleGeometry;
            path.StrokeThickness = thickness;
            path.Opacity = opcity;
        }
        public CustomRectangleGeometry(Brush strokeBrush, string strokeDash, double thickness = 1, double opcity = 1)
        {
            RectangleGeometry = new RectangleGeometry();
            path.Stroke = strokeBrush;
            path.Data = RectangleGeometry;
            path.StrokeDashArray = DoubleCollection.Parse(strokeDash);
            path.StrokeThickness = thickness;
            path.Opacity = opcity;
        }

        public CustomRectangleGeometry(Brush fillBrush, double thickness = 1, double opcity = 1)
        {
            RectangleGeometry = new RectangleGeometry();
         //   path = new Path();
            path.Fill = fillBrush;
            path.Data = RectangleGeometry;
            path.StrokeThickness = thickness;
            path.Opacity = opcity;
            
        }
        public void SetData(Rect rect, int zIndex = 0)
        {
            RectangleGeometry.Rect = rect;
            Panel.SetZIndex(path, zIndex);
        }

        public void SetData(Rect rect, double thickness, double opcity = 1,  int zIndex = 0)
        {
            RectangleGeometry.Rect = rect;
            path.StrokeThickness = thickness;
            Panel.SetZIndex(path, zIndex);
        }

        public void AddGroup(CustomRectangleGeometry rectangleGeometry)
        {
            if(geometryGroup == null)
            {
                geometryGroup = new GeometryGroup();
                geometryGroup.FillRule = FillRule.Nonzero;
            }
            geometryGroup.Children.Add(rectangleGeometry.RectangleGeometry);
            path.Data = geometryGroup;
        }

        public void SetGroupData(Rect rect, int index)
        {
            RectangleGeometry rectangleGeometry = geometryGroup.Children[index] as RectangleGeometry;
            rectangleGeometry.Rect = rect;
        }

    }

    public class CustomPathGeometry : GeometryManager
    {
        //private Path _path;
        //public Path Path
        //{
        //    get
        //    {
        //        return _path;
        //    }
        //    set
        //    {
        //        base.UIElement = value;
        //        base.UIElement.Tag = this;
        //        _path = value;
        //    }
        //}
        private PathGeometry _pathGeometry;
        public PathGeometry PathGeometry
        {
            get
            {
                return _pathGeometry;
            }
            set
            {
                _pathGeometry = value;
            }
        }
        public CustomPathGeometry()
        {
            PathGeometry = new PathGeometry();
         //   path = new Path();
        }
        public CustomPathGeometry(Brush brush, double thickness = 1, double opcity = 1)
        {
            PathGeometry = new PathGeometry();
       //     path = new Path();
            path.Stroke = brush;
            path.Fill = brush;
            path.Data = PathGeometry;
            path.StrokeThickness = thickness;
            path.Opacity = opcity;
        }
        public CustomPathGeometry(Brush strokebrush, string strokeDash,  double thickness = 1, double opcity = 1)
        {
            PathGeometry = new PathGeometry();
            //     path = new Path();
            path.Stroke = strokebrush;
            path.Data = PathGeometry;
            path.StrokeDashArray = DoubleCollection.Parse(strokeDash);
            path.StrokeThickness = thickness;
            path.Opacity = opcity;
        }
        public void SetData(PathFigure figure, double thickness = 1)
        {
            PathGeometry.Figures.Clear();
            PathGeometry.Figures.Add(figure);
            path.StrokeThickness = thickness;
        }
        public void SetBrush(Brush brush)
        {
            path.Fill = brush;
            path.Stroke = brush;
        }
    }

}
