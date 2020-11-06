using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_CAMELLIA.Data
{
    public class RecipeData
    {
        public List<CCircle> DataCandidatePoint { get; set; } = new List<CCircle>();
        public List<CCircle> DataSelectedPoint { get; set; } = new List<CCircle>();
        public List<int> DataMeasurementRoute { get; set; } = new List<int>();
        public double SubPointInterval { get; set; }

        public int CircleCount { get; set; }

        public void ClearPoint()
        {
            DataCandidatePoint.Clear();
            DataSelectedPoint.Clear();
            DataMeasurementRoute.Clear();

            CircleCount = 0;
        }

        public void ClearCandidatePoint()
        {
            DataCandidatePoint.Clear();
          
            CircleCount = 0;
        }


        public void CheckCircleSize()
        {
            int Index;
            List<CCircle> temp = new List<CCircle>();
            foreach (CCircle item in DataSelectedPoint)
            {
                temp.Add(item);
            }
            DataSelectedPoint.Clear();
            foreach (CCircle item in temp)
            {
                if (ContainsData(DataCandidatePoint, item, out Index))
                {
                    CCircle circle;
                    circle = new CCircle(item.x, item.y, DataCandidatePoint[Index].width, DataCandidatePoint[Index].height,
                        0, 0);

                    DataSelectedPoint.Add(circle);
                }
            }
        }


        public bool ContainsData(List<CCircle> list, CCircle circle, out int nIndex)
        {
            bool bRst = false;
            nIndex = -1;

            int nCount = 0;
            foreach (var item in list)
            {
                if (Math.Round(item.x, 3) == Math.Round(circle.x, 3) && Math.Round(item.y, 3) == Math.Round(circle.y, 3))
                {
                    bRst = true;
                    nIndex = nCount;
                }
                nCount++;
            }
            return bRst;
        }
    }

    #region Circle
    public struct CCircle
    {
        public double x, y, width, height;
        public double MeasurementOffsetX, MeasurementOffsetY;
        //public CamelliaState.CamelliaCenterEdge type;
        //public CamelliaState.CamelliaSubMode mode;
        //public int mode, type; /* mode : 0(Point), 1(Die) || type : 0(Blue), 1(Yellow), 2(Red)*/

        //public CCircle(double dX, double dY, double dWidth, double dHeight, CamelliaState.CamelliaCenterEdge cType, CamelliaState.CamelliaSubMode cMode, double dMeasurementOffsetX, double dMeasurementOffsetY)
        //{
        //    x = dX;
        //    y = dY;
        //    width = dWidth;
        //    height = dHeight;
        //    type = cType;
        //    mode = cMode;
        //    MeasurementOffsetX = dMeasurementOffsetX;
        //    MeasurementOffsetY = dMeasurementOffsetY;
        //}
        //public CCircle()
        //{

        //}

        public CCircle(double dX, double dY, double dWidth, double dHeight, double dMeasurementOffsetX, double dMeasurementOffsetY)
        {
            x = dX;
            y = dY;
            width = dWidth;
            height = dHeight;
            MeasurementOffsetX = dMeasurementOffsetX;
            MeasurementOffsetY = dMeasurementOffsetY;
        }

        public void Transform(double dScaleX, double dScaleY)
        {
            x *= dScaleX;
            y *= dScaleY;
            width *= dScaleX;
            height *= dScaleY;
            MeasurementOffsetX *= dScaleX;
            MeasurementOffsetY *= dScaleY;
        }

        public void ScaleOffset(int nScale, int nOffsetX, int nOffsetY)
        {
            x = x * nScale + nOffsetX;
            y = y * nScale + nOffsetY;
            width *= nScale;
            height *= nScale;
            MeasurementOffsetX *= nScale;
            MeasurementOffsetY *= nScale;
        }

        public void ScaleOffset(double dScale, int nOffsetX, int nOffsetY)
        {
            x = x * dScale + nOffsetX;
            y = y * dScale + nOffsetY;
            width *= dScale;
            height *= dScale;
            MeasurementOffsetX *= dScale;
            MeasurementOffsetY *= dScale;
        }
    }
    #endregion
}
