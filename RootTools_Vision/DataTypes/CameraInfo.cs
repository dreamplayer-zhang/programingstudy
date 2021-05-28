using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    public struct CameraInfo
    {
        /// <summary>
        /// Spec 상 카메라 해상도
        /// </summary>
        public double TargetResX;

        /// <summary>
        /// Spec 상 카메라 해상도
        /// </summary>
        public double TargetResY;

        /// <summary>
        /// 실제 셋팅된 카메라 해상도
        /// </summary>
        public double RealResX;

        /// <summary>
        /// 실제 셋팅된 카메라 해상도
        /// </summary>
        public double RealResY;

        //public int ScanDegree;
        //public int ImageHeight;
        //public int CameraHeight;
        //public int CameraPositionOffset;

        /// <summary>
        /// Resolution 정보가 없을 경우 Resolution은 무조건 1로 하여 검사 결과에 영향을 주지않도록함.
        /// </summary>
        /// <param name="targetResX"></param>
        /// <param name="targetResY"></param>
        /// <param name="realResX"></param>
        /// <param name="realResY"></param>
        public CameraInfo(double targetResX = 1, double targetResY = 1, double realResX = 1, double realResY = 1)
        {
            TargetResX = targetResX;
            TargetResY = targetResY;
            RealResX = realResX;
            RealResY = realResY;

            //ScanDegree = 0;
            //ImageHeight = 0;
            //CameraHeight = 0;
            //CameraPositionOffset = 0;
        }
    }
}
