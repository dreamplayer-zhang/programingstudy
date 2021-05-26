using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RootTools_Vision
{
    public partial class Tools
    {
        public static Point FindCircleCenterByPoints(List<Point> circle_points, int centerX, int centerY, int searchLength)
        {
            int points_count = circle_points.Count;
            

            int startX = centerX - searchLength / 2;
            int startY = centerY - searchLength / 2;
            int endX = startX + searchLength;
            int endY = startY + searchLength;

            int minX = centerX, minY = centerX;
            double minStdev = double.MaxValue;

            for (int x = startX; x <= endX; x++)
            {
                for (int y = startY; y < endY; y++)
                {
                    double[] radiusArr = new double[points_count];

                    Parallel.For(0, points_count, (i) =>
                    {
                        double radius = Math.Sqrt(Math.Pow(circle_points[i].X - x, 2) + Math.Pow(circle_points[i].Y - y, 2));
                        radiusArr[i] = radius;
                    });

                    double avg = radiusArr.Average();
                    double stdev = Tools.CalcStdev(radiusArr, avg);
                    if(minStdev > stdev)
                    {
                        minStdev = stdev;
                        minX = x;
                        minY = y;
                    }
                }
            }


            return new Point(minX, minY);
        }

        public static double CalcStdev(double[] dataArr, double avg)
        {
            double sdSum = dataArr.Select(val => (val - avg) * (val - avg)).Sum();
            return Math.Sqrt(sdSum / (dataArr.Length - 1));
        }
    }
}
