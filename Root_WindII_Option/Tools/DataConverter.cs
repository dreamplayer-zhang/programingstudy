using RootTools;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Root_WindII_Option
{
    public class DataConverter
    {
        public static CameraInfo GrabModeToCameraInfo(GrabModeBase grabMode)
        {
            if (grabMode == null) return default(CameraInfo);

            CameraInfo camInfo = new CameraInfo();

            if (grabMode == null)
            {
                return camInfo;
            }

            camInfo.RealResX = grabMode.m_dRealResX_um;
            camInfo.RealResY = grabMode.m_dRealResY_um;
            camInfo.TargetResX = grabMode.m_dTargetResX_um;
            camInfo.TargetResY = grabMode.m_dTargetResY_um;

            return camInfo;
        }

        public static List<Point> CppPointArrayToPointList(Cpp_Point[] points)
        {
            List<Point> result = new List<Point>();
            foreach(var pt in points)
            {
                result.Add(new Point(pt.x, pt.y));
            }
            return result;
        }

        public static List<Point> CPointListToPointList(List<CPoint> points)
        {
            List<Point> result = new List<Point>();
            foreach (var pt in points)
            {
                result.Add(new Point(pt.X, pt.Y));
            }
            return result;
        }
    }
}
