using Emgu.CV;
using RootTools.Memory;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV.CvEnum;

namespace RootTools.ImageProcess
{
    public class VignetteFilter
    {
        public Mat Mask;
        double power, radius;
        public VignetteFilter(int width,int height,double power, double radius)
        {
            Size memSize = new Size(width, height);
            Mask = new Mat(memSize, DepthType.Cv64F, 1);
            this.power = power;
            this.radius = radius;
        }
        double Dist(Point a, Point b)
        {
            return Math.Sqrt(Math.Pow((a.X - b.X), 2) + Math.Pow((a.Y - b.Y), 2));
        }
        double getMasDistFromCorners(Size imgSize, Point CenterPt)
        {
            Point[] corners = new Point[4];
            corners[0] = new Point(0, 0); //lt
            corners[1] = new Point(imgSize.Width, 0); //rt
            corners[2] = new Point(0, imgSize.Height); //lb;
            corners[3] = new Point(imgSize.Width, imgSize.Height); //rb;

            double maxDist = double.MinValue;
            for (int i = 0; i < 4; i++)
            {
                double dist = Dist(corners[i], CenterPt);
                maxDist = maxDist < dist ? dist : maxDist;
            }
            return maxDist;
        }
        public unsafe void GenerateGradient()
        {
            Point firstPT = new Point(Mask.Size.Width / 2, Mask.Size.Height / 2);

            double maxImageRad = radius * getMasDistFromCorners(Mask.Size, firstPT);
            Mask.SetTo(new Emgu.CV.Structure.MCvScalar(1));

            for (int i = 0; i < Mask.Height; i++)
            {
                for (int j = 0; j < Mask.Width; j++)
                {
                    double tmp = Dist(firstPT, new Point(j,i)) / maxImageRad;
                    tmp *= power;
                    double tmp2 = Math.Pow(Math.Cos(tmp), 4);
                    double* ptr = (double*)Mask.DataPointer.ToPointer();
                    ptr[i * Mask.Width + j] = tmp2;
                }
            }
        }
    }
}
