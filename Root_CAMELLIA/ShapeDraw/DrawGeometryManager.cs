using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Root_CAMELLIA.ShapeDraw
{
    class DrawGeometryManager
    {
        #region Constructor
        public DrawGeometryManager()
        {
        }
        #endregion

        #region Method

        
        public EllipseGeometry AddCircle(Circle circle, int nCenterX = 0, int nCenterY = 0)
        {
             EllipseGeometry ellipseGeometry = new EllipseGeometry();

            ellipseGeometry.RadiusX = circle.Width / 2;
            ellipseGeometry.RadiusY = circle.Height / 2;
            ellipseGeometry.Center = new System.Windows.Point(nCenterX + circle.X,  nCenterY - circle.Y);

            return ellipseGeometry;
        }

        public void AddCircle(EllipseGeometry geometry, Circle circle, int nCenterX = 0, int nCenterY = 0)
        {

            geometry.RadiusX = circle.Width / 2;
            geometry.RadiusY = circle.Height / 2;
            geometry.Center = new System.Windows.Point(nCenterX + circle.X, nCenterY - circle.Y);
        }



        public RectangleGeometry AddRect(Line line, int nCenterX, int nCenterY)
        {
            RectangleGeometry rectangleGeometry = new RectangleGeometry();
            double left = nCenterX + line.X - (line.Width * 0.5f);
            double top = nCenterY - line.Y - (line.Height * 0.5f);
            double width = line.Width;
            double Height = line.Height;
            rectangleGeometry.Rect = new Rect(left, top, width, Height);

            return rectangleGeometry;
        }

        public RectangleGeometry AddRect(Rect rect, int nCenterX, int nCenterY)
        {
            RectangleGeometry rectangleGeometry = new RectangleGeometry();
            rectangleGeometry.Rect = rect;

            return rectangleGeometry;
        }

        public PointCollection MakePointCollection(PointF[] point)
        {
            PointCollection pointcollection = new PointCollection();
            
            for(int i = 0; i < point.Length; i++)
            {
                pointcollection.Add(new System.Windows.Point(point[i].X, point[i].Y));
            }

            return pointcollection;
        }

        PathSegmentCollection pathSegments = null;
        public void AddPath(PointF[] point)
        {
            if(pathSegments == null)
            {
                pathSegments = new PathSegmentCollection();
            }

            PolyBezierSegment polyBezierSegment = new PolyBezierSegment();
            polyBezierSegment.Points = MakePointCollection(point);
            pathSegments.Add(polyBezierSegment);
        }

        public void AddLine(PointF[] point)
        {
            if (pathSegments == null)
            {
                pathSegments = new PathSegmentCollection();
            }

            PolyLineSegment polyLineSegment = new PolyLineSegment();
            polyLineSegment.Points = MakePointCollection(point);
            pathSegments.Add(polyLineSegment);
        }

        public PathSegmentCollection GetPathSegments()
        {
            if(pathSegments != null)
            {
                return pathSegments;
            }
            return null;
        }

        public PathFigure GetPathFigure(System.Windows.Point startPoint)
        {
            if (pathSegments != null)
            {

                return new PathFigure(startPoint, pathSegments, false);
            }
            return null;
        }

        public void ClearSegments()
        {
            pathSegments = null;
        }
        #endregion
    }
}
