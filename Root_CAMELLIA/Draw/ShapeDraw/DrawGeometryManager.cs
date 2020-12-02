using Root_CAMELLIA.Data;
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
            ellipseGeometry.Center = new System.Windows.Point(nCenterX + circle.X, nCenterY - circle.Y);

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

            for (int i = 0; i < point.Length; i++)
            {
                pointcollection.Add(new System.Windows.Point(point[i].X, point[i].Y));
            }

            return pointcollection;
        }

        PathSegmentCollection pathSegments = null;
        public void AddPath(PointF[] point)
        {
            if (pathSegments == null)
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
            if (pathSegments != null)
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
        public Circle GetRect(CCircle _circle, int nCenterX, int nCenterY)
        {
            Circle circle = new Circle();
            circle.X = (int)(((_circle.x + _circle.MeasurementOffsetX) - _circle.width * 0.5f) + nCenterX);
            circle.Y = -(int)((-(_circle.y + _circle.MeasurementOffsetY) - _circle.height * 0.5f) + nCenterY);

            circle.Width = (int)(_circle.width);
            circle.Height = (int)(_circle.height);

            return circle;
        }

        public void GetRect(ref Circle _circle, int nCenterX, int nCenterY)
        {
            _circle.X = (int)((_circle.X - _circle.Width * 0.5f) + nCenterX);
            _circle.Y = (int)((-_circle.Y - _circle.Height * 0.5f) + nCenterY);

            _circle.Width = (int)(_circle.Width);
            _circle.Height = (int)(_circle.Height);
        }

        public Rect GetRect(ShapeDraw.Line _line, int nCenterX, int nCenterY)
        {
            Rect rect = new Rect();

            rect.X = (int)((_line.X - _line.Width * 0.5f) + nCenterX);
            rect.Y = (int)((-_line.Y - _line.Height * 0.5f) + nCenterY);

            rect.Width = (int)(_line.Width);
            rect.Height = (int)(_line.Height);

            return rect;
        }

        public Rect GetRect(Rect _rect, int nCenterX, int nCenterY)
        {
            Rect rect = new Rect();

            rect.X = (int)((_rect.X - _rect.Width * 0.5f) + nCenterX);
            rect.Y = (int)((-_rect.Y - _rect.Height * 0.5f) + nCenterY);
            rect.Width = (int)(_rect.Width);
            rect.Height = (int)(_rect.Height);

            return rect;
        }

        public PointF GetPoint(PointF _pointF, int nCenterX, int nCenterY)
        {
            PointF pointF = new PointF();

            pointF.X = _pointF.X + nCenterX;
            pointF.Y = -_pointF.Y + nCenterY;

            return pointF;
        }

        public PointF[] GetPoints(PointF[] _points, int nCenterX, int nCenterY)
        {
            int nNum = _points.Count(); ;
            PointF[] points = new PointF[nNum];

            for (int i = 0; i < nNum; i++)
            {
                points[i].X = _points[i].X + nCenterX;
                points[i].Y = -_points[i].Y + nCenterY;
            }

            return points;
        }

        public PathFigure AddDoubleHole(Arc arcPoint1, Arc arcPoint2, int centerX, int centerY)
        {
            PointF[] points;
            PointF[] pt = new PointF[2];
            System.Windows.Point StartPoint;
            points = GetPoints(arcPoint1.Points, centerX, centerY);
            StartPoint = new System.Windows.Point(points[0].X, points[0].Y); // 시작포인트
            AddPath(points);

            pt[0] = GetPoint(arcPoint1.Points[arcPoint1.Points.Count() - 1], centerX, centerY);
            pt[1] = GetPoint(arcPoint2.Points[0], centerX, centerY);
            AddLine(pt);

            points = GetPoints(arcPoint2.Points, centerX, centerY);
            AddPath(points);

            pt[0] = GetPoint(arcPoint2.Points[arcPoint2.Points.Count() - 1], centerX, centerY);
            pt[1] = GetPoint(arcPoint1.Points[0], centerX, centerY);
            AddLine(pt);

            return new PathFigure(StartPoint, pathSegments, false);
        }
        #endregion
    }
}
