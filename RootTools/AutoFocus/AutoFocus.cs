using RootTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RootTools.AutoFocus
{
    public enum eImageState { GRAY_SCALE, COLOR_RGB };
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
        eImageState m_eImageState = eImageState.COLOR_RGB;
        public eImageState p_eImageState
        {
            get
            {
                return m_eImageState;
            }
            set
            {
                m_eImageState = value;
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
            int nWidth = img.p_Size.X / nVarianceSize;
            int nHeight = img.p_Size.Y / nVarianceSize;

            // implement
            for (int i = 0; i < nWidth; i++)
            {
                for (int j = 0; j < nHeight; j++)
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
            int nVal = 0;

            // implement
            dAvg = GetImageLocalAverage(img, pt, nVarianceSize);

            unsafe
            {
                switch (m_eImageState)
                {
                    case eImageState.GRAY_SCALE:
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
                            break;
                        }
                    case eImageState.COLOR_RGB:
                        {
                            byte* p = (byte*)(img.m_ptrImg.ToPointer());
                            for (int i = 0; i<nVarianceSize; i++)
                            {
                                for (int j = 0; j<nVarianceSize; j++)
                                {
                                    nVal = GetRGBPixelVal((int)pt.X + i, (int)pt.Y + j, p);
                                    dTemp += (nVal - dAvg) * (nVal - dAvg);
                                }
                            }
                            break;
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

        unsafe public byte GetRGBPixelVal(int x, int y, byte *p)
        {
            //200507
            return 0;
        }
    }
}
