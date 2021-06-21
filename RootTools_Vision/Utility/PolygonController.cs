using RootTools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml.Serialization;

namespace RootTools_Vision
{
    public class PolygonController
    {
        public static List<List<Point>> ReadPolygonFile(string filePath)
        {
            if (File.Exists(filePath) == false)
                return new List<List<Point>>();

            using (var sr = new StreamReader(filePath))
            {
                XmlSerializer xs = new XmlSerializer(typeof(List<List<Point>>));
                return (List<List<Point>>)xs.Deserialize(sr);
            }
        }

        public static void WritePolygonFile(string filePath, List<List<Point>> polygon)
        {
            using (StreamWriter wr = new StreamWriter(filePath))
            {
                XmlSerializer xs = new XmlSerializer(typeof(List<List<Point>>));
                xs.Serialize(wr, polygon);
            }
        }


        public static PathGeometry CreatePolygonGeometry(List<List<Point>> polygon)
        {
            PathGeometry pathGeometry = new PathGeometry();

            foreach (List<Point> points in polygon)
            {
                PathFigure figure = new PathFigure();
                figure.StartPoint = points[0];

                PathSegmentCollection segments = new PathSegmentCollection();

                foreach (Point pt in points)
                {
                    if (points[0] == pt) continue;

                    LineSegment lineSegment = new LineSegment();
                    lineSegment.Point = pt;

                    segments.Add(lineSegment);
                }

                figure.Segments = segments;

                pathGeometry.Figures.Add(figure);
            }

            return pathGeometry;
            //
            //PathGeometry geometry = new PathGeometry();
            
            //foreach(List<Point> points in polygon)
            //{
            //    List<PathSegment> pathSegments = new List<PathSegment>();
            //    pathSegments.Add(new PolyLineSegment(points.ToList(), true));
            //    PathFigure path = new PathFigure(points[0], pathSegments, true);

            //    //path.IsClosed = true;
            //    //path.IsFilled = true;
            //    geometry.Figures.Add(path);
            //}

            //return geometry;
        }

        public static bool HitTest(PathGeometry geometry, Point pt)
        {
            return geometry.FillContains(pt);
        }

        public static bool HitTest(PathGeometry geometry, Rect rect)
        {
            RectangleGeometry rectGeometry = new RectangleGeometry();
            rectGeometry.Rect = rect;


            if (geometry.FillContains(rect.TopLeft)) return true;
            if (geometry.FillContains(rect.TopRight)) return true;
            if (geometry.FillContains(rect.BottomLeft)) return true;
            if (geometry.FillContains(rect.BottomRight)) return true;

            return false;
            //return geometry.FillContains(rectGeometry);
        }
    }
}
