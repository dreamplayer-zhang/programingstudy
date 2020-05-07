using RootTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Root_Vega.Module
{
    public class AutoFocus
    {
        double m_dDistanceOfLeftPointToRightPoint;
        public double p_dDistanceOfLeftPointToRightPoint
        {
            get
            {
                return m_dDistanceOfLeftPointToRightPoint;
            }
            set
            {
                m_dDistanceOfLeftPointToRightPoint = value;
            }
        }
        double m_dDifferenceOfFocusDistance;
        public double p_dDifferenceOfFocusDistance
        {
            get
            {
                return m_dDifferenceOfFocusDistance;
            }
            set
            {
                m_dDifferenceOfFocusDistance = value;
            }
        }

        public double GetThetaDegree(double dRadian)
        {
            return dRadian * (180 / Math.PI);
        }
        public double GetThetaRadian()
        {
            return Math.Atan2(m_dDistanceOfLeftPointToRightPoint, m_dDifferenceOfFocusDistance);
        }
        public double GetImageVarianceScore(ImageData img, int nVarianceSize)
        {
            // variable
            double dTemp = 0;
            int nTempCount = 0;
            int nImageWidth  = 0;   // 이미지 Width 넣어야 함
            int nImageHeight = 0;   // 이미지 Height 넣어야 함
            
            // implement
            for (int i = 0; i < nImageWidth; i++)
            {
                for (int j = 0; j < nImageHeight; j++)
                {
                    dTemp += GetImageLocalVariance(img, new Point(i * nVarianceSize, j * nVarianceSize), nVarianceSize);
                    nTempCount++;
                }
            }

            return dTemp / (double)nTempCount;
        }
        public double GetImageLocalVariance(ImageData img, Point pt, int nVarianceSize)
        {
            // variable
            double dTemp = 0.0;
            double dAvg = 0.0;
            int nCount = 0;
            int nVal = 0;

            // implement
            dAvg = GetImageLocalAverage(img, pt, nVarianceSize);

            unsafe
            {
                byte* p = (byte*)(img.m_ptrImg.ToPointer());
                for (int i = 0; i < nVarianceSize; i++)
                {
                    for (int j = 0; j < nVarianceSize; j++)
                    {
                        nVal = p[((int)pt.Y + j) * img.p_Size.X + (int)pt.X + i];
                        dTemp += (nVal - dAvg) * (nVal - dAvg);
                    }
                }
            }

            return dTemp / (double)nVarianceSize / (double)nVarianceSize;
        }

        public double GetImageLocalAverage(ImageData img, Point pt, int nVarianceSize)
        {
            // variable
            double dTemp = 0.0;

            // implement
            unsafe
            {
                byte* p = (byte*)(img.m_ptrImg.ToPointer());
                for (int i = 0; i < nVarianceSize; i++)
                {
                    for (int j = 0; j < nVarianceSize; j++)
                    {
                        dTemp += p[((int)pt.Y + j) * img.p_Size.X + (int)pt.X + i];
                    }
                }
            }
            
            return dTemp / (double)nVarianceSize / (double)nVarianceSize;
        }

    }
}
